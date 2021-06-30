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
					driver.EnqueueSkipMove(player.dbid, sim.CurrentTurnNumber);

				processor.PlayAndConsumeEvents();

				nrSpawnEvents = processor.ConsumedEvents.OfType<CollectablePowerupSpawnEvent>().Count();
				Assert.LessOrEqual(nrSpawnEvents, cfg.MaxPowerupsInWorldAtTheSameTime);
				Assert.AreEqual(nrSpawnEvents, sim.GetCurrentTurn().CollectablePowerups.Count);
			}

			Assert.AreEqual(nrSpawnEvents, cfg.MaxPowerupsInWorldAtTheSameTime);
		}

		[Test]
		public void EventuallyEnds()
		{
			var cfg = CreateDefaultConfiguration();
			var driver = new TestMultiplayerDriver(cfg);
			var onlyOnePlayer = new List<HatsPlayer>() { new HatsPlayer() { dbid = 0 } };
			var sim = new GameSimulation(CreateDefaultBattleGrid(), cfg, onlyOnePlayer, CreateDefaultBotProfile(), DefaultSeed, driver.Queue);
			var processor = new TestProcessor(sim);

			const int maxTurnsToWaitForGameOver = 20;
			for (int t = 0; t < maxTurnsToWaitForGameOver; t++)
			{
				driver.EnqueueSkipMove(onlyOnePlayer[0].dbid, sim.CurrentTurnNumber);
				processor.PlayAndConsumeEvents();
			}

			Assert.AreEqual(processor.ConsumedEvents.OfType<GameOverEvent>().Count(), 1);
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
				driver.EnqueueSkipMove(player.dbid, sim.CurrentTurnNumber);

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
				driver.EnqueueSkipMove(player.dbid, sim.CurrentTurnNumber);

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
				driver.EnqueueSurrenderMove(player.dbid, sim.CurrentTurnNumber);

			processor.PlayAndConsumeEvents();

			foreach (var player in fourPlayers)
				Assert.IsTrue(sim.GetCurrentTurn().GetPlayerState(player.dbid).IsDead);

			Assert.AreEqual(processor.ConsumedEvents.OfType<GameOverEvent>().Count(), 1);

			var gameOverEvent = processor.ConsumedEvents.OfType<GameOverEvent>().Single();
			Assert.IsNull(gameOverEvent.Winner);
		}

		[Test]
		public void DeadPlayer_CanTurnTileIntoSuddenDeath()
		{
			var cfg = CreateDefaultConfiguration();
			var driver = new TestMultiplayerDriver(cfg);
			var fourPlayers = CreateFourNonBotPlayers();
			var grid = CreateGroundOnlyBattleGrid();
			var sim = new GameSimulation(grid, cfg, fourPlayers, CreateDefaultBotProfile(), DefaultSeed, driver.Queue);
			var processor = new TestProcessor(sim);

			processor.PlayAndConsumeEvents();

			var ghostPlayer = fourPlayers[0];
			var otherPlayers = fourPlayers.GetRange(1, 3);

			driver.EnqueueSurrenderMove(ghostPlayer.dbid, sim.CurrentTurnNumber);

			foreach (var player in otherPlayers)
				driver.EnqueueSkipMove(player.dbid, sim.CurrentTurnNumber);

			processor.PlayAndConsumeEvents();

			Assert.IsTrue(sim.GetCurrentTurn().GetPlayerState(ghostPlayer.dbid).IsDead);

			var testCellPos = new Vector3Int(0, 0, 0);

			Assert.IsTrue(grid.IsGround(testCellPos));
			Assert.IsFalse(grid.IsInSuddenDeath(testCellPos));

			driver.Enqueue(new HatsPlayerMove()
			{
				Dbid = ghostPlayer.dbid,
				Direction = Direction.Nowhere,
				MoveType = HatsPlayerMoveType.SUDDEN_DEATH_TILE,
				TurnNumber = sim.CurrentTurnNumber,
				AdditionalTargetCellPos = testCellPos,
			});

			foreach (var player in otherPlayers)
				driver.EnqueueSkipMove(player.dbid, sim.CurrentTurnNumber);

			processor.PlayAndConsumeEvents();

			Assert.IsTrue(grid.IsGround(testCellPos));
			Assert.IsTrue(grid.IsInSuddenDeath(testCellPos));
		}

		[Test]
		public void WaitsFor_AllPlayers_EvenDeadOnes_ToCommit_BeforeTimeout()
		{
			var cfg = CreateDefaultConfiguration();
			var driver = new TestMultiplayerDriver(cfg);
			var fourPlayers = CreateFourNonBotPlayers();
			var grid = CreateDefaultBattleGrid();
			var sim = new GameSimulation(grid, cfg, fourPlayers, CreateDefaultBotProfile(), DefaultSeed, driver.Queue);
			var processor = new TestProcessor(sim);

			processor.PlayAndConsumeEvents();

			var ghostPlayer = fourPlayers[0];
			var otherPlayers = fourPlayers.GetRange(1, 3);

			driver.EnqueueSurrenderMove(ghostPlayer.dbid, sim.CurrentTurnNumber);
			foreach (var player in otherPlayers)
				driver.EnqueueSkipMove(player.dbid, sim.CurrentTurnNumber);
			processor.PlayAndConsumeEvents();

			var sameTurnNumber = sim.CurrentTurnNumber;
			float quarterTurnTimeout = cfg.TurnTimeout * 0.25f;

			driver.EnqueueTickMessagesForTime(quarterTurnTimeout);
			processor.PlayAndConsumeEvents();

			Assert.AreEqual(sameTurnNumber, sim.CurrentTurnNumber);

			foreach (var player in otherPlayers)
				driver.EnqueueSkipMove(player.dbid, sim.CurrentTurnNumber);
			processor.PlayAndConsumeEvents();

			Assert.AreEqual(sameTurnNumber, sim.CurrentTurnNumber);

			driver.EnqueueTickMessagesForTime(quarterTurnTimeout);
			processor.PlayAndConsumeEvents();

			Assert.AreEqual(sameTurnNumber, sim.CurrentTurnNumber);

			driver.EnqueueSkipMove(ghostPlayer.dbid, sim.CurrentTurnNumber);
			processor.PlayAndConsumeEvents();

			int nextTurnNumber = sameTurnNumber + 1;
			Assert.AreEqual(nextTurnNumber, sim.CurrentTurnNumber);
		}

		[Test]
		public void TimesOutTurnsEventually_MakingEveryoneSkipAutomatically_AndImplicitly()
		{
			var cfg = CreateDefaultConfiguration();
			var driver = new TestMultiplayerDriver(cfg);
			var fourPlayers = CreateFourNonBotPlayers();
			var grid = CreateDefaultBattleGrid();
			var sim = new GameSimulation(grid, cfg, fourPlayers, CreateDefaultBotProfile(), DefaultSeed, driver.Queue);
			var processor = new TestProcessor(sim);

			processor.PlayAndConsumeEvents();

			var sameTurnNumber = sim.CurrentTurnNumber;
			float threeQuarterTurnTimeout = cfg.TurnTimeout * 0.75f;

			driver.EnqueueTickMessagesForTime(threeQuarterTurnTimeout);
			processor.PlayAndConsumeEvents();

			Assert.AreEqual(sameTurnNumber, sim.CurrentTurnNumber);

			driver.EnqueueTickMessagesForTime(threeQuarterTurnTimeout);
			processor.PlayAndConsumeEvents();

			int nextTurnNumber = sameTurnNumber + 1;
			Assert.AreEqual(nextTurnNumber, sim.CurrentTurnNumber);

			Assert.AreEqual(processor.ConsumedEvents.OfType<TurnReadyEvent>().Count(), 1);
			Assert.AreEqual(processor.ConsumedEvents.OfType<TurnOverEvent>().Count(), 1);

			Assert.AreEqual(processor.ConsumedEvents.OfType<PlayerCommittedMoveEvent>().Count(), 0);
		}
	}
}