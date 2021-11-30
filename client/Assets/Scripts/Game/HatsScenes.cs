using System.Collections.Generic;
using Hats.Game;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hats.Game
{
	public static class HatsScenes
	{
		public static void LoadMatchmaking()
		{
			Debug.Log("Going to matchmaking ...");
			SceneManager.LoadScene("Scene01Intro");
		}
		
		public static void LoadGameScene(string roomId, List<long> dbids = null)
		{
			Debug.Log("Going to battle ...");
			SceneManager.LoadScene("Scene02Game");
			GameProcessor.RoomId = roomId;
			GameProcessor.Dbids = dbids;
		}

		public static void LoadCharacterSelection()
		{
			Debug.Log("Going to character selection / shop  ...");
			SceneManager.LoadScene("Scene03Inventory");
		}
		
		public static void LoadOptions()
		{
			Debug.Log("Going to options ...");
			SceneManager.LoadScene("Scene04Settings");
		}

		public static void LoadLeaderboards()
		{
			Debug.Log("Going to leaderboards ...");
			SceneManager.LoadScene("Scene05Leaderboards");
		}
	}
}