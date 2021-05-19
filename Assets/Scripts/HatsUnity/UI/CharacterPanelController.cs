using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable.Common.Shop;
using Beamable.Stats;
using HatsContent;
using HatsCore;
using HatsUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPanelController : MonoBehaviour
{
    [Header("Content References")]
    public StoreRef StoreRef;

    [Header("Prefab References")]
    public CharacterOptionBehaviour CharacterOptionPrefab;
    public HatOptionBehaviour HatOptionPrefab;

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

        LoadingSpinner.SetActive(false);
        HintText.text = "Select your Characters";

        foreach (var character in characters)
        {
            var instance = Instantiate(CharacterOptionPrefab, CharacterOptionContainer);
            instance.SetOption(character);
            instance.OnSelected.AddListener(() =>
            {
                SelectCharacter(instance);
            });
        }
        foreach (var hat in hats)
        {
            var instance = Instantiate(HatOptionPrefab, HatOptionContainer);
            instance.SetOption(hat);
            instance.OnSelected.AddListener(() =>
            {
                SelectHat(instance);
            });
        }

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

    public async Task GetListings()
    {
        var beamable = await Beamable.API.Instance;
        var shop = await beamable.CommerceService.GetCurrent(StoreRef.Id);
        foreach (var listing in shop.listings)
        {
            listing.
        }
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
