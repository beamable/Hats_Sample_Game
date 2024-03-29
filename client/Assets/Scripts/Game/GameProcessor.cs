using Beamable;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hats.Content;
using Hats.Simulation;
using JetBrains.Annotations;
using UnityEngine;
using Hats.Game.Data;

namespace Hats.Game
{
	public class GameProcessor : MonoBehaviour
	{
		[CanBeNull] public static string RoomId;

		[CanBeNull] public static List<long> Dbids;
		public GameSimulation Simulation;
		public BattleGridBehaviour BattleGridBehaviour;
		public MultiplayerGameDriver MultiplayerGameDriver;

		public string roomId = "room1";
		public List<GameEventHandler> EventHandlers;

		[SerializeField]
		private Configuration _configuration = null;

		[Header("Internals")]
		[Hats.Simulation.ReadOnly]
		[SerializeField]
		private int currentTurn;

		private Task _bootstrapTask;

		private BeamContext _beamContext;

		public BattleGrid BattleGrid => BattleGridBehaviour.BattleGrid;
		public long LocalPlayerDBID => MultiplayerGameDriver.LocalPlayerDBID;
		public HatsPlayerState CurrentLocalPlayerState => Simulation.GetCurrentTurn().GetPlayerState(LocalPlayerDBID);
		public float SecondsLeftInTurn => Simulation.SecondsLeftInTurn;

		public Task BootstrapTask { get => _bootstrapTask; }
		public Configuration Configuration { get => _configuration; }

		public void StartGame(List<long> dbids, BotProfileContent botProfileContent)
		{
			// TODO: Handle matchmaking
			var messageQueue = MultiplayerGameDriver.Init(roomId, _configuration.FramesPerSecond, new List<long>());

			var players = dbids.Select(dbid => new HatsPlayer
			{
				dbid = dbid
			}).ToList();

			Simulation = new GameSimulation(BattleGridBehaviour.BattleGrid, _configuration, players, botProfileContent, roomId.GetHashCode(), messageQueue);
			BattleGridBehaviour.SetupInitialTileChanges();
			StartCoroutine(PlayGame());
		}

		public HatsPlayerState GetCurrentPlayerState(long dbid)
		{
			return Simulation.GetCurrentTurn().GetPlayerState(dbid);
		}

		private void Start()
		{
			_bootstrapTask = BootstrapGame();
		}

		private async Task BootstrapGame()
		{
			roomId = RoomId ?? roomId;
			Debug.Log($"Game Starting... with roomId=[{roomId}]");
			
			_beamContext = BeamContext.Default;
			await _beamContext.OnReady;
			var botProfile = await _configuration.BotProfileRef.Resolve();
			var dbids = Dbids ?? new List<long> { _beamContext.PlayerId };
			StartGame(dbids, botProfile);
		}

		private IEnumerator PlayGame()
		{
			foreach (var evt in Simulation.PlayGame())
			{
				currentTurn = Simulation.CurrentTurnNumber;
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

					case CollectablePowerupSpawnEvent puSpawnEvt:
						yield return EventHandlers.Handle(this, puSpawnEvt, handler => handler.HandleCollectablePowerupSpawnEvent);
						break;

					case CollectablePowerupDestroyEvent puRemoveEvt:
						yield return EventHandlers.Handle(this, puRemoveEvt, handler => handler.HandleCollectablePowerupDestroyEvent);
						break;

					case SuddenDeathStartedEvent sudden:
						yield return EventHandlers.Handle(this, sudden, handler => handler.HandleSuddenDeathStartedEvent);
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

					case PlayerProjectileAttackEvent attackEvent:
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