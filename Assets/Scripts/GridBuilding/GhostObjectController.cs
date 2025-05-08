using UnityEngine;

/// <summary>
/// Controls the ghost (preview) object for building placement.
/// Follows the mouse, snaps to grid, and tints based on placement validity.
/// </summary>
public class GhostObjectController : MonoBehaviour
{
    public GridSystem gridSystem;
    public Camera mainCamera;
    public GameObject ghostPrefab;
    public Material validMaterial;
    public Material invalidMaterial;

    [Header("Building Footprint")]
    public int footprintWidth = 1;
    public int footprintHeight = 1;

    private GameObject ghostInstance;
    private Renderer[] ghostRenderers;
    private Vector2Int currentGridPos;
    private bool isValidPlacement = false;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        CreateGhostInstance();
    }

    void Update()
    {
        if (gridSystem == null || ghostInstance == null)
            return;

        // Raycast from mouse to ground
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Ground")))
        {
            Vector3 hitPoint = hit.point;
            Vector2Int gridPos = gridSystem.GetGridPosition(hitPoint);

            // Clamp to grid bounds
            gridPos.x = Mathf.Clamp(gridPos.x, 0, gridSystem.Width - footprintWidth);
            gridPos.y = Mathf.Clamp(gridPos.y, 0, gridSystem.Height - footprintHeight);

            // Snap ghost to grid
            Vector3 worldPos = gridSystem.GetWorldPosition(gridPos.x, gridPos.y);
            ghostInstance.transform.position = new Vector3(worldPos.x, hit.point.y, worldPos.z);

            // Validate placement
            isValidPlacement = gridSystem.IsAreaFree(gridPos.x, gridPos.y, footprintWidth, footprintHeight);
            SetGhostMaterial(isValidPlacement);

            currentGridPos = gridPos;
        }
        else
        {
            // Hide ghost if not over ground
            ghostInstance.SetActive(false);
            return;
        }
        ghostInstance.SetActive(true);
    }

    /// <summary>
    /// Instantiates the ghost object and caches its renderers.
    /// </summary>
    private void CreateGhostInstance()
    {
        if (ghostInstance != null)
            Destroy(ghostInstance);

        ghostInstance = Instantiate(ghostPrefab);
        ghostInstance.name = "GhostObject";
        ghostInstance.SetActive(false);
        ghostRenderers = ghostInstance.GetComponentsInChildren<Renderer>();
    }

    /// <summary>
    /// Sets the ghost object's material to indicate valid/invalid placement.
    /// </summary>
    private void SetGhostMaterial(bool valid)
    {
        if (ghostRenderers == null) return;
        Material mat = valid ? validMaterial : invalidMaterial;
        foreach (var rend in ghostRenderers)
        {
            rend.sharedMaterial = mat;
        }
    }

    /// <summary>
    /// Returns the current grid position and placement validity.
    /// </summary>
    public (Vector2Int, bool) GetPlacementInfo()
    {
        return (currentGridPos, isValidPlacement);
    }

    /// <summary>
    /// Sets the building footprint size for the ghost object.
    /// </summary>
    public void SetFootprint(int width, int height)
    {
        footprintWidth = width;
        footprintHeight = height;
    }

    /// <summary>
    /// Sets the ghost prefab to use for preview.
    /// </summary>
    public void SetGhostPrefab(GameObject prefab)
    {
        ghostPrefab = prefab;
        CreateGhostInstance();
    }
}