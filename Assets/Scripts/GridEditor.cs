using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GridEditor : MonoBehaviour
{
    #region Canvases
    [Header("Canvases")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Canvas editGridCavas;
    [SerializeField] private Canvas createGridCavas;

    [SerializeField] private Canvas[] canvases;
    #endregion

    #region Tilemaps
    [Header("Tilemaps")]
    [SerializeField] private Tilemap obstacleTileMap;
    [SerializeField] private Tilemap groundTileMap;
    #endregion

    [Header("Default Map")]
    [SerializeField] private LayoutPresets defaultMap;
    [SerializeField] private LayoutPresets emptyMap;

    #region Tiles
    [Header("Tiles")]
    [SerializeField] private LevelTile groundTile;
    [SerializeField] private LevelTile obstacleTile;
    #endregion


    #region UI Elements
    [Header("UI Elements")]
    [SerializeField] private TMP_Dropdown dropDown;

    [SerializeField] private TMP_InputField createGridInputField;
    [SerializeField] private TMP_InputField renameGridInputField;

    [SerializeField] private Image placeTileImage;
    [SerializeField] private Image placeStartPosImage;

    [SerializeField] private Image placeCreateTileImage;
    [SerializeField] private Image placeCreateStartPosImage;

    [SerializeField] private Color normalColor;
    [SerializeField] private Color selectedColor;
    #endregion

    #region Grid
    [Header("Grid")]
    [SerializeField] private Grid grid;
    #endregion

    [SerializeField] private PathFinder player;

    private Vector3 playerStart;

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
    private Vector3 GetWorldGridPosition(Vector3 worldPosition) => new Vector3(Mathf.Floor(worldPosition.x) + 0.5f, Mathf.Floor(worldPosition.y) + 0.5f);
    private enum PlacementType
    {
        Tile,
        PlayerStart
    }
    private PlacementType placementType;
    private void Start()
    {
        pathfinderManager = PathfinderManager.instance;
        pathfinderManager.ToggleEditMode(false);
        PlaceTiles();
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

    public void SaveToJson()
    {
        Layout newLayout = new Layout();

        newLayout.obstacleTiles = GetTilesFromMap(obstacleTileMap).ToList();
        newLayout.groundTiles = GetTilesFromMap(groundTileMap).ToList();
        newLayout.playerPosition = player.transform.position;

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
        public List<string> PlayerPosition = new List<string>();

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

        float playerX = float.Parse(saveData.PlayerPosition[0].Split("[")[1].Split(",")[0]);
        float playerY = float.Parse(saveData.PlayerPosition[0].Split(",")[1].Split("]")[0]);

        Vector3 playerPosition = new Vector3(playerX, playerY, 0);
        playerStart = playerPosition;

        player.SetPosition(playerPosition);

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
        public List<SavedTile> groundTiles;
        public List<SavedTile> obstacleTiles;
        public Vector3 playerPosition;

        public string Serialize()
        {
            StringBuilder compressedString = new StringBuilder();

            compressedString.Append("{");

            compressedString.Append($@"""PlayerPosition"":[""[{playerPosition.x},{playerPosition.y}]""],");

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

    private void CreateDefaultMap()
    {
        LoadPresetMap(defaultMap);

        fileName = defaultFileName;
        SaveToJson();
    }

    private void LoadPresetMap(LayoutPresets preset)
    {
        static void ReplaceTiles(Tilemap map, Tilemap replacement)
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

        ReplaceTiles(obstacleTileMap, preset.obstacleTileMap);
        ReplaceTiles(groundTileMap, preset.groundTileMap);

        player.SetPosition(preset.playerStart.position);
    }

    public void EditGrid()
    {
        pathfinderManager.ToggleEditMode(true);
        SetActiveCanvas(editGridCavas);
        player.SetPosition(playerStart);
        renameGridInputField.text = fileName;
    }

    public void ReturnToMainMenu()
    {
        pathfinderManager.ToggleEditMode(false);

        SetActiveCanvas(mainCanvas);
        GetJson();
        PathfinderManager.InvokeGenerateGrid();
    }

    private void StopEdit(int dropdownValue)
    {
        SaveToJson();
        UpdateDropDown(dropdownValue);
        ReturnToMainMenu();
    }

    public void StartCreateGrid()
    {
        pathfinderManager.ToggleEditMode(true);
        SetActiveCanvas(createGridCavas);
        LoadPresetMap(emptyMap);
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

    public void SaveEdit() => RenameGrid();

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
            switch(placementType)
            {
                case PlacementType.Tile:
                    SetTile(obstacleTile);
                    break;
                case PlacementType.PlayerStart:
                    ChangePlayerPos();
                    break;
            }
        }
        else if(pathfinderManager.InEditMode && Input.GetMouseButton(1))
        {
            switch (placementType)
            {
                case PlacementType.Tile:
                    SetTile(null);
                    break;
            }
        }
    }

    private void ChangePlayerPos()
    {
        if (EventSystem.current.IsPointerOverGameObject() || !IsMouseOverGameWindow) return;

        Vector3 mousePos = GetWorldGridPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        mousePos.z = player.transform.position.z;

        player.SetPosition(mousePos);
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
    
    public void PlaceTiles()
    {
        placementType = PlacementType.Tile;
        placeTileImage.color = selectedColor;
        placeStartPosImage.color = normalColor;

        placeCreateTileImage.color = selectedColor;
        placeCreateStartPosImage.color = normalColor;
    }
    
    public void PlacePlayerStart()
    {
        placementType = PlacementType.PlayerStart;
        placeTileImage.color = normalColor;
        placeStartPosImage.color = selectedColor;

        placeCreateTileImage.color = normalColor;
        placeCreateStartPosImage.color = selectedColor;
    }

    public void OpenMapFolder()
    {
        EditorUtility.RevealInFinder(fullPath);
    }
}
