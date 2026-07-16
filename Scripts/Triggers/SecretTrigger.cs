using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class SecretTrigger : MonoBehaviour
{
    [SerializeField] private string secretMessage = "A secret is revealed!";
    [SerializeField] private AudioClip secretSound;
    [SerializeField] private float messageDuration = 3f;

    private bool found;
    private static string activeMessage;
    private static float messageEndTime;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (found) return;
        if (!other.CompareTag("Player")) return;

        found = true;
        LevelStats.Instance?.SecretFound();

        // show message
        activeMessage = secretMessage;
        messageEndTime = Time.time + messageDuration;

        if (secretSound != null)
            AudioSource.PlayClipAtPoint(secretSound, transform.position);
    }

    private void OnGUI()
    {
        if (Time.time > messageEndTime) return;
        if (string.IsNullOrEmpty(activeMessage)) return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 28;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.yellow;
        style.alignment = TextAnchor.MiddleCenter;

        float alpha = 1f;
        if (Time.time > messageEndTime - 0.5f)
            alpha = (messageEndTime - Time.time) / 0.5f;

        Color c = style.normal.textColor;
        c.a = alpha;
        style.normal.textColor = c;

        GUI.Label(new Rect(0, Screen.height / 2f - 40, Screen.width, 60), activeMessage, style);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.4f);
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
}