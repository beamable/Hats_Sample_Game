using Hats.Game.UI;
using Hats.Simulation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hats.Game
{
	public enum PlayerMoveBuilderState
	{
		NEEDS_MOVETYPE,
		NEEDS_DIRECTION,
		READY,
		COMMITTED,
		GHOST
	}

	public class PlayerMoveBuilder : GameEventHandler
	{
		public MultiplayerGameDriver NetworkDriver;

		[Header("Prefab References")]
		public SelectionPreviewBehaviour SelectionPreviewPrefab;

		[Header("Internals")]
		[ReadOnly]
		[SerializeField]
		private long PlayerDbid;

		[ReadOnly]
		[SerializeField]
		private List<SelectionPreviewBehaviour> _spawnedPreviews = new List<SelectionPreviewBehaviour>();

		[ReadOnly]
		[SerializeField]
		private HatsPlayerMoveType MoveType;

		[ReadOnly]
		[SerializeField]
		private Direction MoveDirection;

		[ReadOnly]
		[SerializeField]
		private PlayerMoveBuilderState moveBuilderState;

		public PlayerMoveBuilderState MoveBuilderState => moveBuilderState;

		public bool NeedsDirection => MoveType == HatsPlayerMoveType.WALK && MoveDirection != Direction.Nowhere;

		public void StartWalkInteraction() => StartDirectionalMovement(HatsPlayerMoveType.WALK);

		public void StartFireballInteraction() => StartDirectionalMovement(HatsPlayerMoveType.FIREBALL);

		public void StartArrowInteraction() => StartDirectionalMovement(HatsPlayerMoveType.ARROW);

		public void StartSkipInteraction() => StartDirectionlessMovement(HatsPlayerMoveType.SKIP);

		public void StartSurrenderInteraction() => StartDirectionlessMovement(HatsPlayerMoveType.SURRENDER);

		public void StartShieldInteraction() => StartDirectionlessMovement(HatsPlayerMoveType.SHIELD);

		public void StartDirectionalMovement(HatsPlayerMoveType moveType)
		{
			MoveType = moveType;
			MoveDirection = Direction.Nowhere;
			moveBuilderState = PlayerMoveBuilderState.NEEDS_DIRECTION;
			ShowPreviews();
		}

		public void StartDirectionlessMovement(HatsPlayerMoveType moveType)
		{
			MoveType = moveType;
			MoveDirection = Direction.Nowhere;
			moveBuilderState = PlayerMoveBuilderState.READY;
		}

		public void HandleDirectionSelection(SelectionPreviewBehaviour preview, Vector3Int cell)
		{
			moveBuilderState = PlayerMoveBuilderState.READY;
			var state = Game.CurrentLocalPlayerState;
			MoveDirection = Game.BattleGrid.GetDirection(state.Position, cell);

			ClearAllPreviewsExcept(preview);

			if (MoveType == HatsPlayerMoveType.FIREBALL && state.HasFirewallPowerup)
			{
				Vector3Int[] firewallFlankPositions = {
					Game.BattleGrid.InDirection(state.Position, MoveDirection.LookLeft()),
					Game.BattleGrid.InDirection(state.Position, MoveDirection.LookRight()),
				};

				foreach (var pos in firewallFlankPositions)
				{
					if (!Game.BattleGrid.IsRock(pos))
						SpawnMoveCommitPreview(pos);
				}
			}
		}

		public void HandleClick(Vector3Int cell)
		{
			if (moveBuilderState != PlayerMoveBuilderState.GHOST)
			{
				return; // ignore any movement if the player isn't a ghost.
			}
			var state = Game.CurrentLocalPlayerState;
			var direction = Game.BattleGrid.GetDirection(state.Position, cell);
			NetworkDriver.DeclareLocalPlayerAction(new HatsPlayerMove
			{
				Dbid = PlayerDbid,
				TurnNumber = -1, // free-roam is turnless.
				Direction = direction,
				MoveType = HatsPlayerMoveType.WALK
			});
		}

		public void ShowPreviews()
		{
			ClearPreviews();

			var state = Game.CurrentLocalPlayerState;
			var allNeighbors = Game.BattleGridBehaviour.Neighbors(state.Position);

			foreach (var neighbor in allNeighbors)
			{
				// Skip invalid neighbors:
				//  Neighbors with players in them
				//	If it's a walk, neighbors which can't be walked on
				//	If it's a shoot, neighbors which can't be shot through
				if (Game.Simulation.GetCurrentTurn().GetAlivePlayersAtPosition(neighbor).Count > 0
					|| (MoveType == HatsPlayerMoveType.WALK && !Game.BattleGrid.IsWalkable(neighbor))
					|| ((MoveType == HatsPlayerMoveType.ARROW || MoveType == HatsPlayerMoveType.FIREBALL) && Game.BattleGrid.IsRock(neighbor)))
				{
					continue;
				}

				// Create a move preview
				var instance = Game.BattleGridBehaviour.SpawnObjectAtCell(SelectionPreviewPrefab, neighbor);
				instance.Initialize(DirectionExtensions.GetDirection(state.Position, neighbor), MoveType);
				instance.OnClick.AddListener(() =>
				{
					HandleDirectionSelection(instance, neighbor);
				});
				_spawnedPreviews.Add(instance);
			}
		}

		public void Clear()
		{
			MoveType = HatsPlayerMoveType.SKIP;
			MoveDirection = Direction.Nowhere;
			moveBuilderState = PlayerMoveBuilderState.NEEDS_MOVETYPE;
			ClearPreviews();
		}

		public void ClearPreviews()
		{
			foreach (var preview in _spawnedPreviews)
			{
				Destroy(preview.gameObject);
			}
			_spawnedPreviews.Clear();
		}

		public void ClearAllPreviewsExcept(SelectionPreviewBehaviour exceptPreview)
		{
			foreach (var preview in _spawnedPreviews)
			{
				if (preview == exceptPreview) continue;
				Destroy(preview.gameObject);
			}
			_spawnedPreviews.Clear();
			_spawnedPreviews.Add(exceptPreview);
		}

		public void CommitMove()
		{
			moveBuilderState = PlayerMoveBuilderState.COMMITTED;
			NetworkDriver.DeclareLocalPlayerAction(new HatsPlayerMove
			{
				Dbid = PlayerDbid,
				TurnNumber = Game.Simulation.CurrentTurn,
				Direction = MoveDirection,
				MoveType = MoveType
			});
		}

		public override IEnumerator HandleTurnReadyEvent(TurnReadyEvent evt, Action completeCallback)
		{
			if (moveBuilderState == PlayerMoveBuilderState.GHOST)
			{
				completeCallback();
				yield break;
			}

			moveBuilderState = PlayerMoveBuilderState.NEEDS_MOVETYPE;
			ClearPreviews();
			completeCallback();
		}

		public override IEnumerator HandlePlayerKilledEvent(PlayerKilledEvent evt, Action completeCallback)
		{
			if (evt.Victim.dbid != PlayerDbid)
			{
				completeCallback();
				yield break;
			}

			moveBuilderState = PlayerMoveBuilderState.GHOST;
			completeCallback();
		}

		private void SpawnMoveCommitPreview(Vector3Int pos)
		{
			var instance = Game.BattleGridBehaviour.SpawnObjectAtCell(SelectionPreviewPrefab, pos);
			instance.Initialize(MoveDirection, MoveType);
			_spawnedPreviews.Add(instance);
		}

		private async void Start()
		{
			var beamable = await Beamable.API.Instance;
			PlayerDbid = beamable.User.id;
		}
	}
}