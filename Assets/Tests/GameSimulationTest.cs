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
			var sim = new GameSimulation(
				CreateDefaultBattleGrid(),
				CreateDefaultConfiguration(),
				onlyOnePlayer,
				CreateDefaultBotProfile(),
				DefaultSeed,
				CreateDefaultMessageQueue());

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

				nrSpawnEvents = processor.ConsumedEvents.FindAll(e => e is CollectablePowerupSpawnEvent).Count;
				Assert.LessOrEqual(nrSpawnEvents, cfg.MaxPowerupsInWorldAtTheSameTime);
				Assert.AreEqual(nrSpawnEvents, sim.GetCurrentTurn().CollectablePowerups.Count);
			}

			Assert.AreEqual(nrSpawnEvents, cfg.MaxPowerupsInWorldAtTheSameTime);
		}

		[Test]
		public void FireballAnnihilatesArrow()
		{
			var cfg = CreateDefaultConfiguration();
			var driver = new TestMultiplayerDriver(cfg);
			var fourPlayers = CreateFourNonBotPlayers();
			var grid = CreateGroundOnlyBattleGrid();
			var sim = new GameSimulation(grid, cfg, fourPlayers, CreateDefaultBotProfile(), DefaultSeed, driver.Queue);
			var processor = new TestProcessor(sim);

			processor.PlayAndConsumeEvents();

			var bottomLeftPlayer = fourPlayers.First(
				p => sim.GetCurrentTurn().GetPlayerState(p.dbid).Position == new Vector3Int(grid.Min.x, grid.Min.y, 0));

			var bottomRightPlayer = fourPlayers.First(
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

			var arrowAttack = (PlayerProjectileAttackEvent)processor.ConsumedEvents.First(
				e => e is PlayerProjectileAttackEvent && (e as PlayerProjectileAttackEvent).Type == PlayerProjectileAttackEvent.AttackType.ARROW);

			var fireballAttack = (PlayerProjectileAttackEvent)processor.ConsumedEvents.First(
				e => e is PlayerProjectileAttackEvent && (e as PlayerProjectileAttackEvent).Type == PlayerProjectileAttackEvent.AttackType.FIREBALL);

			Assert.IsNull(arrowAttack.KillsPlayer);
			Assert.IsNotNull(fireballAttack.KillsPlayer);

			Assert.IsTrue(sim.GetCurrentTurn().GetPlayerState(bottomLeftPlayer.dbid).IsDead);
			Assert.IsFalse(sim.GetCurrentTurn().GetPlayerState(bottomRightPlayer.dbid).IsDead);
		}

		[Test]
		public void ArrowsAnnihilateEachOther()
		{
			var cfg = CreateDefaultConfiguration();
			var driver = new TestMultiplayerDriver(cfg);
			var fourPlayers = CreateFourNonBotPlayers();
			var grid = CreateGroundOnlyBattleGrid();
			var sim = new GameSimulation(grid, cfg, fourPlayers, CreateDefaultBotProfile(), DefaultSeed, driver.Queue);
			var processor = new TestProcessor(sim);

			processor.PlayAndConsumeEvents();

			var bottomLeftPlayer = fourPlayers.First(
				p => sim.GetCurrentTurn().GetPlayerState(p.dbid).Position == new Vector3Int(grid.Min.x, grid.Min.y, 0));

			var bottomRightPlayer = fourPlayers.First(
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

			var arrowEastboundAttack = (PlayerProjectileAttackEvent)processor.ConsumedEvents.First(
				e => e is PlayerProjectileAttackEvent && (e as PlayerProjectileAttackEvent).Type == PlayerProjectileAttackEvent.AttackType.ARROW
				&& (e as PlayerProjectileAttackEvent).Direction == Direction.East);

			var arrowWestboundAttack = (PlayerProjectileAttackEvent)processor.ConsumedEvents.First(
				e => e is PlayerProjectileAttackEvent && (e as PlayerProjectileAttackEvent).Type == PlayerProjectileAttackEvent.AttackType.ARROW
				&& (e as PlayerProjectileAttackEvent).Direction == Direction.West);

			Assert.IsNull(arrowEastboundAttack.KillsPlayer);
			Assert.IsNull(arrowWestboundAttack.KillsPlayer);

			Assert.IsFalse(sim.GetCurrentTurn().GetPlayerState(bottomLeftPlayer.dbid).IsDead);
			Assert.IsFalse(sim.GetCurrentTurn().GetPlayerState(bottomRightPlayer.dbid).IsDead);
		}
	}
}