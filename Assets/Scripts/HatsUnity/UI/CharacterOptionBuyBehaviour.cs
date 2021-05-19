using System.Collections;
using System.Collections.Generic;
using Beamable;
using Beamable.Api.Payments;
using Beamable.Common.Inventory;
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

    public async void SetOption(CharacterContent character, PlayerListingView listing, CurrencyRef currency, Sprite costIcon, bool canAfford)
    {

        CharacterOptionBehaviour.SetOption(character);
        _listing = listing;

        CostIcon.sprite = costIcon;
        CostText.text = listing.offer.price.amount.ToString();

        var beamable = await Beamable.API.Instance;
        beamable.InventoryService.Subscribe(currency.Id, inventoryView =>
        {
            if (_isBought) return; // reject if we already bought it...

            // recalculate affordability if the price changes...
            var canNowAfford = inventoryView.currencies[currency.Id] >= listing.offer.price.amount;
            AffordMaskImage.gameObject.SetActive(!canNowAfford);
        });

        if (canAfford)
        {
            AffordMaskImage.gameObject.SetActive(false);
        }
    }

    public void CompletePurchase()
    {
        if (_isBought) return;
        _isBought = true;

        foreach (var x in DestroyOnPurchase)
        {
            x.SetActive(false);
        }

    }
}
