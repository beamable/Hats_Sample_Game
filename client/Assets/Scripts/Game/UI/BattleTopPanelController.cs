using System.Collections;
using System.Collections.Generic;
using Hats.Game;
using UnityEngine;
using UnityEngine.UI;

public class BattleTopPanelController : MonoBehaviour
{
    [Header("UI References")]
    public Button MenuButton;

    // Start is called before the first frame update
    void Start()
    {
        MusicManager.Instance.PlayBattleMusic();

        MenuButton.onClick.AddListener(HandleMenu);
    }

    void HandleMenu()
    {
        // TODO: Show a confirmation popup...
        HatsScenes.LoadMatchmaking();
    }
}
