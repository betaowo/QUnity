using UnityEngine;

public class PlayerSounds : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private gameMovement movement;
    [SerializeField] private HealthComponent health;
    [SerializeField] private AudioSource audioSource;

    [Header("Footsteps")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.35f;
    [SerializeField] private float minStepSpeed = 0.5f;

    [Header("Jump")]
    [SerializeField] private AudioClip jumpSound;

    [Header("Death")]
    [SerializeField] private AudioClip deathSound;

    [Header("Landing")]
    [SerializeField] private AudioClip landSound;
    [SerializeField] private float minLandVelocity = -5f;

    private float stepTimer;
    private bool wasGrounded;
    private bool jumpPlayed;

    private void Start()
    {
        if (movement == null)
            movement = GetComponent<gameMovement>();
        if (health == null)
            health = GetComponent<HealthComponent>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (health != null)
            health.OnDeath += OnDeath;
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnDeath -= OnDeath;
    }

    private void Update()
    {
        if (movement == null || health == null || health.IsDead)
            return;

        bool grounded = movement.data.Grounded;
        float speed = movement.data.velocityXZ.magnitude;

        // === STEPS ===
        if (grounded && speed > minStepSpeed)
        {
            float interval = speed > movement.movevars.maxspeed * 0.7f ? runStepInterval : walkStepInterval;
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = interval;
            }
        }
        else if (grounded)
        {
            stepTimer = 0f;
        }

        // === JUMP ===
        if (!grounded && wasGrounded && !jumpPlayed)
        {
            if (movement.data.velocity.y > 1f)
            {
                PlayJump();
                jumpPlayed = true;
            }
        }

        if (grounded)
        {
            jumpPlayed = false;
        }

        // === LANDING ===
        if (grounded && !wasGrounded)
        {
            if (movement.data.velocity.y < minLandVelocity)
            {
                PlayLand();
            }
        }

        wasGrounded = grounded;
    }

    private void PlayFootstep()
    {
        if (footstepSounds == null || footstepSounds.Length == 0)
            return;

        // filter null clips
        var validClips = System.Array.FindAll(footstepSounds, clip => clip != null);
        if (validClips.Length == 0) return;

        AudioClip clip = validClips[Random.Range(0, validClips.Length)];
        audioSource.PlayOneShot(clip, 0.6f);
    }

    private void PlayJump()
    {
        if (jumpSound != null)
            audioSource.PlayOneShot(jumpSound, 0.7f);
    }

    private void PlayLand()
    {
        if (landSound != null)
            audioSource.PlayOneShot(landSound, 0.5f);
    }

    private void OnDeath()
    {
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position, 0.8f);
        }
    }
}