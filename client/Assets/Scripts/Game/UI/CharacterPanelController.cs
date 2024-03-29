using Beamable;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Api.Payments;
using Beamable.Common.Inventory;
using Beamable.Common.Shop;
using Beamable.Stats;
using Beamable.UI.Scripts;
using Hats.Content;
using Hats.Simulation;
using Hats.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPanelController : MonoBehaviour
{
	[Header("Content References")]
	public StoreRef CharacterShopRef;

	public StoreRef HatShopRef;

	[Header("Prefab References")]
	public CharacterOptionBehaviour CharacterOptionPrefab;

	public CharacterOptionBuyBehaviour CharacterOptionBuyBehaviour;
	public HatOptionBehaviour HatOptionPrefab;
	public HatOptionBuyBehaviour HatOptionBuyBehaviour;

	[Header("UI References")]
	public RectTransform CharacterOptionContainer;

	public RectTransform HatOptionContainer;
	public TextMeshProUGUI HintText;
	public TextMeshProUGUI SelectedCharacterText;
	public GameObject LoadingSpinner;
	public GameObject PreviewSpinner;
	public RawImage RenderPreview;
	public TMP_InputField AliasInputField;
	public StatBehaviour AliasStatBehaviour;
	public Button HatsButton;
	public Button CharactersButton;

	public RectTransform CharacterOptionScroller;
	public RectTransform HatOptionScroller;

	[Header("Live Preview References")]
	public RenderTexture PreviewTexture;

	public Transform PreviewPrefabContainer;

	[Header("Internals")]
	[ReadOnly]
	[SerializeField]
	private HatContent _selectedHat;

	[ReadOnly]
	[SerializeField]
	private CharacterContent _selectedCharacter;

	[ReadOnly]
	[SerializeField]
	private CharacterBehaviour _selectedPrefab;

	[ReadOnly]
	[SerializeField]
	private Vector2 _hatsOffset;

	[ReadOnly]
	[SerializeField]
	private Vector2 _charactersOffset;

	private Vector2 _hatsOffsetVel, _characterOffsetVel;

	private BeamContext _beamContext;

	public static Vector2 SizeToParent(RawImage image, float padding = 0)
	{
		var parent = image.transform.parent.GetComponent<RectTransform>();
		var imageTransform = image.GetComponent<RectTransform>();
		if (!parent) { return imageTransform.sizeDelta; } //if we don't have a parent, just return our current width;
		padding = 1 - padding;
		float w = 0, h = 0;
		float ratio = image.texture.width / (float)image.texture.height;
		var bounds = new Rect(0, 0, parent.rect.width, parent.rect.height);
		if (Mathf.RoundToInt(imageTransform.eulerAngles.z) % 180 == 90)
		{
			//Invert the bounds if the image is rotated
			bounds.size = new Vector2(bounds.height, bounds.width);
		}
		//Size by height first
		h = bounds.height * padding;
		w = h * ratio;
		if (w > bounds.width * padding)
		{ //If it doesn't fit, fallback to width;
			w = bounds.width * padding;
			h = w / ratio;
		}
		imageTransform.sizeDelta = new Vector2(w, h);
		return imageTransform.sizeDelta;
	}

	public async Task PopulateCharacterShop()
	{
		_beamContext = BeamContext.Default;
		await _beamContext.OnReady;
		var shop = await _beamContext.Api.CommerceService.GetCurrent(CharacterShopRef.Id);
		var playerCharacters = await PlayerInventory.GetAvailableCharacters();
		foreach (var listing in shop.listings)
		{
			// filter for listings that only contain one character item...
			var hasOneItem = listing.offer.obtainItems.Count == 1;
			var hasAnyCurrency = listing.offer.obtainCurrency.Count > 0;

			if (!hasOneItem || hasAnyCurrency)
			{
				continue; // ignore any listings that aren't just a single item...
			}

			var itemContentId = listing.offer.obtainItems[0].contentId;
			var isCharacter = itemContentId.StartsWith("items.character");
			if (!isCharacter) continue; // only show character listings.

			var hasCharacterAlready = playerCharacters.Any(character => character.Id.Equals(itemContentId));
			if (hasCharacterAlready) continue; // skip this listing because the player already owns the take

			var isCurrencyListing = listing.offer.price.type.Equals("currency");
			if (!isCurrencyListing) continue; // only show listings that can be bought for soft-currency. A listing of type sku needs to be bought differently

			var currencyRef = new CurrencyRef(listing.offer.price.symbol);
			var currencyAmount = await _beamContext.Api.InventoryService.GetCurrency(currencyRef);
			var canAfford = listing.offer.price.IsFree || currencyAmount >= listing.offer.price.amount;

			// add this listing to the character selection options...
			var instance = Instantiate(CharacterOptionBuyBehaviour, CharacterOptionContainer);
			var characterRef = new CharacterRef(listing.offer.obtainItems[0].contentId);
			var character = await characterRef.Resolve();
			var currencyContent = await currencyRef.Resolve();
			var currencyIcon = await currencyContent.icon.LoadSprite();
			instance.SetOption(character, listing, currencyRef, currencyIcon, canAfford);
			instance.CharacterOptionBehaviour.OnSelected.AddListener(() =>
			{
				var _ = TryBuyCharacter(instance, character, listing, canAfford);
			});
		}
	}

	public async Task PopulateHatShop()
	{
		_beamContext = BeamContext.Default;
		await _beamContext.OnReady;
		var shop = await _beamContext.Api.CommerceService.GetCurrent(HatShopRef.Id);
		var playerHats = await PlayerInventory.GetAvailableHats();
		foreach (var listing in shop.listings)
		{
			// filter for listings that only contain one character item...
			var hasOneItem = listing.offer.obtainItems.Count == 1;
			var hasAnyCurrency = listing.offer.obtainCurrency.Count > 0;

			if (!hasOneItem || hasAnyCurrency)
			{
				continue; // ignore any listings that aren't just a single item...
			}

			var itemContentId = listing.offer.obtainItems[0].contentId;
			var isHat = itemContentId.StartsWith("items.hat");
			if (!isHat) continue; // only show character listings.

			var hasHatAlready = playerHats.Any(hat => hat.Id.Equals(itemContentId));
			if (hasHatAlready) continue; // skip this listing because the player already owns the take

			var isCurrencyListing = listing.offer.price.type.Equals("currency");
			if (!isCurrencyListing) continue; // only show listings that can be bought for soft-currency. A listing of type sku needs to be bought differently

			var currencyRef = new CurrencyRef(listing.offer.price.symbol);
			var currencyAmount = await _beamContext.Api.InventoryService.GetCurrency(currencyRef);
			var canAfford = listing.offer.price.IsFree || currencyAmount >= listing.offer.price.amount;

			// add this listing to the character selection options...
			var instance = Instantiate(HatOptionBuyBehaviour, HatOptionContainer);
			var hatRef = new HatRef(listing.offer.obtainItems[0].contentId);
			var hat = await hatRef.Resolve();
			var currencyContent = await currencyRef.Resolve();
			var currencyIcon = await currencyContent.icon.LoadSprite();
			instance.SetOption(hat, listing, currencyRef, currencyIcon, canAfford);
			instance.HatOptionBehaviour.OnSelected.AddListener(() =>
			{
				var _ = TryBuyHat(instance, hat, listing, canAfford);
			});
		}
	}

	public async Task TryBuyCharacter(CharacterOptionBuyBehaviour characterBuyOption, CharacterContent character, PlayerListingView listing, bool canAfford)
	{
		_beamContext = BeamContext.Default;
		await _beamContext.OnReady;
		if (!canAfford) return; // TODO: Show option to buy more soft-currency...

		await _beamContext.Api.CommerceService.Purchase(CharacterShopRef.Id, listing.symbol);

		// select the thing...
		characterBuyOption.CompletePurchase();
		characterBuyOption.CharacterOptionBehaviour.OnSelected.RemoveAllListeners();
		characterBuyOption.CharacterOptionBehaviour.OnSelected.AddListener(() =>
		{
			SelectCharacter(characterBuyOption.CharacterOptionBehaviour);
		});
		SelectCharacter(characterBuyOption.CharacterOptionBehaviour);
	}

	public async Task TryBuyHat(HatOptionBuyBehaviour hatBuyOption, HatContent hat, PlayerListingView listing, bool canAfford)
	{
		_beamContext = BeamContext.Default;
		if (!canAfford) return; // TODO: Show option to buy more soft-currency...

		await _beamContext.Api.CommerceService.Purchase(HatShopRef.Id, listing.symbol);

		// select the thing...
		hatBuyOption.CompletePurchase();
		hatBuyOption.HatOptionBehaviour.OnSelected.RemoveAllListeners();
		hatBuyOption.HatOptionBehaviour.OnSelected.AddListener(() =>
		{
			SelectHat(hatBuyOption.HatOptionBehaviour);
		});
		SelectHat(hatBuyOption.HatOptionBehaviour);
	}

	public void ShowCharacters()
	{
		HintText.text = "Select your Character";
		HatsButton.gameObject.SetActive(true);
		CharactersButton.gameObject.SetActive(false);
		_hatsOffset = new Vector2(Screen.width * 2, HatOptionScroller.anchoredPosition.y);
		_charactersOffset = new Vector2(0, HatOptionScroller.anchoredPosition.y);
	}

	public void ShowHats()
	{
		HintText.text = "Select your Hat";
		HatsButton.gameObject.SetActive(false);
		CharactersButton.gameObject.SetActive(true);
		_charactersOffset = new Vector2(-Screen.width * 2, HatOptionScroller.anchoredPosition.y);
		_hatsOffset = new Vector2(0, HatOptionScroller.anchoredPosition.y);
	}

	public async void SelectCharacter(CharacterOptionBehaviour option)
	{
		_selectedCharacter = option.Character;
		var setTask = PlayerInventory.SetSelectedCharacter(option.Character);
		_selectedPrefab = await option.Character.Prefab.SafeResolve();
		ShowPreview(_selectedCharacter, _selectedPrefab, _selectedHat);
		await setTask;
	}

	public async void SelectHat(HatOptionBehaviour option)
	{
		_selectedHat = option.Hat;
		var setTask = PlayerInventory.SetSelectedHat(option.Hat);
		ShowPreview(_selectedCharacter, _selectedPrefab, _selectedHat);
		await setTask;
	}

	// Start is called before the first frame update
	private async void Start()
	{
		ClearOptions();
		ClearPreview();
		ShowCharacters();

		HatsButton.onClick.AddListener(ShowHats);
		CharactersButton.onClick.AddListener(ShowCharacters);

		LoadingSpinner.SetActive(true);
		PreviewSpinner.SetActive(true);
		HintText.text = "Fetching Content ...";

		AliasInputField.gameObject.SetActive(false);
		AliasStatBehaviour.OnStatReceived.AddListener(alias => AliasInputField.SetTextWithoutNotify(alias));
		AliasInputField.onEndEdit.AddListener(alias => AliasStatBehaviour.Write(alias));

		SelectedCharacterText.text = "Loading";
		_selectedCharacter = await PlayerInventory.GetSelectedCharacter();
		_selectedPrefab = await _selectedCharacter.Prefab.SafeResolve();
		_selectedHat = await PlayerInventory.GetSelectedHat();
		ShowPreview(_selectedCharacter, _selectedPrefab, _selectedHat);
		PreviewSpinner.SetActive(false);

		await AliasStatBehaviour.Read();
		AliasInputField.gameObject.SetActive(true);

		// load up the player's inventory, and show all the available characters...
		var characters = await PlayerInventory.GetAvailableCharacters();
		// load up the player's inventory, and show all the available hats....
		var hats = await PlayerInventory.GetAvailableHats();

		CharacterOptionScroller.gameObject.SetActive(false);
		HatOptionScroller.gameObject.SetActive(false);

		foreach (var character in characters)
		{
			var instance = Instantiate(CharacterOptionPrefab, CharacterOptionContainer);
			instance.SetOption(character);
			instance.OnSelected.AddListener(() =>
			{
				SelectCharacter(instance);
			});
		}
		await PopulateCharacterShop();

		foreach (var hat in hats)
		{
			var instance = Instantiate(HatOptionPrefab, HatOptionContainer);
			instance.SetOption(hat);
			instance.OnSelected.AddListener(() =>
			{
				SelectHat(instance);
			});
		}

		await PopulateHatShop();

		LoadingSpinner.SetActive(false);
		CharacterOptionScroller.gameObject.SetActive(true);
		HatOptionScroller.gameObject.SetActive(true);

		ShowCharacters();
	}

	// Update is called once per frame
	private void Update()
	{
		RenderPreview.color = Color.white;
		RenderPreview.texture = PreviewTexture;

		//-sizeDelta = -100,-50
		if (RenderPreview.rectTransform.rect.size.y > 0)
		{
			var aspect = RenderPreview.rectTransform.rect.size.x / (float)RenderPreview.rectTransform.rect.size.y;

			var uvWidth = aspect;
			var uvX = (uvWidth - 1) / -2;
			RenderPreview.uvRect = new Rect(uvX, 0, uvWidth, 1);

			// as the screen's aspect gets larger (w / h), we need to scale down the uv rect width
		}

		HatOptionScroller.anchoredPosition =
			 Vector2.SmoothDamp(HatOptionScroller.anchoredPosition, _hatsOffset, ref _hatsOffsetVel, .1f);
		CharacterOptionScroller.anchoredPosition =
			 Vector2.SmoothDamp(CharacterOptionScroller.anchoredPosition, _charactersOffset, ref _characterOffsetVel, .1f);
	}

	private void ClearOptions()
	{
		for (var i = 0; i < CharacterOptionContainer.childCount; i++)
		{
			Destroy(CharacterOptionContainer.GetChild(i).gameObject);
		}

		for (var i = 0; i < HatOptionContainer.childCount; i++)
		{
			Destroy(HatOptionContainer.GetChild(i).gameObject);
		}
	}

	private void ClearPreview()
	{
		for (var i = 0; i < PreviewPrefabContainer.childCount; i++)
		{
			Destroy(PreviewPrefabContainer.GetChild(i).gameObject);
		}
	}

	private void ShowPreview(CharacterContent character, CharacterBehaviour prefab, HatContent selectedHat)
	{
		ClearPreview();
		var characterBehaviour = Instantiate(prefab, PreviewPrefabContainer);
		var hatTask = characterBehaviour.SetHat(selectedHat);
		SelectedCharacterText.text = character.Display;
	}
}