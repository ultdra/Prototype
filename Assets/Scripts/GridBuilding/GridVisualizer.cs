using UnityEngine;

/// <summary>
/// Renders a grid overlay on top of the ground using GL.Lines.
/// Only visible in build mode.
/// </summary>
[RequireComponent(typeof(GridSystem))]
public class GridVisualizer : MonoBehaviour
{
    public Color gridColor = new Color(1f, 1f, 1f, 0.25f);
    public Material lineMaterial;
    public bool showGrid = false;

    private GridSystem gridSystem;

    void Awake()
    {
        gridSystem = GetComponent<GridSystem>();
        if (lineMaterial == null)
        {
            // Create a simple colored material if not assigned
            Shader shader = Shader.Find("Unlit/Color");
            lineMaterial = new Material(shader) { color = gridColor };
            Debug.Log("[GridVisualizer] Created default line material with Unlit/Color shader.");
        }
        else
        {
            Debug.Log("[GridVisualizer] Using assigned line material: " + lineMaterial.name);
        }
    }

    void OnRenderObject()
    {
        Debug.Log("[GridVisualizer] OnRenderObject called. showGrid=" + showGrid + ", gridSystem=" + (gridSystem != null) + ", lineMaterial=" + (lineMaterial != null));
        if (!showGrid || gridSystem == null || lineMaterial == null)
        {
            Debug.Log("[GridVisualizer] Not drawing grid: showGrid=" + showGrid + ", gridSystem=" + (gridSystem != null) + ", lineMaterial=" + (lineMaterial != null));
            return;
        }

        lineMaterial.SetPass(0);

        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);
        GL.Begin(GL.LINES);
        GL.Color(gridColor);

        int width = gridSystem.Width;
        int height = gridSystem.Height;
        float cellSize = gridSystem.cellSize;
        Vector3 origin = gridSystem.Origin;

        Debug.Log($"[GridVisualizer] Drawing grid: width={width}, height={height}, cellSize={cellSize}, origin={origin}");

        // Draw vertical lines
        for (int x = 0; x <= width; x++)
        {
            Vector3 start = origin + new Vector3(x * cellSize, 0.01f, 0);
            Vector3 end = origin + new Vector3(x * cellSize, 0.01f, height * cellSize);
            GL.Vertex(start);
            GL.Vertex(end);
        }

        // Draw horizontal lines
        for (int y = 0; y <= height; y++)
        {
            Vector3 start = origin + new Vector3(0, 0.01f, y * cellSize);
            Vector3 end = origin + new Vector3(width * cellSize, 0.01f, y * cellSize);
            GL.Vertex(start);
            GL.Vertex(end);
        }

        GL.End();
        GL.PopMatrix();
        Debug.Log("[GridVisualizer] Finished drawing grid.");
    }
}