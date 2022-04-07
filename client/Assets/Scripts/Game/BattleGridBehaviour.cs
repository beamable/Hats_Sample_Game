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

		[Header("Tiles")]
		[SerializeField]
		private Tile iceTile;

		[SerializeField]
		private Tile rockTile;

		[SerializeField]
		private Tile holeTile;

		[SerializeField]
		private Tile lavaTile;

		[SerializeField]
		private Tile suddenDeathGroundTile;

		[SerializeField]
		private Tile suddenDeathIceTile;

		[SerializeField]
		private GameObject lavaEffectPrefab = null;

		[SerializeField]
		private GameObject suddenDeathEffectPrefab = null;

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

		public GameObject SpawnGameObjectAtCell(GameObject prefab, Vector3Int cell)
		{
			var instance = Instantiate(prefab, Grid.transform);
			var localPosition = Grid.CellToLocal(cell);
			instance.transform.localPosition = localPosition;
			return instance;
		}

		public void SetupInitialTileChanges()
		{
			foreach (var tile in BattleGrid.Tiles)
				HandleTileChange(tile);

			BattleGrid.onTileChange.AddListener(HandleTileChange);
		}

		private void HandleTileChange(Vector3Int tile)
		{
			if (BattleGrid.IsInSuddenDeath(tile))
			{
				SpawnGameObjectAtCell(suddenDeathEffectPrefab, tile);

				if (BattleGrid.GetTileType(tile) == BattleGrid.TileType.Ice)
				{
					Tilemap.SetTile(tile, suddenDeathIceTile);
				}
				else
				{
					Tilemap.SetTile(tile, suddenDeathGroundTile);
				}
			}
			else
			{
				switch (BattleGrid.GetTileType(tile))
				{
					case BattleGrid.TileType.Ice:
						Tilemap.SetTile(tile, iceTile);
						break;

					case BattleGrid.TileType.Rock:
						Tilemap.SetTile(tile, rockTile);
						break;

					case BattleGrid.TileType.Hole:
						Tilemap.SetTile(tile, holeTile);
						break;

					case BattleGrid.TileType.Lava:
						Tilemap.SetTile(tile, lavaTile);
						SpawnGameObjectAtCell(lavaEffectPrefab, tile);
						break;
				}
			}
		}
	}
}