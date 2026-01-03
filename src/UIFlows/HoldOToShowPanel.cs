using UnityEngine;

public class HoldOToShowPanel : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The UI panel (GameObject) that shows your category description. Initially inactive.")]
    public GameObject descriptionPanel;

    [Header("G29 Button Index")]
    [Tooltip("Which button index is the 'O' button on G29? Commonly 1 or 2. Check logs to confirm.")]
    public int oButtonIndex = 2;

    void Awake()
    {
        // Initialize the G29 library for this scene. 
        // If you already do this elsewhere, you can remove or skip here.
        bool init = LogitechGSDK.LogiSteeringInitialize(false);
        Debug.Log($"Initialize G29 in {gameObject.name} => {init}");
    }

    void Update()
    {
        // 1) Update the G29
        if (!LogitechGSDK.LogiUpdate() || !LogitechGSDK.LogiIsConnected(0))
        {
            // G29 not connected or update failed => hide panel and skip
            if (descriptionPanel)
                descriptionPanel.SetActive(false);
            return;
        }

        // 2) Get G29 state
        var rec = LogitechGSDK.LogiGetStateUnity(0);
        byte[] buttons = rec.rgbButtons;
        if (buttons == null || buttons.Length <= oButtonIndex)
        {
            // no data => hide panel
            if (descriptionPanel)
                descriptionPanel.SetActive(false);
            return;
        }

        // 3) Check if O button is being held
        // G29 uses 128 for "pressed", 0 for "not pressed"
        bool oButtonHeld = (buttons[oButtonIndex] == 128);

        // 4) Show panel while O is held, hide otherwise
        if (descriptionPanel)
        {
            descriptionPanel.SetActive(oButtonHeld);
        }
    }

    void OnDestroy()
    {
        // Shutdown G29 (if you aren't using it in other scenes).
        Debug.Log("Shutting down G29 from " + gameObject.name + " => "
                  + LogitechGSDK.LogiSteeringShutdown());
    }
}
