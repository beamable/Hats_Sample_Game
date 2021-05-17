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

      public override bool Equals(object obj)
      {
         if (obj is HatsPlayer other)
         {
            return Equals(other);
         }

         return false;
      }

      protected bool Equals(HatsPlayer other)
      {
         return dbid == other.dbid;
      }

      public override int GetHashCode()
      {
         return dbid.GetHashCode();
      }
   }

   public class HatsPlayerState
   {
      public Vector3Int Position;
      public bool IsDead;
      public bool IsShield;

      public HatsPlayerState Clone()
      {
         return new HatsPlayerState
         {
            Position = Position,
            IsDead = IsDead,
            IsShield = IsShield
         };
      }
   }
}