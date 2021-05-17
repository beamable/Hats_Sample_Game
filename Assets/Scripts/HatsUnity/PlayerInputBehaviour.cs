using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace HatsMultiplayer
{

   [Serializable]
   public class PlayerInputClickEvent : UnityEvent<Vector3Int> {

   }

   public class PlayerInputBehaviour : MonoBehaviour
   {
      public BattleGridBehaviour BattleGridBehaviour;

      public PlayerInputClickEvent OnClick;

      private MouseInputs inputs;

      public float Z = 10;


      private void Awake()
      {
         inputs = new MouseInputs();
      }

      private void OnEnable()
      {
         inputs.Enable();
      }

      private void OnDisable()
      {
         inputs.Disable();
      }

      void Start()
      {
         inputs.MouseInput.Click.performed += OnPlayerClicked;
      }

      private void OnPlayerClicked(InputAction.CallbackContext obj)
      {
         if (TryGetHoveringCell(out var cell))
         {
            OnClick?.Invoke(cell);
         }
      }

      private bool TryGetHoveringCell(out Vector3Int cell)
      {
         var position = inputs.MouseInput.Position.ReadValue<Vector2>();
         Vector3 point = Camera.main.ScreenToWorldPoint(new Vector3(position.x, position.y, Z));
         point.z = 0;
         cell = BattleGridBehaviour.Tilemap.layoutGrid.WorldToCell(point);
         return BattleGridBehaviour.Tilemap.HasTile(cell);
      }


      private Vector3Int lastCell;
      private void Update()
      {
         // check if the user is hovering over a tile, and if so, highlight it?
         // var position = inputs.MouseInput.Position.ReadValue<Vector2>();
         // Vector3 point = Camera.main.ScreenToWorldPoint(new Vector3(position.x, position.y, 10));
         // Debug.Log(point);

         // var tileBase = Tilemap.GetTile(cell);
         // if (tileBase != null)
         // {
         //    Debug.Log("hovering on " + cell);
         // }
      }

   }
}