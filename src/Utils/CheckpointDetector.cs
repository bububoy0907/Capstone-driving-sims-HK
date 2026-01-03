using UnityEngine;

/// <summary>
/// Attach this script to each checkpoint collider (1..6).
/// </summary>
public class CheckpointDetector : MonoBehaviour
{
    [Tooltip("Set a unique index for each checkpoint (1..6).")]
    public int checkpointIndex = 1;

    [Tooltip("Reference to the single CheckpointManager in the scene.")]
    public CheckpointManager checkpointManager;

    private void OnTriggerEnter(Collider other)
    {
        // Only if the player enters
        if (!other.CompareTag("Player"))
            return;

        // Mark this checkpoint as cleared
        checkpointManager.MarkCheckpointCleared(checkpointIndex);
    }
}
