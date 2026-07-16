using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class HealthComponent : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isGodMode = false;

    [Header("Armor")]
    [SerializeField] private float maxArmor = 200f;
    [SerializeField] private float currentArmor;
    [SerializeField] private float armorAbsorb = 0.6f; // green armor = 0.3, red = 0.6, yellow = 0.5

    [Header("Death - References")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private Transform playerBody;
    [SerializeField] private MonoBehaviour[] disableOnDeath;
    [SerializeField] private GameObject[] hideOnDeath;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float CurrentArmor => currentArmor;
    public float MaxArmor => maxArmor;
    public bool IsDead { get; private set; }
    public bool IsGodMode { get => isGodMode; set => isGodMode = value; }

    public System.Action<float, float> OnHealthChanged;
    public System.Action<float, float> OnArmorChanged;
    public System.Action OnDeath;
    public System.Action<float> OnDamage;

    private void Awake()
    {
        currentHealth = maxHealth;
        currentArmor = 0f;

        if (cameraHolder == null) cameraHolder = transform.Find("cameraHolder");
        if (playerBody == null) playerBody = transform;
    }

    private void Update()
    {
        if (IsDead)
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame ||
                Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                RestartScene();
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (IsDead || isGodMode) return;

        // armor absorbs some dmg
        float absorbed = 0f;
        if (currentArmor > 0)
        {
            absorbed = damage * armorAbsorb;
            if (absorbed > currentArmor)
                absorbed = currentArmor;

            currentArmor -= absorbed;
            OnArmorChanged?.Invoke(currentArmor, maxArmor);
        }

        float healthDmg = damage - absorbed;
        currentHealth -= healthDmg;

        OnDamage?.Invoke(damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void GiveArmor(float amount, float absorb = 0.6f)
    {
        if (IsDead) return;
        currentArmor = Mathf.Min(currentArmor + amount, maxArmor);
        armorAbsorb = absorb; // set armor type
        OnArmorChanged?.Invoke(currentArmor, maxArmor);
    }

    public void SetGodMode(bool enabled)
    {
        isGodMode = enabled;
    }

    public void SetHealth(float amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Clamp(amount, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        if (IsDead) return;
        IsDead = true;

        if (disableOnDeath != null)
        {
            foreach (var script in disableOnDeath)
                if (script != null) script.enabled = false;
        }

        if (hideOnDeath != null)
        {
            foreach (var obj in hideOnDeath)
                if (obj != null) obj.SetActive(false);
        }

        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null) playerInput.enabled = false;

        if (cameraHolder != null)
            cameraHolder.SetParent(null);

        if (playerBody != null)
        {
            Rigidbody rb = playerBody.GetComponent<Rigidbody>();
            if (rb == null) rb = playerBody.gameObject.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.isKinematic = true;
            playerBody.rotation = Quaternion.Euler(0f, playerBody.eulerAngles.y, -90f);
        }

        OnDeath?.Invoke();
        Debug.Log("Player died. Press Fire to restart.");
    }

    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnValidate()
    {
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        if (currentArmor > maxArmor) currentArmor = maxArmor;
    }
}