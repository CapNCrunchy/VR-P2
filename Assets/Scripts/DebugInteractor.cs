using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attach to Main Camera. Left-click to interact with trees and collectibles
/// without a VR headset. Remove this script before a real VR build.
/// </summary>
public class DebugInteractor : MonoBehaviour
{
    public float interactRange = 50f;

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            TryInteract();
    }

    void TryInteract()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        Debug.Log($"[DebugInteractor] Ray cast from {ray.origin}");

        if (!Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            Debug.Log("[DebugInteractor] Raycast hit nothing");
            return;
        }

        Debug.Log($"[DebugInteractor] Hit: {hit.collider.gameObject.name} (parent: {hit.collider.transform.parent?.name})");

        // Check for collectible on the hit object or its parent
        Collectible collectible = hit.collider.GetComponentInParent<Collectible>();
        if (collectible != null && collectible.gameObject.activeSelf)
        {
            Debug.Log("[DebugInteractor] Collecting: " + collectible.gameObject.name);
            CollectibleManager.Instance?.OnCollect();
            collectible.gameObject.SetActive(false);
            return;
        }

        // Check for tree shake on the hit object or its parent
        TreeShake tree = hit.collider.GetComponentInParent<TreeShake>();
        if (tree != null)
        {
            Debug.Log("[DebugInteractor] Shaking tree: " + tree.gameObject.name);
            tree.TriggerShake();
            return;
        }

        Debug.Log("[DebugInteractor] Hit object has no Collectible or TreeShake component");
    }
}
