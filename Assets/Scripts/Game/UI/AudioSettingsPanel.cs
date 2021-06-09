using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Hats.Game.UI
{
	public class AudioSettingsPanel : MonoBehaviour
	{
		public AudioMixer audioMixer;
		public Slider masterVolumeSlider;
		public Slider musicVolumeSlider;
		public Slider sfxVolumeSlider;

		private void Awake()
		{
			// Initialize sliders using existing playerprefs values
			masterVolumeSlider.normalizedValue = PlayerPrefs.GetFloat(MusicManager.MasterVolumeKey, 1f);
			musicVolumeSlider.normalizedValue = PlayerPrefs.GetFloat(MusicManager.MusicVolumeKey, 1f);
			sfxVolumeSlider.normalizedValue = PlayerPrefs.GetFloat(MusicManager.SFXVolumeKey, 1f);

			// For convenience, hook up the slider callbacks so this doesn't need to be done manually
			masterVolumeSlider.onValueChanged.AddListener(HandleMasterVolumeSliderValueChanged);
			musicVolumeSlider.onValueChanged.AddListener(HandleMusicVolumeSliderValueChanged);
			sfxVolumeSlider.onValueChanged.AddListener(HandleSFXVolumeSliderValueChanged);
		}

		private void HandleMasterVolumeSliderValueChanged(float value)
		{
			SetVolume(MusicManager.MasterVolumeKey, masterVolumeSlider.normalizedValue);
		}

		private void HandleMusicVolumeSliderValueChanged(float value)
		{
			SetVolume(MusicManager.MusicVolumeKey, musicVolumeSlider.normalizedValue);
		}

		private void HandleSFXVolumeSliderValueChanged(float value)
		{
			SetVolume(MusicManager.SFXVolumeKey, sfxVolumeSlider.normalizedValue);
		}

		private void SetVolume(string key, float linearVolume)
		{
			// Update the volume of the audio mixer and store it in PlayerPrefs
			linearVolume = Mathf.Clamp01(linearVolume);
			audioMixer.SetFloat(key, MusicManager.LinearToDecibel(linearVolume));
			PlayerPrefs.SetFloat(key, linearVolume);
		}

		private void OnDestroy()
		{
			PlayerPrefs.Save();
		}
	}
}