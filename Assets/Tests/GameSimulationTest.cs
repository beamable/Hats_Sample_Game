using Hats.Game.Data;
using Hats.Simulation;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace Hats.Tests
{
	public class GameSimulationTest : GameSimulationTestHelper
	{
		[Test]
		public void FillsUp_SinglePlayerGame_WithBots()
		{
			var onlyOnePlayer = new List<HatsPlayer>() { new HatsPlayer() { dbid = 0 } };
			GameSimulation sim = CreateDefaultGameWithPlayers(onlyOnePlayer);

			Assert.AreNotEqual(GameSimulation.MaxPlayerCount, 1);
			Assert.AreEqual(sim.PlayerCount, GameSimulation.MaxPlayerCount);
		}

		[Test]
		public void SpawnsEnoughPowerups_NeverTooMany_AndThenStopsSpawningThem()
		{
			var cfg = CreateDefaultConfiguration();
			var driver = new TestMultiplayerDriver(cfg);
			var fourPlayers = CreateFourNonBotPlayers();
			var sim = new GameSimulation(CreateDefaultBattleGrid(), cfg, fourPlayers, CreateDefaultBotProfile(), DefaultSeed, driver.Queue);
			var processor = new TestProcessor(sim);

			int nrSpawnEvents = 0;
			const int nrTurnsToObserve = 10;
			for (int t = 0; t < nrTurnsToObserve; t++)
			{
				foreach (var player in fourPlayers)
					driver.EnqueueSkipMoveForDbidAndTurn(player.dbid, sim.CurrentTurnNumber);

				processor.PlayAndConsumeEvents();

				nrSpawnEvents = processor.ConsumedEvents.OfType<CollectablePowerupSpawnEvent>().ToList().Count;
				Assert.LessOrEqual(nrSpawnEvents, cfg.MaxPowerupsInWorldAtTheSameTime);
				Assert.AreEqual(nrSpawnEvents, sim.GetCurrentTurn().CollectablePowerups.Count);
			}

			Assert.AreEqual(nrSpawnEvents, cfg.MaxPowerupsInWorldAtTheSameTime);
		}

		[Test]
		public void FireballAnnihilatesArrow_ArcherDies()
		{
			var cfg = CreateDefaultConfiguration();
			var driver = new TestMultiplayerDriver(cfg);
			var fourPlayers = CreateFourNonBotPlayers();
			var grid = CreateGroundOnlyBattleGrid();
			var sim = new GameSimulation(grid, cfg, fourPlayers, CreateDefaultBotProfile(), DefaultSeed, driver.Queue);
			var processor = new TestProcessor(sim);

			processor.PlayAndConsumeEvents();

			var bottomLeftPlayer = fourPlayers.Single(
				p => sim.GetCurrentTurn().GetPlayerState(p.dbid).Position == new Vector3Int(grid.Min.x, grid.Min.y, 0));

			var bottomRightPlayer = fourPlayers.Single(
				p => sim.GetCurrentTurn().GetPlayerState(p.dbid).Position == new Vector3Int(grid.Max.x, grid.Min.y, 0));

			var otherPlayers = fourPlayers.Where(p => p != bottomLeftPlayer && p != bottomRightPlayer).ToList();

			driver.Enqueue(new HatsPlayerMove()
			{
				Dbid = bottomLeftPlayer.dbid,
				Direction = Direction.East,
				MoveType = HatsPlayerMoveType.ARROW,
				TurnNumber = sim.CurrentTurnNumber
			});

			driver.Enqueue(new HatsPlayerMove()
			{
				Dbid = bottomRightPlayer.dbid,
				Direction = Direction.West,
				MoveType = HatsPlayerMoveType.FIREBALL,
				TurnNumber = sim.CurrentTurnNumber
			});

			foreach (var player in otherPlayers)
				driver.EnqueueSkipMoveForDbidAndTurn(player.dbid, sim.CurrentTurnNumber);

			processor.PlayAndConsumeEvents();

			var arrowAttack = processor.ConsumedEvents.OfType<PlayerProjectileAttackEvent>().Single(e =>
				e.Type == PlayerProjectileAttackEvent.AttackType.ARROW);

			var fireballAttack = processor.ConsumedEvents.OfType<PlayerProjectileAttackEvent>().Single(e =>
				e.Type == PlayerProjectileAttackEvent.AttackType.FIREBALL);

			Assert.IsNull(arrowAttack.KillsPlayer);
			Assert.IsNotNull(fireballAttack.KillsPlayer);

			Assert.IsTrue(sim.GetCurrentTurn().GetPlayerState(bottomLeftPlayer.dbid).IsDead);
			Assert.IsFalse(sim.GetCurrentTurn().GetPlayerState(bottomRightPlayer.dbid).IsDead);
		}

		[Test]
		public void ArrowsAnnihilateEachOther_NobodyGetsHurt()
		{
			var cfg = CreateDefaultConfiguration();
			var driver = new TestMultiplayerDriver(cfg);
			var fourPlayers = CreateFourNonBotPlayers();
			var grid = CreateGroundOnlyBattleGrid();
			var sim = new GameSimulation(grid, cfg, fourPlayers, CreateDefaultBotProfile(), DefaultSeed, driver.Queue);
			var processor = new TestProcessor(sim);

			processor.PlayAndConsumeEvents();

			var bottomLeftPlayer = fourPlayers.Single(
				p => sim.GetCurrentTurn().GetPlayerState(p.dbid).Position == new Vector3Int(grid.Min.x, grid.Min.y, 0));

			var bottomRightPlayer = fourPlayers.Single(
				p => sim.GetCurrentTurn().GetPlayerState(p.dbid).Position == new Vector3Int(grid.Max.x, grid.Min.y, 0));

			var otherPlayers = fourPlayers.Where(p => p != bottomLeftPlayer && p != bottomRightPlayer).ToList();

			driver.Enqueue(new HatsPlayerMove()
			{
				Dbid = bottomLeftPlayer.dbid,
				Direction = Direction.East,
				MoveType = HatsPlayerMoveType.ARROW,
				TurnNumber = sim.CurrentTurnNumber
			});

			driver.Enqueue(new HatsPlayerMove()
			{
				Dbid = bottomRightPlayer.dbid,
				Direction = Direction.West,
				MoveType = HatsPlayerMoveType.ARROW,
				TurnNumber = sim.CurrentTurnNumber
			});

			foreach (var player in otherPlayers)
				driver.EnqueueSkipMoveForDbidAndTurn(player.dbid, sim.CurrentTurnNumber);

			processor.PlayAndConsumeEvents();

			var arrowEastboundAttack = processor.ConsumedEvents.OfType<PlayerProjectileAttackEvent>().Single(e =>
				e.Type == PlayerProjectileAttackEvent.AttackType.ARROW && e.Direction == Direction.East
			);

			var arrowWestboundAttack = processor.ConsumedEvents.OfType<PlayerProjectileAttackEvent>().Single(e =>
				e.Type == PlayerProjectileAttackEvent.AttackType.ARROW && e.Direction == Direction.West
			);

			Assert.IsNull(arrowEastboundAttack.KillsPlayer);
			Assert.IsNull(arrowWestboundAttack.KillsPlayer);

			Assert.IsFalse(sim.GetCurrentTurn().GetPlayerState(bottomLeftPlayer.dbid).IsDead);
			Assert.IsFalse(sim.GetCurrentTurn().GetPlayerState(bottomRightPlayer.dbid).IsDead);
		}

		[Test]
		public void AllPlayersDead_InSameTurn_EndsTheGame_WithoutAWinner()
		{
			var fourPlayers = CreateFourNonBotPlayers();
			TestMultiplayerDriver driver;
			var sim = CreateDefaultGameWithPlayers(fourPlayers, out driver);
			var processor = new TestProcessor(sim);

			processor.PlayAndConsumeEvents();

			foreach (var player in fourPlayers)
			{
				driver.Enqueue(new HatsPlayerMove()
				{
					Dbid = player.dbid,
					Direction = Direction.Nowhere,
					MoveType = HatsPlayerMoveType.SURRENDER,
					TurnNumber = sim.CurrentTurnNumber,
				});
			}

			processor.PlayAndConsumeEvents();

			foreach (var player in fourPlayers)
				Assert.IsTrue(sim.GetCurrentTurn().GetPlayerState(player.dbid).IsDead);

			Assert.AreEqual(processor.ConsumedEvents.OfType<GameOverEvent>().ToList().Count, 1);

			var gameOverEvent = processor.ConsumedEvents.OfType<GameOverEvent>().Single();
			Assert.IsNull(gameOverEvent.Winner);
		}
	}
}