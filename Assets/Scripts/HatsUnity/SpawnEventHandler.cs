using System;
using System.Collections;
using HatsCore;
using UnityEngine;

namespace HatsMultiplayer
{
   public class SpawnEventHandler : GameEventHandler
   {
      public PlayerController playerPrefab; // TODO: Pull this from player state.


      public override IEnumerator HandleSpawnEvent(PlayerSpawnEvent evt, Action callback)
      {
         Debug.Log("Spawning player " + evt.Player.dbid);
         yield return null;
         var playerGob = Instantiate(playerPrefab, GameProcessor.BattleGridBehaviour.Grid.transform);
         playerGob.Setup(GameProcessor, evt.Player);
         var localPosition = GameProcessor.BattleGridBehaviour.Grid.CellToLocal(evt.Position);
         playerGob.transform.localPosition = localPosition; // TODO animation?
         callback();
      }


   }
}