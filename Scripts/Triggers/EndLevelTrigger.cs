using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class EndLevelTrigger : MonoBehaviour
{
    [Header("Next Level")]
    [SerializeField] private string nextScene = "Map_E1M2";
    [SerializeField] private float intermissionTime = 5f;

    [Header("Camera")]
    [SerializeField] private Transform intermissionCam;

    private bool triggered;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        StartCoroutine(EndLevelSequence(other.gameObject));
    }

    private IEnumerator EndLevelSequence(GameObject plr)
    {
        if (LevelStats.Instance != null)
            LevelStats.Instance.FinishLevel();

        var gm = plr.GetComponent<gameMovement>();
        if (gm != null) gm.enabled = false;

        var input = plr.GetComponent<UnityEngine.InputSystem.PlayerInput>();
        if (input != null) input.enabled = false;

        var holder = plr.GetComponentInChildren<WeaponHolder>();
        if (holder != null) holder.gameObject.SetActive(false);

        var hp = plr.GetComponent<HealthComponent>();
        if (hp != null) hp.SetGodMode(true);

        var allEnemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        foreach (var e in allEnemies) e.enabled = false;

        Camera mainCam = Camera.main;
        if (mainCam != null && intermissionCam != null)
        {
            mainCam.transform.position = intermissionCam.position;
            mainCam.transform.rotation = intermissionCam.rotation;
        }

        IntermissionScreen.Show();
        yield return new WaitForSeconds(intermissionTime);
        IntermissionScreen.Hide();

        // destroy plr before loading next scene
        Destroy(plr);

        yield return null; // wait one frame

        UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        if (intermissionCam != null)
            Gizmos.DrawLine(transform.position, intermissionCam.position);
    }
}