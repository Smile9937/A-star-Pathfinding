using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawGrid : MonoBehaviour
{
    [SerializeField] private Image gridTile;
    private PathfinderManager pathfinderManager;

    private void Start()
    {
        pathfinderManager = PathfinderManager.instance;

        ToggleGrid(pathfinderManager.DrawGrid);
    }

    public void ToggleGrid(bool active)
    {
        gridTile.gameObject.SetActive(active);
    }
}
