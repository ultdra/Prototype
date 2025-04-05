using UnityEngine;

/// <summary>
/// Controls character movement in a top-down/isometric style similar to Cult of the Lamb or Don't Starve.
/// Character stays grounded and moves in the direction of input while visual rotation can be separate.
/// </summary>
public class CharacterMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed in units per second")]
    [SerializeField] private float m_MoveSpeed = 5.0f;
    
    [Tooltip("Whether the character should face the direction of movement")]
    [SerializeField] private bool m_FaceMovementDirection = true;
    
    [Tooltip("How quickly the character rotates to face movement direction (degrees per second)")]
    [SerializeField] private float m_RotationSpeed = 720.0f;
    
    // References
    private Transform m_Transform;
    private SpriteRenderer m_SpriteRenderer;
    
    // Input values
    private float m_HorizontalInput;
    private float m_VerticalInput;
    private Vector3 m_MovementDirection;
    
    // Store the initial Y position to stay grounded
    private float m_GroundY;
    
    // Last non-zero movement direction for facing
    private Vector3 m_LastMovementDirection;
    
    /// <summary>
    /// Initialize component references
    /// </summary>
    private void Awake()
    {
        // Get component references
        m_Transform = transform;
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
        
        // Store initial Y position as ground height
        m_GroundY = m_Transform.position.y;
        
        // Initialize last movement direction
        m_LastMovementDirection = new Vector3(0, 0, 1); // Default facing forward
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
        
        // Update character facing if enabled
        if (m_FaceMovementDirection)
        {
            UpdateCharacterFacing();
        }
    }
    
    /// <summary>
    /// Calculate and apply movement based on input
    /// </summary>
    private void CalculateMovement()
    {
        // For top-down/isometric movement, we use world space directly
        m_MovementDirection = new Vector3(m_HorizontalInput, 0, m_VerticalInput).normalized;
        
        // Only update last direction if we have meaningful input
        if (m_MovementDirection.magnitude > 0.1f)
        {
            m_LastMovementDirection = m_MovementDirection;
        }
        
        // Apply movement - keeping the character at ground level
        Vector3 newPosition = m_Transform.position + m_MovementDirection * m_MoveSpeed * Time.deltaTime;
        newPosition.y = m_GroundY; // Keep the character at ground level
        
        // Update position
        m_Transform.position = newPosition;
    }
    
    /// <summary>
    /// Update the character visual facing based on movement
    /// </summary>
    private void UpdateCharacterFacing()
    {
        // Only update rotation if we have a valid last movement direction
        if (m_LastMovementDirection.magnitude > 0.1f)
        {
            // Ensure the character stays upright (no tilt)
            Vector3 eulerAngles = m_Transform.eulerAngles;
            eulerAngles.x = 30f; // Keep the X rotation at 30 which matches the original setup
            m_Transform.eulerAngles = eulerAngles;
        }
    }
}
