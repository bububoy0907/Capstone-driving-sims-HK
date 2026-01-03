using UnityEngine;

/// <summary>
/// Attach this to your "ParkingZone" box collider (IsTrigger = true).
/// Whenever the player enters, we mark 'isInParkingZone = true' in TrafficRuleDetection.
/// Whenever they exit, we mark it false.
/// </summary>
public class ParkingZoneTrigger : MonoBehaviour
{
    public TrafficRuleDetection trafficRuleDetection;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            trafficRuleDetection.SetIsInParkingZone(true);
            Debug.Log("Player entered ParkingZone => isInParkingZone = true");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            trafficRuleDetection.SetIsInParkingZone(false);
            Debug.Log("Player exited ParkingZone => isInParkingZone = false");
        }
    }
}
