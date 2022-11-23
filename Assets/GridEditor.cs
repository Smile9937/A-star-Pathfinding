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
    #region Canvases
    [Header("Canvases")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Canvas editGridCavas;
    [SerializeField] private Canvas createGridCavas;
    //[SerializeField] private Canvas renameGridCavas;

    [SerializeField] private Canvas[] canvases;
    #endregion

    #region Tilemaps
    [Header("Tilemaps")]
    [SerializeField] private Tilemap obstacleTileMap;
    [SerializeField] private Tilemap groundTileMap;
    [Space(6)]
    [SerializeField] private Tilemap defaultObstacleTileMap;
    [SerializeField] private Tilemap defaultGroundTileMap;
    #endregion

    #region Tiles
    [Header("Tiles")]
    [SerializeField] private LevelTile groundTile;
    [SerializeField] private LevelTile obstacleTile;
    [SerializeField] private LevelTile testTile;
    #endregion


    #region UI Elements
    [Header("UI Elements")]
    [SerializeField] private TMP_Dropdown dropDown;

    [SerializeField] private TMP_InputField createGridInputField;
    [SerializeField] private TMP_InputField renameGridInputField;
    #endregion

    #region Grid
    [Header("Grid")]
    [SerializeField] private Grid grid;
    #endregion

    private PathfinderManager pathfinderManager;

    private static int layoutIndex = 0;

    private const string defaultFileName = "Default Map";
    private const string fileExtension = ".json";

    private const string inputFieldEmptyMessage = "Input field is empty!";
    private const string nameInUseMessage = "That name is already in use!";

    private static string fileName = "Default Map";
    private static string path => Application.persistentDataPath + $"/LayoutSaveData";
    private static string fullFileName => $"{fileName}{fileExtension}";
    private static string fullPath => $"{path}/{fullFileName}";
    private DirectoryInfo directoryInfo => new DirectoryInfo(path);
    private FileInfo[] fileInfo => directoryInfo.GetFiles();
    private string GetFileName(FileInfo file) => file.Name.Replace(file.Extension, "");
    private bool NameExists(string name) => Array.Exists(fileInfo, value => GetFileName(value) == name);

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

        grid.CreateGrid();

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
        SetCurrentFilename();
        GetJson();
    }

    private void SetCurrentFilename() => fileName = dropDown.options[layoutIndex].text;

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

        if(fileInfo.Length == 0)
        {
            CreateDefaultMap();
        }
        else
        {
            fileName = fileInfo[0].Name.Replace(fileInfo[0].Extension, "");
            UpdateDropDown(dropDown.value);
            GetJson();
        }

        grid.CreateGrid();

    }
    private void CreateDefaultMap()
    {
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

        ReplaceTiles(obstacleTileMap, defaultObstacleTileMap);
        ReplaceTiles(groundTileMap, defaultGroundTileMap);

        fileName = defaultFileName;
        SaveToJson();
    }

    public void EditGrid()
    {
        pathfinderManager.ToggleEditMode(true);
        SetActiveCanvas(editGridCavas);

        renameGridInputField.text = fileName;
    }

    public void ReturnToMainMenu()
    {
        pathfinderManager.ToggleEditMode(false);

        SetActiveCanvas(mainCanvas);
        PathfinderManager.InvokeGenerateGrid();
    }

    private void StopEdit(int dropdownValue)
    {
        UpdateDropDown(dropdownValue);
        ReturnToMainMenu();
    }

    public void StartCreateGrid()
    {
        pathfinderManager.ToggleEditMode(true);
        SetActiveCanvas(createGridCavas);
    }

    public void CreateGrid()
    {
        string inputText = createGridInputField.text;

        if (inputText == "")
        {
            Debug.Log(inputFieldEmptyMessage);
        }
        else if (NameExists(inputText))
        {
            Debug.Log(nameInUseMessage);
        }
        else
        {
            fileName = inputText;
            SaveToJson();

            int index = 0;

            for (int i = 0; i < fileInfo.Length; i++)
            {
                if (fileInfo[i].Name == fullFileName)
                {
                    index = i;
                    break;
                }
            }

            createGridInputField.text = "";
            StopEdit(index);
        }
    }

    public void Edit()
    {
        RenameGrid();
        SaveToJson();
    }

    public void RenameGrid()
    {
        string inputText = renameGridInputField.text;

        if (inputText == "")
        {
            Debug.Log(inputFieldEmptyMessage);
        }
        else if (inputText == fileName)
        {
            StopEdit(dropDown.value);
        }
        else if (NameExists(inputText))
        {
            Debug.Log(nameInUseMessage);
        }
        else
        {
            string currentFile = fullPath;
            string newFile = fullPath.Replace(fullFileName, inputText + fileExtension);

            File.Move(currentFile, newFile);
            fileName = inputText;

            StopEdit(dropDown.value);
        }
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
            SetTile(obstacleTile);
        }
        else if(pathfinderManager.InEditMode && Input.GetMouseButton(1))
        {
            SetTile(null);
        }
    }

    private bool IsMouseOverGameWindow => 0 <= Input.mousePosition.x && 0 <= Input.mousePosition.y && Screen.width >= Input.mousePosition.x && Screen.height >= Input.mousePosition.y;

    private void SetTile(LevelTile tile)
    {
        if (EventSystem.current.IsPointerOverGameObject() || !IsMouseOverGameWindow) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int pos = obstacleTileMap.WorldToCell(mousePos);

        obstacleTileMap.SetTile(pos, tile);
    }

    public void OpenDropDown()
    {
        UpdateDropDown(dropDown.value);
    }

    private void UpdateDropDown(int value)
    {
        dropDown.ClearOptions();

        if (fileInfo.Length == 0) return;

        foreach (FileInfo file in fileInfo)
        {
            if (file.Extension == fileExtension)
            {
                string name = GetFileName(file);
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
