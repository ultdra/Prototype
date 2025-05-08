using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls build mode activation, building selection, and UI integration.
/// Toggles grid and ghost object visibility.
/// </summary>
public class BuildModeUIController : MonoBehaviour
{
    public GridVisualizer gridVisualizer;
    public GhostObjectController ghostController;
    public BuildingPlacer buildingPlacer;


    private bool buildModeActive = false;

    void OnEnable()
    {
        Debug.Log("[BuildModeUIController] OnEnable called");
    }

    void OnDisable()
    {
        Debug.Log("[BuildModeUIController] OnDisable called");
    }

    void Awake()
    {
        Debug.Log("[BuildModeUIController] Awake called");
    }

    void Start()
    {
        Debug.Log("[BuildModeUIController] Start called");
        SetBuildMode(false);
    }

    /// <summary>
    /// Toggles build mode on/off.
    /// </summary>
    public void ToggleBuildMode()
    {
        SetBuildMode(!buildModeActive);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBuildMode();
        }
    }

    /// <summary>
    /// Sets build mode state and updates system/UI.
    /// </summary>
    public void SetBuildMode(bool active)
    {
        buildModeActive = active;

        if (gridVisualizer != null)
            gridVisualizer.showGrid = active;
        // if (ghostController != null && ghostController.gameObject != null)
        //     ghostController.gameObject.SetActive(active);
    }

    /// <summary>
    /// Called by UI to select a building type.
    /// </summary>
    public void SelectBuilding(GameObject buildingPrefab, int width, int height)
    {
        if (buildingPlacer != null)
        {
            buildingPlacer.SetBuildingPrefab(buildingPrefab);
            buildingPlacer.SetFootprint(width, height);
        }
    }
}