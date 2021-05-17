using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using HatsCore;
using UnityEngine;

namespace HatsMultiplayer
{
   // public interface IGameEventHandler<TEvent>
   //    where TEvent : HatsGameEvent
   // {
   //    IEnumerator HandleEvent(TEvent evt);
   // }

   public delegate IEnumerator GameEventHandlerFunction<in TEvent>(TEvent evt, Action onComplete)
      where TEvent : HatsGameEvent;

   public class GameEventHandler : MonoBehaviour
   {
      [ReadOnly]
      [SerializeField]
      protected GameProcessor GameProcessor;

      private void Start()
      {
         FindGameProcessor();
      }

      protected void FindGameProcessor()
      {
         if (GameProcessor != null) return;
         GameProcessor = FindObjectOfType<GameProcessor>();
         GameProcessor.EventHandlers.Add(this);
      }


      public virtual IEnumerator HandleSpawnEvent(PlayerSpawnEvent evt, Action completeCallback)
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

      public virtual IEnumerator HandleAttackEvent(PlayerAttackEvent evt, Action completeCallback)
      {
         completeCallback();
         yield break;
      }

   }

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
            var func = eventHandlerFunc(handler);
            var enumeration = func(evt, callback);
            context.StartCoroutine(enumeration);
         }
         return yielder;
      }

   }

   public class EventHandlerYielder : CustomYieldInstruction
   {
      private int _totalCallbacksRequired;

      public EventHandlerYielder(int count, out Action completionCounterCallback)
      {
         _totalCallbacksRequired = count;
         completionCounterCallback = () =>
         {
            _totalCallbacksRequired--;
         };
      }

      public override bool keepWaiting => _totalCallbacksRequired > 0;
   }

}