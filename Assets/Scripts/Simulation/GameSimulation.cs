using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Experimental.Api.Sim;
using Hats.Content;
using Hats.Game;
using UnityEngine;

namespace Hats.Simulation
{
	public class GameSimulation
	{
		private readonly BattleGrid _grid;
		private readonly int _framesPerSecond;
		private readonly int _secondsPerTurn;
		private readonly int _turnsUntilSuddenDeath;
		private readonly double _chanceToSpawnSuddenDeathTile;
		private readonly BotProfileContent _botProfileContent;
		private Dictionary<int, Turn> _turnTable = new Dictionary<int, Turn>();
		private Dictionary<long, int> _dbidToScore = new Dictionary<long, int>();

		private int _currentTurnNumber;
		private Queue<HatsGameMessage> _messageQueue;
		private System.Random _random;
		private List<HatsPlayer> _players;
		private List<HatsBot> _bots;
		private long _currentFrameNumber;
		private long _turnStartFrameNumber;
		private bool _gameOver;

		public int PlayerCount => _players.Count;
		public int CurrentTurn => _currentTurnNumber;

		public long CurrentFrame => _currentFrameNumber;
		public float TurnTimeCompletionRatio => (float)(_currentFrameNumber - _turnStartFrameNumber) / TicksPerTurn;

		public float SecondsLeftInTurn => _secondsPerTurn * (1 - TurnTimeCompletionRatio);

		public int TicksPerTurn => _framesPerSecond * _secondsPerTurn;

		public long TurnTimoutFrameNumber => _turnStartFrameNumber + TicksPerTurn;

		public GameSimulation(
			BattleGrid grid,
			int framesPerSecond,
			int secondsPerTurn,
			int turnsUntilSuddenDeath,
			double chanceToSpawnSuddenDeathTile,
			List<HatsPlayer> players,
			BotProfileContent botProfileContent,
			int randomSeed,
			Queue<HatsGameMessage> messages,
			bool fillWithBots = true)
		{
			_grid = grid;
			_framesPerSecond = framesPerSecond;
			_secondsPerTurn = secondsPerTurn;
			_turnsUntilSuddenDeath = turnsUntilSuddenDeath;
			_chanceToSpawnSuddenDeathTile = chanceToSpawnSuddenDeathTile;
			_botProfileContent = botProfileContent;
			_players = players.ToList();
			_messageQueue = messages;
			_random = new System.Random(randomSeed);

			// Set up grid
			_grid.Initialize(_random);

			// Spawn bots
			_bots = new List<HatsBot>();
			if (fillWithBots)
			{
				for (var i = _players.Count; i < 4; i++)
				{
					var bot = new HatsBot((i * 1000) + _random.Next(999), _random, _grid, _botProfileContent);
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
						case HatsPlayerDropped drop:
							// a dropped player is the same as a dead player.
							var droppedPlayer = GetPlayer(drop.Dbid);
							yield return new PlayerLeftEvent(droppedPlayer);
							break;

						case HatsPlayerMove move:
							HandleMove(move);
							yield return new PlayerCommittedMoveEvent();

							// allow free-roam.
							if (move.TurnNumber == -1)
							{
								var isDead = GetCurrentTurn().IsPlayerDead(move.Dbid);
								if (isDead)
								{
									var currPosition = GetCurrentTurn().GetPlayerState(move.Dbid).Position;
									var nextPosition = _grid.InDirection(currPosition, move.Direction);
									GetCurrentTurn().GetPlayerState(move.Dbid).Position = nextPosition;
									yield return new PlayerMoveEvent(GetPlayer(move.Dbid), currPosition, nextPosition);
								}
							}

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
				var isTurnReady = currentTurn.CommitedMovesFromAlivePlayers == currentTurn.GetAlivePlayers().Count;
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

					foreach (var evt in CheckGameState())
					{
						if (evt is GameOverEvent)
						{
							_gameOver = true;
						}
						yield return evt;
					}

					CreateBotMoves(GetCurrentTurn());
				}

				if (_gameOver)
				{
					break; // the game loop is over!
				}
				yield return null;
			}
		}

		public Turn GetCurrentTurn()
		{
			return GetTurn(_currentTurnNumber);
		}

		private static void AddNewProjectile(HatsPlayerMove sourceMove, List<HatsPlayerMove> newMoves, Dictionary<HatsPlayerMove, Vector3Int> newMoveToStartingPosition, Vector3Int startingPosition)
		{
			HatsPlayerMove projectile = new HatsPlayerMove()
			{
				Direction = sourceMove.Direction,
				Dbid = sourceMove.Dbid,
				MoveType = sourceMove.MoveType,
				TurnNumber = sourceMove.TurnNumber,
			};
			newMoveToStartingPosition.Add(projectile, startingPosition);
			newMoves.Add(projectile);
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
				IncreasePlayerScore(player.dbid, 0);
				yield return new PlayerSpawnEvent(player, state.Position);
			}

			var nextTurn = GetTurn(1);
			nextTurn.CopyStateFromTurn(turnZero);
		}

		private void IncreasePlayerScore(long dbid, int score)
		{
			if (!_dbidToScore.ContainsKey(dbid))
			{
				_dbidToScore.Add(dbid, 0);
			}

			_dbidToScore[dbid] += score;
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
			if (move.TurnNumber < 0) return; // ignore any free-roam messages

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

		private List<PlayerResult> CalculateScores()
		{
			// convert the scores into a set of PlayerResult
			var results = _dbidToScore.Select(kvp => new PlayerResult
			{
				playerId = kvp.Key,
				score = kvp.Value,
				rank = 0
			}).ToList();

			// order it by score
			results.Sort((a, b) => a.score > b.score ? -1 : 1);

			// assign ranks based on the score/order
			for (var i = 0; i < results.Count; i++)
			{
				results[i].rank = i + 1;
			}

			return results;
		}

		private IEnumerable<HatsGameEvent> CheckGameState()
		{
			var turn = GetCurrentTurn();

			var alivePlayers = turn.GetAlivePlayers();
			var aliveCount = alivePlayers.Count;
			if (aliveCount == 1)
			{
				// a single player has won!
				IncreasePlayerScore(alivePlayers[0], 50);
				var scores = CalculateScores();
				yield return new GameOverEvent(GetPlayer(alivePlayers[0]), scores);
			}
			else if (aliveCount == 0)
			{
				// all players died this round; and should all be respawned
				foreach (var player in _players)
				{
					var state = turn.GetPlayerState(player.dbid);

					// TODO: Make sure that a player can't respawn ontop of another player
					state.IsDead = false;
					yield return new PlayerRespawnEvent(player, state.Position);
				}
			}
		}

		private IEnumerable<HatsGameEvent> HandleTurn(Turn turn)
		{
			var moves = turn.Moves.Select(kvp => kvp.Value).ToList();
			var next = GetTurn(turn.TurnNumber + 1);
			next.CopyStateFromTurn(turn);

			// Shields up, move, attack, shields down again
			foreach (var evt in HandleShields(moves, turn, next))
				yield return evt;

			foreach (var evt in HandleWalks(moves, turn, next))
				yield return evt;

			foreach (var evt in HandleAttacks(moves, turn, next))
				yield return evt;

			next.DisableAllShields();

			// Surrendering players commit suicide
			foreach (var evt in HandleSurrenders(moves, turn, next))
				yield return evt;

			if (turn.TurnNumber > _turnsUntilSuddenDeath)
			{
				// Create new sudden death tiles, kill players and remove powerups that were on such tiles before
				foreach (var evt in HandleSuddenDeath(turn, next))
					yield return evt;
			}

			foreach (var evt in HandlePowerups(moves, turn, next))
				yield return evt;
		}

		private IEnumerable<HatsGameEvent> HandleSuddenDeath(Turn turn, Turn nextTurn)
		{
			_grid.AdvanceSuddenDeathTiles();

			foreach (var pu in turn.CollectablePowerups)
			{
				if (_grid.IsLava(pu.Position))
				{
					yield return new CollectablePowerupDestroyEvent(pu.Position, CollectablePowerupDestroyEvent.DestroyReason.DESTROYED_AND_LOST);
					nextTurn.CollectablePowerups.Remove(nextTurn.CollectablePowerups.FirstOrDefault(p => p.Position == pu.Position));
				}
			}

			foreach (var player in _players)
			{
				var nextState = nextTurn.GetPlayerState(player.dbid);
				if (!nextState.IsDead)
				{
					if (_grid.IsLava(nextState.Position))
					{
						yield return new PlayerKilledEvent(player, player);
						nextTurn.GetPlayerState(player.dbid).IsDead = true;
					}
				}
			}

			var playerPositions = nextTurn.GetAlivePlayerPositions();
			foreach (var possibleLavaTile in playerPositions)
			{
				if (_random.NextDouble() > _chanceToSpawnSuddenDeathTile)
				{
					_grid.EnterSuddenDeath(possibleLavaTile);
					yield return new SuddenDeathEvent(possibleLavaTile);
				}
			}
		}

		private IEnumerable<HatsGameEvent> HandlePowerups(List<HatsPlayerMove> moves, Turn turn, Turn nextTurn)
		{
			foreach (var dbidToState in nextTurn.PlayerState)
			{
				foreach (var p in dbidToState.Value.Powerups.ToList())
				{
					if (nextTurn.TurnNumber > p.ObtainedInTurnNumber + p.TimeoutInTurns)
					{
						var player = GetPlayer(dbidToState.Key);
						yield return new PowerupRemoveEvent(p, player);
						dbidToState.Value.Powerups.Remove(p);
					}
				}
			}

			const int maxCollectablePowerupsInWorld = 3;
			var spawnNewPowerup = !nextTurn.PlayerState.Any(ps => ps.Value.Powerups.Count > 0) && nextTurn.CollectablePowerups.Count() < maxCollectablePowerupsInWorld;
			if (spawnNewPowerup)
			{
				List<Vector3Int> validTiles = _grid.GetValidPowerupTiles();

				foreach (var kvp in nextTurn.PlayerState)
					validTiles.Remove(kvp.Value.Position);

				foreach (var pu in turn.CollectablePowerups)
					validTiles.Remove(pu.Position);

				const int numberOfCollectablePowerupsToSpawnAtOnce = 10;
				for (int i = 0; i < Math.Min(numberOfCollectablePowerupsToSpawnAtOnce, validTiles.Count); i++)
				{
					if (validTiles.Count > 0)
					{
						Vector3Int newPowerupPosition = validTiles.ElementAt(_random.Next(0, validTiles.Count - 1));
						nextTurn.CollectablePowerups.Add(new HatsPowerup()
						{
							Type = HatsPowerupType.FIREWALL,
							Position = newPowerupPosition,
							TimeoutInTurns = 3,
						});

						yield return new CollectablePowerupSpawnEvent(HatsPowerupType.FIREWALL, newPowerupPosition);
						validTiles.Remove(newPowerupPosition);
					}
				}
			}
		}

		private IEnumerable<HatsGameEvent> HandleSurrenders(List<HatsPlayerMove> moves, Turn turn, Turn nextTurn)
		{
			var surrenderMoves = moves.Where(move => move.IsSurrenderMove);
			foreach (var surrenderMove in surrenderMoves)
			{
				var player = GetPlayer(surrenderMove.Dbid);
				yield return new PlayerKilledEvent(player, player);
				nextTurn.GetPlayerState(player.dbid).IsDead = true;
			}
		}

		private IEnumerable<HatsGameEvent> HandleShields(List<HatsPlayerMove> moves, Turn turn, Turn nextTurn)
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

		private IEnumerable<HatsGameEvent> HandleWalks(List<HatsPlayerMove> moves, Turn turn, Turn nextTurn)
		{
			var walkMoves = moves.Where(move => move.IsWalkMove);

			// invalidate any moves that walk into another player's position
			walkMoves = walkMoves.Where(move =>
			{
				var currPosition = turn.GetPlayerState(move.Dbid).Position;
				var nextPosition = _grid.InDirectionSlideWithIce(currPosition, move.Direction);
				var playersAtSpot = turn.GetAlivePlayersAtPosition(nextPosition);
				return playersAtSpot.Count == 0;
			}).ToList();

			// update the next turn state
			foreach (var walkMove in walkMoves)
			{
				var currPosition = turn.GetPlayerState(walkMove.Dbid).Position;
				var nextPosition = _grid.InDirectionSlideWithIce(currPosition, walkMove.Direction);
				nextTurn.GetPlayerState(walkMove.Dbid).Position = nextPosition;
			}

			// if any players wind up in the same cell, randomly bump one of them back
			foreach (var walkMove1 in walkMoves)
			{
				foreach (var walkMove2 in walkMoves)
				{
					if (walkMove1.Dbid == walkMove2.Dbid) continue; // don't check for intersection with self

					if (nextTurn.GetPlayerState(walkMove1.Dbid).Position !=
						nextTurn.GetPlayerState(walkMove2.Dbid).Position) continue; // the players aren't intersecting

					var bumpSelf = _random.NextDouble() > .5f;
					var moveToChange = bumpSelf
						? walkMove1
						: walkMove2;
					moveToChange.Direction = Direction.Nowhere;
					nextTurn.GetPlayerState(moveToChange.Dbid).Position = turn.GetPlayerState(moveToChange.Dbid).Position;
				}
			}

			// send the validated, adjusted, walk moves.
			foreach (var walkMove in walkMoves)
			{
				var currPosition = turn.GetPlayerState(walkMove.Dbid).Position;
				var nextPosition = nextTurn.GetPlayerState(walkMove.Dbid).Position;
				var player = GetPlayer(walkMove.Dbid);
				yield return new PlayerMoveEvent(player, currPosition, nextPosition);

				// If the player ended up in lava, kill them
				if (_grid.IsLava(nextPosition))
				{
					yield return new PlayerKilledEvent(player, player);
					nextTurn.GetPlayerState(player.dbid).IsDead = true;
				}
				// Collect powerups
				else
				{
					List<HatsPowerup> powerups = nextTurn.CollectablePowerups.Where(p => p.Position == nextPosition).ToList();
					if (powerups.Count > 0)
					{
						HatsPowerup newPowerup = powerups[0];

						var nextState = nextTurn.GetPlayerState(walkMove.Dbid);
						var existingPowerup = nextState.Powerups.Find(p => p.Type == newPowerup.Type);
						if (existingPowerup == default(HatsPowerup))
						{
							newPowerup.ObtainedInTurnNumber = turn.TurnNumber;
							nextState.Powerups.Add(newPowerup);
							yield return new PowerupCollectEvent(newPowerup, player);
						}
						else
							existingPowerup.ObtainedInTurnNumber = turn.TurnNumber;

						yield return new CollectablePowerupDestroyEvent(newPowerup.Position, CollectablePowerupDestroyEvent.DestroyReason.COLLECTED_BY_PLAYER);
					}
				}
			}
		}

		private IEnumerable<HatsGameEvent> HandleAttacks(List<HatsPlayerMove> moves, Turn turn, Turn nextTurn)
		{
			var sourceAttackMoves = moves.Where(move => move.IsFireballMove || move.IsArrowMove).ToList();

			var allAttackMoves = new List<HatsPlayerMove>();

			var allAttackEvents = new List<PlayerProjectileAttackEvent>();
			var allKillEvents = new List<PlayerKilledEvent>();

			var simulatedPositions = new Dictionary<HatsPlayerMove, Vector3Int>();
			var moveToEvent = new Dictionary<HatsPlayerMove, PlayerProjectileAttackEvent>();

			foreach (var sourceMove in sourceAttackMoves)
			{
				if (turn.IsPlayerDead(sourceMove.Dbid)) continue;

				var player = GetPlayer(sourceMove.Dbid);
				var attackType = sourceMove.IsArrowMove
					? PlayerProjectileAttackEvent.AttackType.ARROW
					: PlayerProjectileAttackEvent.AttackType.FIREBALL;

				var newMoves = new List<HatsPlayerMove>();
				var newMoveToStartingPosition = new Dictionary<HatsPlayerMove, Vector3Int>();

				newMoves.Add(sourceMove);
				Vector3Int playerPosition = nextTurn.GetPlayerState(sourceMove.Dbid).Position;
				newMoveToStartingPosition.Add(sourceMove, playerPosition);

				if (sourceMove.MoveType == HatsPlayerMoveType.FIREBALL && turn.GetPlayerState(player.dbid).HasFirewallPowerup)
				{
					Vector3Int[] firewallFlankPositions =
					{
						_grid.InDirection(playerPosition, sourceMove.Direction.LookLeft()),
						_grid.InDirection(playerPosition, sourceMove.Direction.LookRight()),
					};

					foreach (var pos in firewallFlankPositions)
					{
						if (!_grid.IsRock(pos))
							AddNewProjectile(sourceMove, newMoves, newMoveToStartingPosition, pos);
					}

					nextTurn.GetPlayerState(player.dbid).Powerups.RemoveAll(p => p.Type == HatsPowerupType.FIREWALL);
					yield return new PowerupRemoveEvent(new HatsPowerup() { Type = HatsPowerupType.FIREWALL }, player);
				}

				foreach (var move in newMoves)
				{
					allAttackMoves.Add(move);

					var startPosition = newMoveToStartingPosition[move];
					var attackEvent = new PlayerProjectileAttackEvent(player, attackType, startPosition, move.Direction);
					allAttackEvents.Add(attackEvent);

					simulatedPositions[move] = startPosition;
					moveToEvent[move] = attackEvent;
				}
			}

			SimulateProjectiles(turn, nextTurn, allAttackMoves, allKillEvents, simulatedPositions, moveToEvent);

			foreach (var evt in allAttackEvents)
				yield return evt;

			foreach (var evt in allKillEvents)
			{
				IncreasePlayerScore(evt.Murderer.dbid, 10);
				yield return evt;
			}

			yield return null;
		}

		private void SimulateProjectiles(Turn turn, Turn nextTurn, List<HatsPlayerMove> attackMoves, List<PlayerKilledEvent> allKillEvents, Dictionary<HatsPlayerMove, Vector3Int> simulatedPositions, Dictionary<HatsPlayerMove, PlayerProjectileAttackEvent> moveToEvent)
		{
			const int longestRange = 20;
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

					// Both arrows and fireballs are destroyed when hitting rocks
					if (_grid.IsRock(newPosition))
					{
						evt.DestroyAt = newPosition;
					}

					// calculate intersections with other projectiles
					foreach (var otherMove in attackMoves)
					{
						if (ReferenceEquals(otherMove, move)) continue; // don't check ourselves for intersections

						if (newPosition != simulatedPositions[otherMove]) continue; // the projectiles aren't intersecting

						var otherEvt = moveToEvent[otherMove];

						// arrows destroy each-other
						if (evt.Type == PlayerProjectileAttackEvent.AttackType.ARROW &&
							otherEvt.Type == PlayerProjectileAttackEvent.AttackType.ARROW)
						{
							evt.DestroyAt = newPosition;
							otherEvt.DestroyAt = newPosition;
						}

						// fireballs destroy arrows
						if (evt.Type == PlayerProjectileAttackEvent.AttackType.ARROW
							&& otherEvt.Type == PlayerProjectileAttackEvent.AttackType.FIREBALL)
						{
							evt.DestroyAt = newPosition;
						}
					}

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
	}
}