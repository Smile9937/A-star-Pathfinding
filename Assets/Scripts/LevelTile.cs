using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Level Tile", menuName = "2D/Tiles/Level Tile")]
public class LevelTile : Tile
{
    public TileType type;
}

public enum TileType
{
    Ground = 0,
    Obstacle = 1,
    Test = 2
}