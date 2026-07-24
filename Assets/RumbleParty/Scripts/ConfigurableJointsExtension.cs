using UnityEngine;

public static  class ConfigurableJointsExtension
{
    public static void SetTargetRotationLocal(this ConfigurableJoint joint, Quaternion targetLocalRotation,
        Quaternion localStartRotation)
    {
        if (joint.configuredInWorldSpace)
        {
            Debug.LogError("SetTargetRotationLocal called on a joint configured in world space.", joint);
            return;
        }
        joint.targetRotation = FromTo(targetLocalRotation, localStartRotation, Space.Self, joint);
    }
    
    public static void SetTargetRotation(this ConfigurableJoint joint,
        Quaternion targetRotation,
        Quaternion startRotation)
    {
        if (!joint.configuredInWorldSpace)
        {
            Debug.LogError("SetTargetRotation called on a joint configured in local space.", joint);
            return;
        }

        joint.targetRotation = FromTo(targetRotation, startRotation, Space.World, joint);
    }

    private static Quaternion FromTo(Quaternion target, Quaternion start, Space space, ConfigurableJoint joint)
    {
        // Build the joint's own basis. joint.axis is "right", secondaryAxis defines the plane.
        Vector3 right   = joint.axis;
        Vector3 forward = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
        Vector3 up      = Vector3.Cross(forward, right).normalized;

        Quaternion worldToJoint = Quaternion.LookRotation(forward, up);
        Quaternion jointToWorld = Quaternion.Inverse(worldToJoint);

        // Transform into joint space, take the delta from the start pose, transform back.
        // Note the Inverse on target: the drive pulls the *connected* body, so the sign flips.
        Quaternion result;
        if (space == Space.World)
            result = jointToWorld * Quaternion.Inverse(target) * start * worldToJoint;
        else
            result = jointToWorld * start * Quaternion.Inverse(target) * worldToJoint;

        return result;
    }
    public static void SetSlerpDrive(this ConfigurableJoint joint, float spring, float damper, float maxForce)
    {
        joint.slerpDrive = new JointDrive
        {
            positionSpring = spring,
            positionDamper = damper,
            maximumForce   = maxForce
        };
    }
    
    

}
