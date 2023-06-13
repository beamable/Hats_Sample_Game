using Beamable.Api.Payments;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Hats.Game.UI
{
   public class GemListingBehaviour : MonoBehaviour
   {
      public UnityEvent OnSelected;

      [Header("UI References")]
      public TextMeshProUGUI CostText;
      public TextMeshProUGUI AmountText;
      public Button BuyButton;

      private void Start()
      {
         BuyButton.onClick.AddListener(HandleBuy);
      }

      public void SetFor(PlayerListingView listingView)
      {
         CostText.text = listingView.offer.price.GetLocalizedText();
         AmountText.text = listingView.offer.obtainCurrency[0].amount.ToString();
      }

      void HandleBuy()
      {
         OnSelected?.Invoke();
      }
   }
}