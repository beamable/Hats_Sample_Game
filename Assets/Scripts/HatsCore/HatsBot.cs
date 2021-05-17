using System;
using System.Collections.Generic;

namespace HatsCore
{
   [Serializable]
   public class HatsBot : HatsPlayer
   {
      private readonly Random _random;
      private readonly BattleGrid _grid;

      public HatsBot(long botNumber, Random random, BattleGrid grid)
      {
         _random = random;
         _grid = grid;
         dbid = -botNumber;
      }

      public HatsPlayerMove PerformMove(int turnNumber, Dictionary<long, HatsPlayerState> dbidToState)
      {
         // TODO: Add better AI. This one just always skips or throws shield...

         var shouldShield = (_random.NextDouble() > .005);

         if (shouldShield)
         {
            return new HatsPlayerMove
            {
               Dbid = dbid,
               TurnNumber = turnNumber,
               MoveType = HatsPlayerMoveType.SHIELD,
               Direction = Direction.Nowhere
            };
         }

         return new HatsPlayerMove
         {
            Dbid = dbid,
            TurnNumber = turnNumber,
            MoveType = HatsPlayerMoveType.SKIP,
            Direction = Direction.Nowhere
         };
      }
   }
}