using UnityEngine;

namespace HatsCore
{
   public class HatsGameEvent
   {

   }

   public class PlayerSpawnEvent : HatsGameEvent
   {
      public HatsPlayer Player;
      public Vector3Int Position;

      public PlayerSpawnEvent(HatsPlayer player, Vector3Int position)
      {
         Player = player;
         Position = position;
      }

      public override string ToString()
      {
         return $"{nameof(PlayerSpawnEvent)}: plr=[{Player}] position=[{Position}]";
      }
   }

   public class PlayerCommittedMoveEvent : HatsGameEvent
   {

   }

   public class TurnReadyEvent : HatsGameEvent {}


   public class PlayerMoveEvent : HatsGameEvent
   {
      public HatsPlayer Player;
      public Vector3Int OldPosition;
      public Vector3Int NewPosition;

      public PlayerMoveEvent(HatsPlayer player, Vector3Int oldPosition, Vector3Int newPosition)
      {
         Player = player;
         OldPosition = oldPosition;
         NewPosition = newPosition;
      }

      public override string ToString()
      {
         return $"{GetType().Name}: plr=[{Player}] from=[{OldPosition}] to=[{NewPosition}]";
      }
   }

   public class PlayerAttackEvent : HatsGameEvent
   {

   }

   public class GameOverEvent : HatsGameEvent
   {

   }
}