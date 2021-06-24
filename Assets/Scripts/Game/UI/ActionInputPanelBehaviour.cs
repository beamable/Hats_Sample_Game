using Hats.Game;
using Hats.Simulation;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Hats.Game.UI
{
	public class ActionInputPanelBehaviour : GameEventHandler
	{
		[Header("Input references")]
		public PlayerMoveBuilder PlayerMoveBuilder;

		[Header("UI References")]
		public Button WalkButton;

		public Button TeleportButton;

		public Button ShieldButton;
		public Button SkipButton;
		public Button SurrenderButton;
		public Button FireballButton;
		public Button FirewallButton;
		public Button ArrowButton;

		public Button CancelButton;
		public Button CommitButton;

		public GameObject PickDirection;
		public GameObject FreeRoamText;
		public GameObject GameOverText;

		public void ShowGameOverText()
		{
			DisableAllMoveButtons();
			PickDirection.SetActive(false);
			DisableFreeRoamHint();
			GameOverText.SetActive(true);
		}

		public void DisableAll()
		{
			enabled = false; // stop updates
			DisableAllMoveButtons();
			PickDirection.SetActive(false);
			DisableFreeRoamHint();
			GameOverText.SetActive(false);
		}

		public override IEnumerator HandlePowerupCollectEvent(PowerupCollectEvent evt, Action callback)
		{
			callback();

			if (evt.Player.dbid != Game.MultiplayerGameDriver.LocalPlayerDBID)
				yield break;
		}

		public override IEnumerator HandlePowerupRemoveEvent(PowerupRemoveEvent evt, Action callback)
		{
			callback();

			if (evt.Player.dbid != Game.MultiplayerGameDriver.LocalPlayerDBID)
				yield break;
		}

		// Start is called before the first frame update
		private void Start()
		{
			CancelButton.onClick.AddListener(HandleCancel);
			WalkButton.onClick.AddListener(HandleWalk);
			TeleportButton.onClick.AddListener(HandleWalk);
			CommitButton.onClick.AddListener(HandleCommit);
			SkipButton.onClick.AddListener(HandleSkip);
			SurrenderButton.onClick.AddListener(HandleSurrender);
			ShieldButton.onClick.AddListener(HandleShield);
			FireballButton.onClick.AddListener(HandleFireball);
			FirewallButton.onClick.AddListener(HandleFireball);
			ArrowButton.onClick.AddListener(HandleArrow);
		}

		// Update is called once per frame
		private void Update()
		{
			if (GameOverText.activeSelf)
			{
				return;
			}

			if (PlayerMoveBuilder.MoveBuilderState == PlayerMoveBuilderState.GHOST)
			{
				DisableCancelButton();
				DisableCommitButton();
				DisableAllMoveButtons();
				DisableDirectionHint();
				EnableFreeRoamHint();
				return;
			}
			DisableFreeRoamHint();

			if (PlayerMoveBuilder.MoveBuilderState == PlayerMoveBuilderState.NEEDS_MOVETYPE)
			{
				EnableAllMoveButtons();
				DisableCancelButton();
				DisableDirectionHint();
				DisableCommitButton();
			}

			if (PlayerMoveBuilder.MoveBuilderState == PlayerMoveBuilderState.NEEDS_DIRECTION)
			{
				EnableDirectionHint();
			}
			else
			{
				DisableDirectionHint();
			}

			if (PlayerMoveBuilder.MoveBuilderState == PlayerMoveBuilderState.READY)
			{
				EnableCommitButton();
			}
			else
			{
				DisableCommitButton();
			}
		}

		private void HandleWalk()
		{
			DisableAllMoveButtons();
			EnableCancelButton();
			EnableDirectionHint();

			PlayerMoveBuilder.StartWalkInteraction();
		}

		private void HandleSkip()
		{
			DisableAllMoveButtons();
			EnableCancelButton();
			PlayerMoveBuilder.StartSkipInteraction();
		}

		private void HandleSurrender()
		{
			DisableAllMoveButtons();
			EnableCancelButton();
			PlayerMoveBuilder.StartSurrenderInteraction();
		}

		private void HandleShield()
		{
			DisableAllMoveButtons();
			EnableCancelButton();
			PlayerMoveBuilder.StartShieldInteraction();
		}

		private void HandleFireball()
		{
			DisableAllMoveButtons();
			EnableCancelButton();
			PlayerMoveBuilder.StartFireballInteraction();
		}

		private void HandleArrow()
		{
			DisableAllMoveButtons();
			EnableCancelButton();
			PlayerMoveBuilder.StartArrowInteraction();
		}

		private void HandleCommit()
		{
			PlayerMoveBuilder.CommitMove();
		}

		private void HandleCancel()
		{
			DisableDirectionHint();
			DisableCancelButton();
			EnableAllMoveButtons();
			PlayerMoveBuilder.Clear();
		}

		private void EnableCancelButton()
		{
			CancelButton.gameObject.SetActive(true);
		}

		private void DisableCancelButton()
		{
			CancelButton.gameObject.SetActive(false);
		}

		private void EnableCommitButton()
		{
			CommitButton.gameObject.SetActive(true);
		}

		private void DisableCommitButton()
		{
			CommitButton.gameObject.SetActive(false);
		}

		private void EnableDirectionHint()
		{
			PickDirection.SetActive(true);
		}

		private void DisableDirectionHint()
		{
			PickDirection.SetActive(false);
		}

		private void EnableFreeRoamHint()
		{
			FreeRoamText.SetActive(true);
		}

		private void DisableFreeRoamHint()
		{
			FreeRoamText.SetActive(false);
		}

		private void DisableAllMoveButtons()
		{
			WalkButton.gameObject.SetActive(false);
			TeleportButton.gameObject.SetActive(false);
			ShieldButton.gameObject.SetActive(false);
			SkipButton.gameObject.SetActive(false);
			SurrenderButton.gameObject.SetActive(false);
			FireballButton.gameObject.SetActive(false);
			FirewallButton.gameObject.SetActive(false);
			ArrowButton.gameObject.SetActive(false);
		}

		private void EnableAllMoveButtons()
		{
			if (!Game.BootstrapTask.IsCompleted)
				return;

			var localPlayerState = Game.CurrentLocalPlayerState;

			if (localPlayerState.HasTeleportPowerup)
			{
				WalkButton.gameObject.SetActive(false);
				TeleportButton.gameObject.SetActive(true);
			}
			else
			{
				WalkButton.gameObject.SetActive(true);
				TeleportButton.gameObject.SetActive(false);
			}

			ShieldButton.gameObject.SetActive(true);
			SkipButton.gameObject.SetActive(true);
			SurrenderButton.gameObject.SetActive(true);

			if (localPlayerState.HasFirewallPowerup)
			{
				FireballButton.gameObject.SetActive(false);
				FirewallButton.gameObject.SetActive(true);
			}
			else
			{
				FireballButton.gameObject.SetActive(true);
				FirewallButton.gameObject.SetActive(false);
			}

			ArrowButton.gameObject.SetActive(true);
		}
	}
}