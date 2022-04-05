using System;
using System.Collections;
using Hats.Simulation;
using TMPro;
using UnityEngine;

namespace Hats.Game.UI
{
	public class TimeCounterBehaviour : GameEventHandler
	{
		[Header("UI References")]
		public TextMeshProUGUI TimeText;

		[ReadOnly]
		[SerializeField]
		private float _targetTime;

		public override IEnumerator HandleTickEvent(TickEvent evt, Action completeCallback)
		{
			_targetTime = Mathf.Clamp(Game.SecondsLeftInTurn + 1, 1, Game.Configuration.TurnTimeout);
			var text = $"{_targetTime:0}";
			TimeText.text = text;
			return base.HandleTickEvent(evt, completeCallback);
		}
	}
}