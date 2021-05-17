using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HatsUnity;
using UnityEngine;

namespace HatsCore
{
   public class HatsEventProcessor
   {
      private Dictionary<int, Turn> _turnTable = new Dictionary<int, Turn>();
      private int _currentTurnNumber;
      private Queue<HatsGameMessage> _messageQueue;
      private System.Random _random;
      private readonly BattleGrid _grid;
      private readonly int _framesPerSecond;
      private readonly int _secondsPerTurn;
      private List<HatsPlayer> _players;
      private List<HatsBot> _bots;
      private long _currentFrameNumber;

      private long _turnStartFrameNumber;

      public int PlayerCount => _players.Count;
      public int CurrentTurn => _currentTurnNumber;

      public long CurrentFrame => _currentFrameNumber;
      public float TurnTimeCompletionRatio => (float)(_currentFrameNumber - _turnStartFrameNumber) / TicksPerTurn;

      public float SecondsLeftInTurn => _secondsPerTurn * (1 - TurnTimeCompletionRatio);

      public int TicksPerTurn => _framesPerSecond * _secondsPerTurn;

      public long TurnTimoutFrameNumber => _turnStartFrameNumber + TicksPerTurn;

      public HatsEventProcessor(
         BattleGrid grid,
         int framesPerSecond,
         int secondsPerTurn,
         List<HatsPlayer> players,
         int randomSeed,
         Queue<HatsGameMessage> messages,
         bool fillWithBots = true)
      {
         _grid = grid;
         _framesPerSecond = framesPerSecond;
         _secondsPerTurn = secondsPerTurn;
         _players = players.ToList();
         _messageQueue = messages;
         _random = new System.Random(randomSeed);

         _bots = new List<HatsBot>();
         if (fillWithBots)
         {
            for (var i = _players.Count; i < 4; i++)
            {
               var bot = new HatsBot((i * 1000) + _random.Next(999), _random, _grid);
               _players.Add(bot);
               _bots.Add(bot);
            }
         }
      }

      public HatsPlayer GetPlayer(long dbid)
      {
         return _players.Find(player => player.dbid == dbid);
      }

      public IEnumerable<HatsGameEvent> PlayGame()
      {
         // spawn all the characters in...
         foreach (var evt in SetInitialTurn())
         {
            yield return evt;
         }

         _currentTurnNumber = 1;
         _turnStartFrameNumber = 0;

         CreateBotMoves(GetCurrentTurn());
         // settle into the main game loop
         while (true)
         {
            // process the input player move queue...
            if (_messageQueue.Count > 0)
            {
               var message = _messageQueue.Dequeue();
               switch (message)
               {
                  case HatsPlayerMove move:
                     HandleMove(move);
                     yield return new PlayerCommittedMoveEvent();
                     break;
                  case HatsTickMessage tick:
                     _currentFrameNumber = tick.FrameNumber;
                     yield return new TickEvent(_currentFrameNumber);
                     break;
               }
            }

            // its possible that the turn needs to end due to a time limit, even if all players haven't submitted a move.
            if (_currentFrameNumber > TurnTimoutFrameNumber)
            {
               // automatically fill out player's with skip moves.
               ForceUnCommittedPlayersToSkip();
            }

            // check if the current turn is ready to play
            var currentTurn = GetTurn(_currentTurnNumber);
            var isTurnReady = currentTurn.CommittedMoves == PlayerCount;
            if (isTurnReady)
            {
               _turnStartFrameNumber = _currentFrameNumber;
               _currentTurnNumber++;
               yield return new TurnReadyEvent();
               foreach (var evt in HandleTurn(currentTurn))
               {
                  yield return evt;
               }
               yield return new TurnOverEvent();

               CreateBotMoves(GetCurrentTurn());
            }

            // TODO check for a win condition


            yield return null;
         }
      }

      private IEnumerable<HatsGameEvent> SetInitialTurn()
      {
         var startingPositions = new Vector3Int[]
         {
            new Vector3Int(-3, -3, 0),
            new Vector3Int(2, -3, 0),
            new Vector3Int(2, 3, 0),
            new Vector3Int(-3, 3, 0)
         };

         // set up first turn.
         var turnZero = GetTurn(0);
         for (var i = 0; i < _players.Count; i++)
         {
            var player = _players[i];
            var state = turnZero.GetPlayerState(player.dbid);

            state.Position = startingPositions[i];
            yield return new PlayerSpawnEvent(player, state.Position);
         }

         var nextTurn = GetTurn(1);
         nextTurn.CopyStateFromTurn(turnZero);

      }

      private void CreateBotMoves(Turn turn)
      {
         // all bots need to report a move for the turn. They get the current board state as the input.
         foreach (var bot in _bots)
         {
            var move = bot.PerformMove(turn.TurnNumber, turn.PlayerState);
            turn.CommitMove(move);
         }
      }

      private void HandleMove(HatsPlayerMove move)
      {
         var turn = GetTurn(move.TurnNumber);
         turn.CommitMove(move);
      }

      private void ForceUnCommittedPlayersToSkip()
      {
         var turn = GetCurrentTurn();
         var uncommittedPlayers = _players.Where(player => !turn.Moves.ContainsKey(player.dbid)).ToList();
         foreach (var player in uncommittedPlayers)
         {
            var skipMove = new HatsPlayerMove
            {
               Dbid = player.dbid,
               TurnNumber = turn.TurnNumber,
               MoveType = HatsPlayerMoveType.SKIP,
               Direction = Direction.Nowhere
            };
            turn.CommitMove(skipMove);
         }
      }

      private IEnumerable<HatsGameEvent> HandleTurn(Turn turn)
      {
         var moves = turn.Moves.Select(kvp => kvp.Value).ToList();
         var next = GetTurn(turn.TurnNumber + 1);
         next.CopyStateFromTurn(turn);

         // step 1. All shields go up.
         foreach (var evt in HandleShields(moves, turn, next))
         {
            yield return evt;
         }

         // step 2. Then players move.
         foreach (var evt in HandleWalks(moves, turn, next))
         {
            yield return evt;
         }

         // step 3. Then players attack.
         foreach (var evt in HandleAttacks(moves, turn, next))
         {
            yield return evt;
         }

         // step 4. De-activate all shields.
         next.DisableAllShields();

      }

      IEnumerable<HatsGameEvent> HandleShields(List<HatsPlayerMove> moves, Turn turn, Turn nextTurn)
      {
         var shieldMoves = moves.Where(move => move.IsShieldMove);
         foreach (var shieldMove in shieldMoves)
         {
            if (turn.IsPlayerDead(shieldMove.Dbid)) continue;

            var player = GetPlayer(shieldMove.Dbid);
            turn.ActivateShieldForPlayer(player.dbid);
            yield return new PlayerShieldEvent(player);
         }
      }

      IEnumerable<HatsGameEvent> HandleWalks(List<HatsPlayerMove> moves, Turn turn, Turn nextTurn)
      {
         var walkMoves = moves.Where(move => move.IsWalkMove);
         foreach (var walkMove in walkMoves)
         {
            var currPosition = turn.GetPlayerState(walkMove.Dbid).Position;
            var nextPosition = _grid.InDirection(currPosition, walkMove.Direction);
            nextTurn.GetPlayerState(walkMove.Dbid).Position = nextPosition;
         }
         // if any players wind up in the same
         foreach (var walkMove1 in walkMoves)
         {
            foreach (var walkMove2 in walkMoves)
            {
               if (walkMove1.Dbid == walkMove2.Dbid) continue;
               if (nextTurn.GetPlayerState(walkMove1.Dbid).Position == nextTurn.GetPlayerState(walkMove2.Dbid).Position)
               {
                  // TODO: Pick randomly who gets bumped back.
               }
            }
         }

         foreach (var walkMove in walkMoves)
         {
            var currPosition = turn.GetPlayerState(walkMove.Dbid).Position;
            var nextPosition = _grid.InDirection(currPosition, walkMove.Direction);
            var player = GetPlayer(walkMove.Dbid);
            yield return new PlayerMoveEvent(player, currPosition, nextPosition);
         }
      }

      IEnumerable<HatsGameEvent> HandleAttacks(List<HatsPlayerMove> moves, Turn turn, Turn nextTurn)
      {
         var attackMoves = moves.Where(move => move.IsFireballMove || move.IsArrowMove).ToList();
         // spawn a fireball with direction at

         var allAttackEvents = new List<PlayerAttackEvent>();
         var allKillEvents = new List<PlayerKilledEvent>();

         var simulatedPositions = new Dictionary<HatsPlayerMove, Vector3Int>();
         var moveToEvent = new Dictionary<HatsPlayerMove, PlayerAttackEvent>();

         foreach (var move in attackMoves)
         {
            if (turn.IsPlayerDead(move.Dbid)) continue;

            var plr = GetPlayer(move.Dbid);
            var startPosition = nextTurn.GetPlayerState(move.Dbid).Position;
            var attackType = move.IsArrowMove
               ? PlayerAttackEvent.AttackType.ARROW
               : PlayerAttackEvent.AttackType.FIREBALL;
            var attackEvent = new PlayerAttackEvent(plr, attackType, move.Direction);
            allAttackEvents.Add(attackEvent);
            simulatedPositions[move] = startPosition;
            moveToEvent[move] = attackEvent;
         }

         var longestRange = 20;
         for (var i = 0; i < longestRange; i++)
         {
            foreach (var move in attackMoves)
            {
               var plr = GetPlayer(move.Dbid);
               var evt = moveToEvent[move];
               if (evt.DestroyAt.HasValue) continue; // this attack has been simulated to the end.

               var currentPosition = simulatedPositions[move];
               var dir = evt.BounceDirection.HasValue
                  ? evt.BounceDirection.Value
                  : move.Direction;
               var newPosition = _grid.InDirection(currentPosition, dir);

               simulatedPositions[move] = newPosition;
               //TODO: calculate intersections with other attacks

               // calculate intersections with players
               var hitPlayers = nextTurn.GetAlivePlayersAtPosition(newPosition);
               foreach (var hitPlayer in hitPlayers)
               {
                  var victim = GetPlayer(hitPlayer);
                  if (turn.IsPlayerShielding(hitPlayer) && move.IsFireballMove)
                  {
                     evt.BounceAt = newPosition;
                     evt.BounceDirection = move.Direction.Reverse();
                  }
                  else
                  {
                     var killEvent = new PlayerKilledEvent(victim, plr);
                     evt.KillsPlayer = victim;
                     evt.DestroyAt = newPosition;
                     nextTurn.GetPlayerState(victim.dbid).IsDead = true;
                     allKillEvents.Add(killEvent);
                  }
               }
            }
         }

         // return all attack events
         foreach (var evt in allAttackEvents)
         {
            yield return evt;
         }

         // return all kill events
         foreach (var evt in allKillEvents)
         {
            yield return evt;
         }

         yield return null;
      }


      private Turn GetTurn(int turnNumber)
      {
         if (!_turnTable.TryGetValue(turnNumber, out var turn))
         {
            turn = new Turn
            {
               TurnNumber = turnNumber
            };
            _turnTable[turnNumber] = turn;
         }

         return turn;
      }

      public Turn GetCurrentTurn()
      {
         return GetTurn(_currentTurnNumber);
      }
   }
}