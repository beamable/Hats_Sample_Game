using System.Collections;
using System.Collections.Generic;
using Beamable.Experimental.Api.Matchmaking;
using Hats.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSelectionController : MonoBehaviour
{
    [Header("Game References")]
    public MatchmakingBehaviour MatchmakingBehaviour;

    [Header("UI References")]
    public Button CharacterButton;
    public Button StartMatchmakingButton;
    public Button LeaderboardButton;
    public TextMeshProUGUI StatusText;
    public TextMeshProUGUI SecondsRemainingText;
    public TextMeshProUGUI PlayText;
    public GameObject LoadingSpinner;

    // Start is called before the first frame update
    void Start()
    {
        LoadingSpinner.SetActive(false);
        StatusText.gameObject.SetActive(false);
        SecondsRemainingText.gameObject.SetActive(false);

        CharacterButton.onClick.AddListener(HandleCharacter);
        StartMatchmakingButton.onClick.AddListener(HandleStart);
    }

    // Update is called once per frame
    void Update()
    {
        StatusText.text = GetStatusMessage();
        SecondsRemainingText.text = GetSecondsRemainingMessage();
    }

    public void HandleCharacter()
    {
        HatsScenes.LoadCharacterSelection();
    }

    public void HandleStart()
    {
        PlayText.gameObject.SetActive(false);
        StartMatchmakingButton.interactable = false;
        LeaderboardButton.interactable = false;
        CharacterButton.interactable = false;
        LoadingSpinner.SetActive(true);
        StatusText.gameObject.SetActive(true);
        SecondsRemainingText.gameObject.SetActive(true);
        MatchmakingBehaviour.FindGame();
    }

    string GetSecondsRemainingMessage()
    {
        if (MatchmakingBehaviour.MatchmakingHandle == null)
        {
            return "";
        }

        var secondsLeft = MatchmakingBehaviour.SecondsLeft;
        return secondsLeft <= 1
            ? "(Finalizing...)"
            : $"({MatchmakingBehaviour.SecondsLeft:0.0} seconds left)";
    }

    string GetStatusMessage()
    {
        if (MatchmakingBehaviour.MatchmakingHandle == null)
        {
            return "Starting Search...";
        }
        switch (MatchmakingBehaviour.MatchmakingHandle.State)
        {
            case MatchmakingState.Searching:
                return "Searching...";
            case MatchmakingState.Timeout:
                return "Failed.";
            case MatchmakingState.Cancelled:
                return "Cancelled.";
            case MatchmakingState.Ready:
                return "Starting...";
        }
        return null;

    }
}
