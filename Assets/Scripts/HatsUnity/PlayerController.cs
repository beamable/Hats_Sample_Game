using System;
using System.Collections;
using System.Collections.Generic;
using HatsCore;
using HatsMultiplayer;
using UnityEngine;

public class PlayerController : GameEventHandler
{
    [ReadOnly]
    [SerializeField]
    private HatsPlayer _player;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Setup(GameProcessor gameProcessor, HatsPlayer player)
    {
        _player = player;
        GameProcessor = gameProcessor;
        GameProcessor.EventHandlers.Add(this);
    }

    public override IEnumerator HandleMoveEvent(PlayerMoveEvent evt, Action completeCallback)
    {

        var localPosition = GameProcessor.BattleGridBehaviour.Grid.CellToLocal(evt.NewPosition);
        transform.localPosition = localPosition; // TODO animation?
        yield return null;
        completeCallback();
    }
}
