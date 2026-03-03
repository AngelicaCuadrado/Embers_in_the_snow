using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// Manages the overall game state, including game flow control, win and loss conditions, and scene transitions.
/// </summary>
/// <remarks>GameManager implements a singleton pattern to ensure only one instance exists during gameplay. It
/// provides methods to pause, resume, and control the progression of the game, as well as to handle transitions between
/// scenes such as starting the game, returning to the main menu, or quitting the application. This class should be
/// attached to a persistent GameObject in the scene to maintain game state across scene loads.</remarks>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField, Tooltip("Current elapsed time of the game.")] private float gameTimer = 0f;
    [SerializeField, Tooltip("Total duration of the game in seconds.")] private float gameDuration = 300f;
    [SerializeField, Tooltip("Indicates whether the game is currently paused.")] private bool gamePaused = false;

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

    private void Update()
    {
        if (!gamePaused)
        {
            gameTimer += Time.deltaTime;
            if (gameTimer >= gameDuration)
            {
                WinGame();
            }
        }
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
        // Here you can add more game over logic, like showing a UI or restarting the scene.
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
