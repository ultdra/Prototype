using UnityEngine;

/// <summary>
/// Manages the playable area boundaries based on the ground object.
/// This script calculates the bounds and provides methods to clamp positions to stay within bounds.
/// </summary>
public class Ground : MonoBehaviour
{
    [Tooltip("Inset from the edges to create a margin (percentage of size)")]
    [SerializeField, Range(0, 0.5f)] private float m_BoundaryMargin = 0.02f;
    
    // The calculated boundaries
    private Bounds m_Bounds;
    
    // Singleton pattern for easy access
    private static Ground s_Instance;
    public static Ground Instance => s_Instance;

    /// <summary>
    /// Initialize the singleton and calculate the boundaries
    /// </summary>
    private void Awake()
    {
        // Set up singleton
        if (s_Instance != null && s_Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        s_Instance = this;
        
        // Calculate the boundaries on awake
        CalculateBounds();
    }
    
    /// <summary>
    /// Calculate the bounds based on the ground object
    /// </summary>
    private void CalculateBounds()
    {
        // Get the renderer component (assuming the ground has a renderer)
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            Debug.LogError("Ground: Ground object has no collider!");
            return;
        }
        
        // Get the bounds from the renderer
        m_Bounds = collider.bounds;
        
        // Apply the margin to create an inset from the edges
        Vector3 size = m_Bounds.size;
        float marginX = size.x * m_BoundaryMargin;
        float marginZ = size.z * m_BoundaryMargin;
        
        // Create a new bounds with the margin applied
        m_Bounds.size = new Vector3(size.x - (marginX * 2), size.y, size.z - (marginZ * 2));
    }
    
    /// <summary>
    /// Clamp a position to stay within the calculated bounds
    /// </summary>
    /// <param name="_position">The position to clamp</param>
    /// <returns>The clamped position</returns>
    public Vector3 ClampPositionToBounds(Vector3 _position)
    {
        // Get the min and max points of our bounds
        Vector3 min = m_Bounds.min;
        Vector3 max = m_Bounds.max;
        
        // Clamp X and Z coordinates (keep Y as is)
        float x = Mathf.Clamp(_position.x, min.x, max.x);
        float z = Mathf.Clamp(_position.z, min.z, max.z);
        
        return new Vector3(x, _position.y, z);
    }
    
    /// <summary>
    /// Debug draw the bounds in the scene view
    /// </summary>
    private void OnDrawGizmos()
    {
        // Only calculate in edit mode if bounds haven't been calculated yet
        if (Application.isPlaying == false)
            CalculateBounds();
            
        // Draw the bounds as a wire cube
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(m_Bounds.center, m_Bounds.size);
    }
}