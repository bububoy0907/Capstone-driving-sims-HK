using UnityEngine;
using System;

/// <summary>
/// Attach this to an empty GameObject named "G29MenuInput" in the Result Scene.
/// It polls the G29 each frame for D-Pad up/down + O button, then
/// calls G29MenuNavigation for navigation/selection. 
/// Make sure you've removed / commented out the input logic in G29MenuNavigation.Update(),
/// so there's no conflict.
/// </summary>
public class G29ResultInput : MonoBehaviour
{
    [Header("Navigation Script Reference")]
    [Tooltip("Link to your G29MenuNavigation script in the scene.")]
    public G29MenuNavigation menuNav;

    [Header("G29 Input Setup")]
    [Tooltip("Which button index is the 'O' button on G29? Typically 1 or 2; check logs to confirm.")]
    public int oButtonIndex = 2;

    // For detecting new D-Pad (POV) presses
    private int prevPOV = -1;

    // For detecting new button presses
    private byte[] prevButtons = new byte[128];

    void Awake()
    {
        // Optionally initialize G29 here if not done in a global manager
        bool init = LogitechGSDK.LogiSteeringInitialize(false);
        Debug.Log($"[G29ResultInput] G29 Initialized => {init}");
    }

    void Start()
    {
        if (!menuNav)
        {
            Debug.LogWarning("[G29ResultInput] menuNav is not assigned! D-Pad navigation won't do anything.");
        }
    }

    void Update()
    {
        // 1) Update G29
        if (!LogitechGSDK.LogiUpdate() || !LogitechGSDK.LogiIsConnected(0))
        {
            // G29 not connected or not updating
            return;
        }

        // 2) Get G29 state
        var rec = LogitechGSDK.LogiGetStateUnity(0);

        // 3) Check D-Pad (POV)
        int currentPOV = (int)rec.rgdwPOV[0];

        // Debug (uncomment if you want to see what value is read each frame):
        // Debug.Log($"[G29ResultInput] currentPOV = {currentPOV}, prevPOV = {prevPOV}");

        // We say it's a "new press" if currentPOV != -1 and prevPOV == -1
        // i.e. it was not pressed last frame, but is pressed this frame
        bool newPOVPress = (currentPOV != -1 && prevPOV == -1);
        if (newPOVPress && menuNav)
        {
            // Typically: 0 => up, 18000 => down, 9000 => right, 27000 => left
            // Some G29 devices differ. If your up/down are reversed, try 9000 or 27000.
            if (currentPOV == 0)
            {
                Debug.Log("[G29ResultInput] D-Pad Up => NavigateUp()");
                menuNav.NavigateUp();
            }
            else if (currentPOV == 18000)
            {
                Debug.Log("[G29ResultInput] D-Pad Down => NavigateDown()");
                menuNav.NavigateDown();
            }
            // If you want left/right: 27000 => left, 9000 => right
            // else if ...
        }
        prevPOV = currentPOV;

        // 4) Check O button
        byte[] currButtons = rec.rgbButtons;
        bool oJustPressed = false;
        if (currButtons != null && currButtons.Length > oButtonIndex)
        {
            // 128 => pressed
            oJustPressed = (currButtons[oButtonIndex] == 128 && prevButtons[oButtonIndex] == 0);
        }

        // If O just pressed => we call SelectCurrent
        if (oJustPressed && menuNav)
        {
            Debug.Log("[G29ResultInput] O Button => SelectCurrent()");
            menuNav.SelectCurrent();
        }

        // 5) If you want to pass the "held" state to show/hide a description panel, 
        //    you can do so if G29MenuNavigation has a method for it. E.g.:
        bool oHeld = false;
        if (currButtons != null && currButtons.Length > oButtonIndex)
        {
            oHeld = (currButtons[oButtonIndex] == 128);
        }
        // Then call e.g. menuNav.HandleOHeld(oHeld);

        // 6) copy states
        Array.Copy(currButtons, prevButtons, currButtons.Length);
    }

    void OnDestroy()
    {
        Debug.Log("[G29ResultInput] Shutting down G29 => " + LogitechGSDK.LogiSteeringShutdown());
    }
}
