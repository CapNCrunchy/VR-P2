using UnityEngine;

/// <summary>
/// Attach to a VR-grabbable object.
/// Call ShowPlacementGhost() to spawn a transparent preview on the ground in front of the player,
/// and HidePlacementGhost() / ConfirmPlacement() to remove or finalise it.
/// </summary>
public class SandCastleBucket : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The prefab to preview and place. Assign in the Inspector.")]
    public GameObject placementPrefab;

    [Tooltip("The player's camera or XR Rig camera transform used to determine 'in front'.")]
    public Transform playerCamera;

    [Header("Placement Settings")]
    [Tooltip("How far in front of the player (on the horizontal plane) the ghost appears.")]
    public float placementDistance = 1.5f;

    [Tooltip("Transparency alpha for the ghost (0 = invisible, 1 = opaque).")]
    [Range(0f, 1f)]
    public float ghostAlpha = 0.35f;

    [Tooltip("Color tint applied to the ghost material.")]
    public Color ghostTint = new Color(0.4f, 0.8f, 1f, 1f); // light-blue tint

    [Tooltip("Layer mask used when raycasting downward to snap the ghost to the ground.")]
    public LayerMask groundLayerMask = ~0;

    [Tooltip("Maximum ray distance when looking for the ground.")]
    public float groundRayMaxDistance = 10f;

    // ── private state ─────────────────────────────────────────────────────────
    private GameObject _ghost;          // the live preview instance
    private bool _ghostActive = false;

    // ── public API ────────────────────────────────────────────────────────────

    /// <summary>Spawns (or re-activates) the transparent placement ghost.</summary>
    public void ShowPlacementGhost()
    {
        if (placementPrefab == null)
        {
            Debug.LogWarning("[SandCastleBucket] No prefab assigned!", this);
            return;
        }

        if (_ghost == null)
        {
            _ghost = Instantiate(placementPrefab);
            MakeTransparent(_ghost);
            DisablePhysicsAndColliders(_ghost);
        }

        _ghost.SetActive(true);
        _ghostActive = true;
    }

    /// <summary>Hides and destroys the ghost without placing anything.</summary>
    public void HidePlacementGhost()
    {
        if (_ghost != null)
        {
            Destroy(_ghost);
            _ghost = null;
        }
        _ghostActive = false;
    }

    /// <summary>
    /// Places a real (non-transparent) instance of the prefab at the ghost's
    /// current position/rotation, then hides the ghost.
    /// </summary>
    public void ConfirmPlacement()
    {
        if (!_ghostActive || _ghost == null)
        {
            Debug.LogWarning("[SandCastleBucket] No active ghost to confirm.", this);
           //return null;
        }

        GameObject placed = Instantiate(placementPrefab, _ghost.transform.position, _ghost.transform.rotation);
        //HidePlacementGhost();
       // return placed;
    }

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        // Fall back to main camera if none assigned
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;
    }

    private void Update()
    {
        if (_ghostActive && _ghost != null)
            UpdateGhostTransform();
    }

    private void OnDestroy()
    {
        // Clean up ghost if the source object is destroyed while ghost is active
        if (_ghost != null)
            Destroy(_ghost);
    }

    // ── private helpers ───────────────────────────────────────────────────────

    /// <summary>Moves the ghost to the ground-snapped position in front of the player each frame.</summary>
    private void UpdateGhostTransform()
    {
        if (playerCamera == null) return;

        // Project camera forward onto the horizontal plane
        Vector3 flatForward = playerCamera.forward;
        flatForward.y = 0f;
        if (flatForward.sqrMagnitude < 0.001f)
            flatForward = playerCamera.up; // edge case: looking straight up/down
        flatForward.Normalize();

        // Candidate position in front of the player at camera height
        Vector3 candidateOrigin = playerCamera.position + flatForward * placementDistance;

        // Raycast downward to snap to the ground
        Ray downRay = new Ray(candidateOrigin + Vector3.up * 2f, Vector3.down);
        Vector3 groundPosition;

        if (Physics.Raycast(downRay, out RaycastHit hit, groundRayMaxDistance, groundLayerMask))
            groundPosition = hit.point;
        else
            groundPosition = new Vector3(candidateOrigin.x, 0f, candidateOrigin.z); // fallback to y=0

        // Offset upward so the bottom of the ghost's bounds sits on the ground
        // rather than the pivot being buried at the hit point
        float heightOffset = GetGhostGroundOffset();
        _ghost.transform.position = groundPosition + Vector3.up * heightOffset;


        // Rotate ghost to face the same horizontal direction as the player
        _ghost.transform.rotation = Quaternion.Euler(0f, playerCamera.eulerAngles.y, 0f);
    }
    /// <summary>
    /// Returns the Y offset needed to lift the ghost so its lowest point sits on the ground.
    /// Uses the combined renderer bounds; falls back to zero if no renderers are found.
    /// </summary>
    private float GetGhostGroundOffset()
    {
        Renderer[] renderers = _ghost.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return 0f;

        // Compute combined world-space bounds across all renderers
        Bounds combined = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            combined.Encapsulate(renderers[i].bounds);

        // Distance from the ghost's pivot (its transform.position) down to the bottom of the bounds
        float pivotToBottom = _ghost.transform.position.y - combined.min.y;
        return pivotToBottom;
    }


    /// <summary>Replaces all Renderer materials on the ghost with transparent versions.</summary>
    private void MakeTransparent(GameObject target)
    {
        foreach (Renderer rend in target.GetComponentsInChildren<Renderer>())
        {
            // Duplicate materials so we don't modify shared assets
            Material[] mats = rend.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                Material m = new Material(mats[i]);
                //SetMaterialTransparent(m);
                Color c = m.color;
                c.a = ghostAlpha;
                m.color = new Color(c.r * ghostTint.r, c.g * ghostTint.g, c.b * ghostTint.b, ghostAlpha);
                mats[i] = m;
            }
            rend.materials = mats;
        }
    }



    /// <summary>Disables Rigidbodies and Colliders on the ghost so it doesn't affect physics.</summary>
    private static void DisablePhysicsAndColliders(GameObject target)
    {
        foreach (Rigidbody rb in target.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }
        foreach (Collider col in target.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }
}
