using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCar : MonoBehaviour
{
    [Header("WheelCollider")]
    public WheelCollider WheelFL;
    public WheelCollider WheelFR;
    public WheelCollider WheelBL;
    public WheelCollider WheelBR;

    [Header("Physics")]
    private Rigidbody rb;
    private Vector3 center = new Vector3(0, 0, 0);

    [Header("Car Setting")]
    public float maxSteerAngle = 5f;
    public float brakeTorque = 9000f;
    public float handBrakeTorque = 9000f;

    [Header("Speed")]
    public float maxSpeed = 80f;
    public float acceleration = 50f;
    public float reverseAcceleration = 20f;
    public float reverseMaxSpeed = 100f;

    [Header("Handling")]
    public float steeringSpeed = 80f;
    public float rotationTorque = 0.4f;

    public Vector2 moveInput;
    public bool handBrake;
    public float brakeStandard = 0.2f;
    public bool isDead;

    [SerializeField] private UIManager uiManager;

    void Start()
    {
        uiManager = FindAnyObjectByType<UIManager>();

        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = center;

        isDead = false;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnBrake(InputAction.CallbackContext context)
    {
        handBrake = context.ReadValueAsButton();
    }

    void FixedUpdate()
    {
        if (isDead)
            return;

        if (uiManager != null && uiManager.IsPaused)
            return;

        Steer();
        Drive();
    }

    void Steer()
    {
        float steer = moveInput.x * maxSteerAngle;

        WheelFL.steerAngle = steer;
        WheelFR.steerAngle = steer;

        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            float currentSpeed = rb.linearVelocity.magnitude;

            float speedMultiplier =
                Mathf.Clamp01(currentSpeed / 5f);

            float turnAmount =
                moveInput.x *
                steeringSpeed *
                speedMultiplier *
                Time.fixedDeltaTime;

            Quaternion targetRotation =
                rb.rotation *
                Quaternion.Euler(0f, turnAmount, 0f);

            rb.MoveRotation(targetRotation);
        }
    }

    void Drive()
    {
        float forwardSpeed =
            Vector3.Dot(rb.linearVelocity, transform.forward);

        if (handBrake)
        {
            WheelBL.brakeTorque = handBrakeTorque;
            WheelBR.brakeTorque = handBrakeTorque;

            ApplyBrake(0.15f);
            return;
        }

        WheelBL.brakeTorque = 0f;
        WheelBR.brakeTorque = 0f;

        if (moveInput.y > 0f)
        {
            AccelerateForward(forwardSpeed);
        }
        else if (moveInput.y < 0f)
        {
            ReverseOrBrake(forwardSpeed);
        }
        else
        {
            SlowDown();
        }
    }

    void AccelerateForward(float forwardSpeed)
    {
        float maxSpeedMS = maxSpeed / 3.6f;

        float targetSpeed =
            Mathf.MoveTowards(
                forwardSpeed,
                maxSpeedMS,
                acceleration * Time.fixedDeltaTime
            );

        SetForwardSpeed(targetSpeed);
    }

    void ReverseOrBrake(float forwardSpeed)
    {
        if (forwardSpeed > brakeStandard)
        {
            WheelBL.brakeTorque = brakeTorque;
            WheelBR.brakeTorque = brakeTorque;

            ApplyBrake(0.2f);
            return;
        }

        WheelBL.brakeTorque = 0f;
        WheelBR.brakeTorque = 0f;

        float reverseSpeed =
            -Vector3.Dot(
                rb.linearVelocity,
                transform.forward
            );

        float maxReverseSpeedMS =
            reverseMaxSpeed / 3.6f;

        float targetSpeed =
            Mathf.MoveTowards(
                reverseSpeed,
                maxReverseSpeedMS,
                reverseAcceleration * Time.fixedDeltaTime
            );

        SetForwardSpeed(-targetSpeed);
    }

    void SetForwardSpeed(float targetSpeed)
    {
        Vector3 velocity = rb.linearVelocity;

        Vector3 forwardVelocity =
            transform.forward * targetSpeed;

        velocity.x = forwardVelocity.x;
        velocity.z = forwardVelocity.z;

        rb.linearVelocity = velocity;
    }

    void SlowDown()
    {
        Vector3 velocity = rb.linearVelocity;

        Vector3 horizontalVelocity =
            new Vector3(
                velocity.x,
                0f,
                velocity.z
            );

        horizontalVelocity =
            Vector3.MoveTowards(
                horizontalVelocity,
                Vector3.zero,
                10f * Time.fixedDeltaTime
            );

        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.z;

        rb.linearVelocity = velocity;
    }

    void ApplyBrake(float brakeStrength)
    {
        Vector3 velocity = rb.linearVelocity;

        Vector3 horizontalVelocity =
            new Vector3(
                velocity.x,
                0f,
                velocity.z
            );

        horizontalVelocity *=
            Mathf.Clamp01(1f - brakeStrength);

        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.z;

        rb.linearVelocity = velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        float impactForce = collision.relativeVelocity.magnitude;

        if (impactForce < 1f)
            return;

        float volume = Mathf.Clamp01(impactForce / 3f);

        CollisionSoundObject soundObject =
            collision.gameObject.GetComponentInParent<CollisionSoundObject>();

        SoundManager.CollisionSoundType soundType =
            soundObject != null
                ? soundObject.soundType
                : SoundManager.CollisionSoundType.Default;

        SoundManager.Instance.PlayCollisionSound(soundType, volume);
    }
}