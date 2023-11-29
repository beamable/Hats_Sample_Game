using Beamable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Common.Shop;
using Hats.Game.UI;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class NamedGemListingPrefab
{
    public string name;
    public GemListingBehaviour Prefab;
}

public class GemStoreController : MonoBehaviour
{
    [Header("State")]
    public bool IsOpen;

    [Header("Content References")]
    public StoreRef GemStoreRef;

    [Header("Prefab References")]
    public GemListingBehaviour DefaultGemListingPrefab;
    public List<NamedGemListingPrefab> GemListingOverrides;

    [Header("UI References")]
    public RectTransform Container;
    public Button BackButton;
    public GameObject LoadingSpinner;
    public RectTransform ListingsContainer;

    private const string PrefabOverrideKey = "prefabOverride";
    private const string RealGemSku = "skus";
    private const string CurrencyGemId = "currency.gems";
    
    private Vector2 _desiredPosition;
    private Vector2 _desiredPositionVel;

    private BeamContext _beamContext;

    // Start is called before the first frame update
    async void Start()
    {
        BackButton.onClick.AddListener(HandleBack);

        ClearListings();

        LoadingSpinner.SetActive(true);
        await SetupListings();
        LoadingSpinner.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        var desiredY = IsOpen ? 0 : -Screen.height * 3f;
        _desiredPosition = new Vector2(Container.anchoredPosition.x, desiredY);

        Container.anchoredPosition =
            Vector2.SmoothDamp(Container.anchoredPosition, _desiredPosition, ref _desiredPositionVel, .1f);
    }

    void ClearListings()
    {
        for (var i = 0; i < ListingsContainer.childCount; i++)
        {
            Destroy(ListingsContainer.GetChild(i).gameObject);
        }
    }

    async Task SetupListings()
    {
	     _beamContext = BeamContext.Default;
	     await _beamContext.OnReady;
	    
        var gemStore = await GemStoreRef.Resolve();
        
        var storeView = await _beamContext.Api.CommerceService.GetCurrent(gemStore.Id);
        foreach (var listing in storeView.listings)
        {
            var costsRealMoney = listing.offer.price.type.Equals(RealGemSku);
            if (!costsRealMoney) continue;; // only show real-money listings...

            var hasAnyItems = listing.offer.obtainItems.Count > 0;
            var hasOneCurrency = listing.offer.obtainCurrency.Count == 1;

            if (!hasOneCurrency || hasAnyItems)
            {
                continue; // ignore any listings that aren't just a single currency...
            }

            var currencyContentId = listing.offer.obtainCurrency[0].symbol;
            var isGem = currencyContentId.StartsWith(CurrencyGemId);
            if (!isGem) continue; // only show gem listings.

            var listingPrefab = DefaultGemListingPrefab;
            if (listing.ClientData.TryGetValue(PrefabOverrideKey, out var prefabOverride))
            {
                var foundOverride = GemListingOverrides.FirstOrDefault(gemListingOverride =>
                    prefabOverride.Equals(gemListingOverride.name));
                if (foundOverride == null)
                {
                    Debug.LogWarning("No prefab override found for " + prefabOverride);
                }
                else
                {
                    listingPrefab = foundOverride.Prefab;
                }
            }

            var instance = Instantiate(listingPrefab, ListingsContainer);
            instance.SetFor(listing);
            instance.OnSelected.AddListener(async () =>
            {
                var purchaser = await _beamContext.Api.BeamableIAP;
                await purchaser.StartPurchase($"{listing.symbol}:{GemStoreRef.Id}", listing.offer.price.symbol);

            });
        }
    }

    void HandleBack()
    {
        IsOpen = false;
    }
}