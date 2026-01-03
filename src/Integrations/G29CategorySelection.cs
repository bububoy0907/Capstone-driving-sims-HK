using UnityEngine;
using TMPro;

/// <summary>
/// Attach this to a GameObject in your Result scene. It manages 9 category labels (TextMeshPro)
/// and 9 detail boxes. Pressing up/down from your G29 logic calls NavigateUp/Down. 
/// The selected category text is colored yellow, others white. 
/// The matching detail box is active, others inactive.
/// </summary>
public class G29CategorySelection : MonoBehaviour
{
    [Header("Category Texts")]
    [Tooltip("Array of 9 TextMeshProUGUI objects for each category label.")]
    public TextMeshProUGUI[] categoryTexts;  // e.g. 9 categories

    [Header("Detail Boxes")]
    [Tooltip("Array of 9 GameObjects. Each box shows more details about that category.")]
    public GameObject[] categoryDetailBoxes; // also 9

    private int currentIndex = 0;

    void Start()
    {
        // Make sure the array lengths match (both should be 9).
        if (categoryTexts.Length != categoryDetailBoxes.Length)
        {
            Debug.LogWarning("Mismatch: categoryTexts and categoryDetailBoxes have different lengths!");
        }

        // Highlight the initial category (0)
        HighlightCurrent();
    }

    /// <summary>
    /// Called when D-Pad up is pressed => move selection up one item.
    /// </summary>
    public void NavigateUp()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = categoryTexts.Length - 1; // wrap around
        HighlightCurrent();
    }

    /// <summary>
    /// Called when D-Pad down is pressed => move selection down one item.
    /// </summary>
    public void NavigateDown()
    {
        currentIndex++;
        if (currentIndex >= categoryTexts.Length)
            currentIndex = 0; // wrap around
        HighlightCurrent();
    }

    /// <summary>
    /// Highlights the currently selected category by coloring its text yellow 
    /// and activating its detail box, while others are deactivated.
    /// </summary>
    private void HighlightCurrent()
    {
        for (int i = 0; i < categoryTexts.Length; i++)
        {
            // If i == currentIndex => highlight in yellow and show box
            if (i == currentIndex)
            {
                categoryTexts[i].color = Color.yellow;
                categoryDetailBoxes[i].SetActive(true);
            }
            else
            {
                categoryTexts[i].color = Color.white;
                categoryDetailBoxes[i].SetActive(false);
            }
        }
    }
}
