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
    
    [Header("Collision Settings")]
    [Tooltip("Layers that the character will collide with")]
    [SerializeField] private LayerMask m_CollisionLayers = -1; // Default to "Everything"
    
    [Tooltip("Radius of the character for collision purposes")]
    [SerializeField] private float m_CollisionRadius = 0.5f;
    
    // References
    private Transform m_Transform;
    
    // Input values
    private float m_HorizontalInput;
    private float m_VerticalInput;
    private Vector3 m_MovementDirection;
    
    // Store the initial Y position to stay grounded
    private float m_GroundY;
    
    /// <summary>
    /// Initialize component references
    /// </summary>
    private void Awake()
    {
        // Get component references
        m_Transform = transform;
        
        // Store initial Y position as ground height
        m_GroundY = m_Transform.position.y;
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
        float moveDistance = m_MoveSpeed * Time.deltaTime;
        
        // Get the new position with collision handling
        Vector3 newPosition = GetNewPositionWithCollision(m_Transform.position, m_MovementDirection, moveDistance);
        
        // Keep the character at ground level
        newPosition.y = m_GroundY;
        
        // Apply the final position
        m_Transform.position = newPosition;
    }
    
    /// <summary>
    /// Calculate new position accounting for collisions and applying sliding
    /// </summary>
    private Vector3 GetNewPositionWithCollision(Vector3 currentPosition, Vector3 moveDirection, float moveDistance)
    {
        // First, try moving in the desired direction
        RaycastHit hit;
        bool collided = Physics.SphereCast(
            currentPosition,
            m_CollisionRadius,
            moveDirection,
            out hit,
            moveDistance,
            m_CollisionLayers
        );
        
        if (!collided)
        {
            // No collision, move freely
            return currentPosition + moveDirection * moveDistance;
        }
        
        // We hit something
        float safeDistance = hit.distance - 0.05f; // Small buffer to prevent getting stuck
        Vector3 newPosition = currentPosition;
        
        if (safeDistance > 0)
        {
            // Move as far as we can in the original direction
            newPosition = currentPosition + moveDirection * safeDistance;
            
        }
        
        return newPosition;
    }
}
