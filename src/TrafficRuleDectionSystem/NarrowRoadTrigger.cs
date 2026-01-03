using UnityEngine;

/// <summary>
/// Attach this script to a GameObject with a trigger collider. 
/// Whenever the player's car enters or exits, we'll set inNarrowZone accordingly.
/// </summary>
public class NarrowRoadTrigger : MonoBehaviour
{
    public TrafficRuleDetection traffic_rule_detection;
    void Start()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // The player's vehicle is now inside this narrow zone
            traffic_rule_detection.SetInNarrowZone(true);
            Debug.Log($"Entered narrow zone: {this.name}. Setting inNarrowZone = {traffic_rule_detection.GetInNarrowZone()}.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            traffic_rule_detection.SetInNarrowZone(false);
            Debug.Log($"Exited narrow zone: {this.name}. Setting inNarrowZone = {traffic_rule_detection.GetInNarrowZone()}.");
        }
    }
}
