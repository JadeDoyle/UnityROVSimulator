using UnityEngine;

public class RopeGenerator : MonoBehaviour
{
    public Transform startPose;
    public Transform endPose;
    public float thickness = 0.1f;
    public float segmentLength = 0.5f;

    void Start()
    {
        GenerateRope();
    }

    void GenerateRope()
    {
        Vector3 startPosition = startPose.position;
        Vector3 endPosition = endPose.position;
        int segmentCount = Mathf.CeilToInt(Vector3.Distance(startPosition, endPosition) / segmentLength);
        Vector3 segmentDirection = (endPosition - startPosition).normalized;

        // Generate curve control points
        Vector3 controlPoint1 = startPosition + segmentDirection * segmentLength * segmentCount / 3;
        Vector3 controlPoint2 = endPosition - segmentDirection * segmentLength * segmentCount / 3;

        Rigidbody previousSegment = null;

        for (int i = 0; i <= segmentCount; i++)
        {
            float t = i / (float)segmentCount;
            Vector3 position = GetBezierPoint(startPosition, controlPoint1, controlPoint2, endPosition, t);

            // Create capsule segment
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.transform.position = position;
            capsule.transform.localScale = new Vector3(thickness, segmentLength * 0.5f, thickness);
            capsule.transform.rotation = Quaternion.LookRotation(
                GetBezierTangent(startPosition, controlPoint1, controlPoint2, endPosition, t)
            );

            // Add Rigidbody and Collider
            Rigidbody rb = capsule.AddComponent<Rigidbody>();
            CapsuleCollider collider = capsule.GetComponent<CapsuleCollider>();
            collider.radius = thickness * 0.5f;
            collider.height = segmentLength;

            // Disable collision between consecutive segments
            if (previousSegment != null)
            {
                Physics.IgnoreCollision(collider, previousSegment.GetComponent<CapsuleCollider>());
            }

            // Lock to the previous segment
            if (previousSegment != null)
            {
                ConfigurableJoint joint = capsule.AddComponent<ConfigurableJoint>();
                joint.connectedBody = previousSegment;
                joint.anchor = new Vector3(0, -segmentLength * 0.5f, 0);
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = new Vector3(0, segmentLength * 0.5f, 0);

                joint.xMotion = ConfigurableJointMotion.Locked;
                joint.yMotion = ConfigurableJointMotion.Locked;
                joint.zMotion = ConfigurableJointMotion.Locked;
                joint.angularXMotion = ConfigurableJointMotion.Free;
                joint.angularYMotion = ConfigurableJointMotion.Free;
                joint.angularZMotion = ConfigurableJointMotion.Free;
            }
            else
            {
                // Fix start capsule to its initial position but allow free rotation
                ConfigurableJoint joint = capsule.AddComponent<ConfigurableJoint>();
                joint.autoConfigureConnectedAnchor = false;
                joint.anchor = new Vector3(0, -segmentLength * 0.5f, 0);
                joint.connectedAnchor = startPosition; // Use start position as the anchor point

                joint.xMotion = ConfigurableJointMotion.Locked;
                joint.yMotion = ConfigurableJointMotion.Locked;
                joint.zMotion = ConfigurableJointMotion.Locked;
                joint.angularXMotion = ConfigurableJointMotion.Free;
                joint.angularYMotion = ConfigurableJointMotion.Free;
                joint.angularZMotion = ConfigurableJointMotion.Free;
            }

            previousSegment = rb;
        }
    }

    Vector3 GetBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0;
        p += 3 * uu * t * p1;
        p += 3 * u * tt * p2;
        p += ttt * p3;

        return p;
    }

    Vector3 GetBezierTangent(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 tangent = -3 * uu * p0 + 3 * uu * p1 - 6 * u * t * p1 + 6 * u * t * p2 - 3 * tt * p2 + 3 * tt * p3;
        return tangent.normalized;
    }
}
