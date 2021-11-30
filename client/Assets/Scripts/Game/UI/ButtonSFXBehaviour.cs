using Hats.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
   public class ButtonSFXBehaviour : MonoBehaviour
   {
      [Header("UI Reference")]
      public Button Button;

      public void Start()
      {
         Button?.onClick.AddListener(() =>
         {
            MusicManager.Instance.PlayButtonSound();
         });
      }
   }
}