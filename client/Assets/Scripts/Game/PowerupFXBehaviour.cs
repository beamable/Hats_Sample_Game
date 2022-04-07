using Hats.Simulation;
using System;
using System.Collections;
using UnityEngine;

namespace Hats.Game
{
	public class PowerupFXBehaviour : MonoBehaviour
	{
		[SerializeField]
		private GameObject _firewall = null;

		[SerializeField]
		private GameObject _teleport = null;

		[SerializeField]
		private GameObject _movingSprite = null;

		[SerializeField]
		private float _effectDuration = 2.0f;

		[SerializeField]
		private float _collectEffectMoveSpeed = 0.4f;

		[SerializeField]
		private GameObject _destroyParticles = null;

		[SerializeField]
		private GameObject _CollectParticles = null;

		private EffectType _effectType = EffectType.INVALID;
		private HatsPowerupType _powerupType = HatsPowerupType.INVALID_TYPE;

		private SpriteRenderer[] _allSpecificSprites = null;

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

			SetupPowerupSpecificSprites();
		}

		private void SetupPowerupSpecificSprites()
		{
			GameObject specificSpriteGO = null;

			switch (_powerupType)
			{
				case HatsPowerupType.FIREWALL:
					specificSpriteGO = _firewall;
					break;

				case HatsPowerupType.TELEPORT:
					specificSpriteGO = _teleport;
					break;

				default:
					throw new InvalidOperationException();
			}

			specificSpriteGO.SetActive(true);
			_allSpecificSprites = specificSpriteGO.GetComponentsInChildren<SpriteRenderer>();
		}

		private void Start()
		{
			if (_effectType == EffectType.COLLECT)
				StartCoroutine(RunCollectEffect());
			else
				StartCoroutine(RunDestroyEffect());
		}

		private IEnumerator RunCollectEffect()
		{
			_CollectParticles.SetActive(true);

			var movingSprite = _movingSprite.transform;
			var startTime = Time.time;
			var moveDirection = new Vector3(0.0f, 1.0f, 0.0f);
			var oneOverEffectDuration = 1.0f / _effectDuration;

			while (true)
			{
				var t = Time.time;

				var velocity = moveDirection * _collectEffectMoveSpeed * Time.deltaTime;
				movingSprite.localPosition += velocity;
				foreach (var sprite in _allSpecificSprites)
				{
					var newColor = sprite.color;
					newColor.a = Mathf.Lerp(1.0f, 0.0f, (t - startTime) * oneOverEffectDuration);
					sprite.color = newColor;
				}

				if (t - startTime > _effectDuration)
					break;

				yield return null;
			}

			Destroy(gameObject);
		}

		private IEnumerator RunDestroyEffect()
		{
			_destroyParticles.SetActive(true);

			var startTime = Time.time;
			var oneOverEffectDuration = 1.0f / _effectDuration;

			while (true)
			{
				var t = Time.time;

				foreach (var sprite in _allSpecificSprites)
				{
					var newColor = sprite.color;
					newColor.a = Mathf.Lerp(1.0f, 0.0f, (t - startTime) * oneOverEffectDuration);
					sprite.color = newColor;
				}

				if (t - startTime > _effectDuration)
					break;

				yield return null;
			}

			Destroy(gameObject);
		}
	}
}