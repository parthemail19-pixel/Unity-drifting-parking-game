using UnityEngine;
using Unity.VisualScripting;

public class PlayerParking : MonoBehaviour
{
    Rigidbody rb;

    void Start() { rb = GetComponent<Rigidbody>(); }

    void OnTriggerStay(Collider other)
    {
        if (rb.isKinematic) return;
        if (!other.CompareTag("Spot")) return;
        if ((bool)Variables.Object(other.gameObject).Get("Taken")) return;

        Variables.Object(other.gameObject).Set("Taken", true);
        transform.position = other.transform.position;
        transform.rotation = other.transform.rotation;
        rb.isKinematic = true;
    }
}