using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeveloperConsole : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;

    private string input = "";
    private Vector2 scroll;

    private bool opened = false;

    private readonly List<string> log = new();

    private InputAction consoleAction;

    private void Awake()
    {
        log.Add("QEngine pre-alpha");
        log.Add("Type 'help' for list of commands.");
        log.Add("");
    }

    private void OnEnable()
    {
        consoleAction = playerInput.actions["Console"];
        consoleAction.Enable();
    }

    private void OnDisable()
    {
        consoleAction.Disable();
    }

    private void Update()
    {
        if (consoleAction.WasPressedThisFrame())
        {
            if (opened)
                CloseConsole();
            else
                opened = true;
        }

        if (opened)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            if (playerInput != null)
                playerInput.DeactivateInput();
        }
    }

    private void OnGUI()
    {
        if (!opened)
            return;

        // Switchin to console input
        GUI.FocusControl("ConsoleInput");

        // Main console box
        GUI.Box(new Rect(0, 0, Screen.width, 250), "");

        // cls button
        GUI.skin.button.fontSize = 14;
        GUI.skin.button.alignment = TextAnchor.MiddleCenter;
        if (GUI.Button(new Rect(Screen.width - 35, 5, 25, 25), "✕"))
        {
            CloseConsole();
            return;
        }

        GUILayout.BeginArea(new Rect(10, 30, Screen.width - 20, 210));

        scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(150));

        foreach (string line in log)
            GUILayout.Label(line);

        GUILayout.EndScrollView();

        GUILayout.Space(5);

        GUI.SetNextControlName("ConsoleInput");
        input = GUILayout.TextField(input);

        GUILayout.Space(5);

        if (GUILayout.Button("Submit"))
        {
            ExecuteCommand(input);
            input = "";
        }

        GUILayout.EndArea();
    }

    private void CloseConsole()
    {
        opened = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (playerInput != null)
            playerInput.ActivateInput();
    }

    void ExecuteCommand(string cmd)
    {
        if (string.IsNullOrWhiteSpace(cmd))
            return;

        log.Add("] " + cmd);

        Commands.Execute(cmd, log);

        scroll.y = float.MaxValue;
    }
}