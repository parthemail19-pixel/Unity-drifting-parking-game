using UnityEngine;
using Unity.VisualScripting;

public class BotAI : MonoBehaviour
{
    float stuckTimer;
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
        car = GetComponent<PrometeoCarController>();
        rb = GetComponent<Rigidbody>();
        car.isAIControlled = true;
        laneOffset = Random.Range(-0.5f, 0.5f);
        car.maxSpeed = Random.Range(30, 45);

       
        float bestWp = Mathf.Infinity;
        for (int i = 0; i < waypoints.Length; i++)
        {
            float d = Vector3.Distance(transform.position, waypoints[i].position);
            if (d < bestWp) { bestWp = d; progress = i; }
        }

   
        Vector3 startDir = Spline(progress + 1f) - transform.position;
        startDir.y = 0;
        transform.rotation = Quaternion.LookRotation(startDir);
        rb.linearVelocity = startDir.normalized * 8f;
               
    }

    void Update()
{
    
    if (parked) return;

    for (float t = 0; t < waypoints.Length; t += 0.2f)
        Debug.DrawLine(Spline(t), Spline(t + 0.2f), Color.green);

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
       int guard = 0;
       float bestT = progress;
       float bestD = Mathf.Infinity;
       for (float t = progress; t < progress + 3f; t += 0.1f)
       {
           float d = Vector3.Distance(transform.position, Spline(t));
           if (d < bestD) { bestD = d; bestT = t; }
       }
       progress = bestT;
       if (progress >= waypoints.Length) progress -= waypoints.Length;
       if (rb.linearVelocity.magnitude < 1f) stuckTimer += Time.deltaTime;
       else stuckTimer = 0f;
       Vector3 offLine = Spline(progress) - transform.position;
       offLine.y = 0;
       if (stuckTimer > 2f || transform.position.y < Spline(progress).y - 3f || offLine.magnitude > 6f) 
            {
                Vector3 rp = Spline(progress) + Vector3.up * 20f;
                RaycastHit rhit;
                if (Physics.Raycast(rp, Vector3.down, out rhit, 100f, ~LayerMask.GetMask("Cars")))
                transform.position = rhit.point + Vector3.up * 0.5f;
                else transform.position = Spline(progress) + Vector3.up;
                {
                    transform.rotation = Quaternion.LookRotation(Spline(progress + 0.8f) - Spline(progress));
                    rb.angularVelocity = Vector3.zero;
                    rb.linearVelocity = transform.forward * 6f;
                    stuckTimer = 0f;
                }
            }
        

        Vector3 ahead = transform.position + transform.forward * 4f + Vector3.up * 2f;
        if (!Physics.Raycast(ahead, Vector3.down, 6f, ~LayerMask.GetMask("Cars")))
       {
           Vector3 back = Spline(progress) - transform.position;
           back.y = 0;
           car.SetSteer(Mathf.Clamp(Vector3.Dot(transform.right, back.normalized) * 5f, -1f, 1f));
           car.Brakes();
           return;
       }
       Vector3 tangent = (Spline(progress + 0.3f) - Spline(progress)).normalized;
       Vector3 perp = Vector3.Cross(Vector3.up, tangent);
       Vector3 carrot = Spline(progress + 0.4f) + perp * laneOffset;

       Vector3 dir = carrot - transform.position;
       dir.y = 0;
       float side = Vector3.Dot(transform.right, dir.normalized);
       car.SetSteer(Mathf.Clamp(side * 5f, -1f, 1f));
       if (Mathf.Abs(side) > 0.4f && rb.linearVelocity.magnitude > 8f) car.Brakes();
       else car.GoForward();
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

        public void NewRound()
    {
        parked = false;
        target = null;
        float best = Mathf.Infinity;
        for (int i = 0; i < waypoints.Length; i++)
        {
            float d = Vector3.Distance(transform.position, waypoints[i].position);
            if (d < best) { best = d; progress = i; }
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

            void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 4) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Length; i++)
            if (waypoints[i] != null) Gizmos.DrawSphere(waypoints[i].position, 1f);

        Gizmos.color = Color.green;
        int n = waypoints.Length;
        Vector3 prev = Spline(0f);
        for (float t = 0.1f; t <= n; t += 0.1f)
        {
            Vector3 p = Spline(t);
            Gizmos.DrawLine(prev, p);
            prev = p;
        }
        
    }    Vector3 Spline(float t)
        {
            int n = waypoints.Length;
            int i = Mathf.FloorToInt(t);
            float f = t - i;
            Vector3 p0 = waypoints[(i - 1 + n) % n].position;
            Vector3 p1 = waypoints[i % n].position;
            Vector3 p2 = waypoints[(i + 1) % n].position;
            Vector3 p3 = waypoints[(i + 2) % n].position;
            return 0.5f * (2f*p1 + (-p0 + p2)*f + (2f*p0 - 5f*p1 + 4f*p2 - p3)*f*f + (-p0 + 3f*p1 - 3f*p2 + p3)*f*f*f);
        }
    }



