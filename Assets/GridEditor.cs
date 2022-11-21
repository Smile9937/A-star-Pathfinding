using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class GridEditor : MonoBehaviour
{
    [SerializeField] private Canvas mainCanvas;

    [SerializeField] private Canvas editGridCavas;

    [SerializeField] private Tilemap obstacleTileMap;

    [SerializeField] private Tile obstacleTile;

    [SerializeField] private Tile testTile;

    private PathfinderManager pathfinderManager;

    public void SaveIntoJson()
    {
        LayoutData layoutData = new LayoutData();

        foreach(Vector3Int pos in obstacleTileMap.cellBounds.allPositionsWithin)
        {
            if(obstacleTileMap.HasTile(pos))
            {
                TileData tileData = new TileData();

                tileData.tile = obstacleTile;
                tileData.tilePosition = (Vector2Int)pos;

                layoutData.tileData.Add(tileData);
            }
        }

        string tile = JsonUtility.ToJson(layoutData);
        File.WriteAllText(Application.persistentDataPath + "/LayoutSaveData.json", tile);
    }

    public void GetJson()
    {
        string path = Application.persistentDataPath + "/LayoutSaveData.json";
        string jsonString = File.ReadAllText(path);

        LayoutData data = JsonUtility.FromJson<LayoutData>(jsonString);

        obstacleTileMap.ClearAllTiles();

        foreach (TileData tileData in data.tileData)
        {
            obstacleTileMap.SetTile((Vector3Int)tileData.tilePosition, tileData.tile);

        }
    }

    [Serializable]
    public class LayoutData
    {
        public List<TileData> tileData = new List<TileData>();
    }

    [Serializable]
    public class TileData
    {
        public Tile tile;
        public Vector2Int tilePosition;
    }

    private void Start()
    {
        /*LayoutData layoutData = new LayoutData();
        for (int i = 0; i < 5; i++)
        {
            TileData tileData = new TileData();
            tileData.tile = obstacleTile;
            tileData.tilePosition = new Vector2Int(i,i);

            layoutData.tileData.Add(tileData);

        }
        SaveIntoJson(layoutData);

        GetJson();*/
        pathfinderManager = PathfinderManager.instance;
        pathfinderManager.ToggleEditMode(false);
        mainCanvas.enabled = true;
        editGridCavas.enabled = false;
    }

    public void EditGrid()
    {
        pathfinderManager.ToggleEditMode(true);
        mainCanvas.enabled = false;
        editGridCavas.enabled = true;
    }

    public void StopEdit()
    {
        pathfinderManager.ToggleEditMode(false);
        mainCanvas.enabled = true;
        editGridCavas.enabled = false;
        PathfinderManager.InvokeGenerateGrid();
    }

    private void Update()
    {
        if(pathfinderManager.InEditMode && Input.GetMouseButton(0))
        {
            if (EventSystem.current.IsPointerOverGameObject() || !IsMouseOverGameWindow) return;
            SetTile(obstacleTile);
        }
        else if(pathfinderManager.InEditMode && Input.GetMouseButton(1))
        {
            if (EventSystem.current.IsPointerOverGameObject() || !IsMouseOverGameWindow) return;
            SetTile(null);
        }
    }

    private bool IsMouseOverGameWindow => 0 <= Input.mousePosition.x && 0 <= Input.mousePosition.y && Screen.width >= Input.mousePosition.x && Screen.height >= Input.mousePosition.y;

    private void SetTile(Tile tile)
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int pos = obstacleTileMap.WorldToCell(mousePos);

        obstacleTileMap.SetTile(pos, tile);
    }
}
