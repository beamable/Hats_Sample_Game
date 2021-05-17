using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace HatsUnity.UI
{
   public class SelectionPreviewBehaviour : MonoBehaviour, IPointerDownHandler
   {
      public UnityEvent OnClick;

      public void OnPointerDown(PointerEventData eventData)
      {
         OnClick?.Invoke();
      }
   }
}