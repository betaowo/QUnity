using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TeleportTrigger : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform teleportTarget;
    [SerializeField] private bool keepVelocity = true;
    [SerializeField] private bool resetPitch = false;

    [Header("Effects")]
    [SerializeField] private AudioClip teleportSound;
    [SerializeField] private GameObject teleportFx;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (teleportTarget == null)
        {
            Debug.LogWarning("TeleportTrigger has no target assigned!");
            return;
        }

        // move plr
        var gm = other.GetComponent<gameMovement>();
        if (gm != null)
        {
            gm.data.origin = teleportTarget.position;
            other.transform.position = teleportTarget.position;

            if (!keepVelocity)
                gm.data.velocity = Vector3.zero;
        }
        else
        {
            other.transform.position = teleportTarget.position;
        }

        // rotate
        other.transform.rotation = teleportTarget.rotation;

        // reset pitch if needed
        if (resetPitch)
        {
            var aiming = other.GetComponentInChildren<PlayerAiming>();
            if (aiming != null)
                aiming.ResetRotation();
        }

        // fx
        if (teleportSound != null)
            AudioSource.PlayClipAtPoint(teleportSound, transform.position);
        if (teleportFx != null)
            Instantiate(teleportFx, teleportTarget.position, Quaternion.identity);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(transform.position, transform.localScale * 0.5f);

        if (teleportTarget != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(teleportTarget.position, Vector3.one * 0.3f);
            Gizmos.DrawLine(transform.position, teleportTarget.position);
        }
    }
}