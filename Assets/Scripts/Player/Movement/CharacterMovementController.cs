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
    [Tooltip("Distance to check for collisions")]
    [SerializeField] private float m_CollisionCheckDistance = 0.1f;
    [Tooltip("Layer mask for boundary colliders")]
    [SerializeField] private LayerMask m_BoundaryLayer;
    
    private float m_HorizontalInput;
    private float m_VerticalInput;
    private Vector3 m_MovementDirection;
    
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
    /// Calculate and apply movement based on input with collision detection
    /// </summary>
    private void CalculateMovement()
    {
        // For top-down/isometric movement, we use world space directly
        m_MovementDirection = new Vector3(m_HorizontalInput, 0, m_VerticalInput).normalized;
        
        // Skip if no movement input
        if (m_MovementDirection.magnitude <= 0.1f)
            return;
            
        // Check for boundary collision and draw debug ray
        bool hitBoundary = Physics.Raycast(transform.position, m_MovementDirection, m_CollisionCheckDistance, m_BoundaryLayer);
        Debug.DrawRay(transform.position, m_MovementDirection * m_CollisionCheckDistance, 
                     hitBoundary ? Color.red : Color.green);
            
        if (!hitBoundary)
        {
            // Calculate the distance to move this frame
            Vector3 newPosition = transform.position + m_MovementDirection * (m_MoveSpeed * Time.deltaTime);
            
            // Apply the final position
            transform.position = newPosition;
        }
    }
}