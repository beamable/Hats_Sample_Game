using DG.Tweening;
using Hats.Simulation;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Hats.Game
{
	public class SuddenDeathEffect : GameEventHandler
	{
		[SerializeField]
		private WindZone _windZone = null;

		[SerializeField]
		private AudioSource _audioSource = null;

		[SerializeField]
		private AudioSource _burningAudio = null;

		[SerializeField]
		private TMP_Text _suddenDeathText = null;

		[SerializeField]
		private SpriteRenderer _battleBackground = null;

		[SerializeField]
		private SpriteRenderer[] _clouds = null;

		[SerializeField]
		private Color _backgroundColor = Color.white;

		[SerializeField]
		private Color _cloudColor = Color.white;

		[SerializeField]
		private float _effectDuration = 2.0f;

		[SerializeField]
		private float _backgroundChangeDuration = 2.0f;

		[SerializeField]
		private float _burningAudioFinalVolume = 1.0f;

		private Color TransparentWhite = new Color(1.0f, 1.0f, 1.0f, 0.0f);

		public override IEnumerator HandleSuddenDeathStartedEvent(SuddenDeathStartedEvent evt, Action callback)
		{
			callback();

			_audioSource.Play();

			_windZone.gameObject.SetActive(true);

			_battleBackground.DOColor(_backgroundColor, _backgroundChangeDuration).SetLoops(-1, LoopType.Yoyo);
			foreach (var cloud in _clouds)
				cloud.DOColor(_cloudColor, _backgroundChangeDuration * 2.0f).SetLoops(-1, LoopType.Yoyo);

			var textSequence = DOTween.Sequence();
			textSequence.Append(_suddenDeathText.DOColor(Color.white, _effectDuration));
			textSequence.Append(_suddenDeathText.DOColor(TransparentWhite, _effectDuration));
			textSequence.Insert(0.0f, _suddenDeathText.transform.DOShakeScale(_effectDuration));

			_burningAudio.DOFade(_burningAudioFinalVolume, _effectDuration);

			yield break;
		}

		public override IEnumerator HandleGameOverEvent(GameOverEvent evt, Action completeCallback)
		{
			completeCallback();
			_burningAudio.DOFade(0.0f, _effectDuration);
			yield break;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			_battleBackground.DOKill();
			foreach (var cloud in _clouds)
				cloud.DOKill();
		}

		protected override void Awake()
		{
			base.Awake();
			_suddenDeathText.color = TransparentWhite;
		}
	}
}