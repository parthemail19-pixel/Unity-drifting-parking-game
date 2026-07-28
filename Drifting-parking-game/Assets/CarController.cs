using UnityEngine;

public class CarController : MonoBehaviour
{
    public float motorForce = 2500f;  // acceleration power
    public float turnSpeed = 120f;    // steering speed

    [Header("Steering wheels (drag the FRONT wheels here)")]
    public Transform frontLeftWheel;   // your front-left wheel (e.g. LF)
    public Transform frontRightWheel;  // your front-right wheel (e.g. RF)
    public float maxSteerAngle = 30f;  // how far the front wheels visually turn

    Rigidbody rb;
    Quaternion flDefault, frDefault;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (frontLeftWheel)  flDefault = frontLeftWheel.localRotation;
        if (frontRightWheel) frDefault = frontRightWheel.localRotation;
    }

    void Update()
    {
        // Steer the front wheels to match your turn input
        float steer = Input.GetAxis("Horizontal") * maxSteerAngle;
        if (frontLeftWheel)  frontLeftWheel.localRotation  = flDefault * Quaternion.Euler(0f, steer, 0f);
        if (frontRightWheel) frontRightWheel.localRotation = frDefault * Quaternion.Euler(0f, steer, 0f);
    }

    void FixedUpdate()
    {
        float move = Input.GetAxis("Vertical");
        float turn = Input.GetAxis("Horizontal");

        Vector3 flat = transform.forward;
        flat.y = 0f;
        flat.Normalize();
        rb.AddForce(flat * move * motorForce);

        transform.Rotate(0f, turn * turnSpeed * Time.fixedDeltaTime, 0f);
    }
}
