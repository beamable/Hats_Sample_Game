using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hats.Game.UI
{
	public class OptionsPageBehaviour : MonoBehaviour
	{
		private void Start()
		{
			MusicManager.Instance.PlayMenuMusic();
		}
	}
}