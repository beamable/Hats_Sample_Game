using System.Collections;
using System.Collections.Generic;
using Beamable.Api.Payments;
using HatsContent;
using HatsCore;
using HatsUnity;
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
    private PlayerListingView _listing;
    [ReadOnly]
    [SerializeField]
    private bool _isBought;

    public void SetOption(HatContent hat, PlayerListingView listing, Sprite costIcon, bool canAfford)
    {

        HatOptionBehaviour.SetOption(hat);
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
