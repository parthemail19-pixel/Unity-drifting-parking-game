using UnityEngine;

// Chase cam: sits behind the car and stays centred on the car's visual middle,
// so an off-centre model pivot doesn't matter.
public class CameraFollow : MonoBehaviour
{
    public Transform target;        // the Car
    public float distance = 6f;     // how far behind (smaller = zoomed in)
    public float height = 3f;       // how high above
    public float followSpeed = 5f;  // lower = smoother/floatier, higher = tighter
    public float lookHeight = 1f;   // aim a touch above the car's centre

    Vector3 centerLocal;            // car's visual centre, captured once (stable = no shake)

    void Start()
    {
        centerLocal = ComputeCenterLocal();
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 center = target.TransformPoint(centerLocal);

        Vector3 fwd = target.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;
        fwd.Normalize();

        Vector3 desired = center - fwd * distance + Vector3.up * height;

        transform.position = Vector3.Lerp(transform.position, desired, followSpeed * Time.deltaTime);
        transform.LookAt(center + Vector3.up * lookHeight);
    }

    Vector3 ComputeCenterLocal()
    {
        if (target == null) return Vector3.zero;
        var rends = target.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) return Vector3.zero;
        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
        return target.InverseTransformPoint(b.center);
    }
}
