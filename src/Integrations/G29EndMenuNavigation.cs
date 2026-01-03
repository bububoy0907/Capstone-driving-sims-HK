using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;

public class G29EndMenuNavigation : MonoBehaviour
{
    [Header("Menu Navigation")]
    [Tooltip("Script that manages the up/down highlight and select logic.")]
    public G29MenuNavigation menuNavigation;
    public G29CategorySelection categorySelection;
    // We'll store old POV state so we can detect new presses
    private int previousPOV = -1;
    // We'll store old button states so we can detect button press vs. hold
    private byte[] previousButtons = new byte[128];

    [Header("Button Index for 'O' Press")]
    [Tooltip("Which button index on the G29 is the 'O' button? Commonly 1 or 2, but can vary. " +
             "Check logs in OnButtonPress if uncertain.")]
    public int oButtonIndex = 2;

    void Awake()
    {
        // Initialize the G29 library for this scene.
        bool init = LogitechGSDK.LogiSteeringInitialize(false);
        Debug.Log($"Initialize G29 for Start Menu: {init}");
    }

    void Update()
    {
        // Poll the G29 device
        if (!LogitechGSDK.LogiUpdate() || !LogitechGSDK.LogiIsConnected(0))
        {
            // G29 not connected or update failed
            return;
        }

        // Get the current state
        var rec = LogitechGSDK.LogiGetStateUnity(0);

        // 1) Check POV (D-Pad)
        int currentPOV = (int)rec.rgdwPOV[0];
        HandlePOVPress(currentPOV, previousPOV);
        previousPOV = currentPOV;

        // 2) Check button states
        byte[] currentButtons = rec.rgbButtons;
        HandleButtons(currentButtons, previousButtons);
        Array.Copy(currentButtons, previousButtons, 128);
    }

    private void HandlePOVPress(int current, int previous)
    {
        // We'll detect a new press if current != -1 and previous == -1
        // We'll detect a release if current == -1 and previous != -1
        // If you only want to detect initial press, do:
        bool newPress = (current != -1 && previous == -1);

        if (!menuNavigation)
            return;  // no reference => can't do anything

        if (newPress)
        {
            // Up or Down?
            if (current == 0)        // 0 => top
            {
                Debug.Log("D-Pad Up => NavigateUp");
                menuNavigation.NavigateUp();
            }
            else if (current == 18000) // 18000 => bottom
            {
                Debug.Log("D-Pad Down => NavigateDown");
                menuNavigation.NavigateDown();
            }
            // Other possible values: 9000 => right, 27000 => left, etc.
        }
    }

    private void HandleButtons(byte[] current, byte[] previous)
    {
        if (!menuNavigation)
            return;

        // We detect "pressed" if current[i] == 128 && previous[i] == 0
        for (int i = 0; i < current.Length; i++)
        {
            if (current[i] == 128 && previous[i] == 0)
            {
                // New press of button i
                Debug.Log($"Button {i} Pressed on G29 in Start Menu Scene");
                if (i == oButtonIndex)
                {
                    // This is the "O" button => we select the current menu item
                    Debug.Log("O Button => SelectCurrent()");
                    menuNavigation.SelectCurrent();
                }
            }
        }
    }

    private void OnDestroy()
    {
        Debug.Log("Shutting down G29 in Start Menu Scene: " + LogitechGSDK.LogiSteeringShutdown());
    }
}
