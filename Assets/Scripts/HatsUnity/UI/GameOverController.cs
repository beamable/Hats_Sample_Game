using System;
using System.Collections;
using System.Collections.Generic;
using Beamable;
using HatsCore;
using HatsMultiplayer;
using HatsUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverController : GameEventHandler
{
    [Header("UI References")]
    public GameObject Panel;
    public Button HomeButton;
    public TextMeshProUGUI StatusText;

    // Start is called before the first frame update
    void Start()
    {
        Panel.SetActive(false);
        HomeButton.onClick.AddListener(HandleHome);
        FindGameProcessor();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void HandleHome()
    {
        HatsScenes.LoadMatchmaking();
    }

    public override IEnumerator HandleGameOverEvent(GameOverEvent evt, Action completeCallback)
    {
        yield return Beamable.API.Instance.ToYielder();
        var beamable = Beamable.API.Instance.GetResult();
        var selfDbid = beamable.User.id;

        var isWinner = selfDbid == evt.Winner.dbid;
        StatusText.text = isWinner
            ? "victory"
            : "defeat";
        Panel.SetActive(true);

        // TODO: Show a loading spinner
        // TODO: Gather rewards, and player stats on this match, like kills, etc.

        completeCallback();
        yield break;
    }
}
