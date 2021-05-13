using HatsCore;
using UnityEngine;

namespace HatsMultiplayer
{
   public class PlayerMoveBuilder : MonoBehaviour
   {
      public GameProcessor GameProcessor;
      public MultiplayerGameDriver NetworkDriver;

      [ReadOnly]
      [SerializeField]
      private long PlayerDbid;

      async void Start()
      {
         var beamable = await Beamable.API.Instance;
         PlayerDbid = beamable.User.id;
      }

      public void HandleClick(Vector3Int cell)
      {
         var state = GameProcessor.GetCurrentPlayerState(PlayerDbid);
         var direction = GameProcessor.BattleGridBehaviour.BattleGrid.GetDirection(state.Position, cell);
         NetworkDriver.DeclareLocalPlayerAction(new HatsPlayerMove
         {
            Dbid = PlayerDbid,
            TurnNumber = GameProcessor.EventProcessor.CurrentTurn,
            Direction = direction,
            MoveType = HatsPlayerMoveType.WALK
         });
      }
   }
}