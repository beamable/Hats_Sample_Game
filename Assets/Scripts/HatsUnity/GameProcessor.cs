
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HatsCore;
using Unity.Collections;
using UnityEngine;

namespace HatsMultiplayer
{
   public class GameProcessor : MonoBehaviour
   {
      public HatsEventProcessor EventProcessor;
      public BattleGridBehaviour BattleGridBehaviour;
      public MultiplayerGameDriver MultiplayerGameDriver;

      public string roomId = "room1";

      public List<GameEventHandler> EventHandlers;

      [HatsCore.ReadOnly]
      [SerializeField]
      private int currentTurn;


      private async void Start()
      {
         var beamable = await Beamable.API.Instance;
         var dbids = new List<long> {beamable.User.id};
         StartGame(dbids);
      }

      public void StartGame(List<long> dbids)
      {
         // TODO: Handle matchmaking
         var networkSteam = MultiplayerGameDriver.Init(roomId, new List<long>());

         var players = dbids.Select(dbid => new HatsPlayer
         {
            dbid = dbid
         }).ToList();

         EventProcessor = new HatsEventProcessor(BattleGridBehaviour.BattleGrid, players, roomId.GetHashCode(), networkSteam);
         StartCoroutine(PlayGame());
      }

      public HatsPlayerState GetCurrentPlayerState(long dbid)
      {
         return EventProcessor.GetCurrentTurn().GetPlayerState(dbid);
      }

      private IEnumerator PlayGame()
      {
         foreach (var evt in EventProcessor.PlayGame())
         {
            currentTurn = EventProcessor.CurrentTurn;
            if (evt == null)
            {
               yield return null;
               continue;
            }

            Debug.Log($"Game Event: {evt}");
            switch (evt)
            {
               case PlayerSpawnEvent spawnEvt:
                  yield return EventHandlers.Handle(this, spawnEvt, handler => handler.HandleSpawnEvent);
                  break;
               case PlayerMoveEvent moveEvt:
                  yield return EventHandlers.Handle(this, moveEvt, handler => handler.HandleMoveEvent);
                  break;

               // nothing interesting happened; let the next frame happen
               default:
                  yield return null;
                  break;
            }
         }

         Debug.Log("Game Processor has finished processing game stream.");
      }
   }
}