using System.Collections.Generic;
using Beamable.Experimental.Api.Sim;
using UnityEngine;

namespace Hats.Simulation
{
	public class HatsGameEvent
	{
	}

	public class CollectablePowerupSpawnEvent : HatsGameEvent
	{
		public HatsPowerupType PowerupType;
		public Vector3Int Position;

		public CollectablePowerupSpawnEvent(HatsPowerupType type, Vector3Int pos)
		{
			PowerupType = type;
			Position = pos;
		}
	}

	public class CollectablePowerupDestroyEvent : HatsGameEvent
	{
		public Vector3Int Position;

		public CollectablePowerupDestroyEvent(Vector3Int pos)
		{
			Position = pos;
		}
	}

	public class PowerupCollectEvent : HatsGameEvent
	{
		public HatsPowerup Powerup;
		public HatsPlayer Player;

		public PowerupCollectEvent(HatsPowerup powerup, HatsPlayer player)
		{
			Powerup = powerup;
			Player = player;
		}
	}

	public class PowerupRemoveEvent : HatsGameEvent
	{
		public HatsPowerup Powerup;
		public HatsPlayer Player;

		public PowerupRemoveEvent(HatsPowerup powerup, HatsPlayer player)
		{
			Powerup = powerup;
			Player = player;
		}
	}

	public class PlayerSpawnEvent : HatsGameEvent
	{
		public HatsPlayer Player;
		public Vector3Int Position;

		public PlayerSpawnEvent(HatsPlayer player, Vector3Int position)
		{
			Player = player;
			Position = position;
		}

		public override string ToString()
		{
			return $"{nameof(PlayerSpawnEvent)}: plr=[{Player}] position=[{Position}]";
		}
	}

	public class PlayerCommittedMoveEvent : HatsGameEvent
	{
	}

	public class TickEvent : HatsGameEvent
	{
		public long FrameNumber;

		public TickEvent(long frameNumber)
		{
			FrameNumber = frameNumber;
		}

		public override string ToString()
		{
			return $"{nameof(PlayerSpawnEvent)}: frameNumber=[{FrameNumber}]";
		}
	}

	public class TurnReadyEvent : HatsGameEvent { }

	public class TurnOverEvent : HatsGameEvent { }

	public class PlayerShieldEvent : HatsGameEvent
	{
		public HatsPlayer Player;

		public PlayerShieldEvent(HatsPlayer player)
		{
			Player = player;
		}

		public override string ToString()
		{
			return $"{GetType().Name}: plr=[{Player}]";
		}
	}

	public class PlayerMoveEvent : HatsGameEvent
	{
		public HatsPlayer Player;
		public Vector3Int OldPosition;
		public Vector3Int NewPosition;

		public PlayerMoveEvent(HatsPlayer player, Vector3Int oldPosition, Vector3Int newPosition)
		{
			Player = player;
			OldPosition = oldPosition;
			NewPosition = newPosition;
		}

		public override string ToString()
		{
			return $"{GetType().Name}: plr=[{Player}] from=[{OldPosition}] to=[{NewPosition}]";
		}
	}

	public class PlayerProjectileAttackEvent : HatsGameEvent
	{
		public HatsPlayer Player;

		public Vector3Int StartPosition;

		public Direction Direction;

		public Vector3Int? BounceAt;

		public Direction? BounceDirection;

		public Vector3Int? DestroyAt;

		public HatsPlayer KillsPlayer;

		public AttackType Type;

		public enum AttackType
		{
			FIREBALL,
			ARROW
		}

		public PlayerProjectileAttackEvent(HatsPlayer player, AttackType type, Vector3Int startPosition, Direction direction, Vector3Int? bounceAt = null, Direction? bounceDirection = null, Vector3Int? destroyAt = null, HatsPlayer killsPlayer = null)
		{
			Type = type;
			Player = player;
			StartPosition = startPosition;
			Direction = direction;
			BounceAt = bounceAt;
			BounceDirection = bounceDirection;
			DestroyAt = destroyAt;
			KillsPlayer = killsPlayer;
		}

		public override string ToString()
		{
			return $"{GetType().Name}: plr=[{Player}] dir=[{Direction}] bounceAt=[{BounceAt}] bounceDir=[{BounceDirection}] destroyAt=[{DestroyAt}] kills=[{KillsPlayer}]";
		}
	}

	public class PlayerKilledEvent : HatsGameEvent
	{
		public HatsPlayer Victim;
		public HatsPlayer Murderer;

		public PlayerKilledEvent(HatsPlayer victim, HatsPlayer murderer)
		{
			Victim = victim;
			Murderer = murderer;
		}

		public override string ToString()
		{
			return $"{GetType().Name}: victim=[{Victim}] murderer=[{Murderer}]";
		}
	}

	public class PlayerLeftEvent : HatsGameEvent
	{
		public HatsPlayer Quitter;

		public PlayerLeftEvent(HatsPlayer rageQuitter)
		{
			Quitter = rageQuitter;
		}

		public override string ToString()
		{
			return $"{GetType().Name}: quitter=[{Quitter}]";
		}
	}

	public class PlayerRespawnEvent : HatsGameEvent
	{
		public HatsPlayer Player;
		public Vector3Int Position;

		public PlayerRespawnEvent(HatsPlayer player, Vector3Int position)
		{
			Player = player;
			Position = position;
		}

		public override string ToString()
		{
			return $"{GetType().Name}: player=[{Player}] position=[{Position}]";
		}
	}

	public class SuddenDeathEvent : HatsGameEvent
	{
		public Vector3Int Position;

		public SuddenDeathEvent(Vector3Int position)
		{
			this.Position = position;
		}

		public override string ToString()
		{
			return $"{GetType().Name}: position=[{Position}]";
		}
	}

	public class GameOverEvent : HatsGameEvent
	{
		public HatsPlayer Winner;
		public List<PlayerResult> Results;

		public GameOverEvent(HatsPlayer winner, List<PlayerResult> results)
		{
			Winner = winner;
			Results = results;
		}

		public override string ToString()
		{
			return $"{GetType().Name}: winner=[{Winner}]";
		}
	}
}