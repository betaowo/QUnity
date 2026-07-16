using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerInput playerInput;

    private bool isPaused;
    private InputAction pauseAction;

    private void Start()
    {
        if (playerInput == null)
            playerInput = GetComponent<PlayerInput>();

        pauseAction = new InputAction("Pause", binding: "<Keyboard>/escape");
        pauseAction.performed += _ => TogglePause();
        pauseAction.Enable();
    }

    private void OnDestroy()
    {
        pauseAction?.Disable();
    }

    private void TogglePause()
    {
        // don't pause if dead
        var hp = GetComponent<HealthComponent>();
        if (hp != null && hp.IsDead) return;

        isPaused = !isPaused;

        if (isPaused)
            Pause();
        else
            Resume();
    }

    private void Pause()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerInput != null)
            playerInput.enabled = false;
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerInput != null)
            playerInput.enabled = true;
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // kill player before loading menu
        Destroy(gameObject);

        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnGUI()
    {
        if (!isPaused) return;

        // darken screen
        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        float w = 260;
        float h = 220;
        float x = Screen.width / 2f - w / 2f;
        float y = Screen.height / 2f - h / 2f;

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 36;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = new Color(0.9f, 0.7f, 0.3f);
        titleStyle.alignment = TextAnchor.MiddleCenter;

        GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
        btnStyle.fontSize = 22;
        btnStyle.fontStyle = FontStyle.Bold;

        GUI.Box(new Rect(x, y, w, h), "");
        GUI.Label(new Rect(x, y + 10, w, 50), "PAUSED", titleStyle);

        if (GUI.Button(new Rect(x + 30, y + 70, w - 60, 40), "Resume", btnStyle))
            TogglePause();

        if (GUI.Button(new Rect(x + 30, y + 120, w - 60, 40), "Main Menu", btnStyle))
            GoToMainMenu();

        if (GUI.Button(new Rect(x + 30, y + 170, w - 60, 40), "Quit", btnStyle))
            QuitGame();
    }
}