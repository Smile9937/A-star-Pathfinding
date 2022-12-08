using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private Slider playerSpeedSlider;
    [SerializeField] private TMP_Text playerSpeedText;
    [SerializeField] private DrawGrid drawGrid;
    [SerializeField] private Toggle drawGridToggle;
    [SerializeField] private Toggle drawPlayerPathToggle;
    [SerializeField] private Toggle drawPreviousPathToggle;
    private PathfinderManager pathfinderManager;

    private void Start()
    {
        pathfinderManager = PathfinderManager.instance;
        ChangePlayerSpeed(pathfinderManager.PlayerSpeed);
        playerSpeedSlider.value = pathfinderManager.PlayerSpeed;
        drawGridToggle.isOn = pathfinderManager.DrawGrid;
        drawPlayerPathToggle.isOn = pathfinderManager.DrawPlayerPath;
        drawPreviousPathToggle.isOn = pathfinderManager.DrawPreviousPath;
    }

    public void ChangePlayerSpeed(float value)
    {
        pathfinderManager.SetPlayerSpeed(value);
        playerSpeedText.text =  $"Player Speed: {string.Format("{0:0.00}" ,value)}";
    }

    public void ToggleDrawGrid(bool active)
    {
        pathfinderManager.ToggleDrawGrid(active);
        drawGrid.ToggleGrid(active);
    }
    
    public void ToggleDrawPlayerPath(bool active)
    {
        pathfinderManager.ToggleDrawPlayerPath(active);
    }

    public void ToggleDrawPreviousPath(bool active)
    {
        pathfinderManager.ToggleDrawPreviousPath(active);
    }
}
