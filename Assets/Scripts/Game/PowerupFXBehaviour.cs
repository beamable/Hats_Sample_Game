using Hats.Simulation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hats.Game
{
	public class PowerupFXBehaviour : MonoBehaviour
	{
		[SerializeField]
		private GameObject FirewallSprite = null;

		private EffectType _effectType = EffectType.INVALID;
		private HatsPowerupType _powerupType = HatsPowerupType.INVALID_TYPE;

		public enum EffectType
		{
			INVALID,
			COLLECT,
			DESTROY,
		}

		public void Setup(EffectType type, HatsPowerupType powerupType)
		{
			_effectType = type;
			_powerupType = powerupType;

			switch (powerupType)
			{
				case HatsPowerupType.FIREWALL:
					FirewallSprite.SetActive(true);
					break;

				default:
					throw new InvalidOperationException();
			}
		}

		private void Start()
		{
			StartCoroutine(RunEffect());
		}

		private IEnumerator RunEffect()
		{
			yield return new WaitForSecondsRealtime(1.0f);
			Destroy(gameObject);
		}
	}
}