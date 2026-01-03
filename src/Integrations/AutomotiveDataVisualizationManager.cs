using UnityEngine;

public class AutomotiveDataVisualizationManager : MonoBehaviour
{
    public static AutomotiveDataVisualizationManager GetAutomotiveDataVisualizationManager;

    public RCC_CarControllerV3 RCC_Car;
    public RCC_DashboardInputs inputs;
    public RCC_Inputs RccInputs = new RCC_Inputs();

    public Transform RPM;  // RPM
    public Transform Speed;  // Vehicle Speed
    public UnityEngine.UI.Text GearState; //Current Gear shift
    public UnityEngine.UI.Image[] LeftAndRightLighting;


    // Starting angle and angle change every 10 km/h
    private float speedStartAngle = 0f;
    private float speedAnglePerTenKMH = -25f;

    // RPM starting angle and angle change per 100RPM
    private float rpmStartAngle = -22f;
    private float rpmAnglePerHundredRPM = -30f; // -52 - (-22) = -30





    #region G29 Steering data
    public float SteeringWheel = 0.5f; //Steering Wheel
    public float Throttle = 0f; //Accelate
    public float Brake = 0f; //Brake
    #endregion

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetAutomotiveDataVisualizationManager = this;

        GearState.text = "<color=orange>P</color> <color=black>R</color> <color=black>N</color> <color=black>D</color>";
        LeftAndRightLighting[0].color = Color.green;
        LeftAndRightLighting[1].color = Color.green;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.LogWarning("Speed:" + inputs.KMH);
        //Debug.LogWarning("RPM:" + inputs.RPM);
        //Debug.LogWarning("GearState:" + inputs.Gear);

        #region Needle
        // Calculate the current rotation angle of the Speed ​​pointer
        float speedCurrentAngle = speedStartAngle + (inputs.KMH / 15) * speedAnglePerTenKMH;
        // Set the rotation angle of the Speed ​​pointer
        Vector3 speedCurrentRotation = Speed.localEulerAngles;
        speedCurrentRotation.z = speedCurrentAngle;
        Speed.localEulerAngles = speedCurrentRotation;

        // Calculate the current rotation angle of the RPM pointer
        float rpmCurrentAngle = rpmStartAngle + (inputs.RPM / 1000) * rpmAnglePerHundredRPM;
        // Set the rotation angle of the RPM pointer
        Vector3 rpmCurrentRotation = RPM.localEulerAngles;
        rpmCurrentRotation.z = rpmCurrentAngle;
        RPM.localEulerAngles = rpmCurrentRotation;
        #endregion

        #region Light Indicator
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TurnOnTheTurnSignal(0);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            TurnOnTheTurnSignal(1);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            ViewTheLeftCamera();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            GearInputMannage(1);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            GearInputMannage(0);
        }
        #endregion

        if (Input.anyKey)
        {
            CloseObjective();
        }
    }

    public AudioSource LightingSound;
    public void TurnOnTheTurnSignal(int num) // 0 left, 1 right
    {
        LightingSound.Stop();

        switch (num)
        {
            case 0:
                if (LeftAndRightLighting[0].color == Color.green)
                {
                    //On
                    LeftAndRightLighting[0].color = Color.red;
                    LightingSound.Play();
                }
                else
                {
                    //OFF
                    LeftAndRightLighting[0].color = Color.green;
                    LightingSound.Stop();
                }

                LeftAndRightLighting[1].color = Color.green;
                break;

            case 1:
                if (LeftAndRightLighting[1].color == Color.green)
                {
                    //on
                    LeftAndRightLighting[1].color = Color.red;
                    LightingSound.Play();
                }
                else
                {
                    //OFF
                    LeftAndRightLighting[1].color = Color.green;
                    LightingSound.Stop();
                }

                LeftAndRightLighting[0].color = Color.green;
                break;
        }
    }


    //switch the lens and return to the normal lens by pressing a button.
    public GameObject LeftCamera;
    public GameObject RightCamera;
    public void ViewTheLeftCamera()
    {
        LeftCamera.GetComponent<Camera>().enabled = !LeftCamera.GetComponent<Camera>().enabled;
        RightCamera.GetComponent<Camera>().enabled = false;
    }    
    public void ViewTheRightCamera()
    {
        LeftCamera.GetComponent<Camera>().enabled = false;
        RightCamera.GetComponent<Camera>().enabled = !RightCamera.GetComponent<Camera>().enabled;
    }

    public GameObject tooltip_canvas;
    //public UnityEngine.UI.Text tooltip_text;
    public void ToggleToolstip()
    {
        if (tooltip_canvas.activeSelf == false)
        {
            //tooltip_text.setText = "<color=green>Toggle Tool Tips</color>";
            tooltip_canvas.SetActive(true);
        }
        else
        {
            tooltip_canvas.SetActive(false);
        }
    }

    public GameObject objective_canvas;
    public void CloseObjective()
    {
        objective_canvas.SetActive (false);
    }

    public CheckpointManager checkpointManager;
    public GameObject route_object_section1;
    public GameObject route_object_section2;
    public void ToggleRouteAssists()
    {
        checkpointManager.ToggleCurrentRoute();
    }

    public int GearCalculation = 0;       //0 is P gear, 1 is R gear, 2 is N gear, 3 is D gear
    public int GearInput = -2;
    public void GearInputMannage(int type)
    {
        switch (type)
        {
            case 0:
                //Up shift
                if (GearCalculation<3)
                {
                    GearCalculation++;
                }
                else
                {
                    Debug.Log("No more to shift up");
                }
                break;

            case 1:
                //down shift
                if (GearCalculation ==0)
                {
                    Debug.Log("No more to shift down");
                }
                else
                {
                    GearCalculation--;
                }
                break;
        }

        switch (GearCalculation)
        {
            case 0:
                GearInput = 0;  //P gear
                GearState.text = "<color=orange>P</color> <color=black>R</color> <color=black>N</color> <color=black>D</color>";
                break;

            case 1:
                GearInput = -1;  //R gear
                GearState.text = "<color=black>P</color> <color=orange>R</color> <color=black>N</color> <color=black>D</color>";
                break;

            case 2:
                GearInput = -2;  //N gear
                GearState.text = "<color=black>P</color> <color=black>R</color> <color=orange>N</color> <color=black>D</color>";
                break;

            case 3:
                GearInput = 1;  //D gear
                GearState.text = "<color=black>P</color> <color=black>R</color> <color=black>N</color> <color=orange>D</color>";
                break;
        }
    }
}