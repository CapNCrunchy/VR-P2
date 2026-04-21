using UnityEngine;

/// <summary>
/// Spawns the player into the grass field scene.
/// Attach this to the same empty GameObject as GrassFieldSceneSetup,
/// or a separate one — doesn't matter.
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
    void Start()
    {
        // Create player capsule
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.transform.position = new Vector3(0f, 2f, 0f); // spawn above terrain so gravity settles it

        // Remove the default mesh renderer (invisible player in first person)
        Renderer r = player.GetComponent<Renderer>();
        if (r != null) r.enabled = false;

        // Add CharacterController
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0f, 0f, 0f);

        // Add our controller script
        player.AddComponent<RelaxedPlayerController>();
    }
}
