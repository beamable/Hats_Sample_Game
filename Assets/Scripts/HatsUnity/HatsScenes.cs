using HatsMultiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HatsUnity
{
   public static class HatsScenes
   {
      public static void LoadGameScene(string roomId)
      {
         SceneManager.LoadScene("Battle");
         GameProcessor.RoomId = roomId;
      }

      public static void LoadMatchmaking()
      {
         Debug.Log("Going to matchmaking...");
         SceneManager.LoadScene("Matchmaking");
      }
   }
}