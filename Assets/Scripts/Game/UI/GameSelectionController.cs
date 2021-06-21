using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
	public Button QuitButton;

	public Button OptionsButton;
	public Button CharacterButton;
	public Button StartMatchmakingButton;
	public Button LeaderboardButton;
	public TextMeshProUGUI StatusText;
	public TextMeshProUGUI SecondsRemainingText;
	public TextMeshProUGUI PlayText;
	public GameObject LoadingSpinner;

	public void OnAccountToggle(bool isOpen)
	{
		if (isOpen)
		{
			MusicManager.Instance.PlayAccountMusic();
		}
		else
		{
			MusicManager.Instance.PlayMenuMusic();
		}
	}

	public void HandleQuit()
	{
		// TODO: Add confirmation screen.
		Debug.Log("Quit");
		Application.Quit();
	}

	public void HandleCharacter()
	{
		HatsScenes.LoadCharacterSelection();
	}

	public void HandleLeaderboards()
	{
		HatsScenes.LoadLeaderboards();
	}

	public void HandleStart()
	{
		if (MatchmakingBehaviour.IsSearching)
		{
			PlayText.text = "PLAY";
			LeaderboardButton.interactable = true;
			CharacterButton.interactable = true;
			LoadingSpinner.SetActive(false);
			StatusText.gameObject.SetActive(false);
			SecondsRemainingText.gameObject.SetActive(false);
			MatchmakingBehaviour.Cancel();
		}
		else
		{
			PlayText.text = "CANCEL";
			LeaderboardButton.interactable = false;
			CharacterButton.interactable = false;
			LoadingSpinner.SetActive(true);
			StatusText.gameObject.SetActive(true);
			SecondsRemainingText.gameObject.SetActive(true);
			MatchmakingBehaviour.FindGame();
		}
	}

	// Start is called before the first frame update
	private void Start()
	{
		MusicManager.Instance.PlayMenuMusic();

		LoadingSpinner.SetActive(false);
		StatusText.gameObject.SetActive(false);
		SecondsRemainingText.gameObject.SetActive(false);
		QuitButton.onClick.AddListener(HandleQuit);
		OptionsButton.onClick.AddListener(HandleOptions);
		CharacterButton.onClick.AddListener(HandleCharacter);
		LeaderboardButton.onClick.AddListener(HandleLeaderboards);
		StartMatchmakingButton.onClick.AddListener(HandleStart);
	}

	// Update is called once per frame
	private void Update()
	{
		StatusText.text = GetStatusMessage();
		var secondsRemaining = GetSecondsRemainingMessage();
		var playersText = GetPlayersMessage();
		SecondsRemainingText.text = $"{secondsRemaining}\n{playersText}";
	}

	private void HandleOptions()
	{
		HatsScenes.LoadOptions();
	}

	private string GetPlayersMessage()
	{
		if (MatchmakingBehaviour.MatchmakingHandle == null)
		{
			return "";
		}

		var players = MatchmakingBehaviour.MatchmakingHandle.Status.Players.Count;
		var max = MatchmakingBehaviour.MaxPlayers;
		return $"{players}/{max} joined";
	}

	private string GetSecondsRemainingMessage()
	{
		if (MatchmakingBehaviour.MatchmakingHandle == null)
		{
			return "";
		}

		var secondsLeft = MatchmakingBehaviour.SecondsLeft;
		return secondsLeft <= 1
			 ? "Get ready!"
			 : $"{MatchmakingBehaviour.SecondsLeft:0.0} secs";
	}

	private string GetStatusMessage()
	{
		if (MatchmakingBehaviour.MatchmakingHandle == null)
		{
			return "Starting Search...";
		}
		switch (MatchmakingBehaviour.MatchmakingHandle.State)
		{
			case MatchmakingState.Searching:
				return "Searching ...";

			case MatchmakingState.Timeout:
				return "Failed.";

			case MatchmakingState.Cancelled:
				return "Cancelled.";

			case MatchmakingState.Ready:
				return "Starting ...";
		}
		return null;
	}
}