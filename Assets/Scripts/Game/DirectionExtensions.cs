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
			switch (direction)
			{
				case Direction.West:
					return Quaternion.Euler(0, 0, 180);
				case Direction.Northwest:
					return Quaternion.Euler(0, 0, 120);
				case Direction.Northeast:
					return Quaternion.Euler(0, 0, 60);
				case Direction.East:
					return Quaternion.identity; // (0, 0, 0)
				case Direction.Southeast:
					return Quaternion.Euler(0, 0, -60);
				case Direction.Southwest:
					return Quaternion.Euler(0, 0, -120);
				default:
					return Quaternion.identity;
			}
		}

		public static Direction GetDirection(Vector3Int start, Vector3 end)
		{
			if(start == end)
			{
				return Direction.Nowhere;
			}

			var offset = end - start;
			if(start.y % 2 == 0)
			{
				// Starting on an even row shifts the offset one to the right
				offset.x += 1;
			}

			// Determine the direction
			if (offset.x > 0)
			{
				// East
				if (offset.y > 0)
				{
					return Direction.Northeast;
				}
				else if (offset.y < 0)
				{
					return Direction.Southeast;
				}
				return Direction.East;
			}
			else
			{
				// West
				if (offset.y > 0)
				{
					return Direction.Northwest;
				}
				else if (offset.y < 0)
				{
					return Direction.Southwest;
				}
				return Direction.West;
			}
		}
	}
}