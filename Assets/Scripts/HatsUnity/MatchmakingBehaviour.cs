using System;
using Beamable.Common.Content;
using Beamable.Experimental.Api.Matchmaking;
using Unity.Collections;
using UnityEngine;

namespace HatsUnity
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

      // private void Start()
      // {
      //    FindGame();
      // }

      public void Update()
      {

      }

      public async void FindGame()
      {
         var beamable = await Beamable.API.Instance;
         MatchmakingHandle = await beamable.Experimental.MatchmakingService.StartMatchmaking(GameTypeRef);

         MatchmakingHandle.OnMatchTimeout += MatchmakingHandleOnMatchTimeout;
         MatchmakingHandle.OnMatchReady += MatchmakingHandleOnMatchReady;
      }


      private void MatchmakingHandleOnMatchReady(MatchmakingHandle handle)
      {
         StartGame();
      }
      private void MatchmakingHandleOnMatchTimeout(MatchmakingHandle handle)
      {
         // dump into a random game, with all-bots. No rewards viable?
         StartGame();
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