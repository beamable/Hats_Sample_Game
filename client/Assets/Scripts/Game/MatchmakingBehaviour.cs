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
		public bool IsSearching;

		public UnityEvent OnTimedOut;

		[SerializeField]
		private Configuration _configuration = null;

		private BeamContext _beamContext;

		public async void FindGame()
		{
			IsSearching = true;
			_beamContext = BeamContext.Default;
			await _beamContext.OnReady;

			Debug.Log($"Starting matchmaking with game_type={GameTypeRef.Id} override timeout={_configuration.OverrideMaxMatchmakingTimeout} ...");

			MatchmakingHandle = await _beamContext.Api.Experimental.MatchmakingService.StartMatchmaking(
				GameTypeRef.Id,
				handle =>
				{
					// No updates available at the moment when searching a match.
				},
				handle =>
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
				handle =>
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