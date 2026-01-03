using UnityEngine;

/// <summary>
/// Attach this script to the PLAYER vehicle. It handles:
/// 1) Collisions => records "StrikingObjects."
/// 2) Checking if user is too close to NPC vehicle in front => "FollowingTooClose."
/// 3) Checking if user is too close to sidewalk => "DrivingTooCloseSidewalk" (optionally use same or new rule).
/// </summary>
public class PlayerCollisionDetector : MonoBehaviour
{
    [Header("References")]
    public TrafficRuleDetection trafficRuleDetection;

    [Header("Driving Too Close Settings")]
    [Tooltip("Maximum distance for 'too close' to the vehicle in front.")]
    public float frontDistanceThreshold = 3.0f;

    [Tooltip("Maximum distance to sidewalk on left/right to consider 'too close'.")]
    public float sideDistanceThreshold = 1.5f;

    [Header("Sidewalk Cooldown")]
    [Tooltip("Cooldown (in seconds) before another 'too close to sidewalk' violation can trigger.")]
    public float sidewalkCooldown = 5f;

    [Header("Sidewalk Collision Cooldown")]
    [Tooltip("Cooldown (in seconds) before another 'collided with sidewalk' violation can trigger.")]
    public float sidewalkCollisionCooldown = 3f;

    // One-shot booleans
    private bool _wasTooCloseFront = false;
    private bool _wasTooCloseLeft = false;
    private bool _wasTooCloseRight = false;

    // Next time we can trigger a violation again for left or right sidewalk
    private float _nextAllowedLeft = 0f;
    private float _nextAllowedRight = 0f;

    // Next time we can trigger a sidewalk collision violation
    private float _nextAllowedSidewalkCollisionTime = 0f;

    void Start()
    {
        if (!trafficRuleDetection)
        {
            Debug.LogError("PlayerCollisionDetector: Missing reference to TrafficRuleDetection!");
        }
    }

    ///////////////////////////
    // 1) Collisions
    ///////////////////////////
    private void OnCollisionEnter(Collision collision)
    {
        // If collision velocity is small, skip
        if (collision.relativeVelocity.magnitude < 1f)
            return;

        if (!trafficRuleDetection)
            return;

        // Check tags
        if (collision.gameObject.CompareTag("sidewalk_wall"))
        {
            trafficRuleDetection.RecordViolation(
                RuleModule.StrikingObjects,
                "Collided with Sidewalk."
            );
            // set next cooldown
            _nextAllowedSidewalkCollisionTime = Time.time + sidewalkCollisionCooldown;
        }
        else if (collision.gameObject.CompareTag("NPC"))
        {
            trafficRuleDetection.RecordViolation(
                RuleModule.StrikingObjects,
                "Collided with Vehicle."
            );
        }
        else
        {
            trafficRuleDetection.RecordViolation(
                RuleModule.StrikingObjects,
                "Collided with Road Object."
            );
        }
    }

    ///////////////////////////
    // 2) Distance Checks
    ///////////////////////////
    void Update()
    {
        if (!trafficRuleDetection)
            return;

        CheckTooCloseFront();
        CheckTooCloseSidewalk();
    }

    /// <summary>
    /// Checks if there's an NPC directly in front within frontDistanceThreshold.
    /// We'll do a simple Raycast or SphereCast from the front bumper forward.
    /// </summary>
    private void CheckTooCloseFront()
    {
        bool isTooCloseFrontNow = false;

        // 1) define an origin near the front of the vehicle
        // Adjust offset as needed (car length, center pivot, etc.)
        Vector3 frontBumperPos = transform.position + transform.forward * 1.5f;
        Vector3 direction = transform.forward;
        float maxDistance = frontDistanceThreshold;

        // 2) optional: sphere radius or direct ray
        float sphereRadius = 0.5f; // half meter radius for a bigger detection
        RaycastHit hit;

        // Use Physics.SphereCast for a bit of breadth, so we detect center of NPC
        if (Physics.SphereCast(frontBumperPos, sphereRadius, direction, out hit, maxDistance))
        {
            // If the object is tagged NPC, consider it "too close in front"
            if (hit.collider.CompareTag("NPC"))
            {
                isTooCloseFrontNow = true;
            }
        }

        // One-shot logic
        if (isTooCloseFrontNow && !_wasTooCloseFront)
        {
            // Log or record the violation
            trafficRuleDetection.RecordViolation(
                RuleModule.FollowingTooClose,
                "Driving too close to the vehicle in front!"
            );
            _wasTooCloseFront = true;
        }
        else if (!isTooCloseFrontNow)
        {
            _wasTooCloseFront = false;
        }
    }

    /// <summary>
    /// Checks if the player is too close to the sidewalk on either left or right side.
    /// We'll do short Raycasts from left/right side to detect "sidewalk_wall" within sideDistanceThreshold.
    /// </summary>
    private void CheckTooCloseSidewalk()
    {
        bool isTooCloseLeftNow = false;
        bool isTooCloseRightNow = false;

        // We'll define left side origin a bit left from center, right side origin a bit right from center.
        Vector3 leftSideOrigin = transform.position + transform.right * -1f;
        Vector3 rightSideOrigin = transform.position + transform.right * 1f;

        float maxDist = sideDistanceThreshold;

        // left side
        RaycastHit leftHit;
        if (Physics.Raycast(leftSideOrigin, -transform.right, out leftHit, maxDist))
        {
            if (leftHit.collider.CompareTag("sidewalk_wall"))
            {
                isTooCloseLeftNow = true;
            }
        }

        // right side
        RaycastHit rightHit;
        if (Physics.Raycast(rightSideOrigin, transform.right, out rightHit, maxDist))
        {
            if (rightHit.collider.CompareTag("sidewalk_wall"))
            {
                isTooCloseRightNow = true;
            }
        }

        // One-shot logic + cooldown for left side
        if (isTooCloseLeftNow && !_wasTooCloseLeft)
        {
            // only log if time is past the next allowed timestamp
            if (Time.time >= _nextAllowedLeft)
            {
                trafficRuleDetection.RecordViolation(
                    RuleModule.FollowingTooClose, // or a new rule, e.g. "LaneDiscipline"
                    "Driving too close to the left sidewalk!"
                );
                // set next cooldown
                _nextAllowedLeft = Time.time + sidewalkCooldown;
            }
            _wasTooCloseLeft = true;
        }
        else if (!isTooCloseLeftNow)
        {
            // user no longer near left sidewalk => reset
            _wasTooCloseLeft = false;
        }

        // One-shot logic + cooldown for right side
        if (isTooCloseRightNow && !_wasTooCloseRight)
        {
            if (Time.time >= _nextAllowedRight)
            {
                trafficRuleDetection.RecordViolation(
                    RuleModule.FollowingTooClose,
                    "Driving too close to the right sidewalk!"
                );
                _nextAllowedRight = Time.time + sidewalkCooldown;
            }
            _wasTooCloseRight = true;
        }
        else if (!isTooCloseRightNow)
        {
            _wasTooCloseRight = false;
        }
    }
}
