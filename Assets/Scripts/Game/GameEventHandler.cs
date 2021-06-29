using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Hats.Simulation;
using UnityEngine;

namespace Hats.Game
{
	public delegate IEnumerator GameEventHandlerFunction<in TEvent>(TEvent evt, Action onComplete)
		 where TEvent : HatsGameEvent;

	public static class GameEventHandlerExtensions
	{
		public static EventHandlerYielder Handle<TEvent>(this List<GameEventHandler> handlers,
			 MonoBehaviour context,
			 TEvent evt,
			 Func<GameEventHandler, GameEventHandlerFunction<TEvent>> eventHandlerFunc) where TEvent : HatsGameEvent
		{
			var yielder = new EventHandlerYielder(handlers.Count, out var callback);
			foreach (var handler in handlers)
			{
				// TODO: Add a timeout feature here? Or show blocked messages?
				var func = eventHandlerFunc(handler);
				var enumeration = func(evt, callback);
				context.StartCoroutine(enumeration);
			}
			return yielder;
		}
	}

	public class GameEventHandler : MonoBehaviour
	{
		[ReadOnly]
		[SerializeField]
		protected GameProcessor Game;

		public virtual IEnumerator HandleSpawnEvent(PlayerSpawnEvent evt, Action completeCallback)
		{
			completeCallback();
			yield break;
		}

		public virtual IEnumerator HandleCollectablePowerupSpawnEvent(CollectablePowerupSpawnEvent evt, Action completeCallback)
		{
			completeCallback();
			yield break;
		}

		public virtual IEnumerator HandleCollectablePowerupDestroyEvent(CollectablePowerupDestroyEvent evt, Action completeCallback)
		{
			completeCallback();
			yield break;
		}

		public virtual IEnumerator HandleMoveEvent(PlayerMoveEvent evt, Action completeCallback)
		{
			completeCallback();
			yield break;
		}

		public virtual IEnumerator HandleTurnReadyEvent(TurnReadyEvent evt, Action completeCallback)
		{
			completeCallback();
			yield break;
		}

		public virtual IEnumerator HandleTurnOverEvent(TurnOverEvent evt, Action completeCallback)
		{
			completeCallback();
			yield break;
		}

		public virtual IEnumerator HandleTickEvent(TickEvent evt, Action completeCallback)
		{
			completeCallback();
			yield break;
		}

		public virtual IEnumerator HandleShieldEvent(PlayerShieldEvent evt, Action completeCallback)
		{
			completeCallback();
			yield break;
		}

		public virtual IEnumerator HandleAttackEvent(PlayerProjectileAttackEvent evt, Action completeCallback)
		{
			completeCallback();
			yield break;
		}

		public virtual IEnumerator HandleGameOverEvent(GameOverEvent evt, Action completeCallback)
		{
			completeCallback();
			yield break;
		}

		public virtual IEnumerator HandlePlayerKilledEvent(PlayerKilledEvent evt, Action completeCallback)
		{
			completeCallback();
			yield break;
		}

		public virtual IEnumerator HandleSuddenDeathEvent(SuddenDeathEvent evt, Action completeCallback)
		{
			completeCallback();
			yield break;
		}

		public virtual IEnumerator HandlePlayerDroppedEvent(PlayerLeftEvent evt, Action completeCallback)
		{
			completeCallback();
			yield break;
		}

		public virtual IEnumerator HandleSuddenDeathStartedEvent(SuddenDeathStartedEvent evt, Action completeCallback)
		{
			completeCallback();
			yield break;
		}

		protected void FindGameProcessor()
		{
			Debug.Log($"Initializing GameEventHandler={this}");
			if (Game != null) return;
			Game = FindObjectOfType<GameProcessor>();
			Game.EventHandlers.Add(this);
		}

		protected virtual void Awake()
		{
			FindGameProcessor();
		}

		protected virtual void OnDestroy()
		{
			Game?.EventHandlers.Remove(this);
		}
	}

	public class EventHandlerYielder : CustomYieldInstruction
	{
		private int _totalCallbacksRequired;

		public override bool keepWaiting => _totalCallbacksRequired > 0;

		public EventHandlerYielder(int count, out Action completionCounterCallback)
		{
			_totalCallbacksRequired = count;
			completionCounterCallback = () =>
			{
				_totalCallbacksRequired--;
			};
		}
	}
}