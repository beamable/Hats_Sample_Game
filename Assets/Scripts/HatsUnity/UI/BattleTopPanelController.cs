using System.Collections;
using System.Collections.Generic;
using HatsUnity;
using UnityEngine;
using UnityEngine.UI;

public class BattleTopPanelController : MonoBehaviour
{
    [Header("UI References")]
    public Button MenuButton;

    // Start is called before the first frame update
    void Start()
    {
        MenuButton.onClick.AddListener(HandleMenu);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void HandleMenu()
    {
        // TODO: Show a confirmation popup...
        HatsScenes.LoadMatchmaking();
    }
}
