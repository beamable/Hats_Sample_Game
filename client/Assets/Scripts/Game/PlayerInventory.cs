using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Common.Content;
using Hats.Content;

namespace Hats.Game
{
	public static class PlayerInventory
	{
		public const string SELECTED_CHARACTER_STAT = "character";
		public const string SELECTED_HAT_STAT = "hat";

		public static async Task<List<CharacterRef>> GetAllCharacterRefs()
		{
			var beamable = await Beamable.API.Instance;
			var manifest = await beamable.ContentService.GetManifest(new ContentQuery
			{
				TypeConstraints = new HashSet<Type> { typeof(CharacterContent) }
			});
			return manifest.entries
				.Select(entry => new CharacterRef(entry.contentId))
				.ToList();
		}

		public static async Task<List<HatRef>> GetAllHatRefs()
		{
			var beamable = await Beamable.API.Instance;
			var manifest = await beamable.ContentService.GetManifest(new ContentQuery
			{
				TypeConstraints = new HashSet<Type> { typeof(HatContent) }
			});
			return manifest.entries
				.Select(entry => new HatRef(entry.contentId))
				.ToList();
		}

		public static async Task<List<CharacterContent>> GetAvailableCharacters()
		{
			var beamable = await Beamable.API.Instance;
			var characters = await beamable.InventoryService.GetItems<CharacterContent>();

			// all players should start with the goon
			var goonReference = new CharacterRef("items.character.goon");
			var hasGoon = characters.Exists(character => character.ItemContent.Id.Equals(goonReference.Id));
			if (!hasGoon)
			{
				await beamable.InventoryService.AddItem(goonReference.Id);
				return await GetAvailableCharacters();
			}

			return characters.Select(item => item.ItemContent).ToList();
		}

		public static async Task<List<HatContent>> GetAvailableHats()
		{
			var beamable = await Beamable.API.Instance;
			var hats = await beamable.InventoryService.GetItems<HatContent>();
			var helmetReference = new HatRef("items.hat.helmet");
			var hasHelmet = hats.Exists(hat => hat.ItemContent.Id.Equals(helmetReference.Id));
			if (!hasHelmet)
			{
				await beamable.InventoryService.AddItem(helmetReference.Id);
				return await GetAvailableHats();
			}

			return hats.Select(item => item.ItemContent).ToList();
		}

		public static async Task SetSelectedCharacter(CharacterContent character)
		{
			// check that the content is in the player's inventory.
			var availableCharacters = await GetAvailableCharacters();
			var isAvailable = availableCharacters.Exists(availableCharacter => availableCharacter.Id.Equals(character.Id));
			if (!isAvailable) return;

			var beamable = await Beamable.API.Instance;
			var _ = beamable.StatsService.SetStats("public", new Dictionary<string, string>
			{
				{SELECTED_CHARACTER_STAT, character.Id}
			});
		}

		public static async Task SetSelectedHat(HatContent hat)
		{
			var availableHats = await GetAvailableHats();
			var isAvailable = availableHats.Exists(availableHat => availableHat.Id.Equals(hat.Id));
			if (!isAvailable) return;
			var beamable = await Beamable.API.Instance;
			var _ = beamable.StatsService.SetStats("public", new Dictionary<string, string>
			{
				{SELECTED_HAT_STAT, hat.Id}
			});
		}

		public static async Task<CharacterContent> GetSelectedCharacter(long? dbid = null)
		{
			var reference = await GetSelectedCharacterRef(dbid);
			var content = await reference.Resolve();
			return content;
		}

		public static async Task<HatContent> GetSelectedHat(long? dbid = null)
		{
			var reference = await GetSelectedHatRef(dbid);
			var content = await reference.Resolve();
			return content;
		}

		public static async Task<CharacterRef> GetSelectedCharacterRef(long? dbid = null)
		{
			var beamable = await Beamable.API.Instance;
			dbid ??= beamable.User.id;

			var stats = await beamable.StatsService.GetStats("client", "public", "player", dbid.Value);
			if (!stats.TryGetValue(SELECTED_CHARACTER_STAT, out var characterId))
			{
				characterId = "items.character.goon"; // default to goon.
			}

			return new CharacterRef(characterId);
		}

		public static async Task<HatRef> GetSelectedHatRef(long? dbid = null)
		{
			var beamable = await Beamable.API.Instance;
			dbid ??= beamable.User.id;
			var stats = await beamable.StatsService.GetStats("client", "public", "player", dbid.Value);
			if (!stats.TryGetValue(SELECTED_HAT_STAT, out var hatId))
			{
				hatId = "items.hat.helmet"; // default to helmet.
			}

			return new HatRef(hatId);
		}
	}
}