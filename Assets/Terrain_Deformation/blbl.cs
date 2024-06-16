using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class blbl : MonoBehaviour
{
    Rigidbody rb;
    private float startF;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startF = 15f;
    }
    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Space)) rb.AddForce(transform.forward * startF);
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.transform.tag != "Granul") return;
        Vector3 cohesionDirection = collision.transform.position - transform.position;
        rb.AddForce(cohesionDirection.normalized * startF , ForceMode.Force);
    }

}
