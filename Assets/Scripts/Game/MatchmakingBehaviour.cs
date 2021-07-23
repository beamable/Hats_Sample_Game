using Beamable;
using Beamable.Common.Content;
using Beamable.Experimental.Api.Matchmaking;
using Hats.Game.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Hats.Game
{
	[Serializable]
	public class SimGameTypeRef : ContentRef<SimGameType>
	{
	}

	public class MatchmakingBehaviour : MonoBehaviour
	{
		[Header("Content Refs")]
		public SimGameTypeRef GameTypeRef;

		[Header("Runtime Values")]
		[ReadOnly]
		public MatchmakingHandle MatchmakingHandle = null;

		[ReadOnly]
		public float WillFinishAt = 0;

		[ReadOnly]
		public bool IsSearching;

		[ReadOnly]
		public int MaxPlayers;

		public UnityEvent OnTimedOut;

		[SerializeField]
		private Configuration _configuration = null;

		private IBeamableAPI _api = null;

		[ReadOnly]
		[SerializeField]
		private float MatchmakingSecondsLeft = -1.0f;

		public async void FindGame()
		{
			IsSearching = true;
			_api = await Beamable.API.Instance;

			Debug.Log($"Starting matchmaking with game_type={GameTypeRef.Id} override timeout={_configuration.OverrideMaxMatchmakingTimeout} ...");

			MatchmakingHandle = await _api.Experimental.MatchmakingService.StartMatchmaking(
				GameTypeRef.Id,
				maxWait: TimeSpan.FromSeconds(_configuration.OverrideMaxMatchmakingTimeout),
				updateHandler: handle =>
				{
					//WillFinishAt = Time.realtimeSinceStartup + handle.Status.SecondsRemaining;
				},
				readyHandler: handle =>
				{
					Debug.Assert(handle.State == MatchmakingState.Ready);

					var dbids = MatchmakingHandle.Status.Players;
					var gameId = MatchmakingHandle.Status.GameId;
					var matchId = handle.Match.matchId;

					Debug.Log($"Match is ready! Found matchID={matchId} gameId={gameId}");
					Debug.Log($"Starting match with DBIDs={string.Join(",", dbids.ToArray())}");

					List<long> dbidsAsLong = dbids.Select(i => long.Parse(i)).ToList();
					HatsScenes.LoadGameScene(gameId, dbidsAsLong);
				},
				timeoutHandler: handle =>
				{
					Debug.Log($"Matchmaking timed out! state={handle.State}");
					IsSearching = false;
					OnTimedOut?.Invoke();
				}
			);

			Debug.Log($"Matchmaking started with handle={MatchmakingHandle}");
		}

		public void Cancel()
		{
			IsSearching = false;
			MatchmakingHandle.Cancel();
		}
	}
}