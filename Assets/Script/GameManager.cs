using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private float gameTimer = 0f;
    private bool gamePaused = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

    }

    public void PauseGame()
    {
        gamePaused = true;
        Time.timeScale = 0f; // Pause the game
        Debug.Log("Game Paused.");
    }
    public void ResumeGame()
    {
        if (gamePaused)
        {
            gamePaused = false;
            Time.timeScale = 1f; // Resume the game
            Debug.Log("Game Resumed.");
        }
    }

    public void Losegame()
    {
        Debug.Log("Game Over! You froze to death.");
        SceneManager.LoadScene("Lose_Menu");
        PauseGame();
    }
    public void WinGame()
    {
        Debug.Log("Congratulations! You survived the night.");
        // Here you can add more win logic, like showing a UI or transitioning to a new scene.
        PauseGame();
    }
    public void Main_Menu()
    {
        SceneManager.LoadScene("Main_Menu");
    }

    public void Play()
    {
        SceneManager.LoadScene("Level1");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting the game...");
        Application.Quit();
    }
}
