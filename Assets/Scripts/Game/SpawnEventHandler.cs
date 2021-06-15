using System;
using System.Collections;
using Hats.Simulation;
using UnityEngine;

namespace Hats.Game
{
   public class SpawnEventHandler : GameEventHandler
   {
      public PlayerController playerPrefab;

      public override IEnumerator HandleSpawnEvent(PlayerSpawnEvent evt, Action callback)
      {
         Debug.Log("Spawning player " + evt.Player.dbid);
         yield return null;
         var playerGob = Instantiate(playerPrefab, GameProcessor.BattleGridBehaviour.Grid.transform);
         playerGob.Setup(GameProcessor, evt.Player);
         var localPosition = GameProcessor.BattleGridBehaviour.Grid.CellToLocal(evt.Position);
         playerGob.transform.localPosition = localPosition;
         callback();
      }
   }
}