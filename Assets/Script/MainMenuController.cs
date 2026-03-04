using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public void StartGame()
    {
        GameManager.Instance.Play();
    }
    public void QuitGame()
    {
        GameManager.Instance.QuitGame();
    }
}
