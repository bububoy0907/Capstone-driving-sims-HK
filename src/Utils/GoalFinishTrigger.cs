using Ricimi;
using UnityEngine;

/// <summary>
/// Attach this script to the "GoalFinishPoint" collider in the driving scene.
/// When the user (tagged "Player") enters, we finalize the run and load the ResultScene.
/// </summary>
public class GoalFinishTrigger : MonoBehaviour
{
    [Tooltip("Reference to the TrafficRuleDetection script in the scene.")]
    public TrafficRuleDetection ruleDetection;

    [Tooltip("Reference to the RCC car controller, so we can freeze it immediately.")]
    public RCC_CarControllerV3 carController;

    [Tooltip("Name of the scene to load after finishing.")]
    public string resultSceneName = "ResultScene";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FreezeVehicle();
            ruleDetection.FinalizeAndLoadNextScene(resultSceneName);

        }
        
        
    }

    private void FreezeVehicle()
    {
        if (!carController)
            return;

        // Disable the user¡¦s control
        carController.canControl = false;

        // Zero out velocity
        Rigidbody rb = carController.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            // optional: to absolutely freeze
            rb.isKinematic = true;
        }

    }
}
