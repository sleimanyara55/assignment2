using UnityEngine;
using System.Collections.Generic;

public class SnailMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float waypointThreshold = 0.1f;
    
    [Header("Path Settings")]
    [SerializeField] private Transform pathParent;
    [SerializeField] private bool debugPath = true;

    [Header("Animation")]
    [SerializeField] private Sprite[] animationFrames;
    [SerializeField] private float frameRate = 0.2f;
    
    private List<Vector3> waypointPositions;
    private int currentWaypointIndex = 0;
    private SpriteRenderer spriteRenderer;
    private float animationTimer;
    private int currentFrame;
    
    void Start()
    {
        InitializeComponents();
        InitializeWaypoints();
    }

    void InitializeComponents()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found on the snail GameObject!");
            enabled = false;
            return;
        }
    }
    
    void InitializeWaypoints()
    {
        waypointPositions = new List<Vector3>();
        
        if (pathParent == null)
        {
            Debug.LogError("Path Parent not assigned! Please assign a Path object in the inspector.");
            enabled = false;
            return;
        }
        
        foreach (Transform child in pathParent)
        {
            waypointPositions.Add(child.position);
        }
        
        if (waypointPositions.Count == 0)
        {
            Debug.LogError("No waypoints found in Path object!");
            enabled = false;
            return;
        }
        
        transform.position = waypointPositions[0];
    }
    
    void Update()
    {
        if (waypointPositions == null || waypointPositions.Count == 0) return;
        
        MoveSnail();
        UpdateAnimation();
    }

    void MoveSnail()
    {
        Vector3 targetPosition = waypointPositions[currentWaypointIndex];
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        
        // Rotate to face movement direction
        if (directionToTarget != Vector3.zero)
        {
            float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Move towards waypoint
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
        
        // Check if reached waypoint
        if (Vector3.Distance(transform.position, targetPosition) < waypointThreshold)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypointPositions.Count;
        }
    }

    void UpdateAnimation()
    {
        if (animationFrames == null || animationFrames.Length == 0) return;

        animationTimer += Time.deltaTime;
        if (animationTimer >= frameRate)
        {
            animationTimer = 0f;
            currentFrame = (currentFrame + 1) % animationFrames.Length;
            spriteRenderer.sprite = animationFrames[currentFrame];
        }
    }
    
    void OnDrawGizmos()
    {
        if (!debugPath || pathParent == null) return;
        
        Gizmos.color = Color.yellow;
        Transform[] pathWaypoints = pathParent.GetComponentsInChildren<Transform>();
        
        for (int i = 1; i < pathWaypoints.Length; i++)
        {
            Vector3 previousWaypoint = pathWaypoints[i - 1].position;
            Vector3 currentWaypoint = pathWaypoints[i].position;
            Gizmos.DrawLine(previousWaypoint, currentWaypoint);
        }
        
        if (pathWaypoints.Length > 2)
        {
            Gizmos.DrawLine(pathWaypoints[pathWaypoints.Length - 1].position, pathWaypoints[1].position);
        }
    }
}