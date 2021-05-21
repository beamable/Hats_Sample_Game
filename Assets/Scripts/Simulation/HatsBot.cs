using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hats.Content;
using Hats.Game;

namespace Hats.Simulation
{
   [Serializable]
   public class HatsBot : HatsPlayer
   {
      private readonly Random _random;
      private readonly BattleGrid _grid;

      private CharacterRef _characterRef;
      private HatRef _hatRef;
      private string _alias;

      public HatsBot(long botNumber, Random random, BattleGrid grid)
      {
         _random = random;
         _grid = grid;
         dbid = -botNumber;
      }

      public override async Task<CharacterRef> GetSelectedCharacter()
      {
         // a bot gets a randomly assigned character...
         if (_characterRef != null) return _characterRef;

         var allCharacterRefs = await PlayerInventory.GetAllCharacterRefs();
         _characterRef = allCharacterRefs[_random.Next(allCharacterRefs.Count)];
         return _characterRef;
      }

      public override async Task<HatRef> GetSelectedHat()
      {
         // a bot gets a randomly assigned hat....
         if (_hatRef != null) return _hatRef;

         var allHatRefs = await PlayerInventory.GetAllHatRefs();
         _hatRef = allHatRefs[_random.Next(allHatRefs.Count)];
         return _hatRef;
      }

      public override async Task<string> GetPlayerAlias()
      {
         if (_alias != null) return _alias;
         var randomNames = new string[]
         {
            "Evil Fred", "Evil Steve", "Evil Frank", "Evil Dude", "Evil Evil"
         };
         _alias = randomNames[_random.Next(randomNames.Length)];
         return _alias;
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
         //    MoveType = HatsPlayerMoveType.SHIELD,
         //    Direction = Direction.Nowhere
         // };
         var weightedMoves = new Tuple<float, HatsPlayerMoveType>[]
         {
            new Tuple<float, HatsPlayerMoveType>(.5f, HatsPlayerMoveType.SKIP),
            new Tuple<float, HatsPlayerMoveType>(10, HatsPlayerMoveType.WALK),
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