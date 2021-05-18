using System.Collections;
using System.Collections.Generic;
using Beamable.Stats;
using HatsContent;
using HatsUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPanelController : MonoBehaviour
{

    [Header("Prefab References")]
    public CharacterOptionBehaviour CharacterOptionPrefab;

    [Header("UI References")]
    public RectTransform CharacterOptionContainer;
    public TextMeshProUGUI HintText;
    public TextMeshProUGUI SelectedCharacterText;
    public GameObject LoadingSpinner;
    public GameObject PreviewSpinner;
    public RawImage RenderPreview;
    public TMP_InputField AliasInputField;
    public StatBehaviour AliasStatBehaviour;

    [Header("Live Preview References")]
    public RenderTexture PreviewTexture;
    public Transform PreviewPrefabContainer;
    // Start is called before the first frame update
    async void Start()
    {
        ClearOptions();
        ClearPreview();

        LoadingSpinner.SetActive(true);
        PreviewSpinner.SetActive(true);
        HintText.text = "Loading Characters...";

        AliasInputField.gameObject.SetActive(false);
        AliasStatBehaviour.OnStatReceived.AddListener(alias => AliasInputField.SetTextWithoutNotify(alias));
        AliasInputField.onEndEdit.AddListener(alias => AliasStatBehaviour.Write(alias));

        SelectedCharacterText.text = "Loading";
        var selectedCharacter = await PlayerInventory.GetSelectedCharacter();
        var selectedPrefab = await selectedCharacter.Prefab.SafeResolve();
        ShowPreview(selectedCharacter, selectedPrefab);
        PreviewSpinner.SetActive(false);

        await AliasStatBehaviour.Read();
        AliasInputField.gameObject.SetActive(true);



        // load up the player's inventory, and show all the available options...
        var characters = await PlayerInventory.GetAvailableCharacters();
        LoadingSpinner.SetActive(false);
        HintText.text = "Select your Characters";

        foreach (var character in characters)
        {
            var instance = Instantiate(CharacterOptionPrefab, CharacterOptionContainer);
            instance.SetOption(character);
            instance.OnSelected.AddListener(() =>
            {
                Select(instance);
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        RenderPreview.texture = PreviewTexture;
    }

    void ClearOptions()
    {
        for (var i = 0; i < CharacterOptionContainer.childCount; i++)
        {
            Destroy(CharacterOptionContainer.GetChild(i).gameObject);
        }
    }

    void ClearPreview()
    {
        for (var i = 0; i < PreviewPrefabContainer.childCount; i++)
        {
            Destroy(PreviewPrefabContainer.GetChild(i).gameObject);
        }
    }

    void ShowPreview(CharacterContent character, CharacterBehaviour prefab)
    {
        ClearPreview();
        Instantiate(prefab, PreviewPrefabContainer);
        SelectedCharacterText.text = character.Display;

    }

    public async void Select(CharacterOptionBehaviour option)
    {

        var setTask = PlayerInventory.SetSelectedCharacter(option.Character);
        var prefab = await option.Character.Prefab.SafeResolve();
        ShowPreview(option.Character, prefab);
        await setTask;
    }

}
