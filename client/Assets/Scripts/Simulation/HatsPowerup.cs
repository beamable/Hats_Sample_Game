using UnityEngine;

namespace Hats.Simulation
{
	public enum HatsPowerupType
	{
		INVALID_TYPE = 0,
		FIREWALL,
		TELEPORT,
	}

	public class HatsPowerup
	{
		public HatsPowerupType Type = HatsPowerupType.INVALID_TYPE;
		public int ObtainedInTurnNumber = -1;
		public Vector3Int Position = Vector3Int.zero;
		public int TimeoutInTurns = -1;

		public HatsPowerup Clone()
		{
			return new HatsPowerup()
			{
				Type = Type,
				ObtainedInTurnNumber = ObtainedInTurnNumber,
				TimeoutInTurns = TimeoutInTurns,
			};
		}
	}
}