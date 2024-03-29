using System;
using System.Collections;
using Beamable;
using Hats.Simulation;
using Hats.Game;
using UnityEngine;

public class EnemyPanelController : GameEventHandler
{
	public Renderer RightEdgeDetector;

	[Header("Prefabs")]
	public EnemyPanelBehaviour EnemyPrefab;

	[Header("UI References")]
	public RectTransform TopPanel;

	public RectTransform RightPanel;

	public string[] RandomNames = new string[] // TODO: Pull this out as content
	{
		  "Evil Guy",
		  "Evil Bud",
		  "Evil Mack",
		  "Evil Al",
		  "Evil Dude",
		  "Evil Thing",
	};

	[ReadOnly]
	[SerializeField]
	private bool _isTop;

	[ReadOnly]
	[SerializeField]
	private RectTransform _currentPanel;

	private BeamContext _beamContext;

	public override IEnumerator HandleSpawnEvent(PlayerSpawnEvent evt, Action completeCallback)
	{
		var instance = Instantiate(EnemyPrefab, _currentPanel);
		instance.Clear();
		// if the player is a Bot, they don't have stats; so we need to auto generate them.

		PlayerStats stats;
		if (evt.Player.dbid < 0)
		{
			var index = -evt.Player.dbid % RandomNames.Length;
			var alias = RandomNames[index];
			stats = new PlayerStats
			{
				Alias = alias
			};
		}
		else
		{
			_beamContext = BeamContext.Default;
			yield return _beamContext.OnReady.ToYielder();
			var request = _beamContext.Api.StatsService.GetStats("client", "public", "player", evt.Player.dbid);
			yield return request.ToYielder();
			var result = request.GetResult();
			if (!result.TryGetValue("alias", out var alias))
			{
				alias = "Anon";
			}
			stats = new PlayerStats
			{
				Alias = alias
			};
		}

		instance.SetForPlayer(stats);

		completeCallback();
		yield break;
	}

	// Start is called before the first frame update
	private void Start()
	{
		Clear(TopPanel);
		Clear(RightPanel);
	}

	// Update is called once per frame
	private void Update()
	{
		var wasTop = _isTop;
		_isTop = !RightEdgeDetector.isVisible;
		if (wasTop && !_isTop)
		{
			// move everything to the right panel
			Move(TopPanel, RightPanel);
		}
		else if (!wasTop && _isTop)
		{
			Move(RightPanel, TopPanel);
		}
	}

	private void Clear(RectTransform container)
	{
		for (var i = 0; i < container.childCount; i++)
		{
			Destroy(container.GetChild(i).gameObject);
		}
	}

	private void Move(RectTransform from, RectTransform to)
	{
		while (from.childCount > 0)
		{
			from.GetChild(0).SetParent(to, false);
		}

		_currentPanel = to;
	}
}