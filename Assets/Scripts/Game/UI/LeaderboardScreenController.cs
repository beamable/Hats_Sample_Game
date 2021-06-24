using Beamable.Common.Api.Leaderboards;
using Beamable.Common.Leaderboards;
using Beamable.UI.Scripts;
using Game.UI;
using Hats.Game;
using Hats.Game.Data;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardScreenController : MonoBehaviour
{
	[Header("Prefab References")]
	public LeaderboardEntryBehaviour EntryPrefab;

	[Header("UI References")]
	public Button MenuButton;

	public Button HomeButton;
	public GameObject LoadingSpinner;
	public RectTransform RankContainer;

	[Header("Runtime Internals")]
	[ReadOnly]
	public LeaderBoardView view;

	[Header("Configuration")]
	[SerializeField]
	private Configuration _configuration = null;

	// Start is called before the first frame update
	private async void Start()
	{
		MenuButton.onClick.AddListener(HandleMenu);
		HomeButton.onClick.AddListener(HandleMenu);

		// load up the leaderboards
		LoadingSpinner.SetActive(true);
		var beamable = await Beamable.API.Instance;
		view = await beamable.LeaderboardService.GetBoard(_configuration.LeaderboardRef, 0, 50, focus: beamable.User.id);

		LoadingSpinner.SetActive(false);
		for (var i = 0; i < RankContainer.childCount; i++)
		{
			Destroy(RankContainer.GetChild(i).gameObject);
		}

		foreach (var rank in view.rankings)
		{
			// need to load alias, and character stats
			var stats = await beamable.StatsService.GetStats("client", "public", "player", rank.gt);

			var character = await PlayerInventory.GetSelectedCharacter(rank.gt);
			var icon = await character.icon.LoadSprite();
			var alias = "";
			stats.TryGetValue("alias", out alias);
			if (string.IsNullOrEmpty(alias))
			{
				alias = "Anonymous";
			}
			var instance = Instantiate(EntryPrefab, RankContainer);
			instance.Set(alias, icon, rank);

			if (rank.gt == beamable.User.id)
			{
				instance.SetForSelf();
			}
		}
	}

	private void HandleMenu()
	{
		HatsScenes.LoadMatchmaking();
	}
}