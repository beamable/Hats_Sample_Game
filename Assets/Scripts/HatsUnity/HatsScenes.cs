using System.Collections.Generic;
using HatsMultiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HatsUnity
{
   public static class HatsScenes
   {
      public static void LoadGameScene(string roomId, List<long> dbids=null)
      {
         SceneManager.LoadScene("Battle");
         GameProcessor.RoomId = roomId;
         GameProcessor.Dbids = dbids;
      }

      public static void LoadMatchmaking()
      {
         Debug.Log("Going to matchmaking...");
         SceneManager.LoadScene("Matchmaking");
      }
   }
}