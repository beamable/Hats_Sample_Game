using Hats.Content;
using Hats.Game.Data;
using Hats.Simulation;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hats.Tests
{
	public class GameSimulationTest
	{
		[UnityTest]
		public IEnumerator TestFillsUpSinglePlayerGameWithBots()
		{
			Configuration cfg = new Configuration();
			BattleGrid grid = new BattleGrid();
			const int seed = 1;
			Queue<HatsGameMessage> queue = new Queue<HatsGameMessage>();
			List<HatsPlayer> onlyOnePlayer = new List<HatsPlayer>() { new HatsPlayer() { dbid = 0 } };
			BotProfileContent botProfileContent = new BotProfileContent();
			botProfileContent.names = new List<string>() { "botox" };
			botProfileContent.weights = new List<RandomBotAIWeights>() {
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

			GameSimulation sim = new GameSimulation(grid, 20, cfg, onlyOnePlayer, botProfileContent, seed, queue);

			Assert.AreNotEqual(GameSimulation.MaxPlayerCount, 1);
			Assert.AreEqual(sim.PlayerCount, GameSimulation.MaxPlayerCount);

			yield return null;
		}
	}
}