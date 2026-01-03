using UnityEngine;

/// <summary>
/// Attach this script to the player vehicle so it can detect triggers for wrong side / lane boundary colliders.
///
/// If user enters "WrongSideLane" => we toggle _isOnWrongSide in TrafficRuleDetection
/// If user enters "LaneBoundary":
///    - No signals => SignalingErrors
///    - Has signal => check user looked in correct direction(s). If not => FailToCheckTrafficConditions
/// We also have a cooldown to prevent multiple triggers in quick succession.
/// </summary>
public class LaneColliderCheck : MonoBehaviour
{
    public TrafficRuleDetection trafficRuleDetection;

    [Header("Cooldown Settings")]
    [Tooltip("Minimum time between boundary-trigger violations (in seconds).")]
    public float boundaryCooldown = 0.5f;

    public GameObject goal_point;
    public GameObject reverse1;
    public GameObject reverse2;
    public GameObject reverse3;
    public GameObject reverse4;
    public GameObject reverse5;
    public GameObject reverse6;
    public GameObject reverse7;
    public GameObject extra_region;
    private void Update()
    {
        if (goal_point.activeSelf == true)
        {
            reverse1.SetActive(false);
            reverse2.SetActive(false);
            reverse3.SetActive(false);
            reverse4.SetActive(false);
            reverse5.SetActive(false);
            reverse6.SetActive(false);
            reverse7.SetActive(false);
            extra_region.SetActive(false);
        } else
        {
            reverse1.SetActive(true);
            reverse2.SetActive(true);
            reverse3.SetActive(true);           
            reverse4.SetActive(true);
            reverse5.SetActive(true);
            reverse6.SetActive(true);
            reverse7.SetActive(true);
            extra_region.SetActive(true);
        }
    }

    // internal tracking for next allowable time
    private float _nextBoundaryAllowedTime = 0f;

    private void OnTriggerEnter(Collider other)
    {
        /*
        // Make sure it's the Player
        if (!other.CompareTag("Player")) {
            Debug.Log("Returned at Player Tag");
            return;
        }
        */ 
        

        if (other.CompareTag("WrongSideLane"))
        {
            Debug.Log("Entered the WrongSideLane Collider");
            // Mark that we are on the wrong side
            trafficRuleDetection.SetWrongSide(true);
            //return;
        }
        else if (other.CompareTag("LaneBoundary"))
        {
            // Check cooldown
            if (Time.time < _nextBoundaryAllowedTime)
                return;

            // Next time we can trigger again
            _nextBoundaryAllowedTime = Time.time + boundaryCooldown;

            // 1) Are signals on?
            bool leftSignalOn = (trafficRuleDetection.dataManager.LeftAndRightLighting[0].color == Color.red);
            bool rightSignalOn = (trafficRuleDetection.dataManager.LeftAndRightLighting[1].color == Color.red);

            // 2) Did user look recently?
            float now = Time.time;
            float lookWindow = trafficRuleDetection.GetLookTimeWindow();
            bool lookedLeft = (now - trafficRuleDetection.GetLastLookLeftTime() <= trafficRuleDetection.GetLookTimeWindow());
            bool lookedRight = (now - trafficRuleDetection.GetLastLookRightTime() <= trafficRuleDetection.GetLookTimeWindow());


            // 3) Are we in a narrow zone => need to look both ways if any signal is on
            bool inNarrow = trafficRuleDetection.GetInNarrowZone();

            // If no signals => immediate Signaling violation
            if (!leftSignalOn && !rightSignalOn)
            {
                trafficRuleDetection.RecordImmediateViolation(
                    RuleModule.SignalingErrors,
                    "Crossed lane boundary without using any signal."
                );
                return;
            }

            // If user has a left signal -> must look left (and if narrow => also right).
            if (leftSignalOn)
            {
                bool extraOk = true;
                if (inNarrow)
                    extraOk = lookedRight; // also look right if narrow

                if (!lookedLeft || !extraOk)
                {
                    trafficRuleDetection.RecordImmediateViolation(
                        RuleModule.FailToCheckTrafficConditions,
                        "Crossed lane boundary with left signal but failed to look properly."
                    );
                }
            }

            // If user has a right signal -> must look right (and if narrow => also left).
            if (rightSignalOn)
            {
                bool extraOk = true;
                if (inNarrow)
                    extraOk = lookedLeft;

                if (!lookedRight || !extraOk)
                {
                    trafficRuleDetection.RecordImmediateViolation(
                        RuleModule.FailToCheckTrafficConditions,
                        "Crossed lane boundary with right signal but failed to look properly."
                    );
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        /*
        if (!other.CompareTag("Player"))
            return;
        */

        if (other.CompareTag("WrongSideLane"))
        {
            // left the wrong side zone
            trafficRuleDetection.SetWrongSide(false);
        }
        else if (other.CompareTag("LaneBoundary"))
        {
            // if you want to mark drifting out-of-lane as false
           // trafficRuleDetection.SetLaneBoundary(false);
        }
    }
}
