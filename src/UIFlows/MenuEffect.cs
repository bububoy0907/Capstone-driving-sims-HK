using TMPro;
using UnityEngine;

public class MenuEffect : MonoBehaviour
{
    public GameObject tooltips;
    GameObject tooltips_text;
    GameObject ResetSimulation;
    public TMP_Text tooltipButtonText;

    public GameObject RouteAssist;
    public TMP_Text assist_text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (tooltips.activeInHierarchy)
        {
            tooltipButtonText.color = Color.green;
        }
        else
        {
            tooltipButtonText.color = Color.white;
        }

    }
}
