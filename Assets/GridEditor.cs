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
    [SerializeField] private Canvas createGridCavas;

    [SerializeField] private Canvas[] canvases;

    [SerializeField] private Tilemap obstacleTileMap;
    [SerializeField] private Tilemap groundTileMap;
    
    [SerializeField] private Tilemap defaultObstacleTileMap;
    [SerializeField] private Tilemap defaultGroundTileMap;

    [SerializeField] private LevelTile groundTile;
    [SerializeField] private LevelTile obstacleTile;
    [SerializeField] private LevelTile testTile;

    [SerializeField] private Grid grid;

    [SerializeField] private TMP_Dropdown dropDown;

    private PathfinderManager pathfinderManager;

    private static int layoutIndex = 0;

    private const string defaultFileName = "Default Map";

    private static string fileName = "Default Map";
    private static string path => Application.persistentDataPath + $"/LayoutSaveData";
    private static string file => $"{fileName}.json";
    private static string fullPath => $"{path}/{file}";

    public void SaveToJson()
    {
        Layout newLayout = new Layout();

        newLayout.layoutIndex = layoutIndex;

        newLayout.name = fileName;

        newLayout.obstacleTiles = GetTilesFromMap(obstacleTileMap).ToList();
        newLayout.groundTiles = GetTilesFromMap(groundTileMap).ToList();

        IEnumerable<SavedTile> GetTilesFromMap(Tilemap map)
        {
            foreach(Vector3Int pos in map.cellBounds.allPositionsWithin)
            {
                if(map.HasTile(pos))
                {
                    LevelTile levelTile = map.GetTile<LevelTile>(pos);

                    yield return new SavedTile()
                    {
                        position = pos,
                        tile = levelTile
                    };
                }
            }
        }


        string compressedString = newLayout.Serialize();

        //layoutData[layoutIndex].layoutName = "Test";
        //layoutDatas.Add(layoutData[layoutIndex]);

        string saveData = JsonUtility.ToJson(compressedString);

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        File.WriteAllText(fullPath, compressedString);
    }

    public class SaveData
    {
        public List<string> ObstacleTiles = new List<string>();
        public List<string> GroundTiles = new List<string>();

        public static SaveData FromFile()
        {
            string jsonString = File.ReadAllText(fullPath);
        
            return JsonUtility.FromJson<SaveData>(jsonString);
        }
    }

    public void GetJson()
    {
        SaveData saveData = SaveData.FromFile();

        UpdateTilemap(groundTileMap, saveData.GroundTiles);
        UpdateTilemap(obstacleTileMap, saveData.ObstacleTiles);

        void UpdateTilemap(Tilemap map, List<string> tileList)
        {
            map.ClearAllTiles();

            foreach (string tileData in tileList)
            {
                int tileType = int.Parse(tileData[0].ToString());

                int x = int.Parse(tileData.Split("[")[1].Split(",")[0]);
                int y = int.Parse(tileData.Split(",")[1].Split("]")[0]);

                Vector3Int pos = new Vector3Int(x, y, 0);

                switch (tileType)
                {
                    case 0:
                        map.SetTile(pos, groundTile);
                        break;
                    case 1:
                        map.SetTile(pos, obstacleTile);
                        break;
                    case 2:
                        map.SetTile(pos, testTile);
                        break;
                }
            }
        }

    }

    public void LoadLayout(TMP_Dropdown change)
    {
        layoutIndex = change.value;
        fileName = dropDown.options[layoutIndex].text;
        GetJson();
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

            compressedString.Append(@"{");

            AddTilesToJson(groundTiles, "GroundTiles");
            AddTilesToJson(obstacleTiles, "ObstacleTiles");

            void AddTilesToJson(List<SavedTile> tiles, string entryName)
            {
                compressedString.Append($@"""{entryName}"":[");

                foreach (SavedTile obstacleTile in tiles)
                {
                    compressedString.Append($@"""{(int)obstacleTile.tile.type}[{obstacleTile.position.x},{obstacleTile.position.y}]"",");
                }

                if(compressedString[compressedString.Length - 1].ToString() == ",")
                    compressedString.Remove(compressedString.Length - 1, 1);

                compressedString.Append("],");

            }

            if (compressedString[compressedString.Length - 1].ToString() == ",")
                compressedString.Remove(compressedString.Length - 1, 1);

            compressedString.Append("}");

            return compressedString.ToString();
        }
    }

    private void Start()
    {
        pathfinderManager = PathfinderManager.instance;
        pathfinderManager.ToggleEditMode(false);
        SetActiveCanvas(mainCanvas);
        layoutIndex = 0;
        dropDown.value = 0;

        DirectoryInfo info = new DirectoryInfo(path);
        FileInfo[] fileInfo = info.GetFiles();

        void ReplaceTiles(Tilemap map, Tilemap replacement)
        {
            map.ClearAllTiles();

            foreach (Vector3Int pos in replacement.cellBounds.allPositionsWithin)
            {
                if (replacement.HasTile(pos))
                {
                    LevelTile replacementTile = replacement.GetTile<LevelTile>(pos);
                    map.SetTile(pos, replacementTile);
                }
            }
        }

        if(fileInfo.Length == 0)
        {
            ReplaceTiles(obstacleTileMap, defaultObstacleTileMap);
            ReplaceTiles(groundTileMap, defaultGroundTileMap);

            fileName = defaultFileName;
            SaveToJson();
        }
        else
        {
            fileName = fileInfo[0].Name.Replace(fileInfo[0].Extension, "");
            UpdateDropDown();
            GetJson();
        }

        grid.CreateGrid();
    }

    public void EditGrid()
    {
        pathfinderManager.ToggleEditMode(true);
        SetActiveCanvas(editGridCavas); 
    }

    public void StopEdit()
    {
        pathfinderManager.ToggleEditMode(false);
        SetActiveCanvas(mainCanvas);
        PathfinderManager.InvokeGenerateGrid();
    }

    public void SetActiveCanvas(Canvas canvas)
    {
        foreach(Canvas _canvas in canvases)
        {
            _canvas.enabled = _canvas == canvas;
        }
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

    public void OpenDropDown()
    {
        UpdateDropDown();
    }

    private void UpdateDropDown()
    {
        DirectoryInfo info = new DirectoryInfo(path);
        FileInfo[] fileInfo = info.GetFiles();

        int value = dropDown.value;

        dropDown.ClearOptions();

        foreach (FileInfo file in fileInfo)
        {
            if (file.Extension == ".json")
            {
                string name = file.Name.Replace(file.Extension, "");
                TMP_Dropdown.OptionData currentOption = new TMP_Dropdown.OptionData();
                currentOption.text = name;
                dropDown.options.Add(currentOption);
            }
        }
        if (value > dropDown.options.Count)
        {
            dropDown.value = 0;
        }
        else
        {
            dropDown.value = value;
        }
        dropDown.captionText.text = dropDown.options[dropDown.value].text;
    }

    public void CloseDropDown()
    {
        //Debug.Log("Close");
        //dropDown.captionText.text = dropDown.options[dropDown.value].text;
    }
}
