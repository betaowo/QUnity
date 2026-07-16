using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MessageTrigger : MonoBehaviour
{
    [Header("Message")]
    [SerializeField] private string message = "Hello.";
    [SerializeField] private float duration = 3f;
    [SerializeField] private bool showOnce = true;
    [SerializeField] private float triggerDelay = 0f;

    [Header("Audio")]
    [SerializeField] private AudioClip messageSound;

    private bool triggered;
    private float lastTriggerTime;
    private static string activeMsg;
    private static float msgEndTime;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (showOnce && triggered) return;

        if (Time.time - lastTriggerTime < triggerDelay) return;

        triggered = true;
        lastTriggerTime = Time.time;

        activeMsg = message;
        msgEndTime = Time.time + duration;

        if (messageSound != null)
            AudioSource.PlayClipAtPoint(messageSound, transform.position);
    }

    private void OnGUI()
    {
        if (Time.time > msgEndTime) return;
        if (string.IsNullOrEmpty(activeMsg)) return;

        // split lines (Quake messages use \n)
        string[] lines = activeMsg.Split('\n');

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 22;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.MiddleCenter;

        float alpha = 1f;
        if (Time.time > msgEndTime - 0.5f)
            alpha = (msgEndTime - Time.time) / 0.5f;

        Color c = style.normal.textColor;
        c.a = alpha;
        style.normal.textColor = c;

        float yStart = Screen.height / 2f - (lines.Length - 1) * 30f;
        for (int i = 0; i < lines.Length; i++)
        {
            GUI.Label(new Rect(0, yStart + i * 30f, Screen.width, 40), lines[i], style);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 1f, 1f, 0.3f);
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
}