using UnityEngine;
using Unity.VisualScripting;

public class BotAI : MonoBehaviour
{
    float progress;
    float myRadius;
    bool parked = false;
    float laneOffset;
    public Transform[] waypoints;
    public GameObject platform;
    int currentWaypoint = 0;
    PrometeoCarController car;
    Rigidbody rb;
    public Transform target;

    void Start()
    {
        laneOffset = Random.Range(-0.5f, 0.5f);
        car = GetComponent<PrometeoCarController>();
        rb = GetComponent<Rigidbody>();
        car.isAIControlled = true;
        myRadius = Vector3.Distance(transform.position, platform.transform.position);
        Vector3 startToCenter = platform.transform.position - transform.position;
        startToCenter.y = 0;
        transform.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.up, startToCenter));
        rb.linearVelocity = transform.forward * 8f;
    }

    void Update()
{
    
    if (parked) return;
    Transform aim;
    

    if (platform.activeInHierarchy)
    {
        if (target == null ||
        (bool)Variables.Object(target.gameObject).Get("Taken"))
        target = FindNearestFreeSpot();
        aim = target;

    }
    else
    {
      Vector3 aimPoint = Spline(progress + 1f);
      Vector3 tangent = (Spline(progress + 1.2f) - Spline(progress + 0.8f)).normalized;
      Vector3 perp = Vector3.Cross(Vector3.up, tangent);
      Vector3 carrot = aimPoint + perp * laneOffset;
      Vector3 dir = carrot - transform.position;
      dir.y = 0;
      float side = Vector3.Dot(transform.right, dir.normalized);
      car.SetSteer(Mathf.Clamp(side * 2f, -1f, 1f));
      car.GoForward();

      if(Vector3.Distance(transform.position, Spline(progress)) < 6f)
      
        progress += 0.1f;
        if (progress >= waypoints.Length) progress -= waypoints.Length;
        return;
    }

    if (aim == null) return;

   
    Vector3 leftFeeler  = transform.position + transform.forward * 4f - transform.right * 1.5f + Vector3.up;
    Vector3 rightFeeler = transform.position + transform.forward * 4f + transform.right * 1.5f + Vector3.up;
    if (!Physics.Raycast(leftFeeler, Vector3.down, 5f)) car.TurnRight();
    else if (!Physics.Raycast(rightFeeler, Vector3.down, 5f)) car.TurnLeft();
    else
    {
        Vector3 dir = (aim.position - transform.position).normalized;
        float side = Vector3.Dot(transform.right, dir);
        if (side > 0.1f) car.TurnRight();
        else if (side < -0.1f) car.TurnLeft();
        else car.ResetSteeringAngle();
    }

    float distance = Vector3.Distance(transform.position, aim.position);
    if (distance < 3f)
    {
        Variables.Object(target.gameObject).Set("Taken", true);
        rb.isKinematic = true;
        transform.position = target.position;
        transform.rotation = target.rotation;
        parked = true;
    }
    else
    {
        car.GoForward();
    }
}

    Transform FindNearestFreeSpot()
    {
        Transform best = null;
        float bestDist = Mathf.Infinity;
        foreach (GameObject spot in GameObject.FindGameObjectsWithTag("Spot"))
        {
            bool taken = (bool)Variables.Object(spot).Get("Taken");
            if (taken) continue;
            float d = Vector3.Distance(transform.position, spot.transform.position);
            if (d < bestDist) { bestDist = d; best = spot.transform;}
        }   
    return best;
    }
    Vector3 Spline(float t)
        {
            int n = waypoints.Length;
            int i = Mathf.FloorToInt(t);
            float f = t - i;
            Vector3 p0 = waypoints[(i - 1 + n % n)].position;
            Vector3 p1 = waypoints[i % n].position;
            Vector3 p2 = waypoints[(i + 1) % n].position;
            Vector3 p3 = waypoints[(i + 2) % n].position;
            return 0.5f * (2f*p1 + (-p0 + p2)*f + (2f*p0 - 5f*p1 + 4f*p2 - p3)*f*f + (-p0 + 3f*p1 - 3f*p2 + p3)*f*f*f);
        }
    }



