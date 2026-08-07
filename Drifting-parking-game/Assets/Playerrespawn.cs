using UnityEngine;
using System.Collections.Generic;

public class PlayerRecovery : MonoBehaviour
{
    Rigidbody rb;

    struct Snap { public float time; public Vector3 pos; public Quaternion rot; }
    Queue<Snap> history = new Queue<Snap>();

    void Start() { rb = GetComponent<Rigidbody>(); }

    void Update()
    {
        if (rb.isKinematic) return;

        history.Enqueue(new Snap { time = Time.time, pos = transform.position, rot = transform.rotation });
        while (history.Count > 0 && Time.time - history.Peek().time > 2f)
            history.Dequeue();

        if (history.Count > 0 && transform.position.y < history.Peek().pos.y - 5f)
            Rewind();
    }

    void OnCollisionEnter(Collision col)
    {
        if (rb.isKinematic) return;
        Rewind();
    }

    void Rewind()
    {
        if (history.Count == 0) return;
        Snap s = history.Peek();
        transform.position = s.pos;
        transform.rotation = s.rot;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        history.Clear();
    }
}