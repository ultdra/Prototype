using UnityEngine;

/// <summary>
/// Manages a 2D grid overlay for building placement, based on a ground object's bounds.
/// Handles grid occupancy and provides utility methods for placement validation.
/// </summary>
public class GridSystem : MonoBehaviour
{
    public MeshRenderer groundRenderer;
    public float cellSize = 1f;

    private Vector3 origin;
    private int width;
    private int height;
    private bool[,] occupied;

    public int Width => width;
    public int Height => height;
    public Vector3 Origin => origin;

    void Awake()
    {
        if (groundRenderer == null)
        {
            Debug.LogError("GridSystem: Ground Renderer not assigned!");
            return;
        }
        InitializeGrid();
    }

    /// <summary>
    /// Initializes the grid based on the ground object's bounds.
    /// </summary>
    private void InitializeGrid()
    {
        Bounds bounds = groundRenderer.bounds;
        origin = new Vector3(
            bounds.min.x,
            bounds.min.y,
            bounds.min.z
        );

        width = Mathf.RoundToInt(bounds.size.x / cellSize);
        height = Mathf.RoundToInt(bounds.size.z / cellSize);

        occupied = new bool[width, height];
    }

    /// <summary>
    /// Checks if a rectangular area is free (not occupied and within bounds).
    /// </summary>
    public bool IsAreaFree(int x, int y, int w, int h)
    {
        if (!IsWithinBounds(x, y, w, h))
            return false;

        for (int ix = 0; ix < w; ix++)
        {
            for (int iy = 0; iy < h; iy++)
            {
                if (occupied[x + ix, y + iy])
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Marks a rectangular area as occupied.
    /// </summary>
    public void OccupyArea(int x, int y, int w, int h)
    {
        for (int ix = 0; ix < w; ix++)
        {
            for (int iy = 0; iy < h; iy++)
            {
                occupied[x + ix, y + iy] = true;
            }
        }
    }

    /// <summary>
    /// Frees a rectangular area.
    /// </summary>
    public void FreeArea(int x, int y, int w, int h)
    {
        for (int ix = 0; ix < w; ix++)
        {
            for (int iy = 0; iy < h; iy++)
            {
                occupied[x + ix, y + iy] = false;
            }
        }
    }

    /// <summary>
    /// Converts grid coordinates to world position (center of cell).
    /// </summary>
    public Vector3 GetWorldPosition(int x, int y)
    {
        return origin + new Vector3((x + 0.5f) * cellSize, 0, (y + 0.5f) * cellSize);
    }

    /// <summary>
    /// Converts a world position to grid coordinates.
    /// </summary>
    public Vector2Int GetGridPosition(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x - origin.x) / cellSize);
        int y = Mathf.FloorToInt((worldPos.z - origin.z) / cellSize);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// Checks if a rectangular area is within grid bounds.
    /// </summary>
    private bool IsWithinBounds(int x, int y, int w, int h)
    {
        return x >= 0 && y >= 0 && (x + w) <= width && (y + h) <= height;
    }
}