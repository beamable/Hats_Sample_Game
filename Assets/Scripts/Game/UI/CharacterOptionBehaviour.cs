using System.Collections;
using System.Collections.Generic;
using Beamable.Common;
using Beamable.UI.Scripts;
using Hats.Content;
using Hats.Simulation;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CharacterOptionBehaviour : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent OnSelected;

    [Header("UI References")]
    public Image IconPreview;
    public TextMeshProUGUI LabelText;
    public GameObject LoadingSpinner;
    public Button SelectionButton;

    [Header("Internals")]
    [ReadOnly]
    [SerializeField]
    public CharacterContent Character;

    // Start is called before the first frame update
    void Start()
    {
        if (Character == null)
        {
            LabelText.gameObject.SetActive(false);
            IconPreview.gameObject.SetActive(false);
            LoadingSpinner.SetActive(true);
            SelectionButton.interactable = false;
        }

        SelectionButton.onClick.AddListener(HandleSelected);

    }

    // Update is called once per frame
    void Update()
    {

    }

    void HandleSelected()
    {
        OnSelected.Invoke();
    }

    public void SetOption(CharacterContent character)
    {
        Character = character;
        LabelText.text = character.Display;
        var spriteOperation = AddressableSpriteLoader.LoadSprite(character.icon);
        spriteOperation.ToPromise().Then(sprite =>
        {
            IconPreview.sprite = sprite;
            SelectionButton.interactable = true;

            LabelText.gameObject.SetActive(true);
            IconPreview.gameObject.SetActive(true);
            LoadingSpinner.SetActive(false);
        });
    }
}
