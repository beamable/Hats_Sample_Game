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
		private Vector3Int _position = Vector3Int.zero;

		public void Setup(Vector3Int pos)
		{
			_position = pos;
		}

		public override IEnumerator HandleCollectablePowerupDestroyEvent(CollectablePowerupDestroyEvent evt, Action callback)
		{
			callback();

			if (evt.Position == _position)
			{
				Debug.Log($"Removing powerup at pos={evt.Position} id={ RuntimeHelpers.GetHashCode(this)}");
				Destroy(gameObject);
			}

			yield return null;
		}
	}
}