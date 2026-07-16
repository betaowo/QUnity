using UnityEngine;
using System.Collections.Generic;

public enum WaterType
{
    Water,
    Slime,
    Lava
}

[RequireComponent(typeof(Collider))]
public class WaterVolume : MonoBehaviour
{
    [Header("Water Settings")]
    [SerializeField] private WaterType waterType = WaterType.Water;
    [SerializeField] private float waterGravity = -10f;
    [SerializeField] private float waterMaxSpeed = 15f;
    [SerializeField] private float waterAccelerate = 3f;
    [SerializeField] private float waterFriction = 3f;
    [SerializeField] private float buoyancy = 1.5f;
    [SerializeField] private float surfaceLevel;

    [Header("Damage")]
    [SerializeField] private float damagePerSecond = 0f;
    [SerializeField] private float damageTickRate = 0.5f;

    private Collider waterCollider;
    private float nextDamageTime;
    private static HashSet<WaterVolume> activeVolumes = new();

    public static bool IsInWater { get; private set; }
    public static WaterType CurrentType { get; private set; }
    public static float Gravity { get; private set; }
    public static float MaxSpeed { get; private set; }
    public static float Accelerate { get; private set; }
    public static float Friction { get; private set; }
    public static float Buoyancy { get; private set; }
    public static float SurfaceLevel { get; private set; }

    private void Awake()
    {
        waterCollider = GetComponent<Collider>();
        waterCollider.isTrigger = true;

        if (waterCollider is BoxCollider box)
            surfaceLevel = transform.position.y + box.center.y + box.size.y * 0.5f;
        else
            surfaceLevel = transform.position.y + waterCollider.bounds.extents.y;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Ignore water in noclip
        if (gameMovement.Instance != null && gameMovement.Instance.noclip)
            return;

        activeVolumes.Add(this);
        UpdateWaterState();
        nextDamageTime = Time.time + damageTickRate;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (gameMovement.Instance != null && gameMovement.Instance.noclip)
            return;

        if ((waterType == WaterType.Slime || waterType == WaterType.Lava) &&
            damagePerSecond > 0 && Time.time >= nextDamageTime)
        {
            var health = other.GetComponent<HealthComponent>();
            health?.TakeDamage(damagePerSecond * damageTickRate);
            nextDamageTime = Time.time + damageTickRate;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        activeVolumes.Remove(this);
        UpdateWaterState();
    }

    private void UpdateWaterState()
    {
        if (activeVolumes.Count == 0)
        {
            IsInWater = false;
            return;
        }

        IsInWater = true;
        float maxDensity = 0;
        float highestSurface = float.MinValue;

        foreach (var vol in activeVolumes)
        {
            if (vol.buoyancy > maxDensity)
            {
                maxDensity = vol.buoyancy;
                Gravity = vol.waterGravity;
                MaxSpeed = vol.waterMaxSpeed;
                Accelerate = vol.waterAccelerate;
                Friction = vol.waterFriction;
                Buoyancy = vol.buoyancy;
            }
            if (vol.surfaceLevel > highestSurface)
                highestSurface = vol.surfaceLevel;
        }

        SurfaceLevel = highestSurface;
    }

    public static void ForceExit()
    {
        IsInWater = false;
        activeVolumes.Clear();
    }
}