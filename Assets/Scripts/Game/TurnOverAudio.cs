using Hats.Simulation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hats.Game
{
	public class TurnOverAudio : GameEventHandler
	{
		[SerializeField]
		private AudioSource _audioSource = null;

		[SerializeField]
		private float _basePitch = 0.5f;

		[SerializeField]
		private float _perTurnPitchShift = 0.05f;

		public override IEnumerator HandleTurnOverEvent(TurnOverEvent evt, Action completeCallback)
		{
			completeCallback();

			_audioSource.pitch = +_basePitch + Game.Simulation.CurrentTurn * _perTurnPitchShift;
			_audioSource.Play();
			yield break;
		}
	}
}