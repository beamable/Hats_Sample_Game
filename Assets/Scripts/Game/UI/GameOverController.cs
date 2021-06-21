using System;
using System.Collections;
using System.Collections.Generic;
using Beamable;
using Beamable.Common;
using Beamable.Common.Leaderboards;
using Hats.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hats.Game.UI
{
	public class GameOverController : GameEventHandler
	{
		public float gameOverDelay = 3f; // How long to wait until the game ends

		[Header("UI References")]
		public ActionInputPanelBehaviour actionInputPanel;

		public GameObject Panel;
		public Button HomeButton;
		public TextMeshProUGUI StatusText;
		public TextMeshProUGUI EarnText;
		public TextMeshProUGUI EarnAmountText;
		public Image GemIcon;

		[Header("Player Rank References")]
		public List<PlayerRankBehaviour> PlayerRankBehaviours;

		[SerializeField]
		private LeaderboardRef _leaderboardRef;

		private LeaderboardContent _leaderboardContent;
		private IBeamableAPI _beamableAPI = null;

		public override IEnumerator HandleGameOverEvent(GameOverEvent evt, Action completeCallback)
		{
			// Turn off the action panel
			actionInputPanel.ShowGameOverText();

			// Wait a delay to let the end of the game settle before proceeding and showing UI
			yield return new WaitForSecondsRealtime(gameOverDelay);

			// Clear all UI from the action panel
			actionInputPanel.DisableAll();

			var reportTask = Game.MultiplayerGameDriver.DeclareResults(evt.Results);
			yield return reportTask.ToPromise().ToYielder();
			var results = reportTask.Result;
			if (results.cheatingDetected)
			{
				Debug.LogWarning("Cheating was detected! This likely means players have reported different scores. This could be that a player is cheating, or the simulation is no longer deterministic.");
			}

			yield return Beamable.API.Instance.ToYielder();
			var beamable = Beamable.API.Instance.GetResult();

			var selfDbid = beamable.User.id;

			var isWinner = selfDbid == evt.Winner.dbid;

			if (isWinner)
			{
				_beamableAPI.LeaderboardService.IncrementScore(_leaderboardRef.Id, 1);
			}

			StatusText.text = isWinner
				 ? "victory"
				 : "defeat";

			EarnText.text = isWinner
				 ? "You Earned"
				 : "You Still Earned";

			if (results.currenciesGranted.Count > 0)
			{
				EarnAmountText.text = results.currenciesGranted[0].amount.ToString();
			}
			else
			{
				EarnText.text = "No Rewards Today";
				Destroy(EarnAmountText.gameObject);
				Destroy(GemIcon.gameObject);
			}

			for (var i = 0; i < evt.Results.Count; i++)
			{
				var instance = PlayerRankBehaviours[i];
				var result = evt.Results[i];
				var isSelf = result.playerId == selfDbid;
				var _ = instance.Set(Game.Simulation.GetPlayer(result.playerId), result);
				if (isSelf)
				{
					instance.Glow();
				}
			}

			Panel.SetActive(true);

			// TODO: Show a loading spinner
			// TODO: Gather rewards, and player stats on this match, like kills, etc.

			completeCallback();
			yield break;
		}

		private async void SetupBeamable()
		{
			await Beamable.API.Instance.Then(beamableAPI =>
			{
				try
				{
					_beamableAPI = beamableAPI;
				}
				catch (Exception)
				{
				}
			});
		}

		// Start is called before the first frame update
		private void Start()
		{
			SetupBeamable();
			Panel.SetActive(false);
			HomeButton.onClick.AddListener(HandleHome);
			Debug.Log("_leaderboardRef.Id" + _leaderboardRef.Id);
		}

		// Update is called once per frame
		private void Update()
		{
		}

		private void HandleHome()
		{
			HatsScenes.LoadMatchmaking();
		}
	}
}