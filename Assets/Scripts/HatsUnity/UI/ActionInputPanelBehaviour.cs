using HatsMultiplayer;
using UnityEngine;
using UnityEngine.UI;

namespace HatsUnity.UI
{
    public class ActionInputPanelBehaviour : MonoBehaviour
    {
        [Header("Input references")]
        public PlayerMoveBuilder PlayerMoveBuilder;

        [Header("UI References")]
        public Button WalkButton;
        public Button ShieldButton;
        public Button SkipButton;
        public Button FireballButton;
        public Button ArrowButton;

        public Button CancelButton;
        public Button CommitButton;

        public GameObject PickDirection;

        private Button[] _moveButtons;

        // Start is called before the first frame update
        void Start()
        {
            _moveButtons = new[]
            {
                WalkButton, ShieldButton, SkipButton, FireballButton, ArrowButton
            };

            CancelButton.onClick.AddListener(HandleCancel);
            WalkButton.onClick.AddListener(HandleWalk);
            CommitButton.onClick.AddListener(HandleCommit);
            SkipButton.onClick.AddListener(HandleSkip);
            ShieldButton.onClick.AddListener(HandleShield);
            FireballButton.onClick.AddListener(HandleFireball);
            ArrowButton.onClick.AddListener(HandleArrow);

        }

        // Update is called once per frame
        void Update()
        {
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

        void HandleWalk()
        {
            DisableAllMoveButtons();
            EnableCancelButton();
            EnableDirectionHint();

            PlayerMoveBuilder.StartWalkInteraction();
        }

        void HandleSkip()
        {
            DisableAllMoveButtons();
            EnableCancelButton();
            PlayerMoveBuilder.StartSkipInteraction();
        }

        void HandleShield()
        {
            DisableAllMoveButtons();
            EnableCancelButton();
            PlayerMoveBuilder.StartShieldInteraction();
        }

        void HandleFireball()
        {
            DisableAllMoveButtons();
            EnableCancelButton();
            PlayerMoveBuilder.StartFireballInteraction();
        }

        void HandleArrow()
        {
            DisableAllMoveButtons();
            EnableCancelButton();
            PlayerMoveBuilder.StartArrowInteraction();
        }

        void HandleCommit()
        {
            PlayerMoveBuilder.CommitMove();
        }

        void HandleCancel()
        {
            DisableDirectionHint();
            DisableCancelButton();
            EnableAllMoveButtons();
            PlayerMoveBuilder.Clear();
        }

        void EnableCancelButton()
        {
            CancelButton.gameObject.SetActive(true);
        }

        void DisableCancelButton()
        {
            CancelButton.gameObject.SetActive(false);
        }

        void EnableCommitButton()
        {
            CommitButton.gameObject.SetActive(true);
        }

        void DisableCommitButton()
        {
            CommitButton.gameObject.SetActive(false);
        }

        void EnableDirectionHint()
        {
            PickDirection.SetActive(true);
        }

        void DisableDirectionHint()
        {
            PickDirection.SetActive(false);
        }

        void DisableAllMoveButtons()
        {
            foreach (var button in _moveButtons)
            {
                button.gameObject.SetActive(false);
            }
        }

        void EnableAllMoveButtons()
        {
            foreach (var button in _moveButtons)
            {
                button.gameObject.SetActive(true);
            }
        }
    }
}
