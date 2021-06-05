using System.Collections;
using System.Collections.Generic;
using Hats.Game;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardScreenController : MonoBehaviour
{
    [Header("UI References")]
    public Button MenuButton;

    // Start is called before the first frame update
    void Start()
    {
        MenuButton.onClick.AddListener(HandleMenu);
    }


    void HandleMenu()
    {
        HatsScenes.LoadMatchmaking();
    }
}