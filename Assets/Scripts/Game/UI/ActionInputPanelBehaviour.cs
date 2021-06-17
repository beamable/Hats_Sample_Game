using Hats.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Hats.Game.UI
{
	public class ActionInputPanelBehaviour : MonoBehaviour
	{
		[Header("Input references")]
		public PlayerMoveBuilder PlayerMoveBuilder;

		[Header("UI References")]
		public Button WalkButton;

		public Button ShieldButton;
		public Button SkipButton;
		public Button SurrenderButton;
		public Button FireballButton;
		public Button ArrowButton;

		public Button CancelButton;
		public Button CommitButton;

		public GameObject PickDirection;
		public GameObject FreeRoamText;
		public GameObject GameOverText;

		private Button[] _moveButtons;

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
			_moveButtons = new[]
			{
					 WalkButton, ShieldButton, SkipButton, SurrenderButton, FireballButton, ArrowButton
				};

			CancelButton.onClick.AddListener(HandleCancel);
			WalkButton.onClick.AddListener(HandleWalk);
			CommitButton.onClick.AddListener(HandleCommit);
			SkipButton.onClick.AddListener(HandleSkip);
			SurrenderButton.onClick.AddListener(HandleSurrender);
			ShieldButton.onClick.AddListener(HandleShield);
			FireballButton.onClick.AddListener(HandleFireball);
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
			foreach (var button in _moveButtons)
			{
				button.gameObject.SetActive(false);
			}
		}

		private void EnableAllMoveButtons()
		{
			foreach (var button in _moveButtons)
			{
				button.gameObject.SetActive(true);
			}
		}
	}
}