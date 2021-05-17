using System.Collections;
using System.Collections.Generic;
using HatsUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSelectionController : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI GameIdInput;
    public Button JoinButton;

    // Start is called before the first frame update
    void Start()
    {
        JoinButton.onClick.AddListener(HandleJoin);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void HandleJoin()
    {
        var gameId = GameIdInput.text;
        Debug.Log("Game Id" + gameId);

        HatsScenes.LoadGameScene(gameId);
    }
}
