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

		public Button SuddenDeathTileButton;

		public Button CancelButton;

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

		// Start is called before the first frame update
		private void Start()
		{
			CancelButton.onClick.AddListener(HandleCancel);
			WalkButton.onClick.AddListener(HandleWalk);
			TeleportButton.onClick.AddListener(HandleWalk);
			SkipButton.onClick.AddListener(HandleSkip);
			SurrenderButton.onClick.AddListener(HandleSurrender);
			ShieldButton.onClick.AddListener(HandleShield);
			FireballButton.onClick.AddListener(HandleFireball);
			FirewallButton.onClick.AddListener(HandleFireball);
			ArrowButton.onClick.AddListener(HandleArrow);
			SuddenDeathTileButton.onClick.AddListener(HandleSuddenDeathTile);
		}

		// Update is called once per frame
		private void Update()
		{
			if (GameOverText.activeSelf)
			{
				return;
			}

			DisableFreeRoamHint();

			if (PlayerMoveBuilder.MoveBuilderState == PlayerMoveBuilderState.NEEDS_MOVETYPE)
			{
				EnableAllMoveButtons();
				DisableCancelButton();
				DisableDirectionHint();
			}

			if (PlayerMoveBuilder.MoveBuilderState == PlayerMoveBuilderState.NEEDS_DIRECTION)
			{
				EnableDirectionHint();
			}
			else
			{
				DisableDirectionHint();
			}

			if (PlayerMoveBuilder.MoveBuilderState == PlayerMoveBuilderState.COMMITTED)
				DisableCancelButton();
		}

		private void HandleWalk()
		{
			DisableAllMoveButtons();
			EnableCancelButton();
			EnableDirectionHint();

			PlayerMoveBuilder.StartWalkInteraction();
		}

		private void HandleSuddenDeathTile()
		{
			DisableAllMoveButtons();
			EnableCancelButton();
			EnableDirectionHint();

			PlayerMoveBuilder.StartSuddenDeathTileInteraction();
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
			SuddenDeathTileButton.gameObject.SetActive(false);
		}

		private void EnableAllMoveButtons()
		{
			if (!Game.BootstrapTask.IsCompleted)
				return;

			var localPlayerState = Game.CurrentLocalPlayerState;

			if (PlayerMoveBuilder.IsGhost())
			{
				DisableAllMoveButtons();
				SkipButton.gameObject.SetActive(true);
				SuddenDeathTileButton.gameObject.SetActive(true);
			}
			else
			{
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
}