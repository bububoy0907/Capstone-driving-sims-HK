using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach this to a UI Button (GameObject with Button component).
/// In the Inspector, assign a reference to the "descriptionPanel"
/// that shows all module descriptions.
/// 
/// When the user presses (pointer down), we set that panel active.
/// When they release or pointer up, we hide it again.
/// </summary>
public class CategoryDescriptionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Tooltip("UI panel (GameObject) that contains the description of each module. Initially inactive.")]
    public GameObject descriptionPanel;

    // Called when the user **presses** down on the button
    public void OnPointerDown(PointerEventData eventData)
    {
        if (descriptionPanel)
        {
            descriptionPanel.SetActive(true);
        }
    }

    // Called when the user **releases** the button
    public void OnPointerUp(PointerEventData eventData)
    {
        if (descriptionPanel)
        {
            descriptionPanel.SetActive(false);
        }
    }
}
