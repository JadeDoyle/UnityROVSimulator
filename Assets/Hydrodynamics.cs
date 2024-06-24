using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HydrodynamicsScript : MonoBehaviour
{
    [Header("ROV Properties")]
    public float mass = 1.0f; // Mass of the ROV in kg
    public bool isSubmerged = true; // Whether the ROV is submerged
    public bool isNeutrallyBuoyant = true; // Whether the ROV is neutrally buoyant
    public float specifiedBuoyancy; // Buoyancy in kg if not neutrally buoyant
    public Vector3 centerOfBuoyancyOffset; // Offset of the center of buoyancy relative to the ROV's center of mass

    [Header("Fluid Properties")]
    public float fluidDensity = 1025f; // Density of water in kg/m^3

    [Header("Rigidbody Properties")]
    public float linearDamping = 1.0f; // Linear drag
    public float angularDamping = 1.0f; // Angular drag

    [Header("Debugging")]
    public bool showGUI = true; // Toggle GUI display

    private float volume; // Volume of the ROV
    private Vector3 buoyantForce; // Buoyant force acting on the ROV
    private Vector3 gravityForce; // Gravity force acting on the ROV
    private Rigidbody rb; // Rigidbody component of the ROV

    private const float initialNudgeForce = 0.0f; // Adjust as needed
    private const float initialNudgeTorque = 100.0f; // Adjust as needed

    void Start()
    {
        // Ensure the Rigidbody component is attached
        rb = GetComponent<Rigidbody>();

        // Ensure mass is positive
        mass = Mathf.Max(0.1f, mass);
        rb.mass = mass;

        // Configure Rigidbody properties
        rb.useGravity = false; // We'll handle gravity manually
        rb.linearDamping = linearDamping; // Set drag value
        rb.angularDamping = angularDamping; // Set angular drag value

        // Calculate volume if neutrally buoyant
        if (isNeutrallyBuoyant)
        {
            volume = mass / fluidDensity;
        }
        else
        {
            // Calculate the volume based on specified buoyancy
            volume = specifiedBuoyancy / fluidDensity;
        }

        // Calculate gravity force
        gravityForce = new Vector3(0, mass * Physics.gravity.y, 0);

        // Apply an initial nudge
        rb.AddForce(new Vector3(0, initialNudgeForce, 0), ForceMode.Impulse);
        rb.AddTorque(new Vector3(initialNudgeTorque, 0, initialNudgeTorque), ForceMode.Impulse);
    }

    void FixedUpdate()
    {
        // Apply gravity force
        rb.AddForce(gravityForce);

        if (isSubmerged)
        {
            // Calculate buoyant force
            buoyantForce = new Vector3(0, fluidDensity * volume * -Physics.gravity.y, 0);

            // Apply buoyant force at the center of buoyancy
            Vector3 worldCenterOfBuoyancy = rb.position + rb.rotation * centerOfBuoyancyOffset;
            rb.AddForceAtPosition(buoyantForce, worldCenterOfBuoyancy);
        }
    }

    void OnGUI()
    {
        if (showGUI)
        {
            // Display real-time values
            GUI.Label(new Rect(10, 10, 300, 20), "Mass: " + mass + " kg");
            GUI.Label(new Rect(10, 30, 300, 20), "Volume: " + volume + " m^3");
            GUI.Label(new Rect(10, 50, 300, 20), "Fluid Density: " + fluidDensity + " kg/m^3");
            GUI.Label(new Rect(10, 70, 300, 20), "Buoyant Force: " + buoyantForce + " N");
            GUI.Label(new Rect(10, 90, 300, 20), "Gravity Force: " + gravityForce + " N");
        }
    }

    void Update()
    {
        // Key controls for interactivity
        if (Input.GetKeyDown(KeyCode.B))
        {
            isSubmerged = !isSubmerged; // Toggle submersion
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            isNeutrallyBuoyant = !isNeutrallyBuoyant; // Toggle neutral buoyancy
            if (isNeutrallyBuoyant)
            {
                volume = mass / fluidDensity;
            }
            else
            {
                volume = specifiedBuoyancy / fluidDensity;
            }
        }

        // Update Rigidbody drag in real-time
        rb.linearDamping = linearDamping;
        rb.angularDamping = angularDamping;
    }

    void OnDrawGizmos()
    {
        if (rb != null)
        {
            // Draw center of buoyancy in the editor
            Gizmos.color = Color.blue;
            Vector3 worldCenterOfBuoyancy = transform.position + transform.rotation * centerOfBuoyancyOffset;
            Gizmos.DrawSphere(worldCenterOfBuoyancy, 0.1f);

            // Draw center of mass in the editor
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.TransformPoint(rb.centerOfMass), 0.1f);
        }
    }
}
