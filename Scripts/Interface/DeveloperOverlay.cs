using UnityEngine;
using UnityEngine.InputSystem;

public class DeveloperOverlay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private gameMovement movement;

    [Header("Displaying")]
    bool show = false;

    float fps;

    void Start()
    {
        if (movement == null)
            movement = FindObjectOfType<gameMovement>();
    }

    void Update()
    {
        fps = 1f / Time.unscaledDeltaTime;

        if (Keyboard.current != null &&
            Keyboard.current.f3Key.wasPressedThisFrame)
        {
            show = !show;
        }
        
    }

    public void Toggle()
    {
        show = !show;
    }

    void OnGUI()
    {
        if (!show || movement == null)
            return;

        GUI.Box(new Rect(10, 10, 320, 300), "");

        GUILayout.BeginArea(new Rect(20, 20, 300, 280));

        GUILayout.Label("<b>Developer Overlay</b>");

        GUILayout.Space(10);

        GUILayout.Label($"FPS: {fps:F0}");

        GUILayout.Space(10);

        GUILayout.Label("Movement");

        GUILayout.Label($"Grounded: {movement.data.Grounded}");

        GUILayout.Label($"Speed: {movement.data.velocityXZ.magnitude:F2}");

        GUILayout.Label($"Velocity X: {movement.data.velocity.x:F2}");

        GUILayout.Label($"Velocity Y: {movement.data.velocity.y:F2}");

        GUILayout.Label($"Velocity Z: {movement.data.velocity.z:F2}");

        GUILayout.Space(10);

        GUILayout.Label("Position");

        GUILayout.Label($"X: {movement.transform.position.x:F2}");

        GUILayout.Label($"Y: {movement.transform.position.y:F2}");

        GUILayout.Label($"Z: {movement.transform.position.z:F2}");

        GUILayout.EndArea();
    }
}