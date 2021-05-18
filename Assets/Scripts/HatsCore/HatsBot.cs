using System;
using System.Collections.Generic;
using System.Linq;

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
         // TODO: Add better AI. This one picks random moves...

         var self = dbidToState[dbid];
         if (self.IsDead) return Skip(turnNumber); // BOT AI GHOSTS DON'T MOVE YET.
         // return new HatsPlayerMove
         // {
         //    Dbid = dbid,
         //    TurnNumber = turnNumber,
         //    MoveType = HatsPlayerMoveType.WALK,
         //    Direction = Direction.West
         // };
         var weightedMoves = new Tuple<float, HatsPlayerMoveType>[]
         {
            new Tuple<float, HatsPlayerMoveType>(.5f, HatsPlayerMoveType.SKIP),
            new Tuple<float, HatsPlayerMoveType>(2, HatsPlayerMoveType.WALK),
            new Tuple<float, HatsPlayerMoveType>(1, HatsPlayerMoveType.ARROW),
            new Tuple<float, HatsPlayerMoveType>(1, HatsPlayerMoveType.FIREBALL),
            new Tuple<float, HatsPlayerMoveType>(1, HatsPlayerMoveType.SHIELD),
         };

         var weightSum = weightedMoves.Select(kvp => kvp.Item1).Sum();

         // randomly pick an action...
         var moveRandomNumber = _random.NextDouble();
         var moveType = HatsPlayerMoveType.SKIP;
         var randomStart = 0f;
         foreach (var kvp in weightedMoves)
         {
            var randomEnd = randomStart + (kvp.Item1 / weightSum);
            if (moveRandomNumber >= randomStart && moveRandomNumber < randomEnd)
            {
               moveType = kvp.Item2;
               break;
            }
            randomStart = randomEnd;
         }

         // randomly pick a direction.
         var neighbors = _grid.Neighbors(self.Position).ToList();
         var randomNeighbor = neighbors[_random.Next(neighbors.Count)];
         var direction = _grid.GetDirection(self.Position, randomNeighbor);

         return new HatsPlayerMove
         {
            Dbid = dbid,
            TurnNumber = turnNumber,
            MoveType = moveType,
            Direction = direction
         };
      }

      HatsPlayerMove Skip(int turnNumber)
      {
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