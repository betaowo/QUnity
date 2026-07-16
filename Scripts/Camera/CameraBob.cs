using UnityEngine;

public class CameraBob : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private gameMovement movement;
    [SerializeField] private Transform viewModel;

    [Header("Bob (cl_bob)")]
    [SerializeField] public float bobValue = 0.02f;
    [SerializeField] private float bobCycle = 0.6f;
    [SerializeField] private float bobUp = 0.5f;

    [Header("Roll (cl_rollangle, cl_rollspeed)")]
    [SerializeField] private float rollAngle = 2.0f;
    [SerializeField] public float rollSpeed = 200f;

    private void Start()
    {
        if (movement == null)
            movement = GetComponentInParent<gameMovement>();
    }

    private void Update()
    {
        if (movement == null) return;

        // === V_CalcBob ===
        float bob = CalcBob();

        transform.localPosition = new Vector3(0f, bob, 0f);

        // === V_CalcViewRoll ===
        float roll = CalcRoll();
        transform.localRotation = Quaternion.Euler(0f, 0f, roll);

        // === Viewmodel ===
        if (viewModel != null)
        {
            float forwardBob = bob * 0.4f;

            float upBob = bob;

            viewModel.localPosition = new Vector3(0f, upBob, forwardBob);
        }
    }

    private float CalcBob()
    {
        if (!movement.data.Grounded)
            return 0f;

        float speed = movement.data.velocityXZ.magnitude;
        if (speed < 0.5f)
            return 0f;

        // cycle = (cl.time % cl_bobcycle) / cl_bobcycle
        float cycle = (Time.time % bobCycle) / bobCycle;

        // Нелинейный цикл
        float cycleRad;
        if (cycle < bobUp)
            cycleRad = Mathf.PI * cycle / bobUp;
        else
            cycleRad = Mathf.PI + Mathf.PI * (cycle - bobUp) / (1.0f - bobUp);

        // bob = speed * cl_bob.value
        float bob = speed * bobValue;
        bob = bob * 0.3f + bob * 0.7f * Mathf.Sin(cycleRad);

        if (bob > 4f) bob = 4f;
        if (bob < -7f) bob = -7f;

        return bob;
    }

    private float CalcRoll()
    {
        if (!movement.data.Grounded)
            return 0f;

        Vector3 right = transform.right;
        float side = Vector3.Dot(movement.data.velocity, right);
        float sign = side < 0 ? -1f : 1f;
        side = Mathf.Abs(side);

        float value = rollAngle;

        if (side < rollSpeed)
            side = side * value / rollSpeed;
        else
            side = value;

        return side * sign;
    }

    public void SetBobValue(float value)
    {
        bobValue = value;
    }

    public void SetRollAngle(float value)
    {
        rollAngle = value;
    }
}