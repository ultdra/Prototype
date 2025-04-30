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
    private Rigidbody m_RigidBody;
    private Vector3 m_MovementDirection;

    private void Awake()
    {
        m_RigidBody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
                // Get input values
        m_HorizontalInput = Input.GetAxis("Horizontal");
        m_VerticalInput = Input.GetAxis("Vertical");

        m_MovementDirection = new Vector3(m_HorizontalInput, 0, m_VerticalInput).normalized;

        if(m_MovementDirection.magnitude > 0.1f)
        {
            Vector3 targetPosition = m_RigidBody.position + m_MovementDirection * (m_MoveSpeed * Time.fixedDeltaTime);

            m_RigidBody.MovePosition(targetPosition);
        }
    }
}