
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hats.Content;
using Hats.Simulation;
using Hats.Game;
using JetBrains.Annotations;
using Unity.Collections;
using UnityEngine;

namespace Hats.Game
{
	public class GameProcessor : MonoBehaviour
	{
		public GameSimulation EventProcessor;
		public BattleGridBehaviour BattleGridBehaviour;
		public MultiplayerGameDriver MultiplayerGameDriver;
		public BotProfileRef BotProfileRef;

		public string roomId = "room1";
		public int framesPerSecond = 20;
		public int turnTime = 5;
		public int turnsUntilSuddenDeath = 4;
		public double chanceToSpawnSuddenDeathTile = .65;

		[CanBeNull] public static string RoomId;
		[CanBeNull] public static List<long> Dbids;

		public List<GameEventHandler> EventHandlers;

		[Hats.Simulation.ReadOnly]
		[SerializeField]
		private int currentTurn;


		private async void Start()
		{
			roomId = RoomId ?? roomId;
			Debug.Log($"Game Starting... with roomId=[{roomId}]");
			var beamable = await Beamable.API.Instance;
			var botProfile = await BotProfileRef.Resolve();
			var dbids = Dbids ?? new List<long> { beamable.User.id };
			StartGame(dbids, botProfile);
		}

		public void StartGame(List<long> dbids, BotProfileContent botProfileContent)
		{
			// TODO: Handle matchmaking
			var messageQueue = MultiplayerGameDriver.Init(roomId, framesPerSecond, new List<long>());

			var players = dbids.Select(dbid => new HatsPlayer
			{
				dbid = dbid
			}).ToList();

			EventProcessor = new GameSimulation(BattleGridBehaviour.BattleGrid, framesPerSecond, turnTime, turnsUntilSuddenDeath, chanceToSpawnSuddenDeathTile, players, botProfileContent, roomId.GetHashCode(), messageQueue);
			StartCoroutine(PlayGame());
		}

		public HatsPlayerState GetCurrentPlayerState(long dbid)
		{
			return EventProcessor.GetCurrentTurn().GetPlayerState(dbid);
		}

		public float SecondsLeftInTurn => EventProcessor.SecondsLeftInTurn;

		private IEnumerator PlayGame()
		{
			yield return null; // wait a single frame
			yield return null; // wait a single frame (FIXME: should wait until the simulation is ready and grid is initialized)
			foreach (var evt in EventProcessor.PlayGame())
			{
				currentTurn = EventProcessor.CurrentTurn;
				if (evt == null)
				{
					yield return null;
					continue;
				}

				if (!(evt is TickEvent))
				{
					Debug.Log($"Game Event: {evt}");
				}

				switch (evt)
				{
					case PlayerSpawnEvent spawnEvt:
						yield return EventHandlers.Handle(this, spawnEvt, handler => handler.HandleSpawnEvent);
						break;
					case PlayerMoveEvent moveEvt:
						yield return EventHandlers.Handle(this, moveEvt, handler => handler.HandleMoveEvent);
						break;
					case PlayerShieldEvent shieldEvt:
						yield return EventHandlers.Handle(this, shieldEvt, handler => handler.HandleShieldEvent);
						break;
					case TurnReadyEvent turnEvt:
						yield return EventHandlers.Handle(this, turnEvt, handler => handler.HandleTurnReadyEvent);
						break;
					case TurnOverEvent turnOverEvt:
						yield return EventHandlers.Handle(this, turnOverEvt, handler => handler.HandleTurnOverEvent);
						break;
					case PlayerAttackEvent attackEvent:
						yield return EventHandlers.Handle(this, attackEvent, handler => handler.HandleAttackEvent);
						break;
					case TickEvent tickEvt:
						yield return EventHandlers.Handle(this, tickEvt, handler => handler.HandleTickEvent);
						break;
					case GameOverEvent gameOverEvt:
						yield return EventHandlers.Handle(this, gameOverEvt, handler => handler.HandleGameOverEvent);
						break;
					case PlayerKilledEvent killEvt:
						yield return EventHandlers.Handle(this, killEvt, handler => handler.HandlePlayerKilledEvent);
						break;
					case PlayerRespawnEvent respawnEvt:
						yield return EventHandlers.Handle(this, respawnEvt, handler => handler.HandlePlayerRespawnedEvent);
						break;
					case PlayerLeftEvent leftEvt:
						yield return EventHandlers.Handle(this, leftEvt, handler => handler.HandlePlayerDroppedEvent);
						break;
					case SuddenDeathEvent suddenDeathEvt:
						yield return EventHandlers.Handle(this, suddenDeathEvt, handler => handler.HandleSuddenDeathEvent);
						break;
					// nothing interesting happened; let the next frame happen
					default:
						yield return null;
						break;
				}
			}

			Debug.Log("Game Processor has finished processing game stream.");
		}
	}
}