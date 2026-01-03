using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.ComponentModel;

/// <summary>
/// Single script handling:
/// - Pass/Fail final result display,
/// - Violation stats,
/// - Up/Down navigation among result items (categories + "Back"),
/// - Holding O button to show a description panel (unless user is on "Back" item).
/// 
/// Attach in the result scene. Provide references in the Inspector.
/// </summary>
public class G29MenuNavigation : MonoBehaviour
{
    [Header("UI for Final Result")]
    public GameObject passprefab;  // Shown if user passed
    public GameObject failprefab;  // Shown if user failed

    [Tooltip("Displays the violation counts per category")]
    public TextMeshProUGUI violationCountsText;

    [Tooltip("Displays the top violation recommendation header")]
    public TextMeshProUGUI recommendationText;

    [Tooltip("Displays the top violation recommendation details")]
    public TextMeshProUGUI recommendationText_content;

    [Header("Result Menu Items (Navigation)")]
    [Tooltip("List of UI Buttons, e.g. categories + final 'Back to Menu' item at the end.")]
    public Button[] menuButtons;

    [Tooltip("How much bigger the selected button scales relative to normal (1.0). If you don't want scaling, set 1.")]
    public float highlightScale = 1.2f;

    [Header("Description Panel")]
    [Tooltip("UI panel that shows category details when user holds O, except on 'Back to Menu' item.")]
    public GameObject descriptionPanel;

    [Header("G29 Input Setup")]
    [Tooltip("Which button index is the 'O' button on G29? Commonly 1 or 2, check logs to confirm.")]
    public int oButtonIndex = 2;

    [Tooltip("Name of the G29 POV for up/down. We'll read rgdwPOV[0] in the code, so no field needed here.")]
    // We'll just assume we're using rgdwPOV[0].

    private int currentIndex = 0;            // Which item is highlighted
    private int backButtonIndex = -1;        // We'll set this to menuButtons.Length - 1 (the final "Back" item)
    private byte[] prevButtons = new byte[128];  // track last frame's button states
    private int prevPOV = -1;               // track last frame's D-Pad

    void Start()
    {
        // 1) Summarize final results
        bool userFailed = TrafficRuleDetection.FinalResultFailed;
        Dictionary<RuleModule, int> finalData = TrafficRuleDetection.FinalViolationData;

        DisplayPassFail(userFailed);
        DisplayViolationCounts(finalData);
        DisplayTopViolation(finalData);

        // 2) If the final item in menuButtons is "Back to Menu," store that index
        backButtonIndex = 1;

        // 3) Highlight the initial item
        HighlightCurrent();
    }

    private void Update()
    {
        var rec = LogitechGSDK.LogiGetStateUnity(0);
        // 2) Check "O" button pressed or held
        byte[] buttons = rec.rgbButtons;
        bool oPressed = false;
        bool oJustPressed = false;

        if (buttons != null && buttons.Length > oButtonIndex)
        {
            oPressed = (buttons[oButtonIndex] == 128);
            // "Just Pressed" => current=128 & prev=0
            oJustPressed = (buttons[oButtonIndex] == 128 && prevButtons[oButtonIndex] == 0);
            descriptionPanel.SetActive(oPressed && currentIndex != backButtonIndex);
        }
        /*
        // Update G29
        if (!LogitechGSDK.LogiUpdate() || !LogitechGSDK.LogiIsConnected(0))
        {
            // If G29 not connected, hide the description panel
            if (descriptionPanel)
                descriptionPanel.SetActive(false);
            return;
        }

        var rec = LogitechGSDK.LogiGetStateUnity(0);

        // 1) Handle Up/Down from the D-Pad (POV)
        int currentPOV = (int)rec.rgdwPOV[0];
        bool newPOVPress = (currentPOV != -1 && prevPOV == -1);
        if (newPOVPress)
        {
            if (currentPOV == 0)        // Up
                NavigateUp();
            else if (currentPOV == 18000) // Down
                NavigateDown();
        }
        prevPOV = currentPOV;

        // 2) Check "O" button pressed or held
        byte[] buttons = rec.rgbButtons;
        bool oPressed = false;
        bool oJustPressed = false;

        if (buttons != null && buttons.Length > oButtonIndex)
        {
            oPressed = (buttons[oButtonIndex] == 128);
            // "Just Pressed" => current=128 & prev=0
            oJustPressed = (buttons[oButtonIndex] == 128 && prevButtons[oButtonIndex] == 0);
        }

        // 3) If user is highlighting the "Back to Menu" item & they just pressed O => select that
        if (currentIndex == 1)
        {
            SelectCurrent();
        }
        else
        {
            // If user is not on the back item => hold O => show description
            // If user is not pressing => hide
            if (descriptionPanel)
            {
                descriptionPanel.SetActive(oPressed && currentIndex != backButtonIndex);
            }
        }

        // store button states
        System.Array.Copy(buttons, prevButtons, buttons.Length);
        */
    }

    // ---------------------------
    // Navigation logic
    // ---------------------------
    public void NavigateUp()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = menuButtons.Length - 1;
        HighlightCurrent();
    }

    public void NavigateDown()
    {
        currentIndex++;
        if (currentIndex >= menuButtons.Length)
            currentIndex = 0;
        HighlightCurrent();
    }

    private void HighlightCurrent()
    {
        // For each button, scale it if selected
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (i == currentIndex)
                menuButtons[i].transform.localScale = Vector3.one * highlightScale;
            else
                menuButtons[i].transform.localScale = Vector3.one;
        }
    }

    public void SelectCurrent()
    {
        Debug.Log($"Selecting menu item {currentIndex}: {menuButtons[currentIndex].name}");
        menuButtons[currentIndex].onClick.Invoke();
    }

    // ---------------------------
    // Final result logic
    // ---------------------------
    private void DisplayPassFail(bool userFailed)
    {
        if (userFailed)
        {
            if (passprefab) passprefab.SetActive(false);
            if (failprefab) failprefab.SetActive(true);
        }
        else
        {
            if (passprefab) passprefab.SetActive(true);
            if (failprefab) failprefab.SetActive(false);
        }
    }

    private void DisplayViolationCounts(Dictionary<RuleModule, int> finalData)
    {
        StringBuilder sb = new StringBuilder();
        int count_total = 0;
        foreach (var kvp in finalData)
        {
            RuleModule module = kvp.Key;
            int count = kvp.Value;
            sb.AppendLine($"- {ruleToalert(module)}: <color=white>{count}</color>");
            count_total+= count;
        }
        sb.AppendLine($"<color=red>- Total:</color> <color=white>{count_total}</color>");
        violationCountsText.text = sb.ToString();
    }

    private void DisplayTopViolation(Dictionary<RuleModule, int> finalData)
    {
        RuleModule topModule = RuleModule.FailToCheckTrafficConditions;
        int topCount = 0;

        foreach (var kvp in finalData)
        {
            if (kvp.Value > topCount)
            {
                topCount = kvp.Value;
                topModule = kvp.Key;
            }
        }

        if (topCount == 0)
        {
            recommendationText.text = "<b><color=yellow>Great job! No violations recorded.</color></b>";
            recommendationText_content.text = "";
            return;
        }

        string recMessage = GetRecommendationForModule(topModule);
        recommendationText.text = $"<color=red>Most Violations:</color> <color=orange>{ruleToalert(topModule)}</color>";
        recommendationText_content.text = recMessage;
    }

    private string GetRecommendationForModule(RuleModule module)
    {
        switch (module)
        {
            case RuleModule.FailToCheckTrafficConditions:
                return "Try to consistently use your mirrors and a quick shoulder check before any maneuver. " +
                       "Anticipate other road users by scanning the road well ahead and to the sides, " +
                       "and double-check blind spots when changing lanes or merging.";

            case RuleModule.UnintendedRolling:
                return "When stopped on an incline, keep your foot on the brake until you apply the throttle. " +
                       "If your vehicle has a handbrake, engage it before releasing the foot brake. " +
                       "Practice smooth transitions between brake and accelerator to avoid drift.";

            case RuleModule.StrikingObjects:
                return "Maintain a wider buffer zone around parked cars, curbs, and obstacles. " +
                       "Use your mirrors and look over your shoulder when maneuvering near tight spaces. " +
                       "Reduce speed and take extra caution in areas with pedestrians or tight lanes.";

            case RuleModule.FollowingTooClose:
                return "Use the ¡¥two-second rule¡¦ (or more in bad weather): pick a roadside object, and " +
                       "ensure at least two seconds elapse between the car in front passing it and you passing it. " +
                       "If you¡¦re too close, gently ease off the accelerator to open up space.";

            case RuleModule.ImproperStoppingOrParking:
                return "Only stop or park in permitted areas and use hazard lights if necessary. " +
                       "Check for signs or road markings indicating valid parking or stopping zones. " +
                       "When parking on a slope, turn your wheels to the curb and use the handbrake.";

            case RuleModule.SignalingErrors:
                return "Develop the habit of signaling well before you begin turning or changing lanes¡Xideally 3-5 seconds " +
                       "in advance. Cancel your signal once the maneuver is complete. " +
                       "Always verify your signals match your actual direction of travel.";

            case RuleModule.GearHandbrakeIssues:
                return "Shift gears only when your car is stopped if it¡¦s an automatic with P, R, N, D. " +
                       "Avoid pressing the throttle and brake at the same time. " +
                       "Use the handbrake on slopes or when parked, and release it gently while applying throttle.";

            case RuleModule.SpeedControl:
                return "Familiarize yourself with speed limits in different zones. " +
                       "Gradually adjust your speed when approaching changes in limit or traffic conditions. " +
                       "Use your speedometer often to ensure you¡¦re not creeping above limits.";

            case RuleModule.LaneDiscipline:
                return "Stay centered in your lane by aligning your car¡¦s position with lane markings or the middle of the lane. " +
                       "Signal and check mirrors/blind spots before you change lanes. " +
                       "Never drive on the wrong side unless making a permitted turn.";

            case RuleModule.TrafficLaws:
                return "Respect all traffic signals, signs, and markings. " +
                       "Stop fully at red lights and stop signs. " +
                       "Follow right-of-way rules at intersections, and yield to pedestrians in crosswalks.";

            default:
                return "No specific recommendation available.";
        }
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
                return module.ToString(); // fallback
        }
    }
}
