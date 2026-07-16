using UnityEngine;

public class ImpactEffect : MonoBehaviour
{
    [Header("Particle Settings")]
    [SerializeField] private int particleCount = 8;
    [SerializeField] private float particleSpeed = 2f;
    [SerializeField] private float particleLifetime = 1f;
    [SerializeField] private float particleSize = 0.1f;
    [SerializeField] private Color particleColor = Color.gray;

    [Header("References")]
    [SerializeField] private GameObject particlePrefab;

    private void Start()
    {
        SpawnParticles();
        Destroy(gameObject, particleLifetime + 0.5f);
    }

    private void SpawnParticles()
    {
        for (int i = 0; i < particleCount; i++)
        {
            // random dir in hemisphere
            Vector3 randomDir = Random.onUnitSphere;
            if (Vector3.Dot(randomDir, transform.forward) < 0)
                randomDir = -randomDir;

            GameObject particle = Instantiate(particlePrefab, transform.position, Quaternion.identity);

            // size
            particle.transform.localScale = Vector3.one * particleSize;

            // color
            SpriteRenderer sr = particle.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = particleColor;
                sr.sortingOrder = 10;
            }

            // mover
            ImpactParticleMover mover = particle.AddComponent<ImpactParticleMover>();
            mover.Initialize(randomDir * particleSpeed, particleLifetime);

            Destroy(particle, particleLifetime);
        }
    }
}

// tiny mover for impact particles
public class ImpactParticleMover : MonoBehaviour
{
    private Vector3 velocity;
    private float lifetime;
    private float age;

    public void Initialize(Vector3 vel, float life)
    {
        velocity = vel;
        lifetime = life;
        age = 0f;
    }

    private void Update()
    {
        age += Time.deltaTime;
        float t = age / lifetime;

        // slow down over time
        transform.position += velocity * Time.deltaTime * (1f - t);

        // fade out
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f - t;
            sr.color = c;
        }
    }
}