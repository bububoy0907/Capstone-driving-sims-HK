using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

/// <summary>
/// Attach this to a GameObject in your Start Scene.
/// This script polls the G29 each frame, reading D-Pad (POV) for up/down
/// and an O button press for selection. 
/// It highlights the current button (e.g. by scaling it) and triggers onClick on selection.
/// </summary>
public class G29StartSceneNavigation : MonoBehaviour
{
    [Tooltip("Menu Buttons in top-to-bottom or desired order.")]
    public Button[] menuButtons;

    // Scale factor for highlight
    [Tooltip("How much bigger the selected button scales relative to normal (1.0).")]
    public float highlightScale = 1.2f;

    private int currentIndex = 0;

    void Start()
    {
        HighlightCurrent();
    }

    private void HighlightCurrent()
    {
        // For each button, set localScale = Vector3.one if not selected, or Vector3( highlightScale ) if selected
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (i == currentIndex)
            {
                menuButtons[i].transform.localScale = Vector3.one * highlightScale;
            }
            else
            {
                menuButtons[i].transform.localScale = Vector3.one;
            }
        }
    }

    public void NavigateUp()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = menuButtons.Length - 1; // wrap around
        HighlightCurrent();
    }

    public void NavigateDown()
    {
        currentIndex++;
        if (currentIndex >= menuButtons.Length)
            currentIndex = 0; // wrap around
        HighlightCurrent();
    }

    public void SelectCurrent()
    {
        Debug.Log($"Selecting menu item {currentIndex}: {menuButtons[currentIndex].name}");
        menuButtons[currentIndex].onClick.Invoke();
    }
}
