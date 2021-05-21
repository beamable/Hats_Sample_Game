using System.Collections.Generic;
using Hats.Game;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hats.Game
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

      public static void LoadCharacterSelection()
      {
         SceneManager.LoadScene("CharacterPage");
      }
   }
}