using Beamable.Common;
using Beamable.UI.Scripts;
using HatsContent;
using HatsCore;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HatsUnity
{
   public class HatOptionBehaviour : MonoBehaviour
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
      public HatContent Hat;

      void Start()
      {
         if (Hat == null)
         {
            LabelText.gameObject.SetActive(false);
            IconPreview.gameObject.SetActive(false);
            LoadingSpinner.SetActive(true);
            SelectionButton.interactable = false;
         }

         SelectionButton.onClick.AddListener(HandleSelected);

      }
      void HandleSelected()
      {
         OnSelected.Invoke();
      }

      public void SetOption(HatContent hat)
      {
         Hat = hat;
         LabelText.text = hat.Display;
         var spriteOperation = AddressableSpriteLoader.LoadSprite(hat.icon);
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
}