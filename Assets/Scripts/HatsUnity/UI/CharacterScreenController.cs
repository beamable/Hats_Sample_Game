using System.Collections;
using System.Collections.Generic;
using HatsUnity;
using UnityEngine;
using UnityEngine.UI;

public class CharacterScreenController : MonoBehaviour
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
        HatsScenes.LoadMatchmaking();
    }
}
