using Hats.Content;
using Hats.Game.Data;
using Hats.Simulation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hats.Tests
{
	public class GameSimulationTestHelper
	{
		protected static int DefaultSeed = 0;

		protected class TestProcessor
		{
			private IEnumerator<HatsGameEvent> _simPlayEnumerator = null;
			private GameSimulation _sim = null;
			private List<HatsGameEvent> _consumedEvents = new List<HatsGameEvent>();

			public List<HatsGameEvent> ConsumedEvents { get => _consumedEvents; }

			public TestProcessor(GameSimulation sim)
			{
				_sim = sim;
				_simPlayEnumerator = _sim.PlayGame().GetEnumerator();
			}

			public void DebugPrintConsumedEvents()
			{
				foreach (var e in _consumedEvents)
					Debug.Log($"Consumed={e}");
			}

			public void PlayAndConsumeEvents()
			{
				int nullEventsSinceLastEvent = 0;
				int newEventsInCurrentBatchCount = 0;
				const int maxNullEventsInARow = 10;
				const int maxEventsPerBatch = 1000;

				while (_simPlayEnumerator.MoveNext())
				{
					var newEvent = _simPlayEnumerator.Current;

					if (newEvent != null)
					{
						nullEventsSinceLastEvent = 0;
						_consumedEvents.Add(newEvent);
						newEventsInCurrentBatchCount++;
						if (newEventsInCurrentBatchCount >= maxEventsPerBatch)
							break;

						continue;
					}

					nullEventsSinceLastEvent++;
					if (nullEventsSinceLastEvent > maxNullEventsInARow)
						break;
				}
			}
		}

		protected class TestMultiplayerDriver
		{
			private Queue<HatsGameMessage> _queue;
			private int _framesPerSecond;
			private int _currentTick;

			public Queue<HatsGameMessage> Queue { get => _queue; }

			public TestMultiplayerDriver(Configuration cfg)
			{
				_currentTick = 0;
				_queue = CreateDefaultMessageQueue();
				_framesPerSecond = cfg.FramesPerSecond;
			}

			public void EnqueueSkipMoveForDbidAndTurn(long dbid, int turnNumber)
			{
				Enqueue(new HatsPlayerMove()
				{
					Dbid = dbid,
					Direction = Direction.Nowhere,
					MoveType = HatsPlayerMoveType.SKIP,
					TurnNumber = turnNumber,
				});
			}

			public void EnqueueTickMessagesForTime(float timeSecs)
			{
				EnqueueTickMessages((int)Mathf.Ceil(_framesPerSecond * timeSecs));
			}

			public void Enqueue(HatsGameMessage message)
			{
				_queue.Enqueue(message);
			}

			private void EnqueueTickMessages(int number)
			{
				for (int i = 0; i < number; i++)
				{
					_queue.Enqueue(new HatsTickMessage() { FrameNumber = _currentTick });
					_currentTick++;
				}
			}
		}

		protected static Queue<HatsGameMessage> CreateDefaultMessageQueue()
		{
			return new Queue<HatsGameMessage>();
		}

		protected static Configuration CreateDefaultConfiguration()
		{
			return ScriptableObject.CreateInstance<Configuration>();
		}

		protected static BattleGrid CreateDefaultBattleGrid()
		{
			return new BattleGrid();
		}

		protected static BattleGrid CreateGroundOnlyBattleGrid()
		{
			var grid = new BattleGrid();
			grid.holeQuantityRange = Vector2Int.zero;
			grid.iceQuantityRange = Vector2Int.zero;
			grid.lavaQuantityRange = Vector2Int.zero;
			grid.rockQuantityRange = Vector2Int.zero;
			return grid;
		}

		protected static BotProfileContent CreateDefaultBotProfile()
		{
			BotProfileContent testBotProfileContent = ScriptableObject.CreateInstance<BotProfileContent>();
			testBotProfileContent.names = new List<string>() { "botox" };
			testBotProfileContent.weights = new List<RandomBotAIWeights>() {
				new RandomBotAIWeights()
				{
					name = "Randomizer",
					walkWeight = 1.0f,
					fireballWeight = 1.0f,
					arrowWeight = 1.0f,
					shieldWeight = 1.0f,
					skipWeight = 1.0f,
				}
			};

			return testBotProfileContent;
		}

		protected static List<HatsPlayer> CreateFourNonBotPlayers()
		{
			return new List<HatsPlayer>() {
				new HatsPlayer() { dbid = 0 },
				new HatsPlayer() { dbid = 1 },
				new HatsPlayer() { dbid = 2 },
				new HatsPlayer() { dbid = 3 },
			};
		}

		protected static GameSimulation CreateDefaultGameWithPlayers(List<HatsPlayer> players)
		{
			TestMultiplayerDriver driver;
			return CreateDefaultGameWithPlayers(players, out driver);
		}

		protected static GameSimulation CreateDefaultGameWithPlayers(List<HatsPlayer> players, out TestMultiplayerDriver driver)
		{
			var cfg = CreateDefaultConfiguration();
			driver = new TestMultiplayerDriver(cfg);
			return new GameSimulation(
				CreateDefaultBattleGrid(),
				cfg,
				players,
				CreateDefaultBotProfile(),
				DefaultSeed,
				driver.Queue);
		}
	}
}