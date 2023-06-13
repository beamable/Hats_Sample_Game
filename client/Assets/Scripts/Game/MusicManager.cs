using UnityEngine;
using UnityEngine.Audio;

namespace Hats.Game
{
	public class MusicManager : MonoBehaviour
	{
		public const string MasterVolumeKey = "MasterVolume";
		public const string MusicVolumeKey = "MusicVolume";
		public const string SFXVolumeKey = "SFXVolume";
		public AudioMixer audioMixer;
		public AudioSource MenuMusic;
		public AudioSource BattleMusic;
		public AudioSource AccountMusic;
		public AudioMixerSnapshot loudMusicSnapshot = null;
		public AudioMixerSnapshot quieterMusicSnapshot = null;
		public AudioMixerSnapshot bassyMusicSnpashot = null;
		public AudioSource ButtonSound;
		private const float DecibelRange = 45f;
		private static MusicManager _instance;
		private AudioSource _currentMusic;

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

		public void Start()
		{
			LoadAndApplyUserAudioSettings();
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

		public void MakeMusicLoud(float transitionTime)
		{
			loudMusicSnapshot.TransitionTo(transitionTime);
		}

		public void MakeMusicQuieter(float transitionTime)
		{
			quieterMusicSnapshot.TransitionTo(transitionTime);
		}

		public void MakeMusicBassy(float transitionTime)
		{
			bassyMusicSnpashot.TransitionTo(transitionTime);
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

		private void LoadAndApplyUserAudioSettings()
		{
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
	}
}