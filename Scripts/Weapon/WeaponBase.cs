using UnityEngine;

public enum AmmoType
{
    None,       // melee
    Shells,     // shotgun
    Nails,      // nailgun
    Rockets,    // rocket launcher
    Cells       // thunderbolt
}

public enum WeaponType
{
    Melee,
    SingleShot,
    Burst
}

public enum ReloadType
{
    None,
    Magazine,
    Single
}

public class WeaponBase : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private WeaponType weaponType = WeaponType.SingleShot;
    [SerializeField] private string weaponName = "Weapon";
    [SerializeField] private float damage = 10f;
    [SerializeField] private float range = 100f;
    [SerializeField] private float fireRate = 0.1f;
    [SerializeField] private bool isAuto = false;
    [SerializeField] private int pelletsPerShot = 1;
    [SerializeField] private float spreadAngle = 0f;

    [Header("Ammo")]
    [SerializeField] private AmmoType ammoType = AmmoType.None;
    [SerializeField] private bool infiniteAmmo = false;
    [SerializeField] private int maxAmmo = 100;
    [SerializeField] private int currentAmmo;

    [Header("Magazine")]
    [SerializeField] private bool hasMagazine = false;
    [SerializeField] private int magSize = 30;
    [SerializeField] private int currentMag;
    [SerializeField] private ReloadType reloadType = ReloadType.Magazine;
    [SerializeField] private float reloadTime = 1.5f;
    [SerializeField] private float singleReloadTime = 0.3f;

    [Header("Refs")]
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private MuzzleFlash muzzleFlash;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private AudioClip reloadSound;
    [SerializeField] private WeaponAnimator weaponAnimator;
    [SerializeField] private GameObject impactEffectPrefab;

    // state
    private float nextFireTime;
    private bool isReloading;
    private float reloadEndTime;
    private int reloadsDone;

    [Header("Ownership")]
    [SerializeField] private bool owned = false;

    public string WeaponName => weaponName;
    public AmmoType GetAmmoType() => ammoType;
    public int CurrentAmmo => hasMagazine ? currentMag : currentAmmo;
    public int ReserveAmmo => hasMagazine ? currentAmmo : currentAmmo;
    public int MaxAmmo => maxAmmo;
    public int CurrentMag => currentMag;
    public int MagSize => magSize;
    public bool IsReloading => isReloading;
    public bool HasMagazine => hasMagazine;
    public bool IsAuto => isAuto;
    public WeaponType Type => weaponType;
    public bool IsOwned
    {
        get => owned;
        set
        {
            owned = value;
            gameObject.SetActive(value && IsCurrentWeapon);
        }
    }
    public bool IsCurrentWeapon { get; set; }

    private void Start()
    {
        if (!infiniteAmmo)
        {
            currentAmmo = maxAmmo;
            if (hasMagazine)
            {
                currentMag = magSize;
                currentAmmo = maxAmmo - magSize;
            }
        }
        else
        {
            currentAmmo = 999;
            if (hasMagazine) currentMag = magSize;
        }

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // single reload logic
        if (isReloading && reloadType == ReloadType.Single)
        {
            if (Time.time >= reloadEndTime && currentMag < magSize && currentAmmo > 0)
            {
                currentMag++;
                currentAmmo--;
                reloadEndTime = Time.time + singleReloadTime;
                reloadsDone++;

                if (weaponAnimator != null)
                    weaponAnimator.PlaySingleReloadAnimation();
                if (audioSource != null && reloadSound != null)
                    audioSource.PlayOneShot(reloadSound);
            }

            if (currentMag >= magSize || currentAmmo <= 0)
            {
                isReloading = false;
            }
        }
    }

    public void Fire()
    {
        if (Time.time < nextFireTime) return;
        if (isReloading) return;

        // check ammo
        bool hasAmmo = infiniteAmmo || (hasMagazine ? currentMag > 0 : currentAmmo > 0);
        if (weaponType != WeaponType.Melee && !hasAmmo)
        {
            // dryfire + auto reload
            if (weaponAnimator != null)
                weaponAnimator.PlayDryfireAnimation();
            if (hasMagazine && currentAmmo > 0)
                StartReload();
            return;
        }

        // spend ammo
        if (!infiniteAmmo)
        {
            if (hasMagazine) currentMag--;
            else currentAmmo--;
        }

        nextFireTime = Time.time + fireRate;

        // effects
        PlayFireEffects();

        // shoot
        switch (weaponType)
        {
            case WeaponType.Melee:
                DoMeleeAttack();
                break;
            case WeaponType.SingleShot:
                DoHitscan(1);
                break;
            case WeaponType.Burst:
                DoHitscan(pelletsPerShot);
                break;
        }

        // auto reload
        if (!infiniteAmmo && hasMagazine && currentMag <= 0 && currentAmmo > 0)
            StartReload();
    }

    public void Reload()
    {
        if (isReloading) return;

        // kill any playing anim before reload
        if (weaponAnimator != null)
        {
            weaponAnimator.StopCurrentAnimation();
            weaponAnimator.ResetAllBlendShapes();
        }

        if (weaponType == WeaponType.Melee) return;
        if (infiniteAmmo)
        {
            if (hasMagazine) currentMag = magSize;
            return;
        }
        if (hasMagazine && currentMag >= magSize) return;
        if (!hasMagazine && currentAmmo >= maxAmmo) return;
        if (currentAmmo <= 0) return;

        StartReload();
    }

    private void StartReload()
    {
        if (isReloading) return;

        if (weaponAnimator != null)
        {
            weaponAnimator.StopCurrentAnimation();
            weaponAnimator.ResetAllBlendShapes();
        }

        isReloading = true;

        if (reloadType == ReloadType.Magazine)
        {
            int needed = magSize - currentMag;
            int toReload = Mathf.Min(needed, currentAmmo);
            currentMag += toReload;
            currentAmmo -= toReload;

            // anim and snd
            if (weaponAnimator != null)
                weaponAnimator.PlayReloadAnimation();
            if (audioSource != null && reloadSound != null)
                audioSource.PlayOneShot(reloadSound);

            Invoke(nameof(FinishMagReload), reloadTime);
        }
        else
        {
            reloadEndTime = Time.time;
            reloadsDone = 0;
        }
    }

    private void FinishMagReload()
    {
        isReloading = false;
    }

    private void PlayFireEffects()
    {
        if (weaponAnimator != null)
        {
            weaponAnimator.StopCurrentAnimation();
            weaponAnimator.PlayFireAnimation();
        }
        if (muzzleFlash != null) muzzleFlash.Play();
        if (audioSource != null && fireSound != null)
            audioSource.PlayOneShot(fireSound);
    }

    private void DoHitscan(int pellets)
    {
        for (int i = 0; i < pellets; i++)
        {
            Vector3 dir = GetSpreadDir();
            Debug.DrawRay(muzzlePoint.position, dir * range, Color.red, 0.5f);

            int mask = ~LayerMask.GetMask("Player", "Weapon", "Ignore Raycast");
            if (Physics.Raycast(muzzlePoint.position, dir, out RaycastHit hit, range, mask, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform.root))
                    continue;

                var bum = hit.collider.GetComponent<EnemyBase>();
                bum?.TakeDamage(damage);

                var hp = hit.collider.GetComponent<HealthComponent>();
                hp?.TakeDamage(damage);

                SpawnImpact(hit.point, hit.normal);
            }
        }
    }

    private void DoMeleeAttack()
    {
        if (Camera.main == null || muzzlePoint == null) return;

        Vector3 dir = Camera.main.transform.forward;
        int mask = ~LayerMask.GetMask("Player", "Weapon", "Ignore Raycast");

        if (Physics.Raycast(muzzlePoint.position, dir, out RaycastHit hit, 2f, mask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform.root))
                return;

            var bum = hit.collider.GetComponent<EnemyBase>();
            bum?.TakeDamage(damage);

            var hp = hit.collider.GetComponent<HealthComponent>();
            hp?.TakeDamage(damage);

            SpawnImpact(hit.point, hit.normal);
        }
    }

    private Vector3 GetSpreadDir()
    {
        Vector3 baseDir = Camera.main.transform.forward;
        if (spreadAngle <= 0f) return baseDir;
        float x = Random.Range(-spreadAngle, spreadAngle);
        float y = Random.Range(-spreadAngle, spreadAngle);
        return Quaternion.Euler(x, y, 0f) * baseDir;
    }

    private void SpawnImpact(Vector3 pos, Vector3 normal)
    {
        if (impactEffectPrefab == null) return;
        GameObject fx = Instantiate(impactEffectPrefab, pos, Quaternion.LookRotation(normal));
        Destroy(fx, 2f);
    }

    public void AddAmmo(int amount)
    {
        if (infiniteAmmo) return;
        currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
    }
}