using UnityEngine;
using System.Collections.Generic;

public class ROVControlScript : MonoBehaviour
{
    public float HorizontalGain = 100f; // Force applied by each thruster
    public float VerticalGain = 100f; // Force applied by vertical thrusters
    public float RotationGain = 50f; // Force applied for rotation
    public float ArrowScale = 0.05f; // Scales length of thrust arrow gizmos
    public bool ShowGizmos = true; // Control whether Gizmos are drawn

    private Transform[] horizontalThrusters;
    private Transform[] verticalThrusters;

    private Rigidbody rb;
    private Dictionary<Transform, Vector3> thrusterForces;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component not found. Ensure a Rigidbody is attached to the ROV.");
            return;
        }

        // Initialize thrusters arrays
        horizontalThrusters = new Transform[4];
        verticalThrusters = new Transform[3];
        thrusterForces = new Dictionary<Transform, Vector3>();

        // Find thruster transforms
        Transform thrustersParent = FindChildRecursive(transform, "Thrusters");
        if (thrustersParent != null)
        {
            // Horizontal thrusters
            horizontalThrusters[0] = thrustersParent.Find("FrontLeftThruster");
            horizontalThrusters[1] = thrustersParent.Find("FrontRightThruster");
            horizontalThrusters[2] = thrustersParent.Find("RearLeftThruster");
            horizontalThrusters[3] = thrustersParent.Find("RearRightThruster");

            // Vertical thrusters
            verticalThrusters[0] = thrustersParent.Find("TopRearThruster");
            verticalThrusters[1] = thrustersParent.Find("TopFrontLeftThruster");
            verticalThrusters[2] = thrustersParent.Find("TopFrontRightThruster");

            // Initialize thruster forces
            foreach (Transform thruster in horizontalThrusters)
            {
                if (thruster != null) thrusterForces[thruster] = Vector3.zero;
            }
            foreach (Transform thruster in verticalThrusters)
            {
                if (thruster != null) thrusterForces[thruster] = Vector3.zero;
            }
        }
        else
        {
            Debug.LogError("Thrusters parent not found. Ensure there is a 'Thrusters' GameObject under the ROV.");
        }
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        // Input controls
        float moveForward = Input.GetAxis("Vertical"); // W/S or Up/Down Arrow keys
        float moveRight = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow keys
        float moveUp = Input.GetKey(KeyCode.Space) ? 1f : (Input.GetKey(KeyCode.LeftControl) ? -1f : 0f); // Space or Left Control keys
        float rotate = Input.GetKey(KeyCode.Q) ? -1f : (Input.GetKey(KeyCode.E) ? 1f : 0f); // Q/E keys for rotation

        // Apply forces to horizontal thrusters
        if (horizontalThrusters[0] && horizontalThrusters[1] && horizontalThrusters[2] && horizontalThrusters[3])
        {
            Vector3 forwardForce = moveForward * HorizontalGain * transform.up;
            Vector3 rightForce = moveRight * HorizontalGain * transform.up;
            Vector3 rotationForce = rotate * RotationGain * transform.up;

            ApplyThrust(horizontalThrusters[0], -forwardForce - rightForce - rotationForce);
            ApplyThrust(horizontalThrusters[1], -forwardForce + rightForce + rotationForce);
            ApplyThrust(horizontalThrusters[2], forwardForce - rightForce + rotationForce);
            ApplyThrust(horizontalThrusters[3], forwardForce + rightForce - rotationForce);
        }

        // Apply forces to vertical thrusters
        if (verticalThrusters[0] && verticalThrusters[1] && verticalThrusters[2])
        {
            Vector3 verticalForce = moveUp * VerticalGain * transform.up;

            ApplyThrust(verticalThrusters[0], verticalForce);
            ApplyThrust(verticalThrusters[1], verticalForce);
            ApplyThrust(verticalThrusters[2], verticalForce);
        }
    }

    void ApplyThrust(Transform thruster, Vector3 force)
    {
        Vector3 localForce = thruster.TransformDirection(force);
        rb.AddForceAtPosition(localForce, thruster.position);
        thrusterForces[thruster] = localForce;
    }

    void OnDrawGizmos()
    {
        if (ShowGizmos && thrusterForces != null && thrusterForces.Count > 0)
        {
            foreach (var thrusterForce in thrusterForces)
            {
                DrawThrustArrow(thrusterForce.Key, thrusterForce.Value);
            }
        }
    }

    void DrawThrustArrow(Transform thruster, Vector3 force)
    {
        if (thruster != null)
        {
            // Set color based on force direction
            if (Vector3.Dot(thruster.up, force) > 0)
            {
                Gizmos.color = Color.green; // Thrust direction aligns with thruster up direction
            }
            else
            {
                Gizmos.color = Color.red; // Thrust direction aligns with thruster down direction
            }

            Vector3 thrusterDirection = force * ArrowScale;
            Gizmos.DrawLine(thruster.position, thruster.position + thrusterDirection);
            if (force != Vector3.zero) // Check to avoid zero vector
            {
                DrawArrowhead(thruster.position + thrusterDirection, thrusterDirection);
            }
        }
    }

    void DrawArrowhead(Vector3 position, Vector3 direction)
    {
        float arrowHeadLength = 0.05f;
        float arrowHeadAngle = 20.0f;

        if (direction != Vector3.zero) // Check to avoid zero vector
        {
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;
            Gizmos.DrawLine(position, position + right * arrowHeadLength);
            Gizmos.DrawLine(position, position + left * arrowHeadLength);
        }
    }

    Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }

            Transform result = FindChildRecursive(child, childName);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }
}
