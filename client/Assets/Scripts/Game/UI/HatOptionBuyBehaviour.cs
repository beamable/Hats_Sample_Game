using System.Collections;
using System.Collections.Generic;
using Beamable;
using Beamable.Api.Payments;
using Beamable.Common.Inventory;
using Hats.Content;
using Hats.Simulation;
using Hats.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HatOptionBuyBehaviour : MonoBehaviour
{

    [Header("Internal References")]
    public HatOptionBehaviour HatOptionBehaviour;

    [Header("UI References")]
    public TextMeshProUGUI CostText;
    public Image CostIcon;
    public Image AffordMaskImage;
    public List<GameObject> DestroyOnPurchase;

    [Header("Internals")]
    [ReadOnly]
    [SerializeField]
    private bool _isBought;

    private BeamContext _beamContext;

    public async void SetOption(HatContent hat, PlayerListingView listing, CurrencyRef currency, Sprite costIcon, bool canAfford)
    {

        HatOptionBehaviour.SetOption(hat);

        CostIcon.sprite = costIcon;
        CostText.text = listing.offer.price.amount.ToString();

        _beamContext = BeamContext.Default;
        await _beamContext.OnReady;
        _beamContext.Api.InventoryService.Subscribe(currency.Id, inventoryView =>
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
            if (x)
            {
                x.SetActive(false);
            }
        }
    }
}
