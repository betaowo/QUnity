using UnityEngine;

public enum PickupType
{
    Health_Small,
    Health_Large,
    Armor_Green,
    Armor_Yellow,
    Armor_Red,
    Ammo,
    Weapon,
    QuadDamage,
    RingOfShadows,
    Biosuit
}

[RequireComponent(typeof(Collider))]
public class Pickup : MonoBehaviour
{
    [Header("Type")]
    [SerializeField] private PickupType pickupType = PickupType.Health_Small;

    [Header("Weapon (if type = Weapon)")]
    [SerializeField] private string weaponName = "Shotgun";

    [Header("Ammo (if type = Ammo)")]
    [SerializeField] private AmmoType ammoType = AmmoType.Shells;
    [SerializeField] private int ammoAmount = 25;

    [Header("Amount (health/armor)")]
    [SerializeField] private float amount = 15f;

    [Header("Armor Absorb")]
    [SerializeField] private float absorb = 0.6f;

    [Header("Respawn")]
    [SerializeField] private bool isRespawnable = true;
    [SerializeField] private float respawnTime = 30f;

    [Header("Animation")]
    [SerializeField] private bool playAnim = true;
    [SerializeField] private float bobHeight = 0.1f;
    [SerializeField] private float bobSpeed = 3f;
    [SerializeField] private float rotateSpeed = 60f;

    [Header("Effects")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private GameObject pickupFx;

    private Collider myColl;
    private Renderer myRenderer;
    private bool pickedUp;
    private float respawnTimer;
    private Vector3 startPos;

    private void Start()
    {
        myColl = GetComponent<Collider>();
        myColl.isTrigger = true;
        myRenderer = GetComponent<Renderer>();
        startPos = transform.position;
    }

    private void Update()
    {
        if (pickedUp)
        {
            if (isRespawnable)
            {
                respawnTimer -= Time.deltaTime;
                if (respawnTimer <= 0) Respawn();
            }
            return;
        }

        if (playAnim)
        {
            float bob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = startPos + Vector3.up * bob;
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (pickedUp || !other.CompareTag("Player")) return;

        var hp = other.GetComponent<HealthComponent>();
        var holder = other.GetComponentInChildren<WeaponHolder>();
        bool taken = false;

        switch (pickupType)
        {
            case PickupType.Health_Small:
                if (hp != null && hp.CurrentHealth < hp.MaxHealth)
                { hp.Heal(15); taken = true; }
                break;

            case PickupType.Health_Large:
                if (hp != null && hp.CurrentHealth < hp.MaxHealth)
                { hp.Heal(25); taken = true; }
                break;

            case PickupType.Armor_Green:
                if (hp != null) { hp.GiveArmor(100, 0.3f); taken = true; }
                break;

            case PickupType.Armor_Yellow:
                if (hp != null) { hp.GiveArmor(150, 0.5f); taken = true; }
                break;

            case PickupType.Armor_Red:
                if (hp != null) { hp.GiveArmor(200, 0.6f); taken = true; }
                break;

            case PickupType.Ammo:
                if (holder != null)
                {
                    taken = GiveAmmoToWeapon(holder, ammoType, ammoAmount);
                }
                break;

            case PickupType.Weapon:
                if (holder != null && !string.IsNullOrEmpty(weaponName))
                {
                    if (!holder.HasWeapon(weaponName))
                    { holder.GiveWeapon(weaponName); taken = true; }
                }
                break;

            case PickupType.QuadDamage:
            case PickupType.RingOfShadows:
            case PickupType.Biosuit:
                taken = true; // stub
                break;
        }

        if (taken)
        {
            pickedUp = true;

            if (pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            if (pickupFx != null)
                Instantiate(pickupFx, transform.position, Quaternion.identity);
            if (myRenderer != null)
                myRenderer.enabled = false;
            myColl.enabled = false;

            if (isRespawnable)
                respawnTimer = respawnTime;
            else
                Destroy(gameObject, 0.1f);
        }
    }

    private bool GiveAmmoToWeapon(WeaponHolder holder, AmmoType type, int amt)
    {
        foreach (var wep in holder.GetAllWeapons())
        {
            if (wep.GetAmmoType() == type && holder.HasWeapon(wep.WeaponName))
            {
                wep.AddAmmo(amt);
                return true;
            }
        }
        return false;
    }

    private void Respawn()
    {
        pickedUp = false;
        if (myRenderer != null) myRenderer.enabled = true;
        myColl.enabled = true;
        respawnTimer = 0f;
    }

    private void OnValidate()
    {
        amount = pickupType switch
        {
            PickupType.Health_Small => 15,
            PickupType.Health_Large => 25,
            PickupType.Armor_Green => 100,
            PickupType.Armor_Yellow => 150,
            PickupType.Armor_Red => 200,
            _ => amount
        };
    }
}