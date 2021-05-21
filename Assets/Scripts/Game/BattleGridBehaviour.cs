using System;
using System.Collections.Generic;
using System.Linq;
using Hats.Simulation;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Hats.Game
{
   public class BattleGridBehaviour : MonoBehaviour
   {
      public BattleGrid BattleGrid;

      public Tilemap Tilemap;
      public Grid Grid;


      public List<Vector3Int> Neighbors(Vector3Int cell)
      {
         return BattleGrid.Neighbors(cell).Where(n => Tilemap.HasTile(n)).ToList();
      }

      public T SpawnObjectAtCell<T>(T prefab, Vector3Int cell) where T : Component
      {
         var instance = Instantiate(prefab, Grid.transform);
         var localPosition = Grid.CellToLocal(cell);
         instance.transform.localPosition = localPosition;
         return instance;
      }
   }
}