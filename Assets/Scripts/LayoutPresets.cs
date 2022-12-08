using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Create Layout Preset", fileName = "New Layout Preset")]
public class LayoutPresets : ScriptableObject
{
    public Transform playerStart;
    public Tilemap groundTileMap;
    public Tilemap obstacleTileMap;
}