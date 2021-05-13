using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HatsCore
{
   public class HatsEventProcessor
   {
      private Dictionary<int, Turn> _turnTable = new Dictionary<int, Turn>();
      private int _currentTurnNumber;
      private Queue<HatsPlayerMove> _moveQueue;
      private System.Random _random;
      private readonly BattleGrid _grid;
      private List<HatsPlayer> _players;

      public int PlayerCount => _players.Count;
      public int CurrentTurn => _currentTurnNumber;

      public HatsEventProcessor(BattleGrid grid, List<HatsPlayer> players, int randomSeed, Queue<HatsPlayerMove> moves)
      {
         _grid = grid;
         _players = players;
         _moveQueue = moves;
         _random = new System.Random(randomSeed);

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

         // settle into the main game loop
         while (true)
         {
            // process the input player move queue...
            if (_moveQueue.Count > 0)
            {
               var move = _moveQueue.Dequeue();
               HandleMove(move);
               yield return new PlayerCommittedMoveEvent();
            }

            // check if the current turn is ready to play
            var currentTurn = GetTurn(_currentTurnNumber);
            var isTurnReady = currentTurn.CommittedMoves == PlayerCount;
            if (isTurnReady)
            {
               _currentTurnNumber++;
               yield return new TurnReadyEvent();
               foreach (var evt in HandleTurn(currentTurn))
               {
                  yield return evt;
               }
            }

            // check for a win condition

            yield return null;
         }
      }

      private IEnumerable<HatsGameEvent> SetInitialTurn()
      {
         var startingPositions = new Vector3Int[]
         {
            new Vector3Int(-3, -3, 0),
            new Vector3Int(1, -1, 0),
            new Vector3Int(-1, 1, 0),
            new Vector3Int(1, 1, 0),
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

      private void HandleMove(HatsPlayerMove move)
      {
         var turn = GetTurn(move.TurnNumber);
         turn.CommitMove(move);
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
      }

      IEnumerable<HatsGameEvent> HandleShields(List<HatsPlayerMove> moves, Turn turn, Turn nextTurn)
      {
         var shieldMoves = moves.Where(move => move.IsShieldMove);
         foreach (var shieldMove in shieldMoves)
         {
            yield return new PlayerAttackEvent(); // TODO: make this correct.
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