using Gley.TrafficSystem;
using UnityEngine;

public class destroyVehicle : MonoBehaviour
{
    private readonly string[] npcVehicleNames = new string[]
        {
        "OriginalSmallSedan",
        "SmallSedanBlue2",
        "SmallSedanGreen1",
        "SmallSedanGreen2",
        "SmallSedanGrey",
        "SmallSedanPurple",
        "SmallSedanRed",
        "SmallSedanWhite",
        "SmallSedanYellow1",
        "SmallSedanYellow2"
        };

    private void Start()
    {
        // Ensure the cube's BoxCollider is set as a trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleTrigger(other);
    }

    private void OnTriggerStay(Collider other)
    {
        HandleTrigger(other);
    }

    private void HandleTrigger(Collider other)
    {
        Transform current = other.transform;

        for (int i = 0; i < 5 && current != null; i++)
        {
            string objName = current.gameObject.name;

            foreach (string baseName in npcVehicleNames)
            {
                for (int j = 0; j <= 25; j++)
                {
                    string expectedName = $"{baseName}(Clone){j}";
                    if (objName.StartsWith(expectedName))
                    {
                        GameObject target = GameObject.Find(expectedName);
                        if (target != null)
                        {
                            API.RemoveVehicle(target);
                            Debug.Log($"✅ Removed vehicle: {expectedName}");
                        }
                        return;
                    }
                }
            }

            current = current.parent;
        }
    }
}
