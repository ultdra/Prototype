using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles building placement on the grid when the user clicks.
/// Instantiates the building prefab and updates grid occupancy.
/// </summary>
public class BuildingPlacer : MonoBehaviour
{
    public GridSystem gridSystem;
    public GhostObjectController ghostController;
    public GameObject buildingPrefab;

    // Optionally track placed buildings
    public List<GameObject> placedBuildings = new List<GameObject>();

    void Update()
    {
        if (gridSystem == null || ghostController == null || buildingPrefab == null)
            return;

        // Only allow placement on left mouse click and if ghost is active
        if (Input.GetMouseButtonDown(0))
        {
            var (gridPos, isValid) = ghostController.GetPlacementInfo();
            int w = ghostController.footprintWidth;
            int h = ghostController.footprintHeight;

            if (isValid)
            {
                // Place building at snapped position
                Vector3 worldPos = gridSystem.GetWorldPosition(gridPos.x, gridPos.y);
                GameObject building = Instantiate(buildingPrefab, worldPos, Quaternion.identity);
                placedBuildings.Add(building);

                // Mark grid as occupied
                gridSystem.OccupyArea(gridPos.x, gridPos.y, w, h);
            }
        }
    }

    /// <summary>
    /// Sets the building prefab to place.
    /// </summary>
    public void SetBuildingPrefab(GameObject prefab)
    {
        buildingPrefab = prefab;
        ghostController.SetGhostPrefab(prefab);
    }

    /// <summary>
    /// Sets the building footprint size.
    /// </summary>
    public void SetFootprint(int width, int height)
    {
        ghostController.SetFootprint(width, height);
    }
}