using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

public class ROVControlScript : MonoBehaviour
{
    [Header("Thrust Settings")]
    [SerializeField] private float horizontalGain = 200;
    [SerializeField] private float verticalGain = 200f;
    [SerializeField] private float rotationGain = 100f;
    [SerializeField] private float arrowScale = 0.05f;
    [SerializeField] private bool showGizmos = true;

    [Header("Horizontal Thrusters")]
    [SerializeField] private Transform[] horizontalThrusters = new Transform[4];

    [Header("Vertical Thrusters")]
    [SerializeField] private Transform[] verticalThrusters = new Transform[3];

    private Rigidbody rb;
    private Dictionary<Transform, Vector3> thrusterForces = new Dictionary<Transform, Vector3>();

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            UnityEngine.Debug.LogError("Rigidbody component not found. Ensure a Rigidbody is attached to the ROV.");
        }
        InitializeThrusters();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void InitializeThrusters()
    {
        if (horizontalThrusters.Length != 4)
        {
            horizontalThrusters = new Transform[4];
        }

        if (verticalThrusters.Length != 3)
        {
            verticalThrusters = new Transform[3];
        }

        if (!ThrustersAssignedManually())
        {
            Transform thrustersParent = FindChildRecursive(transform, "Thrusters");
            if (thrustersParent != null)
            {
                AssignThrusters(thrustersParent);
            }
            else
            {
                UnityEngine.Debug.LogError("Thrusters parent not found. Ensure there is a 'Thrusters' GameObject under the ROV.");
            }
        }

        foreach (Transform thruster in horizontalThrusters)
        {
            if (thruster != null)
            {
                thrusterForces[thruster] = Vector3.zero;
            }
        }
        foreach (Transform thruster in verticalThrusters)
        {
            if (thruster != null)
            {
                thrusterForces[thruster] = Vector3.zero;
            }
        }

        ValidateThrusters();
    }

    private void OnValidate()
    {
        if (horizontalThrusters.Length != 4)
        {
            horizontalThrusters = new Transform[4];
        }

        if (verticalThrusters.Length != 3)
        {
            verticalThrusters = new Transform[3];
        }

        if (!ThrustersAssignedManually())
        {
            Transform thrustersParent = FindChildRecursive(transform, "Thrusters");
            if (thrustersParent != null)
            {
                AssignThrusters(thrustersParent);
            }
            else
            {
                UnityEngine.Debug.LogError("Thrusters parent not found. Ensure there is a 'Thrusters' GameObject under the ROV.");
            }
        }
    }

    private bool ThrustersAssignedManually()
    {
        foreach (Transform thruster in horizontalThrusters)
        {
            if (thruster == null)
            {
                return false;
            }
        }

        foreach (Transform thruster in verticalThrusters)
        {
            if (thruster == null)
            {
                return false;
            }
        }

        return true;
    }

    private void AssignThrusters(Transform thrustersParent)
    {
        horizontalThrusters[0] = thrustersParent.Find("FrontLeftThruster");
        horizontalThrusters[1] = thrustersParent.Find("FrontRightThruster");
        horizontalThrusters[2] = thrustersParent.Find("RearLeftThruster");
        horizontalThrusters[3] = thrustersParent.Find("RearRightThruster");

        verticalThrusters[0] = thrustersParent.Find("TopRearThruster");
        verticalThrusters[1] = thrustersParent.Find("TopFrontLeftThruster");
        verticalThrusters[2] = thrustersParent.Find("TopFrontRightThruster");
    }

    private void ValidateThrusters()
    {
        foreach (Transform thruster in horizontalThrusters)
        {
            if (thruster == null)
            {
                UnityEngine.Debug.LogError("One or more horizontal thrusters are not assigned correctly.");
            }
        }
        foreach (Transform thruster in verticalThrusters)
        {
            if (thruster == null)
            {
                UnityEngine.Debug.LogError("One or more vertical thrusters are not assigned correctly.");
            }
        }
    }

    private void HandleMovement()
    {
        Vector3 forwardForce = Input.GetAxis("Vertical") * horizontalGain * transform.up;
        Vector3 rightForce = Input.GetAxis("Horizontal") * horizontalGain * transform.up;
        Vector3 rotationForce = (Input.GetKey(KeyCode.Q) ? -1f : Input.GetKey(KeyCode.E) ? 1f : 0f) * rotationGain * transform.up;
        Vector3 verticalForce = (Input.GetKey(KeyCode.Space) ? 1f : Input.GetKey(KeyCode.LeftControl) ? -1f : 0f) * verticalGain * transform.up;

        ApplyThrusts(horizontalThrusters, forwardForce, rightForce, rotationForce);
        ApplyThrusts(verticalThrusters, verticalForce, Vector3.zero, Vector3.zero);
    }

    private void ApplyThrusts(Transform[] thrusters, Vector3 primaryForce, Vector3 secondaryForce, Vector3 rotationForce)
    {
        if (thrusters == horizontalThrusters)
        {
            ApplyThrust(horizontalThrusters[0], -primaryForce - secondaryForce - rotationForce);
            ApplyThrust(horizontalThrusters[1], -primaryForce + secondaryForce + rotationForce);
            ApplyThrust(horizontalThrusters[2], primaryForce - secondaryForce + rotationForce);
            ApplyThrust(horizontalThrusters[3], primaryForce + secondaryForce - rotationForce);
        }
        else
        {
            foreach (Transform thruster in thrusters)
            {
                if (thruster != null)
                {
                    ApplyThrust(thruster, primaryForce);
                }
            }
        }
    }

    private void ApplyThrust(Transform thruster, Vector3 force)
    {
        Vector3 localForce = thruster.TransformDirection(force);
        rb.AddForceAtPosition(localForce, thruster.position);
        thrusterForces[thruster] = localForce;
    }

    private void OnDrawGizmos()
    {
        if (showGizmos && thrusterForces != null)
        {
            foreach (var thrusterForce in thrusterForces)
            {
                DrawThrustArrow(thrusterForce.Key, thrusterForce.Value);
            }
        }
    }

    private void DrawThrustArrow(Transform thruster, Vector3 force)
    {
        if (thruster != null)
        {
            Gizmos.color = Vector3.Dot(thruster.up, force) > 0 ? Color.green : Color.red;
            Vector3 thrusterDirection = force * arrowScale;
            Gizmos.DrawLine(thruster.position, thruster.position + thrusterDirection);
            if (force != Vector3.zero)
            {
                DrawArrowhead(thruster.position + thrusterDirection, thrusterDirection);
            }
        }
    }

    private void DrawArrowhead(Vector3 position, Vector3 direction)
    {
        float arrowHeadLength = 0.05f;
        float arrowHeadAngle = 20.0f;

        if (direction != Vector3.zero)
        {
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * Vector3.forward;
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * Vector3.forward;
            Gizmos.DrawLine(position, position + right * arrowHeadLength);
            Gizmos.DrawLine(position, position + left * arrowHeadLength);
        }
    }

    private Transform FindChildRecursive(Transform parent, string childName)
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
