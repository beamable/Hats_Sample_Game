using System;
using System.Collections.Generic;
using Beamable.Common.Content;
using UnityEngine;

namespace Game.UI
{
   public class DontDestroyOnLoadBehaviour : MonoBehaviour
   {
      private static Dictionary<string, DontDestroyOnLoadBehaviour> _existingObjects =
         new Dictionary<string, DontDestroyOnLoadBehaviour>();

      public OptionalString DurableName;

      private void Start()
      {
         var durableName = DurableName.HasValue
            ? DurableName.Value
            : gameObject.name;

         if (!_existingObjects.ContainsKey(durableName))
         {
            DontDestroyOnLoad(gameObject);
            _existingObjects.Add(durableName, this);
         }
         else
         {
            Destroy(gameObject);
         }
      }
   }
}