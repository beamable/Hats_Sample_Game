using Hats.Simulation;
using UnityEngine;

namespace Hats.Game
{
   public static class DirectionExtensions
   {
      public static Direction Reverse(this Direction direction)
      {
         var reversed = Direction.Nowhere;
         switch (direction)
         {
            case Direction.West:
               reversed = Direction.East;
               break;
            case Direction.Northwest:
               reversed = Direction.Southeast;
               break;
            case Direction.Northeast:
               reversed = Direction.Southwest;
               break;
            case Direction.East:
               reversed = Direction.West;
               break;
            case Direction.Southeast:
               reversed = Direction.Northwest;
               break;
            case Direction.Southwest:
               reversed = Direction.Northeast;
               break;
            case Direction.Nowhere:
               reversed = Direction.Nowhere;
               break;
         }

         return reversed;
      }

      public static Quaternion GetRotation(this Direction direction)
      {
         var origin = 0;
         var rotation = Quaternion.Euler(0, 0, 0);
         switch (direction)
         {
            case Direction.West:
               rotation = Quaternion.Euler(0, 0, origin + 180);
               break;
            case Direction.Northwest:
               rotation = Quaternion.Euler(0, 0, origin + 120);
               break;
            case Direction.Northeast:
               rotation = Quaternion.Euler(0, 0, origin + 60);
               break;
            case Direction.East:
               rotation = Quaternion.Euler(0, 0, origin + 0);
               break;
            case Direction.Southeast:
               rotation = Quaternion.Euler(0, 0, origin -60);
               break;
            case Direction.Southwest:
               rotation = Quaternion.Euler(0, 0, origin -120);
               break;
            case Direction.Nowhere:
               rotation = Quaternion.Euler(0, 0,origin + 0);
               break;
         }

         return rotation;
      }
   }
}