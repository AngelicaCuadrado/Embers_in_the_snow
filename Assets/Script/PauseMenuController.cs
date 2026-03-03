using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // requires Input System package
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

public class PauseMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject pauseCanvas; // world-space canvas root
    [SerializeField] private InputActionReference pauseAction; // assign the Menu/Pause action
    [SerializeField] private List<LocomotionProvider> locomotionProviders = new List<LocomotionProvider>();

    private bool isPaused = false;

    private void Awake()
    {
        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (pauseAction != null)
            pauseAction.action.performed += OnPausePerformed;
    }

    private void OnDisable()
    {
        if (pauseAction != null)
            pauseAction.action.performed -= OnPausePerformed;
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        TogglePause();
    }

    public void TogglePause()
    {
        if (isPaused) Resume(); else Pause();
    }

    public void Pause()
    {
        isPaused = true;
        // show UI
        if (pauseCanvas != null) pauseCanvas.SetActive(true);

        // stop game time
        Time.timeScale = 0f;

        // disable locomotion providers so player can't move (teleport/continuous)
        foreach (var lp in locomotionProviders)
            if (lp != null) lp.enabled = false;

        // Optionally disable other gameplay input here
        GameManager.Instance.PauseGame(); // keeps your GameManager state consistent
    }

    public void Resume()
    {
        isPaused = false;
        if (pauseCanvas != null) pauseCanvas.SetActive(false);

        Time.timeScale = 1f;

        foreach (var lp in locomotionProviders)
            if (lp != null) lp.enabled = true;

        GameManager.Instance.ResumeGame();
    }

    // Hook these to the UI buttons
    public void OnResumeButton() => Resume();
    public void OnQuitButton() => GameManager.Instance.QuitGame();
}