using Beamable;
using Beamable.Common.Content;
using Beamable.Experimental.Api.Matchmaking;
using UnityEngine;
using UnityEngine.UI;

public class MatchmakingExample : MonoBehaviour
{
	[Header("Beamable Setup")]
	public SimGameTypeRef gameType;
	
	[Header("User Interface")]
	[SerializeField]
	private Button startMatchmakingBtn;
	
	private BeamContext playerOne;
	private BeamContext playerTwo;
	[Header("Runtime State")]
	[SerializeField]
	private Match match;
	
	async void Start()
	{
		startMatchmakingBtn.enabled = false;
		playerOne = BeamContext.Default;
		playerTwo = BeamContext.ForPlayer("playerTwo");

		await playerOne.OnReady;
		await playerTwo.OnReady;
		startMatchmakingBtn.enabled = true;
	}

	public async void OnMatchStartButtonPressed()
	{
		if (gameType == null)
			return;

		match = null;

		// Player One joins the matchmaking queue
		var playerOneHandle = await playerOne.Api.Experimental.MatchmakingService.StartMatchmaking(gameType);
		startMatchmakingBtn.enabled = false;
		
		// Player Two joins the matchmaking queue
		await playerTwo.Api.Experimental.MatchmakingService.StartMatchmaking(gameType);

		// Can await until the matchmaking reaches resolution (timeout or complete).
		// Can also hook events or pass in handlers to StartMatchmaking invocation
		await playerOneHandle.WhenCompleted();

		match = playerOneHandle.Match;
		startMatchmakingBtn.enabled = true;
		switch (playerOneHandle.State)
		{
			case MatchmakingState.Searching:
				// Handle error state -- we shouldn't be in a searching status if the matchmaking process is completed
				Debug.LogError("Should not be in a searching state!");
				break;
			case MatchmakingState.Cancelled:
				// Reset UI
				Debug.Log("Matchmaking cancelled.");
				break;
			case MatchmakingState.Ready:
				// Start match
				Debug.Log("Match has been found!");
				break;
			case MatchmakingState.Timeout:
				// Trigger game versus POGMAN bot
				Debug.Log("A wild POGMAN has appeared!");
				break;
		}
	}
}
