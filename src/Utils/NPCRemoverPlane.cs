using UnityEngine;

[RequireComponent(typeof(Collider))]
public class NPCRemoverPlane : MonoBehaviour
{
    private void Start()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            Destroy(other.gameObject);
        }
    }
}
