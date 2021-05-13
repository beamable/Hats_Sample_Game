namespace HatsCore
{
   public class HatsPlayerMove
   {
      public long Dbid;
      public int TurnNumber;
      public HatsPlayerMoveType MoveType;
      public Direction Direction;

      public bool IsShieldMove => MoveType == HatsPlayerMoveType.SHIELD;
      public bool IsWalkMove => MoveType == HatsPlayerMoveType.WALK;

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