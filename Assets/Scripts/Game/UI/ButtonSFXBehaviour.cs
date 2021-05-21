using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
   public class ButtonSFXBehaviour : MonoBehaviour
   {
      [Header("UI Reference")]
      public Button Button;

      [Header("Audio To Play")]
      public AudioSource OnClickAudio;

      public void Start()
      {
         Button?.onClick.AddListener(() =>
         {
            OnClickAudio?.Play();
         });
      }
   }
}