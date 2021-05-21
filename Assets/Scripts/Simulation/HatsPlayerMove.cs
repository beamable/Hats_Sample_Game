namespace Hats.Simulation
{
   public class HatsGameMessage
   {

   }

   public class HatsTickMessage : HatsGameMessage
   {
      public long FrameNumber;
   }

   public class HatsPlayerMove : HatsGameMessage
   {
      public long Dbid;
      public int TurnNumber;
      public HatsPlayerMoveType MoveType;
      public Direction Direction;

      public bool IsShieldMove => MoveType == HatsPlayerMoveType.SHIELD;
      public bool IsWalkMove => MoveType == HatsPlayerMoveType.WALK;
      public bool IsFireballMove => MoveType == HatsPlayerMoveType.FIREBALL;
      public bool IsArrowMove => MoveType == HatsPlayerMoveType.ARROW;

      public override string ToString()
      {
         return $"move for {Dbid} for turn {TurnNumber}. {MoveType} towards {Direction}";
      }
   }

   public enum HatsPlayerMoveType
   {
      SKIP,
      WALK,
      SHIELD,
      FIREBALL,
      ARROW
   }

   public enum Direction
   {
      Nowhere = -1,
      West = 0,
      Northwest,
      Northeast,
      East,
      Southeast,
      Southwest,
   }
}