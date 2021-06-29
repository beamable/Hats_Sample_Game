using Beamable.Common.Content;
using Beamable.Common.Leaderboards;
using Beamable.Common.Shop;
using Hats.Content;
using UnityEngine;

namespace Hats.Game.Data
{
	/// <summary>
	/// Store the common configuration for easy editing ats
	/// EditTime and RuntTime with the Unity Inspector Window.
	/// </summary>
	[CreateAssetMenu(
		fileName = Title,
		menuName = BeamableConstants.MENU_ITEM_PATH_ASSETS_BEAMABLE_SAMPLES + "/" +
		"Multiplayer/Create New " + Title,
		order = BeamableConstants.MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_1)]
	public class Configuration : ScriptableObject
	{
		//  Constants  -----------------------------------
		private const string Title = "HATS Configuration";

		[Header("Game Content")]
		[SerializeField]
		private SimGameTypeRef _gameTypeRef = null;

		//  Fields ---------------------------------------
		[SerializeField]
		private LeaderboardRef _leaderboardRef = null;

		[SerializeField]
		private BotProfileRef _botProfileRef = null;

		[Header("Network")]
		[SerializeField]
		private int _framesPerSecond = 20;

		[Header("Gameplay")]
		[SerializeField]
		private int _turnTimeout = 5;

		[SerializeField]
		private int _turnsUntilSuddenDeath = 3;

		[SerializeField]
		private double _chanceToSpawnSuddenDeathTileBelowPlayer = 0.45;

		[SerializeField]
		private int _maxPowerupsInWorldAtTheSameTime = 4;

		[SerializeField]
		private int _maxPowerupsToSpawnInOneTurn = 2;

		[SerializeField]
		private int _powerupsValidForTurns = 3;

		public SimGameTypeRef GameTypeRef { get => _gameTypeRef; }

		public LeaderboardRef LeaderboardRef { get => _leaderboardRef; }

		public int TurnTimeout { get => _turnTimeout; }
		public int TurnsUntilSuddenDeath { get => _turnsUntilSuddenDeath; }
		public double ChanceToSpawnSuddenDeathTileBelowPlayer { get => _chanceToSpawnSuddenDeathTileBelowPlayer; }
		public int MaxPowerupsInWorldAtTheSameTime { get => _maxPowerupsInWorldAtTheSameTime; }
		public int MaxPowerupsToSpawnInOneTurn { get => _maxPowerupsToSpawnInOneTurn; }
		public BotProfileRef BotProfileRef { get => _botProfileRef; }
		public int PowerupsValidForTurns { get => _powerupsValidForTurns; }
		public int FramesPerSecond { get => _framesPerSecond; }
	}
}