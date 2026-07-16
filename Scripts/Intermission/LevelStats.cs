using UnityEngine;

public class LevelStats : MonoBehaviour
{
    public static LevelStats Instance { get; private set; }

    [Header("Map Info")]
    [SerializeField] private string mapName = "E1M1";
    [SerializeField] public int totalEnemies = 8;
    [SerializeField] public int totalSecrets = 3;

    // runtime
    public int enemiesKilled { get; private set; }
    public int secretsFound { get; private set; }
    public float startTime { get; private set; }
    public bool isFinished { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        startTime = Time.time;
        isFinished = false;
        enemiesKilled = 0;
        secretsFound = 0;
    }

    public void EnemyKilled()
    {
        enemiesKilled++;
    }

    public void SecretFound()
    {
        secretsFound++;
    }

    public void FinishLevel()
    {
        isFinished = true;
    }

    public float GetElapsedTime()
    {
        return Time.time - startTime;
    }

    public string GetTimeString()
    {
        float t = GetElapsedTime();
        int min = Mathf.FloorToInt(t / 60f);
        int sec = Mathf.FloorToInt(t % 60f);
        return $"{min}:{sec:D2}";
    }

    public float GetCompletionPercent()
    {
        float total = totalEnemies + totalSecrets;
        if (total <= 0) return 100f;
        float done = enemiesKilled + secretsFound;
        return Mathf.Clamp01(done / total) * 100f;
    }
}