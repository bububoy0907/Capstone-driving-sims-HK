using UnityEngine;

public class QuitButton : MonoBehaviour
{
    // This method can be linked to the button's OnClick event
    public void QuitGame()
    {
        Application.Quit();
    }
}
