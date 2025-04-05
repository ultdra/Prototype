using UnityEngine;

/// <summary>
/// Controls character movement in a top-down/isometric style similar to Cult of the Lamb or Don't Starve.
/// Character stays grounded and moves in the direction of input while visual rotation can be separate.
/// Handles collisions with obstacles like trees and enables sliding along surfaces.
/// </summary>
public class CharacterMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed in units per second")]
    [SerializeField] private float m_MoveSpeed = 5.0f;
    
    [Header("Boundary Settings")]
    [Tooltip("Reference to the Ground script")]
    [SerializeField] private Ground m_Ground;
    
    // Input values
    private float m_HorizontalInput;
    private float m_VerticalInput;
    private Vector3 m_MovementDirection;
    
    /// <summary>
    /// Initialize component references
    /// </summary>
    private void Awake()
    {
        // If no ground reference is set, try to find it in the scene
        if (m_Ground == null)
        {
            m_Ground = FindObjectOfType<Ground>();
            if (m_Ground == null)
            {
                Debug.LogWarning("CharacterMovementController: No Ground script found in scene. Player movement will not be bounded.");
            }
        }
    }
    
    /// <summary>
    /// Process input and apply movement
    /// </summary>
    private void Update()
    {
        // Get input values
        m_HorizontalInput = Input.GetAxis("Horizontal");
        m_VerticalInput = Input.GetAxis("Vertical");
        
        // Calculate movement
        CalculateMovement();
    }
    
    /// <summary>
    /// Calculate and apply movement based on input with collision detection and sliding
    /// </summary>
    private void CalculateMovement()
    {
        // For top-down/isometric movement, we use world space directly
        m_MovementDirection = new Vector3(m_HorizontalInput, 0, m_VerticalInput).normalized;
        
        // Skip if no movement input
        if (m_MovementDirection.magnitude <= 0.1f)
            return;
        
        // Calculate the distance to move this frame
        Vector3 newPosition = transform.position + m_MovementDirection * (m_MoveSpeed * Time.deltaTime);
        
        // Clamp position to stay within ground boundaries if we have a reference to the ground
        if (m_Ground != null)
        {
            newPosition = m_Ground.ClampPositionToBounds(newPosition);
        }
        
        // Apply the final position
        transform.position = newPosition;
    }
}