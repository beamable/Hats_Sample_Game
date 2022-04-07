using Hats.Game;
using UnityEngine;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
	public Button button;

	private void Awake()
	{
		button.onClick.AddListener(HandleClick);
	}

	void HandleClick()
	{
		HatsScenes.LoadMatchmaking();
	}
}
