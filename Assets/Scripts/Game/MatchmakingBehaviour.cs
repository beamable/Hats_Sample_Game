using System;
using Beamable.Common.Content;
using Beamable.Experimental.Api.Matchmaking;
using Unity.Collections;
using UnityEngine;

namespace Hats.Game
{
   [Serializable]
   public class SimGameTypeRef : ContentRef<SimGameType>
   {
      // TODO: Pull this into Beamable 0.10.0
   }

   public class MatchmakingBehaviour : MonoBehaviour
   {
      [Header("Content Refs")]
      public SimGameTypeRef GameTypeRef;

      [Header("Runtime Values")]
      [ReadOnly]
      public MatchmakingHandle MatchmakingHandle;

      [ReadOnly]
      public float WillFinishAt = 0;

      [ReadOnly]
      public bool IsSearching;

      [ReadOnly]
      public int MaxPlayers;

      public float SecondsLeft => Mathf.Max(1, WillFinishAt - Time.realtimeSinceStartup);

      public void Update()
      {

      }

      public async void FindGame()
      {
         IsSearching = true;
         var beamable = await Beamable.API.Instance;
         var gameType = await GameTypeRef.Resolve();
         MaxPlayers = gameType.maxPlayers;
         MatchmakingHandle = await beamable.Experimental.MatchmakingService.StartMatchmaking(GameTypeRef);

         MatchmakingHandle.OnMatchTimeout += MatchmakingHandleOnMatchTimeout;
         MatchmakingHandle.OnUpdate += MatchmakingHandleOnOnUpdate;
         MatchmakingHandle.OnMatchReady += MatchmakingHandleOnMatchReady;
      }

      public void Cancel()
      {
         IsSearching = false;
         MatchmakingHandle.Cancel();
      }

      private void MatchmakingHandleOnOnUpdate(MatchmakingHandle handle)
      {
         WillFinishAt = Time.realtimeSinceStartup + handle.Status.SecondsRemaining;
      }


      private void MatchmakingHandleOnMatchReady(MatchmakingHandle handle)
      {
         StartGame();
      }
      private void MatchmakingHandleOnMatchTimeout(MatchmakingHandle handle)
      {
         Debug.LogError("Failed to find a match : ( ");
      }

      public void StartGame()
      {
         var dbids = MatchmakingHandle.Status.Players;
         var gameId = MatchmakingHandle.Status.GameId;
         Debug.Log("Starting game " + gameId + " / " + dbids);
         HatsScenes.LoadGameScene(gameId, dbids);
      }
   }
}