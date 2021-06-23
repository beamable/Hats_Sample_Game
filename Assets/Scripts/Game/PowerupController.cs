using Hats.Simulation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Hats.Game
{
	public class PowerupController : GameEventHandler
	{
		[SerializeField]
		private PowerupFXBehaviour PowerupFXPrefab;

		private Vector3Int _position = Vector3Int.zero;
		private HatsPowerupType _powerupType = HatsPowerupType.INVALID_TYPE;

		public PowerupController()
		{
		}

		public void Setup(Vector3Int pos, HatsPowerupType type)
		{
			_position = pos;
			_powerupType = type;
		}

		public override IEnumerator HandleCollectablePowerupDestroyEvent(CollectablePowerupDestroyEvent evt, Action callback)
		{
			callback();

			if (evt.Position == _position)
			{
				PowerupFXBehaviour fxInstance = Game.BattleGridBehaviour.SpawnObjectAtCell(PowerupFXPrefab, evt.Position);
				PowerupFXBehaviour.EffectType fxType = evt.Reason == CollectablePowerupDestroyEvent.DestroyReason.COLLECTED_BY_PLAYER ?
					PowerupFXBehaviour.EffectType.COLLECT : PowerupFXBehaviour.EffectType.DESTROY;

				Debug.Log($"PWRTYPE={_powerupType}");
				fxInstance.Setup(fxType, _powerupType);

				Destroy(gameObject);
			}

			yield return null;
		}
	}
}