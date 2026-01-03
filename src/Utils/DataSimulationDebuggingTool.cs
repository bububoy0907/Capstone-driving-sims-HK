using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataSimulationDebuggingTool : MonoBehaviour
{
    public static DataSimulationDebuggingTool GetDataSimulationDebuggingTool;

    public int gearInput = 0;
    public float throttleInput = 0;
    public float brakeInput = 0;

    private void Awake()
    {
        GetDataSimulationDebuggingTool = this;
    }
}
