using UnityEngine;
using System.Collections;

public enum EnemyType
{
    Melee,
    Ranged
}

public class EnemyBase : MonoBehaviour
{
    [Header("Type")]
    [SerializeField] private EnemyType enemyType = EnemyType.Ranged;

    [Header("Stats")]
    [SerializeField] private string enemyName = "Bum";
    [SerializeField] private float maxHp = 50f;
    [SerializeField] private float curHp;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSpeed = 8f;

    [Header("Combat")]
    [SerializeField] private float attackRange = 30f;
    [SerializeField] private float meleeRange = 2.5f;
    [SerializeField] private float attackRate = 0.8f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private int pelletsPerShot = 1;
    [SerializeField] private float spreadAngle = 3f;
    [SerializeField] private Transform muzzlePoint;

    [Header("Sight")]
    [SerializeField] private float sightRange = 40f;
    [SerializeField] private LayerMask sightMask;

    [Header("Pain")]
    [SerializeField] private float painChance = 0.5f;
    [SerializeField] private float painCooldown = 0.3f;
    [SerializeField] private float painAnimTime = 0.4f;

    [Header("Death")]
    [SerializeField] private float corpseTime = 3f;
    [SerializeField] private GameObject gibPrefab;
    [SerializeField] private float gibHealth = -30f;

    [Header("Loot")]
    [SerializeField] private GameObject dropPrefab;
    [SerializeField] private float dropChance = 0.3f;

    [Header("Refs")]
    [SerializeField] private EnemyAnimator animator;
    [SerializeField] private AudioSource audioSrc;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip painSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip spotSound;
    [SerializeField] private Collider myColl;
    [SerializeField] private GameObject impactFxPrefab;

    // state
    private bool isDead;
    private Transform plr;
    private Rigidbody rb;
    private float nextAttackTime;
    private float nextPainTime;
    private float painEndTime;
    private bool inPain;
    private Vector3 moveDir;

    public bool IsDead => isDead;

    private void Start()
    {
        curHp = maxHp;
        plr = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (animator == null) animator = GetComponent<EnemyAnimator>();
        if (audioSrc == null) audioSrc = GetComponent<AudioSource>();
        if (myColl == null) myColl = GetComponent<Collider>();

        // physics - add RB if none, freeze rotation so he dont tip over
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.mass = 80f;
        rb.drag = 0f; // no drag in air
        rb.angularDrag = 0f;

        if (myColl != null)
        {
            // ignore collision with plr - we use OverlapSphere for melee
            GameObject plrObj = GameObject.FindGameObjectWithTag("Player");
            if (plrObj != null)
            {
                Collider plrCol = plrObj.GetComponent<Collider>();
                if (plrCol != null)
                    Physics.IgnoreCollision(myColl, plrCol, true);
            }
        }

        animator?.PlayIdle();
    }

    private void Update()
    {
        if (isDead || plr == null) return;

        // pain timer
        if (inPain && Time.time >= painEndTime)
            inPain = false;

        float dist = Vector3.Distance(transform.position, plr.position);
        bool seePlr = dist <= sightRange && CanSeePlayer();

        if (!seePlr)
        {
            animator?.SetWalking(false);
            // stop moving when blind
            if (rb != null)
                rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            return;
        }

        // face plr
        FaceTarget(plr.position);

        Vector3 dir = (plr.position - transform.position).normalized;
        dir.y = 0f;

        if (enemyType == EnemyType.Melee)
        {
            if (dist > meleeRange)
            {
                // move toward plr
                if (rb != null)
                    rb.velocity = new Vector3(dir.x * moveSpeed, rb.velocity.y, dir.z * moveSpeed);
                else
                    transform.position += dir * moveSpeed * Time.deltaTime;

                animator?.SetWalking(true);
            }
            else
            {
                // attack
                if (rb != null)
                    rb.velocity = new Vector3(0f, rb.velocity.y, 0f);

                animator?.SetWalking(false);

                if (!inPain && Time.time >= nextAttackTime)
                {
                    DoAttack();
                    nextAttackTime = Time.time + attackRate;
                }
            }
        }
        else // ranged
        {
            float idealDist = attackRange * 0.6f;

            if (dist > attackRange)
            {
                // move closer
                if (rb != null)
                    rb.velocity = new Vector3(dir.x * moveSpeed, rb.velocity.y, dir.z * moveSpeed);
                else
                    transform.position += dir * moveSpeed * Time.deltaTime;

                animator?.SetWalking(true);
            }
            else if (dist < idealDist)
            {
                // back up
                Vector3 backDir = -dir;
                if (rb != null)
                    rb.velocity = new Vector3(backDir.x * moveSpeed, rb.velocity.y, backDir.z * moveSpeed);
                else
                    transform.position += backDir * moveSpeed * Time.deltaTime;

                animator?.SetWalking(true);
            }
            else
            {
                // hold position, shoot
                if (rb != null)
                    rb.velocity = new Vector3(0f, rb.velocity.y, 0f);

                animator?.SetWalking(false);

                if (!inPain && Time.time >= nextAttackTime)
                {
                    DoAttack();
                    nextAttackTime = Time.time + attackRate;
                }
            }
        }

        if (rb != null && !rb.isKinematic)
            rb.AddForce(Vector3.down * 20f, ForceMode.Acceleration); // fake heavier gravity
    }

    private bool CanSeePlayer()
    {
        Vector3 dir = (plr.position - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, plr.position);

        Vector3 eye = transform.position + Vector3.up * 0.6f;
        if (Physics.Raycast(eye, dir, out RaycastHit hit, dist, sightMask))
        {
            return hit.collider.CompareTag("Player");
        }
        return true; // no obstacle = see plr
    }

    private void FaceTarget(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        dir.y = 0f;
        if (dir.magnitude < 0.01f) return;

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * turnSpeed);
    }

    private void DoAttack()
    {
        if (inPain) return;

        animator?.PlayAttack();
        if (audioSrc != null && attackSound != null)
            audioSrc.PlayOneShot(attackSound);

        float dist = Vector3.Distance(transform.position, plr.position);

        if (enemyType == EnemyType.Melee)
        {
            // use overlap sphere for melee hit detection — way more reliable
            float swingRange = meleeRange + 1f;
            Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 0.5f, swingRange);

            bool hitPlr = false;
            foreach (var col in hits)
            {
                if (col.CompareTag("Player"))
                {
                    var hp = col.GetComponent<HealthComponent>();
                    if (hp != null)
                    {
                        hp.TakeDamage(damage);
                        hitPlr = true;
                        Debug.Log($"Melee hit plr for {damage}! Plr HP: {hp.CurrentHealth}");
                    }
                }
            }

            if (!hitPlr)
                Debug.Log("Melee swing missed");
        }

        else if (enemyType == EnemyType.Ranged)
        {
            Vector3 muzzlePos = muzzlePoint != null ? muzzlePoint.position : transform.position + Vector3.up * 0.6f + transform.forward * 0.5f;

            for (int i = 0; i < pelletsPerShot; i++)
            {
                Vector3 shootDir = GetSpreadDir();
                Debug.DrawRay(muzzlePos, shootDir * attackRange, Color.red, 0.3f);

                if (Physics.Raycast(muzzlePos, shootDir, out RaycastHit hit, attackRange))
                {
                    var hp = hit.collider.GetComponent<HealthComponent>();
                    hp?.TakeDamage(damage);

                    if (impactFxPrefab != null)
                    {
                        GameObject fx = Instantiate(impactFxPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(fx, 1f);
                    }
                }
            }
        }
    }

    private Vector3 GetSpreadDir()
    {
        Vector3 baseDir = (plr.position - (muzzlePoint != null ? muzzlePoint.position : transform.position + Vector3.up * 0.6f)).normalized;
        if (spreadAngle <= 0f) return baseDir;
        return Quaternion.Euler(Random.Range(-spreadAngle, spreadAngle), Random.Range(-spreadAngle, spreadAngle), 0f) * baseDir;
    }

    public void TakeDamage(float dmg)
    {
        if (isDead) return;
        curHp -= dmg;

        if (!inPain && Time.time >= nextPainTime && Random.value < painChance)
        {
            animator?.PlayPain();
            inPain = true;
            painEndTime = Time.time + painAnimTime;
            if (audioSrc != null && painSound != null)
                audioSrc.PlayOneShot(painSound);
            nextPainTime = Time.time + painCooldown;
        }

        if (curHp <= 0)
        {
            if (curHp <= gibHealth && gibPrefab != null)
                Gib();
            else
                Die();
        }
    }

    private void Die()
    {
        isDead = true;
        animator?.PlayDeath();
        if (audioSrc != null && deathSound != null)
            audioSrc.PlayOneShot(deathSound);

        if (dropPrefab != null && Random.value < dropChance)
            Instantiate(dropPrefab, transform.position + Vector3.up * 0.3f, Quaternion.identity);

        Destroy(gameObject, corpseTime);
    }

    private void Gib()
    {
        isDead = true;
        if (gibPrefab != null) Instantiate(gibPrefab, transform.position, Quaternion.identity);
        if (audioSrc != null && deathSound != null) audioSrc.PlayOneShot(deathSound);
        Destroy(gameObject, 0.05f);
    }
}