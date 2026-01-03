using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using Ricimi;

public enum RuleModule
{
    FailToCheckTrafficConditions,  // For checking mirrors / environment
    UnintendedRolling,             // Rolling backwards/forwards incorrectly
    StrikingObjects,               // Collisions with objects (other than normal collisions)
    FollowingTooClose,
    ImproperStoppingOrParking,     // Stopping / parking in wrong position
    SignalingErrors,               // Omit signal, fail to signal in good time, wrong signal
    GearHandbrakeIssues,           // Neutral rolling, not using handbrake on slope, etc.
    SpeedControl,                  // Over speed limit, or failing to adjust
    LaneDiscipline,                // Wrong side driving, lane drifting, tailgating, etc.
    TrafficLaws,                   // Traffic signals, yield signs, crossing, etc.
}

/// <summary>
/// Demonstration rules detection script that references RCC_CarControllerV3,
/// AutomotiveDataVisualizationManager, and spawns UI alerts for each violation.
/// Follows the one-shot approach, continuous violations won't spam multiple times.
/// </summary>
public class TrafficRuleDetection : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Main car controller (RCC_CarControllerV3).")]
    public RCC_CarControllerV3 carController;

    [Tooltip("Data manager for G29 Steering, signals, etc.")]
    public AutomotiveDataVisualizationManager dataManager;

    [Header("UI / Prefabs")]
    [Tooltip("Parent with a VerticalLayoutGroup for stacking multiple alerts.")]
    public Transform alertParent;

    [Tooltip("Prefab for each violation alert. Must have a child TextMeshProUGUI.")]
    public GameObject alertPrefab;

    [Header("Rule Violation Settings")]
    [Tooltip("Number of total violations before user is considered failed.")]
    public int maxAllowedViolations = 4;

    // Track how many times each module was violated
    private Dictionary<RuleModule, int> _violationsCount = new Dictionary<RuleModule, int>();

    // Keep logs of each violation message, e.g. "[SpeedControl] Speeding at 60.0"
    private List<string> _violationLogs = new List<string>();

    // Whether the user has crossed the fail threshold
    private bool _isTestFailed = false;

    // For transferring final stats to another scene
    public static Dictionary<RuleModule, int> FinalViolationData = new Dictionary<RuleModule, int>();
    public static bool FinalResultFailed { get; private set; }
    public static List<string> FinalViolationLogs = new List<string>();

    // Dictionary to ensure we only log once per continuous violation
    private Dictionary<RuleModule, bool> _currentlyViolating = new Dictionary<RuleModule, bool>();

    // -----------------------
    private float _steerThresholdForTurn_M1 = 0.3f; // or reuse the same as module5
    private float _turnCommitTime_M1 = 0.5f;        // hold steering for 0.5s => actual turn
    private float _turnTimer_M1 = 0f;
    private int _turnSign_M1 = 0; // +1 => right, -1 => left, 0 => none

    private bool _didStartOffCheck = false;
    // track time of last "look left/right" press
    private static float lastLookLeftTime = -999f;
    private static float lastLookRightTime = -999f;
    // How many seconds after pressing D-Pad is it still valid
    private float lookTimeWindow = 35f;
    private float BothlookTimeWindow = 35f;
    public float GetLastLookLeftTime() { return lastLookLeftTime; }
    public float GetLastLookRightTime() { return lastLookRightTime; }
    public float GetLookTimeWindow() { return lookTimeWindow; }

    private bool inNarrowZone = false;
    public void SetInNarrowZone(bool val) { inNarrowZone = val; }
    public bool GetInNarrowZone() { return inNarrowZone; }

    // Time spent at near-zero speed
    private float _timeStopped = 0f;
    // If speed < 1 km/h for 2 seconds, treat that as a "full stop"
    private float stopThreshold = 2f;

    // Additional needed for module4 logic
    private bool _isInParkingZone = false;  // Toggled by ParkingZoneTrigger
    private float _timeOutsideZone = 0f;
    private bool _wasViolation = false;

    /////////////////////////////////////////////
    // MODULE 5: Signal-Related Errors
    /////////////////////////////////////////////
    // Tuning parameters:
    private float _speedMinForTurning = 5f;    // must be going >5 km/h to consider a real turn
    private float _steerThresholdForTurn = 0.3f;  // steering beyond ±0.3 => potential turn
    private float _turnCommitTime = 0.5f;  // must hold that steering for 0.5s to confirm turn
    private float _brakeIgnoreThreshold = 0.4f;  // if brake >0.4 and user not accelerating => skip turn detection

    private float _speedMinForLaneChange = 10f;   // must be >10 km/h for lane changes
    private float _maxSteerForLaneChange = 0.2f;  // lane change only if steering < ±0.2
    private float _laneChangeThreshold = 1.0f;  // must drift >1.0m sideways
    private float _laneCommitTime = 0.3f;  // user must hold small steering for 0.3s

    // Working vars
    private float _turnTimer = 0f;   // how long we've held a large steering angle
    private int _turnSign = 0;    // +1 => right, -1 => left, 0 => none
    private float _laneTimer = 0f;   // how long we've had small steering
    private float _accumulatedLateral = 0f;  // track net sideways drift
    private Vector3 _lastPosition;          // store last frame's position

    // Module 6 fields
    private int _lastGearInput = 1;                 // or whatever gear sim starts with
    private float _speedThresholdForGearChange = 2f; // Must be below 2 km/h to safely change gear

    private float _pedalPressThreshold = 0.1f;      // if both throttle & brake > 0.1 => violation
    private bool _pedalsInitialized = false;        // to skip checking if throttle/brake both default to 0.5
    private float _defaultPedalValue = 0.5f;        // default
    private float _toleranceForDefault = 0.01f;     // if within ±0.01 of 0.5, consider it default

    //////////////////////////////////////////
    // MODULE 8: Lane / Spacing / Positioning
    //////////////////////////////////////////
    private bool _isOnWrongSide = false;
    private bool _isDriftingOutOfLane = false; //  drifting checks

    // module 9:
    private bool _ranRedOrYellowLight = false;

    private void Start()
    {
        // Initialize each module’s count and violation state
        foreach (RuleModule mod in System.Enum.GetValues(typeof(RuleModule)))
        {
            _violationsCount[mod] = 0;
            _currentlyViolating[mod] = false;
        }

        _isTestFailed = false;
        _lastPosition = carController.transform.position;
        _lastGearInput = dataManager.GearInput;
    }

    private void Update()
    {
        if (!carController || !dataManager)
            return; // References not set

        // ----- Start calling rule checks in each Update -----
        CheckFailToCheckTrafficConditions();
        CheckUnintendedRolling();
        CheckStoppingParking();
        CheckSignalingErrors();
        CheckGearHandbrakeUsage();
        CheckSpeedControl();
        CheckLaneDiscipline();
        CheckTrafficLaws();

        // The "StrikingObjects" or collisions is handled in OnCollisionEnter below
    }

    #region Rule Modules

    #region Module 1: Fail to Check Traffic Conditions
    /// <summary>
    /// 1) Fail to Check Traffic Conditions:
    ///    e.g. not checking mirrors or environment before starting, stopping, reversing, turning, etc.
    /// </summary>
    private void CheckFailToCheckTrafficConditions()
    {
        bool isViolating = false;

        ///////////////////////////////////////////////////////
        // PART A: SIGNAL-BASED LOOK CHECK
        ///////////////////////////////////////////////////////
        // If the user’s left signal is on => must have looked left recently
        // If the user’s right signal is on => must have looked right recently
        // If in narrow zone => must have looked both
        float steering = dataManager.SteeringWheel; // -1..+1
        float brake = dataManager.Brake;
        float speed = carController.speed;

        float now = Time.time;
        bool lookedLeftRecently = ((now - lastLookLeftTime) <= lookTimeWindow);
        bool lookedRightRecently = ((now - lastLookRightTime) <= lookTimeWindow);

        // We skip the turn detection if user is effectively stopping => brake>some threshold, speed<some threshold
        // (like in your old code for module5). Tweak as needed:
        if (speed > _speedMinForTurning && brake < _brakeIgnoreThreshold)
        {
            int sign = 0;
            if (steering > _steerThresholdForTurn_M1) sign = +1;
            if (steering < -_steerThresholdForTurn_M1) sign = -1;

            if (sign == 0)
            {
                // reset
                _turnTimer_M1 = 0f;
                _turnSign_M1 = 0;
            }
            else
            {
                // same direction as last frame => accumulate
                if (sign == _turnSign_M1)
                {
                    _turnTimer_M1 += Time.deltaTime;
                }
                else
                {
                    // changed direction => reset
                    _turnTimer_M1 = 0f;
                    _turnSign_M1 = sign;
                }

                // once we hold that direction for 0.5s => real turn
                if (_turnTimer_M1 >= _turnCommitTime_M1)
                {
                    // if turning left => must look left
                    if (sign == -1)
                    {
                        bool extraOk = true;
                        if (inNarrowZone)
                            extraOk = lookedRightRecently; // need to look both ways

                        if (!lookedLeftRecently || !extraOk)
                        {
                            isViolating = true;
                        }
                    }
                    // if turning right => must look right
                    if (sign == +1)
                    {
                        bool extraOk = true;
                        if (inNarrowZone)
                            extraOk = lookedLeftRecently; // need to look both ways

                        if (!lookedRightRecently || !extraOk)
                        {
                            isViolating = true;
                        }
                    }
                }
            }
        }
        else
        {
            _turnTimer_M1 = 0f;
            _turnSign_M1 = 0;
        }

        ///////////////////////////////////////////////////////
        // PART B: STARTOFF CHECK (one-time, only in ParkingZone)
        ///////////////////////////////////////////////////////
        // If in ParkingZone, user is fully stopped for >2s, 
        // then throttle >0.1 => must look BOTH ways. 
        // Only do this once.
        if (!_didStartOffCheck && _isInParkingZone)
        {
            //float speed = carController.speed;
            float throttle = dataManager.Throttle;
            bool wasFullyStopped = (_timeStopped >= 2f); // or use stopThreshold
            //Debug.Log("Part B speed value: " + speed);
            //Debug.Log("Part B throttle value: " + throttle);
            // Condition: user tries to move off from near-zero speed
            bool isStartingOff = (speed > 2f);
            //Debug.Log("isStartingOff value: " + isStartingOff);
            if (isStartingOff)
            {
                // Must have looked BOTH ways
                if (!lookedLeftRecently || !lookedRightRecently)
                {
                    Debug.Log("Module 1: Part B violation triggered");
                    isViolating = true; // “didn’t check both sides before starting off”
                }
                _didStartOffCheck = true; // we do not repeat
            }
        }

        ///////////////////////////////////////////////////////
        // PART C: REVERSE CHECK
        ///////////////////////////////////////////////////////
        // If gear=R, speed<2, user pressing throttle => must look BOTH ways
        int gear = dataManager.GearInput; // -1 => R
        float brakeVal = dataManager.Brake;
        float thrVal = dataManager.Throttle;

        // If gear=R, speed <2 => reversing from standstill
        if (gear == -1 && carController.speed < 2f)
        {
            // If user tries to accelerate in reverse
            if (thrVal > 0.1f)
            {
                // Must have looked BOTH ways
                if (!lookedLeftRecently || !lookedRightRecently)
                {
                    Debug.Log("Module 1: Part C violation triggered");
                    isViolating = true;
                }
            }
        }

        ///////////////////////////////////////////////////////
        // PART D: FINAL
        ///////////////////////////////////////////////////////
        CheckOneShotViolation(
            RuleModule.FailToCheckTrafficConditions,
            isViolating,
            "Failed to check surrounding environment before maneuvering."
        );
    }

    /// <summary>
    /// We require the user to look both left and right in 3 special narrow-road areas.
    /// Suppose we have triggers or zone checks. This is just a placeholder.
    /// Return true if user is in one of those 3 narrow roads needing special checks.
    /// </summary>
    private bool IsInSpecialNarrowRoadZone()
    {
        return inNarrowZone;
    }

    /// <summary>
    /// Helper to see if user has looked left or right in last 4.5s
    /// (Used for the 'starting off from standstill' example.)
    /// </summary>
    private bool HasLookedAtLeastOneSide()
    {
        float t = Time.time;
        bool leftOk = (t - lastLookLeftTime <= lookTimeWindow);
        bool rightOk = (t - lastLookRightTime <= lookTimeWindow);

        return (leftOk || rightOk);
    }

    // -------------------------------------------------------------------------
    // Called by G29 code: user pressed D-Pad left => user is "looking left"
    // -------------------------------------------------------------------------
    public static void RegisterLookLeft()
    {
        lastLookLeftTime = Time.time;
    }

    // -------------------------------------------------------------------------
    // Called by G29 code: user pressed D-Pad right => user is "looking right"
    // -------------------------------------------------------------------------
    public static void RegisterLookRight()
    {
        lastLookRightTime = Time.time;
    }

    #endregion

    #region Module 2: Unintended Rolling
    /// <summary>
    /// 2) Unintended Rolling:
    ///    If the vehicle is supposed to be stationary (on slope, or starting) but it drifts away.
    /// </summary>
    private void CheckUnintendedRolling()
    {
        bool isViolating = false;

        // 1) Determine slope angle
        // We'll measure how many degrees from horizontal the car's forward vector is.
        // E.g. if slopeAngle > 3 => “on slope”
        float slopeAngle = GetSlopeAngle();
        bool onSlope = (slopeAngle > 3f);  // Adjust as needed

        // 2) Check gear and velocity
        int gear = dataManager.GearInput;         // -2 => P/N, -1 => R, +1 => D
        float speedKMH = carController.speed;     // forward speed in km/h
        float brake = dataManager.Brake;          // 0..1
        float throttle = dataManager.Throttle;    // 0..1

        // If not on slope, we might skip the rolling check or you can still allow
        // a minimal rolling check. We'll do slope-based approach now:
        if (onSlope)
        {
            // A) If gear = D => user is intended to move forward. If speedKMH < -0.5 => rolling backward
            //    AND brake < threshold => user is not trying to hold brake => violation
            if (gear == 1)  // D
            {
                if (speedKMH < -0.5f && brake < 0.2f)
                {
                    isViolating = true;
                }
            }
            // B) If gear = R => user is intended to move backward. If speedKMH > 1 => rolling forward
            //    AND brake < threshold => violation
            else if (gear == -1)
            {
                if (speedKMH > 1f && brake < 0.2f)
                {
                    isViolating = true;
                }
            }
            // C) If gear = P/N => user is expecting no movement. If speedKMH > 2 => rolling
            //    AND brake < threshold => violation
            else if (gear == -2)
            {
                if (speedKMH > 2f && brake < 0.2f)
                {
                    isViolating = true;
                }
            }
        }

        CheckOneShotViolation(RuleModule.UnintendedRolling, isViolating,
            "Vehicle is rolling unintentionally (not holding brake).");
    }

    /// <summary>
    /// Helper to measure slope angle in front/back direction
    /// E.g. returns 0 on flat ground, 30 if going up/down a 30° slope, etc.
    /// </summary>
    private float GetSlopeAngle()
    {
        // We can measure the local forward vector relative to horizontal.
        // 1) Project the car's forward direction onto a plane
        // or simpler: use transform.forward.y to approximate slope if your car is oriented well.
        // This is not perfect if the car is pitched from suspension, but good enough:
        Vector3 forward = carController.transform.forward;
        // Dot with Vector3.up => cos( angle ), but we want the angle from horizontal:
        // Actually let's do the angle from horizontal => angle with plane:
        // angle = 90 - (angle with vertical). We'll do a simpler approach:
        float slopeAngle = Vector3.Angle(Vector3.up, forward);
        // slopeAngle near 90 => flat, near 0 => steep uphill, near 180 => steep downhill
        // We can convert that to "pitch from horizontal" by:
        float pitchFromHorizontal = 90f - slopeAngle;
        // pitchFromHorizontal > 0 => uphill, < 0 => downhill
        return Mathf.Abs(pitchFromHorizontal);
    }

    #endregion

    #region Module 3: Striking or Colliding with Objects (Summary)
    /// <summary>
    /// 3) Striking or Colliding with Objects:
    ///    We'll handle this in OnCollisionEnter. 
    ///    If you want separate logic for certain objects (like cones or fences), you can do so in OnCollisionEnter or with triggers.
    /// </summary>
    // See OnCollisionEnter below

    #endregion

    #region Module 4: Stopping / Parking in an Improper Position
    /// <summary>
    /// 4) Stopping / Parking in an Improper Position:
    ///    E.g. final coordinates, rotation relative to lanes, parking lines, or slope zone.
    /// </summary>
    private void CheckStoppingParking()
    {
        // Condition A: gear==0 => 'P'
        bool isGearP = (dataManager.GearInput == 0);
        // Condition B: speed < 0.5 => effectively not moving
        bool isVerySlow = (carController.speed < 0.5f);

        // If either gearP or speed is basically 0 => "parked/stopped"
        bool isParkedOrStopped = (isGearP || isVerySlow);

        // If parked/stopped outside zone => accumulate time
        if (isParkedOrStopped && !_isInParkingZone)
        {
            _timeOutsideZone += Time.deltaTime;

            // Check threshold for 8 seconds
            if ((_timeOutsideZone >= 19f || isGearP ) && !_wasViolation)
            {
                // Log violation
                RecordViolation(
                    RuleModule.ImproperStoppingOrParking,
                    "Parked/Stood outside the designated ParkingZone."
                );

                // Mark that we've triggered once
                _wasViolation = true;

                // IMPORTANT: We do NOT reset _timeOutsideZone here,
                // so user could remain parked, but we only record once
                // until they reset by driving or changing out of P
            }
        }
        else
        {
            // If not parked or is inside zone => reset detection
            _timeOutsideZone = 0f;
            _wasViolation = false;
        }
    }

    public void SetIsInParkingZone(bool val)
    {
        _isInParkingZone = val;
        if (val)
        {
            // If entering the zone, reset the timer & violation for outside zone
            _timeOutsideZone = 0f;
            _wasViolation = false;
        }
    }

    #endregion

    #region Module 5: Signal-Related Errors
    /// <summary>
    /// 5) Signal-Related Errors:
    ///    Omit signal, fail to signal on time, or wrong signal.
    ///    We'll keep it simple: if user is turning left but no left signal, or turning right but no right signal.
    /// </summary>
    private void CheckSignalingErrors()
    {
        bool isViolatingNow = false;

        float currentSpeed = carController.speed;      // in km/h
        float steering = dataManager.SteeringWheel; // -1..+1
        float throttle = dataManager.Throttle;     //  0..1
        float brake = dataManager.Brake;        //  0..1
        bool leftSignalOn = (dataManager.LeftAndRightLighting[0].color == Color.red);
        bool rightSignalOn = (dataManager.LeftAndRightLighting[1].color == Color.red);

        ////////////////////////////////////////////////////
        // 1) TURN DETECTION (time-based "commit")
        ////////////////////////////////////////////////////
        // If user is going faster than speedMinForTurning,
        // brake is not pressed heavily (so they're not obviously stopping),
        // and steering is beyond ±_steerThresholdForTurn, we accumulate time.

        if (currentSpeed > _speedMinForTurning && brake < _brakeIgnoreThreshold)
        {
            int sign = 0; // none
            if (steering > _steerThresholdForTurn) sign = +1;
            if (steering < -_steerThresholdForTurn) sign = -1;

            // If sign == 0 => user not steering heavily => reset turn
            if (sign == 0)
            {
                _turnTimer = 0f;
                _turnSign = 0;
            }
            else
            {
                // If the sign hasn't changed (still the same direction), accumulate
                if (sign == _turnSign)
                {
                    _turnTimer += Time.deltaTime;
                }
                else
                {
                    // direction changed => reset
                    _turnTimer = 0f;
                    _turnSign = sign;
                }

                // Once we've held steering in the same direction for enough time => it's an actual turn
                if (_turnTimer >= _turnCommitTime)
                {
                    // Check signals
                    if (sign == +1 && !rightSignalOn)  // turning right, no right signal => violation
                        isViolatingNow = true;

                    if (sign == -1 && !leftSignalOn)   // turning left, no left signal => violation
                        isViolatingNow = true;
                }
            }
        }
        else
        {
            // reset turning
            _turnTimer = 0f;
            _turnSign = 0;
        }

        ////////////////////////////////////////////////////
        // 3) One-shot check
        ////////////////////////////////////////////////////
        CheckOneShotViolation(RuleModule.SignalingErrors, isViolatingNow, "Turning without Signal.");
    }

    #endregion

    #region Module 6: Gear / Handbrake Issues
    /// <summary>
    /// 6) Gear / Handbrake Issues:
    ///    - If the vehicle is moving (speed > some threshold) but gear is N => "Freewheeling"
    ///    - If the user is on a slope or parked but didn't engage handbrake => etc.
    /// </summary>
    private void CheckGearHandbrakeUsage()
    {
        bool isViolatingNow = false;

        float currentSpeed = carController.speed; // in km/h
        float throttle = dataManager.Throttle;
        float brake = dataManager.Brake;
        int currentGear = dataManager.GearInput; // -2 => N, -1 => R, 0 => P, +1 => D

        //////////////////////////////////////////
        // 1) Gear shifting while moving
        //////////////////////////////////////////
        if (currentGear != _lastGearInput)
        {
            // The user just changed gear
            if (currentSpeed > _speedThresholdForGearChange)
            {
                // e.g. "Shifted gear from D to R or P at 5 km/h => violation"
                isViolatingNow = true;
            }
            // Update last gear
            _lastGearInput = currentGear;
        }

        //////////////////////////////////////////
        // 2) Pedals pressed simultaneously
        //////////////////////////////////////////
        // First, check if we've 'initialized' the pedals
        // If user is at default 0.5 for both, we ignore until they move at least one pedal
        if (!_pedalsInitialized)
        {
            // If either pedal is away from the default 0.5 by more than _toleranceForDefault
            // we consider them "initialized"
            if (Mathf.Abs(throttle - _defaultPedalValue) > _toleranceForDefault ||
                Mathf.Abs(brake - _defaultPedalValue) > _toleranceForDefault)
            {
                _pedalsInitialized = true;
            }
        }
        else
        {
            // Once pedals are initialized, we do the simultaneous-press check
            if (throttle > _pedalPressThreshold && brake > _pedalPressThreshold)
            {
                // e.g. "You pressed throttle and brake at once => violation"
                isViolatingNow = true;
            }
        }

        //////////////////////////////////////////
        // 3) One-shot check
        //////////////////////////////////////////
        CheckOneShotViolation(RuleModule.GearHandbrakeIssues, isViolatingNow,
            "Incorrect Gear / Pedal Handling. (No both pedal pressing/ free wheeling)");
    }

    #endregion

    #region Module 7: Speed Control
    /// <summary>
    /// 7) Speed Control:
    ///    e.g. over speed limit, or not adjusting speed properly.
    /// </summary>
    private void CheckSpeedControl()
    {
        bool isViolating = false;

        float speed = carController.speed; // in km/h
        // Suppose the speed limit is 50 for demonstration
        if (speed > 50f)
        {
            isViolating = true;
        }

        // You can also check "driving too slow" or "not adjusting to traffic" here

        CheckOneShotViolation(RuleModule.SpeedControl, isViolating,
            $"Driving over speed limit (Speed over 50 km/h). Latest Recorded Speed: {speed:0.0} km/h");
    }

    #endregion

    #region Module 8: Lane / Spacing / Positioning
    /// <summary>
    /// 8) Lane / Spacing / Positioning:
    ///    e.g. user drifting out of lane, tailgating, or turning wide, etc.
    ///    Usually you'd have lane markers or triggers, or a path system to measure lateral offset from center.
    /// </summary>
    private void CheckLaneDiscipline()
    {
        bool isViolatingNow = false;

        // For "Wrong side of the road"
        if (_isOnWrongSide)
        {
            // We consider it a violation
            isViolatingNow = true;
        }

        // For drifting out of lane boundaries
        if (_isDriftingOutOfLane)
        {
            // Also a violation
            isViolatingNow = true;
        }
       
        CheckOneShotViolation(RuleModule.LaneDiscipline, isViolatingNow,
            "Wrong lane Discipline (Wrong Side of the Road) or Drifting out of Lane Boundaries.");
    }

    public void SetWrongSide(bool val)
    {
        _isOnWrongSide = val;
    }

    public void SetLaneBoundary(bool val)
    {
        _isDriftingOutOfLane = val;
    }


    #endregion

    #region Module 9: Traffic Laws & Right-of-Way
    /// <summary>
    /// 9) Traffic Laws & Right-of-Way:
    ///    e.g. ignoring traffic lights, yield signs, or crosswalks, etc.
    /// </summary>
    private void CheckTrafficLaws()
    {
        CheckOneShotViolation(
            RuleModule.TrafficLaws,
            _ranRedOrYellowLight,
            "Ignore Traffic Signal. (Ran on Red Light)"
        );
    }

    public void SetTrafficLightViolationCheck(bool violation)
    {
        _ranRedOrYellowLight = violation;
    }

    public void RecordImmediateViolation(RuleModule mod, string description)
    {
        // calls the same logic your other modules use 
        RecordViolation(mod, description);
    }

    #endregion

    #endregion

    #region One-Shot Logic

    /// <summary>
    /// One-shot violation helper: logs one violation event only once per continuous condition = true.
    /// If condition is false, we reset so we can trigger again next time.
    /// </summary>
    private void CheckOneShotViolation(RuleModule mod, bool condition, string description)
    {
        bool wasViolating = _currentlyViolating[mod];

        if (condition)
        {
            if (!wasViolating)
            {
                // Just started offending
                RecordViolation(mod, description);
                _currentlyViolating[mod] = true;
            }
        }
        else
        {
            // Condition ended, reset
            _currentlyViolating[mod] = false;
        }
    }

    #endregion

    #region Module 3: Collision Handling (Actual Handling Code)

    /// <summary>
    /// 3) Striking or Colliding with Objects:
    ///    We'll handle collisions here. If you want to differentiate "striking object" from normal collisions,
    ///    you can check the tag or layer of the object (cones, fences, traffic signs, etc.).
    /// </summary>
    /// 
    /*
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision detected (outside if)");
        //bool isViolating = false;
        // If it hits the ground or road, skip
        if (collision.gameObject.CompareTag("sidewalk_wall")) {
            Debug.Log("Collision detected");
            
            CheckOneShotViolation(RuleModule.StrikingObjects, true,
               "Collided with object: " + collision.gameObject.name);
        }
        // If it's small enough velocity, skip
        //if (collision.relativeVelocity.magnitude < 2f)
        //  return;

        // Possibly check object tag, e.g. "TrafficCone", "Wall", "Vehicle"
        // For demonstration, let's call them all "StrikingObjects" rule

    }
    */

    #endregion

    #region Logging & Alert Spawning

    public void RecordViolation(RuleModule mod, string description)
    {
        // 1) increment the count
        _violationsCount[mod]++;
        
        // 2) build the message
        string msg = $"<b><color=orange>[{ruleToalert(mod)}]</color></b> {description}";

        // 3) log it
        _violationLogs.Add(msg);

        // 4) spawn the alert
        SpawnAlert(msg);

        // 5) check pass/fail
        int total = 0;
        foreach (var kvp in _violationsCount)
            total += kvp.Value;

        if (total >= maxAllowedViolations && !_isTestFailed)
        {
            _isTestFailed = true;
            // We do NOT show a "failed" message in real-time
        }
    }

    private void SpawnAlert(string text)
    {
        if (!alertPrefab || !alertParent)
        {
            Debug.LogWarning($"TrafficRuleDetection: Missing references (alertPrefab or alertParent). Cannot spawn alert: {text}");
            return;
        }

        GameObject alertGO = Instantiate(alertPrefab, alertParent);
        TextMeshProUGUI txt = alertGO.GetComponentInChildren<TextMeshProUGUI>();
        if (txt)
        {
            txt.text = text;
            Debug.Log($"[TrafficRuleDetection] Spawned violation: {text}");
        }
        else
        {
            Debug.LogWarning($"TrafficRuleDetection: No TMP text found in alertPrefab children. Cannot set message: {text}");
        }

        // Optional auto-destroy
         Destroy(alertGO, 4f);
    }

    private static string ruleToalert(RuleModule module)
    {
        switch (module)
        {
            case RuleModule.FailToCheckTrafficConditions:
                return "Awareness";
            case RuleModule.UnintendedRolling:
                return "Rolling";
            case RuleModule.StrikingObjects:
                return "Striking";
            case RuleModule.FollowingTooClose:
                return "Following Distance";
            case RuleModule.ImproperStoppingOrParking:
                return "Stopping/Parking";
            case RuleModule.SignalingErrors:
                return "Signaling";
            case RuleModule.GearHandbrakeIssues:
                return "Gear/Handbrake Operation";
            case RuleModule.SpeedControl:
                return "Speed Control";
            case RuleModule.LaneDiscipline:
                return "Lane Discipline";
            case RuleModule.TrafficLaws:
                return "Traffic Rules";
            default:
                return module.ToString(); // fallback to enum name
        }
    }


    #endregion

    #region Finalizing

    /// <summary>
    /// Call this when the simulation ends to store final stats and load next scene.
    /// </summary>
    public void FinalizeAndLoadNextScene(string nextSceneName)
    {
        // Copy final counts
        FinalViolationData.Clear();
        foreach (var kvp in _violationsCount)
            FinalViolationData[kvp.Key] = kvp.Value;

        // Copy logs
        FinalViolationLogs.Clear();
        foreach (var log in _violationLogs)
            FinalViolationLogs.Add(log);

        // Mark final fail
        FinalResultFailed = _isTestFailed;

        // Load
        //SceneManager.LoadScene();
        //SceneTransition.GetSceneTransition.SetScene("ResultScene");
        SceneTransition.GetSceneTransition.PerformTransition(1);
    }

    #endregion
}
