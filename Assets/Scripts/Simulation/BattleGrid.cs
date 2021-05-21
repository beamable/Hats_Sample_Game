using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hats.Simulation
{
    [Serializable]
    public class BattleGrid
    {
        public readonly Vector3Int[] START_POSITIONS =
        {
            new Vector3Int(-3, 4, 0),
            new Vector3Int(3, 4, 0),
            new Vector3Int(-3, -4, 0),
            new Vector3Int(3, -4, 0),
        };

        public Vector2Int Min, Max;

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


        public bool IsCellInBounds(Vector3Int cell)
        {
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

            // False cases: too far away, or my exact hex.
            if (Mathf.Abs(b.x - a.x) > 1) return false;
            if (Mathf.Abs(b.y - a.y) > 1) return false;
            if (b.x == a.x && b.y == a.y) return false;

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