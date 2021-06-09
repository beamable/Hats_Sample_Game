using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Hats.Game
{
	public class MusicManager : MonoBehaviour
	{
		public const string MasterVolumeKey = "MasterVolume";
		public const string MusicVolumeKey = "MusicVolume";
		public const string SFXVolumeKey = "SFXVolume";
		private const float DecibelRange = 45f;

		public AudioMixer audioMixer;
		public AudioSource MenuMusic;
		public AudioSource BattleMusic;
		public AudioSource AccountMusic;

		public AudioSource ButtonSound;

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
			// Make sure audio mixer has the preferred volume settings
			audioMixer.SetFloat(MasterVolumeKey, LinearToDecibel(PlayerPrefs.GetFloat(MasterVolumeKey, 1f)));
			audioMixer.SetFloat(MusicVolumeKey, LinearToDecibel(PlayerPrefs.GetFloat(MusicVolumeKey, 1f)));
			audioMixer.SetFloat(SFXVolumeKey, LinearToDecibel(PlayerPrefs.GetFloat(SFXVolumeKey, 1f)));
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
			if (music != _currentMusic)
			{
				StopMusic();
				_currentMusic = music;
				_currentMusic.Play();
			}
		}

		public void StopMusic()
		{
			if (_currentMusic != null)
			{
				_currentMusic.Stop();
				_currentMusic = null;
			}
		}

		public void PlayButtonSound()
		{
			ButtonSound.Stop();
			ButtonSound.Play();
		}

		public static float LinearToDecibel(float linear)
		{
			linear = Mathf.Clamp01(linear);
			if (linear != 0f)
			{
				return DecibelRange * Mathf.Log10(linear);
			}
			return -80f;
		}

		public static float DecibelToLinear(float decibel)
		{
			return Mathf.Clamp01(Mathf.Pow(10f, decibel / DecibelRange));
		}
	}
}