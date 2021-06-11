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
		private readonly BotProfileContent _botProfileContent;

		private CharacterRef _characterRef;
		private HatRef _hatRef;
		private string _alias;
		private RandomBotAIWeights _weights;

		public HatsBot(long botNumber, Random random, BattleGrid grid, BotProfileContent botProfileContent)
		{
			_random = random;
			_grid = grid;
			_botProfileContent = botProfileContent;
			dbid = -botNumber;

			_alias = _botProfileContent.names[_random.Next(_botProfileContent.names.Count)];
			_weights = _botProfileContent.weights[_random.Next(_botProfileContent.weights.Count)];
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
				new Tuple<float, HatsPlayerMoveType>(_weights.skipWeight, HatsPlayerMoveType.SKIP),
				new Tuple<float, HatsPlayerMoveType>(_weights.walkWeight, HatsPlayerMoveType.WALK),
				new Tuple<float, HatsPlayerMoveType>(_weights.arrowWeight, HatsPlayerMoveType.ARROW),
				new Tuple<float, HatsPlayerMoveType>(_weights.fireballWeight, HatsPlayerMoveType.FIREBALL),
				new Tuple<float, HatsPlayerMoveType>(_weights.shieldWeight, HatsPlayerMoveType.SHIELD),
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

			// if the tile is going to explode, move.
			if (_grid.IsInSuddenDeath(self.Position))
			{
				moveType = HatsPlayerMoveType.WALK;
			}

			// randomly pick a direction.
			var neighbors = _grid.Neighbors(self.Position);
			var direction = Direction.Nowhere;
			if (moveType == HatsPlayerMoveType.WALK)
			{
				// Don't walk into unwalkable neighbors or lava
				neighbors = neighbors.Where(neighbor => _grid.IsWalkable(neighbor) && !_grid.IsLava(neighbor) && !_grid.IsInSuddenDeath(neighbor)).ToList();
				if (!neighbors.Any())
				{
					moveType = HatsPlayerMoveType.SHIELD; // if there is nowhere to go, always shield.
				}
			}
			else if (moveType == HatsPlayerMoveType.ARROW || moveType == HatsPlayerMoveType.FIREBALL)
			{
				// Don't shoot projectiles straight into rocks
				neighbors = neighbors.Where(neighbor => !_grid.IsRock(neighbor)).ToList();
			}

			if(neighbors.Count() > 0)
			{
				var randomNeighbor = neighbors.ElementAt(_random.Next(neighbors.Count()));
				if (neighbors.Any())
				{
					direction = _grid.GetDirection(self.Position, randomNeighbor);
				}
			}

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