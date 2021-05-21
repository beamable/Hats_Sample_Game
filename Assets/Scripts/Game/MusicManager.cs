using System;
using UnityEngine;

namespace Hats.Game
{
   public class MusicManager : MonoBehaviour
   {
      public AudioSource MenuMusic;
      public AudioSource BattleMusic;
      public AudioSource AccountMusic;

      private AudioSource _currentMusic;

      private static MusicManager _instance;

      public static MusicManager Instance
      {
         get
         {
            if (_instance == null)
            {
               _instance = FindObjectOfType<MusicManager>();
            }

            return _instance;
         }
      }


      public void Start()
      {

      }

      private void OnDestroy()
      {
         if (_instance == this)
         {
            _instance = null;
         }
      }

      public void PlayMenuMusic()
      {
         StartMusic(MenuMusic);
      }
      public void PlayBattleMusic()
      {
         StartMusic(BattleMusic);
      }
      public void PlayAccountMusic()
      {
         StartMusic(AccountMusic);
      }

      public void StartMusic(AudioSource music)
      {
         StopMusic();
         _currentMusic = music;
         _currentMusic.Play();
      }

      public void StopMusic()
      {
         if (_currentMusic != null)
         {
            _currentMusic.Stop();
            _currentMusic = null;
         }
      }
   }
}