using System.Collections.Generic;
using Hats.Game;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hats.Game
{
	public static class HatsScenes
	{
		public static void LoadGameScene(string roomId, List<long> dbids = null)
		{
			Debug.Log("Going to battle ...");
			SceneManager.LoadScene("Battle");
			GameProcessor.RoomId = roomId;
			GameProcessor.Dbids = dbids;
		}

		public static void LoadMatchmaking()
		{
			Debug.Log("Going to matchmaking ...");
			SceneManager.LoadScene("Matchmaking");
		}

		public static void LoadOptions()
		{
			Debug.Log("Going to options ...");
			SceneManager.LoadScene("OptionsPage");
		}

		public static void LoadCharacterSelection()
		{
			Debug.Log("Going to character selection / shop  ...");
			SceneManager.LoadScene("CharacterPage");
		}

		public static void LoadLeaderboards()
		{
			Debug.Log("Going to leaderboards ...");
			SceneManager.LoadScene("Leaderboards");
		}
	}
}