using System;
using UnityEngine;

namespace HatsCore
{
   [Serializable]
   public class HatsPlayer
   {
      public long dbid;

      public override string ToString()
      {
         return $"{dbid}";
      }
   }

   public class HatsPlayerState
   {
      public Vector3Int Position;

      public HatsPlayerState Clone()
      {
         return new HatsPlayerState
         {
            Position = Position
         };
      }
   }
}