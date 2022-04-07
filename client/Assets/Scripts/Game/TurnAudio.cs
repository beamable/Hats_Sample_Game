using Hats.Simulation;
using System;
using System.Collections;
using UnityEngine;

namespace Hats.Game
{
	public class TurnAudio : GameEventHandler
	{
		[SerializeField]
		private AudioSource _audioSource = null;

		[SerializeField]
		private float _basePitch = 0.5f;

		[SerializeField]
		private float _perTurnPitchShift = 0.05f;

		public override IEnumerator HandleTurnReadyEvent(TurnReadyEvent evt, Action completeCallback)
		{
			completeCallback();

			MusicManager.Instance.MakeMusicLoud(0.1f);
			yield break;
		}

		public override IEnumerator HandleTurnOverEvent(TurnOverEvent evt, Action completeCallback)
		{
			completeCallback();

			_audioSource.pitch = +_basePitch + Game.Simulation.CurrentTurnNumber * _perTurnPitchShift;
			_audioSource.Play();

			MusicManager.Instance.MakeMusicBassy(Game.Configuration.TurnTimeout);
			yield break;
		}

		protected void Start()
		{
			MusicManager.Instance.MakeMusicBassy(Game.Configuration.TurnTimeout);
		}
	}
}