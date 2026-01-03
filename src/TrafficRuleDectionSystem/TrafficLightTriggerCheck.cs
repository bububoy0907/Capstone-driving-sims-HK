using UnityEngine;

public class TrafficLightTriggerCheck : MonoBehaviour
{
    public GameObject redLightOn;
    public GameObject yellowLightOn;
    public GameObject greenLightOn;

    public TrafficRuleDetection trafficRuleDetection;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // If red or yellow light is active, flag a violation
        if ((redLightOn != null && redLightOn.activeSelf) ||
            (yellowLightOn != null && yellowLightOn.activeSelf))
        {
            trafficRuleDetection.SetTrafficLightViolationCheck(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Clear the violation after player exits the intersection zone
        trafficRuleDetection.SetTrafficLightViolationCheck(false);
    }
}
