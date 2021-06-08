using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hats.Simulation
{
    [Serializable]
    public class BattleGrid
    {
		private const int SanityCheck = 100;

		// TODO: Randomize start positions within quadrants
        public readonly Vector3Int[] START_POSITIONS =
        {
            new Vector3Int(-3, 4, 0),
            new Vector3Int(3, 4, 0),
            new Vector3Int(-3, -4, 0),
            new Vector3Int(3, -4, 0),
        };

		public Vector2Int Min, Max;
		public Vector2Int iceQuantityRange;
		public Vector2Int rockQuantityRange;
		public Vector2Int holeQuantityRange;
		public Vector2Int lavaQuantityRange;

		public List<Vector3Int> startTiles = new List<Vector3Int>();
		public List<Vector3Int> iceTiles = new List<Vector3Int>();
		public List<Vector3Int> rockTiles = new List<Vector3Int>();
		public List<Vector3Int> holeTiles = new List<Vector3Int>();
		public List<Vector3Int> lavaTiles = new List<Vector3Int>();

        // public BattleGrid(int xMin, int xMax, int yMin, int yMax)
        // {
        //     Min = new Vector2Int(xMin, yMin);
        //     Max = new Vector2Int(xMax, yMax);
        // }

        // Deltas relative to center in an even (long) row, clockwise from west.
        private Vector3Int[] _evenRow =
        {
            new Vector3Int(-1, 0, 0),
            new Vector3Int(-1, 1, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(-1, -1, 0)
        };

        // Deltas relative to center in an odd (short) row, clockwise from west.
        private Vector3Int[] _oddRow =
        {
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(1, 1, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(1, -1, 0),
            new Vector3Int(0, -1, 0)
        };

		// Initialize the grid by placing different types of tiles
		public void Initialize(System.Random random)
		{
			// Make sure starting tiles are correct
			startTiles = new List<Vector3Int>
			{
				new Vector3Int(Min.x, Min.y, 0),
				new Vector3Int(Max.x, Min.y, 0),
				new Vector3Int(Min.x, Max.y, 0),
				new Vector3Int(Max.x, Max.y, 0),
			};

			// Place ice tiles
			var iceCount = random.Next(iceQuantityRange.x, iceQuantityRange.y + 1);
			for (int index = 0; iceTiles.Count < iceCount && index < SanityCheck; index++)
			{
				// Can spawn anwhere but on the left and right edges of the map
				var ice = new Vector3Int(0, random.Next(Min.y, Max.y + 1), 0);
				ice.x = random.Next(Min.x + 1, Max.x + (ice.y % 2 == 0 ? 1 : 0));

				iceTiles.Add(ice);
			}

			// Place rock tiles
			var rockCount = random.Next(rockQuantityRange.x, rockQuantityRange.y + 1);
			for (int index = 0; rockTiles.Count < rockCount && index < SanityCheck; index++)
			{
				// Can spawn anywhere
				var rock = new Vector3Int(random.Next(Min.x, Max.x + 1), random.Next(Min.y, Max.y + 1), 0);

				// ... except on top of ice
				if(IsIce(rock))
				{
					continue;
				}

				// ... except on or next to start positions
				if (IsAdjacentToStartPosition(rock))
				{
					continue;
				}

				// ... except on or next to other rocks
				if (IsAdjacentToRock(rock))
				{
					continue;
				}

				rockTiles.Add(rock);
			}

			// Place hole tiles
			var holeCount = random.Next(holeQuantityRange.x, holeQuantityRange.y + 1);
			for (int index = 0; holeTiles.Count < holeCount && index < SanityCheck; index++)
			{
				// Can spawn anwhere but on the edges of the map
				var hole = new Vector3Int(0, random.Next(Min.y + 1, Max.y - 1), 0);
				hole.y = random.Next(Min.x + (hole.y % 2 == 0 ? 2 : 1), Max.x - 1);

				// ... except on top of ice
				if (IsIce(hole))
				{
					continue;
				}

				// ... except on top of rocks
				if (IsRock(hole))
				{
					continue;
				}

				// ... except on or next to start positions
				if(IsAdjacentToStartPosition(hole))
				{
					continue;
				}

				// ... except on or next to other holes
				if (IsAdjacentToHole(hole))
				{
					continue;
				}

				holeTiles.Add(hole);
			}

			// Place lava tiles
			var lavaCount = random.Next(lavaQuantityRange.x, lavaQuantityRange.y + 1);
			for (int index = 0; lavaTiles.Count < lavaCount && index < SanityCheck; index++)
			{
				// Can spawn anywhere
				var lava = new Vector3Int(random.Next(Min.x, Max.x + 1), random.Next(Min.y, Max.y + 1), 0);

				// ... except on top of ice
				if (IsIce(lava))
				{
					continue;
				}

				// ... except on top of rocks
				if (IsRock(lava))
				{
					continue;
				}

				// ... Except on top of holes
				if(IsHole(lava))
				{
					continue;
				}

				// ... except on or next to start positions
				if (IsAdjacentToStartPosition(lava))
				{
					continue;
				}

				lavaTiles.Add(lava);
			}
		}

		public bool IsCellInBounds(Vector3Int cell)
        {
            if (cell.y % 2 == 0 && cell.x < 0)
            {
                cell.x -= 1;
            }
            return cell.x >= Min.x && cell.x <= Max.x && cell.y >= Min.y && cell.y <= Max.y;
        }

        public IEnumerable<Vector3Int> Neighbors(Vector3Int origin)
        {
            var result = new List<Vector3Int>();
            var table = origin.y % 2 == 0 ? _evenRow : _oddRow;
            foreach (var delta in table)
            {
                var gridPosition = origin + delta;
                 if (IsCellInBounds(gridPosition))
                {
                    result.Add(gridPosition);
                }
            }

            return result;
        }

        // // ReSharper disable once MemberCanBePrivate.Global
        // public bool IsOnBoard(Vector3Int gridPosition)
        // {
        //     return tileMap.GetTile(gridPosition) != null;
        // }

        // // ReSharper disable once MemberCanBePrivate.Global
        // public bool OccupiedByPlayer(Vector3Int gridPosition, List<GamePlayer> players)
        // {
        //     return players.Find(player => player.gridPosition == gridPosition);
        // }

        public bool IsAdjacent(Vector3Int a, Vector3Int b)
        {
			//    A   B
			//  C   D   E
			//    F   G

			// In short rows, x=0 is to the right of the center line.
			// If D = (0, 0) then C = (-1, 0) and E = (1, 0)
			// and A = (-1, -1), B = (0, -1)
			// and F = (-1, 1), G = (0, 1)

			// True case: same value
			if (a.Equals(b)) return true;

			// False cases: too far away
			if (Mathf.Abs(b.x - a.x) > 1) return false;
            if (Mathf.Abs(b.y - a.y) > 1) return false;

            // Easy true case: same row, delta-x of one.
            if (b.y == a.y && Mathf.Abs(b.x - a.x) == 1) return true;

            // Maybe case: adjacent row, delta-x in the range [-1, 1].
            if (a.y % 2 == 0)
            {
                // Even row. In our map these are long rows.
                return b.x - a.x < 1;
            }
            else
            {
                // Odd row. Short row.
                return b.x - a.x >= 0;
            }
        }

		// Returns true if the tile at this position is a start position
		public bool IsStartPosition(Vector3Int tile)
		{
			foreach (var startTile in START_POSITIONS)
			{
				if(startTile.Equals(tile))
				{
					return true;
				}
			}
			return false;
		}

		// Returns true if the tile at this position is adjacent to a start position
		public bool IsAdjacentToStartPosition(Vector3Int tile)
		{
			foreach (var startTile in startTiles)
			{
				if (IsAdjacent(tile, startTile))
				{
					return true;
				}
			}
			return false;
		}

		// Returns true if the tile at this position is an ice tile
		public bool IsIce(Vector3Int tile)
		{
			foreach (var iceTile in iceTiles)
			{
				if(iceTile.Equals(tile))
				{
					return true;
				}
			}
			return false;
		}

		// Returns true if the tile at this position is adjacent to an ice tile
		public bool IsAdjacentToIce(Vector3Int tile)
		{
			foreach (var iceTile in iceTiles)
			{
				if (IsAdjacent(tile, iceTile))
				{
					return true;
				}
			}
			return false;
		}

		// Returns true if the tile at this position is a rock tile
		public bool IsRock(Vector3Int tile)
		{
			foreach (var rockTile in rockTiles)
			{
				if (rockTile.Equals(tile))
				{
					return true;
				}
			}
			return false;
		}

		// Returns true if the tile at this position is adjacent to a rock tile
		public bool IsAdjacentToRock(Vector3Int tile)
		{
			foreach (var rockTile in rockTiles)
			{
				if (IsAdjacent(tile, rockTile))
				{
					return true;
				}
			}
			return false;
		}

		// Returns true if the tile at this position is a hole tile
		public bool IsHole(Vector3Int tile)
		{
			foreach (var holeTile in holeTiles)
			{
				if (holeTile.Equals(tile))
				{
					return true;
				}
			}
			return false;
		}

		// Returns true if the tile at this position is adjacent to a hole tile
		public bool IsAdjacentToHole(Vector3Int tile)
		{
			foreach (var holeTile in holeTiles)
			{
				if (IsAdjacent(tile, holeTile))
				{
					return true;
				}
			}
			return false;
		}

		public bool IsLava(Vector3Int tile)
		{
			foreach (var lavaTile in lavaTiles)
			{
				if(lavaTile.Equals(tile))
				{
					return true;
				}
			}
			return false;
		}

		// Returns true if the tile at this position can be walked on
		public bool IsWalkable(Vector3Int tile)
		{
			return IsCellInBounds(tile) && !IsHole(tile) && !IsRock(tile);
		}

		public Direction GetDirection(Vector3Int origin, Vector3Int target)
        {
            if (!IsAdjacent(origin, target))
            {
                return Direction.Nowhere;
            }

            var table = origin.y % 2 == 0 ? _evenRow : _oddRow;
            var delta = target - origin;
            for (var i = 0; i < 6; i++)
            {
                var testVector = table[i];
                if (delta == testVector)
                {
                    return (Direction) i;
                }
            }

            return Direction.Nowhere;
        }

        public Vector3Int InDirection(Vector3Int gridPosition, Direction direction)
        {
            if (direction == Direction.Nowhere)
            {
                return gridPosition;
            }

            var table = gridPosition.y % 2 == 0 ? _evenRow : _oddRow;
            return gridPosition + table[(int) direction];
        }
    }
}