using Hats.Game;
using TMPro;
using UnityEngine;

namespace Hats.Game.UI
{
	public class TurnCounterBehaviour : GameEventHandler
	{
		[Header("UI References")]
		public TextMeshProUGUI TurnText;

		// Update is called once per frame
		private void Update()
		{
			TurnText.text = Game.Simulation?.CurrentTurn.ToString();
		}
	}
}