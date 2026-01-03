using System;
using System.Text;
using Ricimi;
using UnityEngine;

public class LogitechSteeringWheelEquipmentData : MonoBehaviour
{
    // Controller Properties
    LogitechGSDK.LogiControllerPropertiesData properties;

    // Button status
    private byte[] previousButtons = new byte[128]; // Button status from last frame
    private int previousPOV = -1; // POV status from last frame£¨-1 = no specific direction£©
    //public G29MenuNavigation menuNavigation;
    void Start()
    {
        //Debug.LogError("w:" + LogitechGSDK.LogiSteeringInitialize(false));
        Debug.Log("Initialize Logitech G29 Steering Wheel Controller:" + LogitechGSDK.LogiSteeringInitialize(false));
    }

    void Update()
    {
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
        {
            #region Equipment Information
            StringBuilder deviceName = new StringBuilder(256);
            LogitechGSDK.LogiGetFriendlyProductName(0, deviceName, 256);
            Debug.Log($"Current Controller: {LogitechGSDK.LogiGetFriendlyProductName(0, deviceName, 256)}\n");

            LogitechGSDK.LogiControllerPropertiesData actualProperties = new LogitechGSDK.LogiControllerPropertiesData();
            LogitechGSDK.LogiGetCurrentControllerProperties(0, ref actualProperties);
            //Debug.Log($"Steering wheel range: {actualProperties.wheelRange}¡ã\n");
            #endregion

            #region axes data normalize in 0-1
            LogitechGSDK.DIJOYSTATE2ENGINES rec = LogitechGSDK.LogiGetStateUnity(0);

            // Steering wheel (left-most 0 ¡ú middle 0.5 ¡ú right-most 1)
            float steering = Mathf.InverseLerp(-32767, 32767, rec.lX) * 2 - 1;

            //Debug.Log($"Steering Wheel£º{steering:F2}");
            AutomotiveDataVisualizationManager.GetAutomotiveDataVisualizationManager.SteeringWheel = steering;

            // acceleration pedal 0 ¡ú pressed 1
            float throttle = 1 - Mathf.InverseLerp(-32767, 32767, rec.lY);
            //Debug.Log($"Acceleration pedal£º{throttle:F2}");
            AutomotiveDataVisualizationManager.GetAutomotiveDataVisualizationManager.Throttle = throttle;

            // brake pedal 0 ¡ú pressed 1
            float brake = 1 - Mathf.InverseLerp(-32767, 32767, rec.lRz);
            //Debug.Log($"Brake pedal Force: {brake:F2}");
            AutomotiveDataVisualizationManager.GetAutomotiveDataVisualizationManager.Brake = brake;
            #endregion

            /*
            if(brake > 0.3f || throttle > 0.3f)
            {
                GetComponent<AutomotiveDataVisualizationManager>().CloseObjective();
            }
            */

            #region POV Status Checking Pressed/Pressing/Release
            int currentPOV = (int)rec.rgdwPOV[0];
            string povState = GetPOVState(currentPOV, previousPOV);

            if (povState == "Pressed") HandlePOVPress(currentPOV);
            if (povState == "Released") HandlePOVRelease(previousPOV);

            // Record the current POV status for next frame
            previousPOV = currentPOV;
            #endregion

            #region Current Button Status Pressed/Pressing/Released
            byte[] currentButtons = rec.rgbButtons;
            for (int i = 0; i < 128; i++)
            {
                byte prev = previousButtons[i];
                byte curr = currentButtons[i];

                if (curr == 128 && prev == 0) HandleButtonPress(i);   // Pressed
                if (curr == 0 && prev == 128) HandleButtonRelease(i); // Released
            }
            
            // Record the current button status for next frame
            Array.Copy(currentButtons, previousButtons, 128);
            #endregion
        }
        else if (!LogitechGSDK.LogiIsConnected(0))
        {
            Debug.LogError("G29 is not connected");
        }
    }

    #region POV Event Handler
    private string GetPOVState(int current, int previous)
    {
        if (current != -1 && previous == -1) return "Pressed";
        if (current == -1 && previous != -1) return "Released";
        return "Pressing";
    }

    private void HandlePOVPress(int povValue)
    {
        string dir = GetPOVString(povValue);
        Debug.Log($"POV {dir} Pressed");
        GetComponent<AutomotiveDataVisualizationManager>().CloseObjective();
        switch (povValue)
        {
            case 9000:
                TrafficRuleDetection.RegisterLookRight();
                GetComponent<AutomotiveDataVisualizationManager>().ViewTheRightCamera();
                break;
            case 27000:
                TrafficRuleDetection.RegisterLookLeft();
                GetComponent<AutomotiveDataVisualizationManager>().ViewTheLeftCamera();
                break;
        }
    }

    private void HandlePOVRelease(int povValue)
    {
        string dir = GetPOVString(povValue);
        Debug.Log($"POV {dir} Released");
    }

    private string GetPOVString(int povValue)
    {
        return povValue switch
        {
            0 => "top",
            4500 => "top-right",
            9000 => "right",
            13500 => "right-bottom",
            18000 => "bottom",
            22500 => "left-bottom",
            27000 => "left",
            31500 => "top-left",
            _ => "center"
        };
    }
    #endregion

    #region Button Event Handler
    private void HandleButtonPress(int btnIndex)
    {
        Debug.Log($"Button {btnIndex} Pressed");
        GetComponent<AutomotiveDataVisualizationManager>().CloseObjective();
        switch (btnIndex)
        {
            case 1:
                Debug.Log("Route Assists off");
                AutomotiveDataVisualizationManager.GetAutomotiveDataVisualizationManager.ToggleRouteAssists();
                break;
            case 3: 
                Debug.Log("Tool tips off");
                AutomotiveDataVisualizationManager.GetAutomotiveDataVisualizationManager.ToggleToolstip();
                break;
            case 4: // right gear shift
                Debug.Log("Shifted Gear (right-up)");
                AutomotiveDataVisualizationManager.GetAutomotiveDataVisualizationManager.GearInputMannage(0);
                break;

            case 5: //  left gear shift
                Debug.Log("Shifted Gear (left-down)");
                AutomotiveDataVisualizationManager.GetAutomotiveDataVisualizationManager.GearInputMannage(1);
                break;

            case 6: // right turnning indicator
                Debug.Log("Right turning indicator: ON");
                AutomotiveDataVisualizationManager.GetAutomotiveDataVisualizationManager.TurnOnTheTurnSignal(1);
                break;

            case 7: // left turnning indicator
                Debug.Log("Left turning indicator: ON");
                AutomotiveDataVisualizationManager.GetAutomotiveDataVisualizationManager.TurnOnTheTurnSignal(0);
                break;
            case 9: // Reload scene (reset)
                Debug.Log("Reset scene");
                SceneTransition.GetSceneTransition.SetScene("main");
                SceneTransition.GetSceneTransition.PerformTransition(0);
                break;
        }
    }

    private void HandleButtonRelease(int btnIndex)
    {
        Debug.Log($"Button {btnIndex} Released");
        
        switch (btnIndex)
        {
            case 6:
            case 7: 
                Debug.Log("Indicator light OFF");
                break;
        }
        
    }
    #endregion

    #region add-ons
    void OnApplicationQuit()
    {
        Debug.Log("Shutting Down device:" + LogitechGSDK.LogiSteeringShutdown());
    }
    #endregion
}