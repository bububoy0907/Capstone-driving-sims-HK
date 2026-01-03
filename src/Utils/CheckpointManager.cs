using TMPro;
using UnityEngine;

/// <summary>
/// Attach this to a GameObject named "CheckpointManager".
/// Tracks which checkpoints are cleared, toggles route assists, and activates the final goal object.
/// Fixes the toggle bug by always toggling whichever route is "unlocked" (1 before checkpoint4, or 2 after).
/// </summary>
public class CheckpointManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag route section 1 here. Active by default in the scene.")]
    public GameObject routeSection1;

    [Tooltip("Drag route section 2 here. Inactive by default in the scene.")]
    public GameObject routeSection2;

    [Tooltip("Finish goal object. Inactive by default in the scene.")]
    public GameObject finishGoal;

    [Header("Checkpoint Settings")]
    [Tooltip("Total number of checkpoints required.")]
    public int totalCheckpoints = 6;
    public TMP_Text assist_text;
    private bool[] _checkpointsCleared;  // which checkpoints have been triggered
    private int _checkpointsCount = 0;   // how many unique checkpoints we've triggered

    private bool _section2Activated = false; // track if routeSection2 is unlocked at checkpoint4
    private bool _finishActivated = false;   // track if finishGoal is shown
    bool toggled_temp = true;
    void Start()
    {
        // Initialize array for 6 checkpoints => index 1..6
        _checkpointsCleared = new bool[totalCheckpoints + 1];

        // routeSection1 active, routeSection2 inactive, finishGoal inactive
        if (routeSection1) routeSection1.SetActive(true);
        if (routeSection2) routeSection2.SetActive(false);
        if (finishGoal) finishGoal.SetActive(false);
    }

    /// <summary>
    /// Called by CheckpointDetector when user crosses a checkpoint.
    /// </summary>
    public void MarkCheckpointCleared(int index)
    {
        // If index is out of range => ignore
        if (index < 1 || index > totalCheckpoints)
            return;

        // If we haven't cleared this checkpoint yet
        if (!_checkpointsCleared[index])
        {
            _checkpointsCleared[index] = true;
            _checkpointsCount++;
            Debug.Log($"Checkpoint {index} cleared. Total cleared = {_checkpointsCount}");

            // If index == 4 => we unlock routeSection2, disable routeSection1
            if (index == 4 && !_section2Activated)
            {
                if (routeSection1) routeSection1.SetActive(false);
                if (routeSection2) routeSection2.SetActive(true);
                _section2Activated = true;
                Debug.Log("Route section2 activated, section1 deactivated upon hitting checkpoint 4");
            }

            // If all 6 are cleared => show finishGoal
            if (_checkpointsCount >= totalCheckpoints && !_finishActivated)
            {
                if (finishGoal) finishGoal.SetActive(true);
                _finishActivated = true;
                Debug.Log("All checkpoints cleared => finish goal activated!");
            }
        }
    }

    /// <summary>
    /// Toggles the currently-unlocked route assist on/off.
    /// If we haven't hit checkpoint4, routeSection1 is the route. 
    /// If we have hit checkpoint4, routeSection2 is the route.
    /// This fixes the bug so you can turn a route off and on repeatedly.
    /// </summary>
    public void ToggleCurrentRoute()
    {
        toggled_temp = !toggled_temp;
        // If we haven't yet unlocked routeSection2 => routeSection1 is the relevant route
        if (!_section2Activated)
        {
            // routeSection1 is the only route
            if (!routeSection1) return; // safety check
            // toggle routeSection1
            bool isActive = routeSection1.activeSelf;
            routeSection1.SetActive(!isActive);
            Debug.Log($"Toggled routeSection1 => Now {(routeSection1.activeSelf ? "On" : "Off")}");
            if (toggled_temp == true)
            {
                assist_text.color = Color.green;
            } else
            {
                assist_text.color = Color.white;
            }
        }
        else
        {
            // routeSection2 is the route
            if (!routeSection2) return; // safety check
            bool isActive = routeSection2.activeSelf;
            routeSection2.SetActive(!isActive);
            Debug.Log($"Toggled routeSection2 => Now {(routeSection2.activeSelf ? "On" : "Off")}");
            if (toggled_temp == true)
            {
                assist_text.color = Color.green;
            }
            else
            {
                assist_text.color = Color.white;
            }
        }
    }
}
