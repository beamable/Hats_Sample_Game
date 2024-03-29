using Beamable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hats.Content;
using Hats.Game;
using UnityEngine;

namespace Hats.Simulation
{
	[Serializable]
	public class HatsPlayer
	{
		public long dbid;

		private BeamContext _beamContext;

		public async Task PurgeBeamableStatsCache()
		{
			_beamContext = BeamContext.Default;
			await _beamContext.OnReady;
			const string PublicPlayerClientPrefix = "client.public.player";
			_beamContext.Api.StatsService.GetCache(PublicPlayerClientPrefix).Remove(dbid);
		}

		public virtual async Task<CharacterRef> GetSelectedCharacter()
		{
			return await PlayerInventory.GetSelectedCharacterRef(dbid);
		}

		public virtual async Task<HatRef> GetSelectedHat()
		{
			return await PlayerInventory.GetSelectedHatRef(dbid);
		}

		public virtual async Task<string> GetPlayerAlias()
		{
			_beamContext = BeamContext.Default;
			await _beamContext.OnReady;
			var stats = await _beamContext.Api.StatsService.GetStats("client", "public", "player", dbid);
			if (!stats.TryGetValue("alias", out var alias))
			{
				alias = "Anonymous";
			}

			return alias;
		}

		public override string ToString()
		{
			return $"{dbid}";
		}

		public override bool Equals(object obj)
		{
			if (obj is HatsPlayer other)
			{
				return Equals(other);
			}

			return false;
		}

		public override int GetHashCode()
		{
			return dbid.GetHashCode();
		}

		protected bool Equals(HatsPlayer other)
		{
			return dbid == other.dbid;
		}
	}

	public class HatsPlayerState
	{
		public Vector3Int Position;
		public bool IsDead;
		public bool IsShield;
		public List<HatsPowerup> Powerups = new List<HatsPowerup>();

		public bool HasFirewallPowerup => Powerups.Exists(p => p.Type == HatsPowerupType.FIREWALL);
		public bool HasTeleportPowerup => Powerups.Exists(p => p.Type == HatsPowerupType.TELEPORT);

		public HatsPlayerState Clone()
		{
			return new HatsPlayerState
			{
				Position = Position,
				IsDead = IsDead,
				IsShield = IsShield,
				Powerups = Powerups.Select(p => p.Clone()).ToList(),
			};
		}
	}
}