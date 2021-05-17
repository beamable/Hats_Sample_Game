using System.Collections;
using System.Collections.Generic;
using Beamable.Experimental.Api.Matchmaking;
using HatsUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSelectionController : MonoBehaviour
{
    [Header("Game References")]
    public MatchmakingBehaviour MatchmakingBehaviour;

    [Header("UI References")]
    public TextMeshProUGUI GameIdInput;
    public Button JoinButton;
    public Button StartMatchmakingButton;
    public TextMeshProUGUI StatusText;
    public TextMeshProUGUI SecondsRemainingText;
    public GameObject LoadingSpinner;

    // Start is called before the first frame update
    void Start()
    {
        LoadingSpinner.SetActive(false);
        StatusText.gameObject.SetActive(false);
        SecondsRemainingText.gameObject.SetActive(false);

        JoinButton.onClick.AddListener(HandleJoin);
        StartMatchmakingButton.onClick.AddListener(HandleStart);
    }

    // Update is called once per frame
    void Update()
    {
        StatusText.text = GetStatusMessage();
        SecondsRemainingText.text = GetSecondsRemainingMessage();
    }

    public void HandleJoin()
    {
        var gameId = GameIdInput.text;
        Debug.Log("Game Id" + gameId);

        HatsScenes.LoadGameScene(gameId);
    }

    public void HandleStart()
    {
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

        return $"({MatchmakingBehaviour.MatchmakingHandle.Status.SecondsRemaining} seconds left)";
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
