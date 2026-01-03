using UnityEngine;

/// <summary>
/// Attach this script to your *camera* object.
/// It will read the steerInput from your RCC_CarControllerV3 and
/// slightly rotate the camera toward the direction of the turn.
/// 
/// This simulates a small "look" to the inside of the turn, a la Forza Horizon.
/// </summary>
public class SteeringCameraTilt : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the RCC Car Controller that we read the steering input from.")]
    public RCC_CarControllerV3 carController;

    [Header("Camera Tilt Settings")]
    [Tooltip("Maximum additional yaw (left/right) in degrees the camera will rotate when turning.")]
    public float maxTurnAngle = 5f;

    [Tooltip("How quickly the camera angle adjusts to changes in steering.")]
    public float rotationSmoothing = 5f;

    private Vector3 _originalLocalEuler;    // The camera's original local rotation
    private float _currentTiltAngle = 0f;   // Our current extra yaw offset

    void Start()
    {
        // Store the camera's starting rotation (local, so we can pivot around the local Y)
        _originalLocalEuler = transform.localEulerAngles;

        if (!carController)
        {
            // Attempt to find the active player car automatically (if desired).
            // Otherwise, you can just assign it manually in the Inspector.
            var possibleCar = FindObjectOfType<RCC_CarControllerV3>();
            if (possibleCar)
            {
                carController = possibleCar;
                Debug.Log("SteeringCameraTilt: Auto-found RCC_CarControllerV3 on " + possibleCar.name);
            }
            else
            {
                Debug.LogWarning("SteeringCameraTilt: No RCC_CarControllerV3 assigned or found. This script won't do anything.");
            }
        }
    }

    void Update()
    {
        if (!carController)
            return;

        // 1) Read the "steerInput" from the car. It's typically in -1..+1.
        float steerInput = carController.steerInput;

        // 2) Multiply by maxTurnAngle => desired extra yaw.
        float desiredAngle = steerInput * maxTurnAngle;

        // 3) Smoothly lerp _currentTiltAngle toward desiredAngle:
        _currentTiltAngle = Mathf.Lerp(_currentTiltAngle, desiredAngle, Time.deltaTime * rotationSmoothing);

        // 4) Apply the new local rotation around Y. We'll keep original pitch & roll.
        Vector3 newEuler = new Vector3(_originalLocalEuler.x,
                                       _originalLocalEuler.y + _currentTiltAngle,
                                       _originalLocalEuler.z);
        transform.localEulerAngles = newEuler;
    }
}
