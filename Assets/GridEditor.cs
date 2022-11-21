using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class GridEditor : MonoBehaviour
{
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Canvas editGridCavas;

    [SerializeField] private Tilemap obstacleTileMap;
    [SerializeField] private Tilemap groundTileMap;

    [SerializeField] private LevelTile groundTile;
    [SerializeField] private LevelTile obstacleTile;
    [SerializeField] private LevelTile testTile;

    private List<LayoutData> layoutDatas = new List<LayoutData>();

    private PathfinderManager pathfinderManager;

    private int layoutIndex = 0;

    [Serializable]
    public class LayoutData
    {
        public string layoutName;
        public List<LevelTile> tileData = new List<LevelTile>();
    }

    [SerializeField] List<LayoutData> layoutData = new List<LayoutData>();
    public void SaveToJson()
    {
        Layout newLayout = new Layout();

        newLayout.layoutIndex = layoutIndex;

        newLayout.name = $"Layout {layoutIndex}";

        newLayout.obstacleTiles = GetTilesFromMap(obstacleTileMap).ToList();
        newLayout.groundTiles = GetTilesFromMap(groundTileMap).ToList();

        IEnumerable<SavedTile> GetTilesFromMap(Tilemap map)
        {
            foreach(Vector3Int pos in obstacleTileMap.cellBounds.allPositionsWithin)
            {
                if(obstacleTileMap.HasTile(pos))
                {
                    LevelTile levelTile = obstacleTileMap.GetTile<LevelTile>(pos);

                    yield return new SavedTile()
                    {
                        position = pos,
                        tile = levelTile
                    };

                    //layoutData[layoutIndex].tileData.Add(levelTile);
                }
            }

        }


        string compressedString = newLayout.Serialize();

        //layoutData[layoutIndex].layoutName = "Test";
        //layoutDatas.Add(layoutData[layoutIndex]);

        string saveData = JsonUtility.ToJson(compressedString);
        File.WriteAllText(Application.persistentDataPath + "/LayoutSaveData.json", compressedString);
    }

    public class SaveData
    {
        public List<string> Tiles = new List<string>();

        public static SaveData FromFile()
        {
            string path = Application.persistentDataPath + "/LayoutSaveData.json";
            string jsonString = File.ReadAllText(path);
        
            return JsonUtility.FromJson<SaveData>(jsonString);
        }
    }

    public void GetJson()
    {
        SaveData saveData = SaveData.FromFile();

        obstacleTileMap.ClearAllTiles();

        foreach (string tileData in saveData.Tiles)
        {
            int tileType = int.Parse(tileData[0].ToString());

            int x = int.Parse(tileData.Split("[")[1].Split(",")[0]);
            int y = int.Parse(tileData.Split(",")[1].Split("]")[0]);

            Vector3Int pos = new Vector3Int(x, y, 0);

            switch (tileType)
            {
                case 0:
                    obstacleTileMap.SetTile(pos, groundTile);
                    break;
                case 1:
                    obstacleTileMap.SetTile(pos, obstacleTile);
                    break;
                case 2:
                    obstacleTileMap.SetTile(pos, testTile);
                    break;
            }
        }
    }

    public void LoadLayout(TMP_Dropdown change)
    {
        Debug.Log(change.value);
    }

    [Serializable]
    public class SavedTile
    {
        public Vector3Int position;
        public LevelTile tile;
    }

    public struct Layout
    {
        public string name;
        public int layoutIndex;
        public List<SavedTile> groundTiles;
        public List<SavedTile> obstacleTiles;

        public string Serialize()
        {
            StringBuilder compressedString = new StringBuilder();

            compressedString.Append(@"{""Tiles"":[");

            foreach (var obstacleTile in obstacleTiles)
            {
                compressedString.Append($@"""{(int)obstacleTile.tile.type}[{obstacleTile.position.x},{obstacleTile.position.y}]"",");
            }
            compressedString.Length--;
            compressedString.Append(@"]}");

            return compressedString.ToString();
        }
    }
    /*private string Serialize(LayoutData layoutData)
    {
        StringBuilder compressedString = new StringBuilder();

        compressedString.Append(@"{""Tiles"":[");

        foreach (Vector3Int pos in obstacleTileMap.cellBounds.allPositionsWithin)
        {
            if (obstacleTileMap.HasTile(pos))
            {
                LevelTile levelTile = (LevelTile)obstacleTileMap.GetTile(pos);

                layoutData.tileData.Add(levelTile);

                compressedString.Append($@"""{(int)levelTile.type}[{pos.x},{pos.y}]"",");
            }
        }
        compressedString.Length--;
        compressedString.Append(@"]}");

        return compressedString.ToString();
    }*/


    private void Start()
    {
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

    private void SetTile(LevelTile tile)
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int pos = obstacleTileMap.WorldToCell(mousePos);

        obstacleTileMap.SetTile(pos, tile);
    }
}
