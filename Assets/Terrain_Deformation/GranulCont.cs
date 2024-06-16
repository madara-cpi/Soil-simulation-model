using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GranulCont : MonoBehaviour
{
    Rigidbody rb;
    public GameObject my_obj;
    public float radius;
    public Vector3 obj_velocity;
    private SphereCollider sphereCollider;
    float  mu;
    float  cohesion;
    Vector3 relativeVelocity = Vector3.zero;
    public static int count = 0;
    private float mass = 0f;
    private float mu_coeff = 0f;
    public bool isRock = false;
    private float G = 6.674f * Mathf.Pow(10, -11); // Гравитационная постоянная

    private float enterTimer = 0f;
    private float stayTimer = 0f;
    


    void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        mu = (float)PhysicalFootprint.GetMu();
        cohesion = (float)PhysicalFootprint.GetCohesion();
        mu_coeff = 1/ Mathf.Exp((float)PhysicalFootprint.GetShear());
    }

    private void Start()
    {
        count++;
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        mass = 4f/3f * Mathf.PI * Mathf.Pow(radius, 3f) * (float)PhysicalFootprint.granDensity;
        rb.mass = mass;
    }
    private void OnEnable()
    {
    }
    private void FixedUpdate()
    {
        if ((rb.velocity.magnitude < 0.25f && (my_obj.transform.position - transform.position).magnitude>4f) )
        {
            GranulManager.SetGranuled(transform.position, 2f * radius, gameObject);
            //Destroy(gameObject);
        }
       
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (rb == null) return;
        Rigidbody otherRb = collision.rigidbody;
        if (collision.transform.tag == "Granul")
        {

            Vector3 relativeVelocity = otherRb.velocity - rb.velocity;
            Vector3 newVelocity = rb.velocity + (1 + mu_coeff) * otherRb.mass / (rb.mass + otherRb.mass) * relativeVelocity;
            rb.velocity = newVelocity;
            Vector3 otherNewVelocity = otherRb.velocity - mu_coeff * rb.mass / (rb.mass + otherRb.mass) * relativeVelocity;
            otherRb.velocity = otherNewVelocity;
        }
        else if (collision.transform.tag != "Soil")
        {
            Vector3 gforce = CalculateGravitationalForce(collision.gameObject, otherRb.mass, gameObject, mass);
            if (PhysicalFootprint.totalForces.ContainsKey(collision.gameObject)) PhysicalFootprint.totalForces[collision.gameObject] += gforce;
            else PhysicalFootprint.totalForces[collision.gameObject] = gforce;
        }
       
    }
    private void OnCollisionStay(Collision collision)
    {
        if (rb == null || collision.collider.attachedRigidbody == null) return;
        Vector3 normalForce = collision.contacts[0].normal * collision.impulse.magnitude;
        float frictionForce = mu * normalForce.magnitude;
        
        if (PhysicalFootprint.useCohesion && collision.transform.tag == "Granul")
        {
            Vector3 cohDirection = collision.transform.position - transform.position;
            rb.AddForce(cohDirection * cohesion*Mathf.Exp(-1));
            return;
            
        }
        rb.AddForce(-normalForce.normalized * frictionForce, ForceMode.Force);
    }
   
    Vector3 CalculateGravitationalForce(GameObject obj1, float m1, GameObject obj2, float m2)
    {
        Vector3 direction = obj2.transform.position - obj1.transform.position; 
        float distance = direction.magnitude; 
        direction.Normalize(); 
        float forceMagnitude = G * (m1 * m2) / Mathf.Pow(distance, 2); 
        Vector3 force = forceMagnitude * direction; 

        return force;
    }

}
