using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Api.Payments;
using Beamable.Common.Inventory;
using Beamable.Common.Shop;
using Beamable.Stats;
using Beamable.UI.Scripts;
using HatsContent;
using HatsCore;
using HatsUnity;
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
    // Start is called before the first frame update
    async void Start()
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

        HintText.text = "Select your Characters";

    }

    // Update is called once per frame
    void Update()
    {
        RenderPreview.color = Color.white;
        RenderPreview.texture = PreviewTexture;

        HatOptionScroller.anchoredPosition =
            Vector2.SmoothDamp(HatOptionScroller.anchoredPosition, _hatsOffset, ref _hatsOffsetVel, .1f);
        CharacterOptionScroller.anchoredPosition =
            Vector2.SmoothDamp(CharacterOptionScroller.anchoredPosition, _charactersOffset, ref _characterOffsetVel, .1f);
    }

    public async Task PopulateCharacterShop()
    {
        var beamable = await Beamable.API.Instance;
        var shop = await beamable.CommerceService.GetCurrent(CharacterShopRef.Id);
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
            var currencyAmount = await beamable.InventoryService.GetCurrency(currencyRef);
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
        var beamable = await Beamable.API.Instance;
        var shop = await beamable.CommerceService.GetCurrent(HatShopRef.Id);
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
            var currencyAmount = await beamable.InventoryService.GetCurrency(currencyRef);
            var canAfford = listing.offer.price.IsFree || currencyAmount >= listing.offer.price.amount;

            // add this listing to the character selection options...
            var instance = Instantiate(HatOptionBuyBehaviour, HatOptionContainer);
            var hatRef = new HatRef(listing.offer.obtainItems[0].contentId);
            var hat = await hatRef.Resolve();
            var currencyContent = await currencyRef.Resolve();
            var currencyIcon = await currencyContent.icon.LoadSprite();
            instance.SetOption(hat, listing, currencyIcon, canAfford);
            instance.HatOptionBehaviour.OnSelected.AddListener(() =>
            {
                var _ = TryBuyHat(instance, hat, listing, canAfford);
            });
        }
    }

    public async Task TryBuyCharacter(CharacterOptionBuyBehaviour characterBuyOption, CharacterContent character, PlayerListingView listing, bool canAfford)
    {
        var beamable = await Beamable.API.Instance;
        if (!canAfford) return; // TODO: Show option to buy more soft-currency...

        await beamable.CommerceService.Purchase(CharacterShopRef.Id, listing.symbol);

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
        var beamable = await Beamable.API.Instance;
        if (!canAfford) return; // TODO: Show option to buy more soft-currency...

        await beamable.CommerceService.Purchase(HatShopRef.Id, listing.symbol);

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
        HatsButton.gameObject.SetActive(true);
        CharactersButton.gameObject.SetActive(false);
        _hatsOffset = new Vector2(Screen.width, HatOptionScroller.anchoredPosition.y);
        _charactersOffset = new Vector2(0, HatOptionScroller.anchoredPosition.y);
    }

    public void ShowHats()
    {
        HatsButton.gameObject.SetActive(false);
        CharactersButton.gameObject.SetActive(true);
        _charactersOffset = new Vector2(-Screen.width, HatOptionScroller.anchoredPosition.y);
        _hatsOffset = new Vector2(0, HatOptionScroller.anchoredPosition.y);
    }

    void ClearOptions()
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

    void ClearPreview()
    {
        for (var i = 0; i < PreviewPrefabContainer.childCount; i++)
        {
            Destroy(PreviewPrefabContainer.GetChild(i).gameObject);
        }
    }

    void ShowPreview(CharacterContent character, CharacterBehaviour prefab, HatContent selectedHat)
    {
        ClearPreview();
        var characterBehaviour = Instantiate(prefab, PreviewPrefabContainer);
        var hatTask = characterBehaviour.SetHat(selectedHat);
        SelectedCharacterText.text = character.Display;

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

}
