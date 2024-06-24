using System;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class IkLegSolver : MonoBehaviour
{
    public GameObject armature;
    [Header("bones")]
    public GameObject upperLeg;
    private GameObject lowerLeg;
    private GameObject foot;
    private GameObject footEnd;

    [Header("target")]
    public GameObject target;

    //constraints?
    private int joint3Limit = -60;

    //internal measurements
    private float upperLegLen;
    private float lowerLegLen;


    [Header("other stuff")]
    public int iterations = 1;
    public float threshold = 0.001f;
    private float initYRotation;


    private Vector3 debugStart;
    private Vector3 debugTarget;
    private Vector3 gizmosTarget1;
    private Vector3 gizmosTarget2;
    private Vector3 gizmosTarget3;

    void Start()
    {
        lowerLeg = upperLeg.transform.GetChild(0).gameObject;
        foot = lowerLeg.transform.GetChild(0).gameObject;
        footEnd = foot.transform.GetChild(0).gameObject;
        initYRotation = NormalizeAngle(upperLeg.transform.localEulerAngles.y);

        upperLegLen = Vector3.Distance(upperLeg.transform.position, lowerLeg.transform.position);
        lowerLegLen = Vector3.Distance(lowerLeg.transform.position, foot.transform.position);
    }

    void Update()
    {
        float angle;

        //rotate upper leg to match target on local y axis
        Vector3 upperLegToTarget = (target.transform.position - upperLeg.transform.position).normalized;
        Vector3 upperLegToEndRef = (lowerLeg.transform.position - upperLeg.transform.position).normalized;
        upperLegToTarget = Vector3.ProjectOnPlane(upperLegToTarget, armature.transform.up);
        upperLegToEndRef = Vector3.ProjectOnPlane(upperLegToEndRef, armature.transform.up);

        angle = Vector3.Angle(upperLegToEndRef, upperLegToTarget);
        Vector3 cross = Vector3.Cross(upperLegToEndRef, upperLegToTarget);
        upperLeg.transform.rotation = Quaternion.AngleAxis(angle, cross) * upperLeg.transform.rotation;

        SolveIK();
    }

    void SolveIK()
    {
        // Calculate current distance to target
        RotateTowardsTarget(foot, footEnd, target.transform.position, joint3Limit, 0);
        RotateJoint(upperLeg, upperLegLen, lowerLeg);
        RotateJoint(lowerLeg, lowerLegLen, foot);

    }

    void RotateJoint(GameObject joint1, float len, GameObject joint2) //upperLeg, lowerLeg
    {
        float distToTarget = Vector3.Distance(joint1.transform.position, target.transform.position);
        float lf = Vector3.Distance(joint2.transform.position, footEnd.transform.position);

        float angle = (float)Math.Acos((len * len + distToTarget * distToTarget - lf * lf) / (2 * len * distToTarget));
        angle = angle * 180 / math.PI;

        if (float.IsNaN(angle)) angle = 0;

        var v = target.transform.position - joint1.transform.position;
        var right = Vector3.ProjectOnPlane(target.transform.position-upperLeg.transform.position, armature.transform.up);
        right = Vector3.Cross(right, armature.transform.up);
        v = Quaternion.AngleAxis(angle, right) * v;
        v = v.normalized * len;
        Vector3 v2 = Vector3.Cross(v, -right);
        joint1.transform.rotation = Quaternion.LookRotation(v2, v);


        //gizmos
        if (joint1 == upperLeg)
        {
            gizmosTarget1 = upperLeg.transform.position + v;
        }
        else
        {
            gizmosTarget2 = lowerLeg.transform.position + v;
        }
        
    }

    void RotateTowardsTarget(GameObject joint, GameObject endRef, Vector3 target, int lowerJointLimit, int upperJointLimit)
    {
        var toTarget = (target - joint.transform.position); //to IK bone vector
        var parentUp = joint.transform.parent.transform.up; //local up vector basically


        ////limit angles by jointLimit
        float angle = Vector3.SignedAngle(parentUp, toTarget, joint.transform.right);
        ////Debug.Log(angle);
        if (angle < lowerJointLimit || angle > upperJointLimit)
        {
            //new clamped toTarget rotation set to jointLimit
            if (angle > upperJointLimit)
                toTarget = parentUp;
            else
                toTarget = (Quaternion.AngleAxis(lowerJointLimit, joint.transform.parent.transform.right) * parentUp).normalized;
            toTarget *= Vector3.Distance(endRef.transform.position, joint.transform.position); //set correct length
        }

        #region gizmos stuff
        ////gizmosTarget = where ik bone is at
        //gizmosTarget = joint.transform.position + toTarget;
        ////gizmosTarget2 = where toes are
        //gizmosTarget2 = joint.transform.position + toEndRef;
        #endregion


        Vector3 toEndRef = (endRef.transform.position - joint.transform.position);

        //point joint at corrected target
        angle = Vector3.Angle(toEndRef, toTarget);
        Vector3 cross = Vector3.Cross(toEndRef, toTarget);
        joint.transform.rotation = Quaternion.AngleAxis(angle, cross) * joint.transform.rotation;
    }

    // void ClampTarget(float angle, Vector3 toTarget, Vector3 axis, float limit) {
    //
    //     if (Mathf.Abs(angle) > limit)
    //     {
    //         // Calculate the rotation required to limit the angle
    //         float excessAngle = Mathf.Abs(angle) - limit;
    //
    //         // Create a rotation that rotates toTarget by the necessary angle to limit it
    //         Quaternion rotation = Quaternion.AngleAxis(-excessAngle * Mathf.Sign(angle), axis);
    //         //Debug.Log(rotation);
    //         Vector3 clampedToTarget = rotation * toTarget;
    //
    //         target.transform.position = armature.transform.position + clampedToTarget;
    //     }
    // }

    float NormalizeAngle(float angle)
    {
        angle = angle % 360;
        if (angle > 180)
            angle -= 360;
        else if (angle < -180)
            angle += 360;
        return angle;
    }

    private void OnDrawGizmos()
    {
        //Handles.color = Color.blue;
        //Handles.DrawAAPolyLine(armature.transform.position, armature.transform.position + gizmosTarget3);
        //Handles.color = Color.red;
        //Handles.DrawAAPolyLine(armature.transform.position, armature.transform.position + gizmosTarget2);
        //Handles.color = Color.green;
        //if (lowerLeg)
        //    Handles.DrawAAPolyLine(lowerLeg.transform.position, lowerLeg.transform.position + gizmosTarget1);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(gizmosTarget1, .1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(gizmosTarget2, .1f);
        //Gizmos.DrawSphere(armature.transform.position + gizmosTarget3, .3f);
    }

}
