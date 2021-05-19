using System.Collections;
using System.Collections.Generic;
using Beamable.Api.Payments;
using HatsContent;
using HatsCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterOptionBuyBehaviour : MonoBehaviour
{
    [Header("Internal References")]
    public CharacterOptionBehaviour CharacterOptionBehaviour;

    [Header("UI References")]
    public TextMeshProUGUI CostText;
    public Image CostIcon;
    public Image AffordMaskImage;
    public List<GameObject> DestroyOnPurchase;

    [Header("Internals")]
    [ReadOnly]
    [SerializeField]
    private PlayerListingView _listing;
    [ReadOnly]
    [SerializeField]
    private bool _isBought;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetOption(CharacterContent character, PlayerListingView listing, Sprite costIcon, bool canAfford)
    {

        CharacterOptionBehaviour.SetOption(character);
        _listing = listing;

        CostIcon.sprite = costIcon;
        CostText.text = listing.offer.price.amount.ToString();

        if (canAfford)
        {
            Destroy(AffordMaskImage.gameObject);
        }
    }

    public void CompletePurchase()
    {
        if (_isBought) return;
        _isBought = true;

        foreach (var x in DestroyOnPurchase)
        {
            if (x)
            {
                Destroy(x);
            }
        }

    }
}
