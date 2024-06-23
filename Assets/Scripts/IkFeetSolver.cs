using System;
using System.Collections;
using Cinemachine.Utility;
using UnityEditor;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

public class IkFeetSolver : MonoBehaviour
{

    public LayerMask layer;
    public bool isSafe;
    public GameObject safeZone;
    public bool isMoving; //this foot is moving
    public IkFeetSolver oppFoot;

    private Vector3 anchorPosition;
    private GameObject armature;

    //spider parameters
    private float SafeRadius { get; } = 2f;
    private float SafeAngle { get; } = 40;
    private float IdleRadius { get; } = 0.5f;
    private float InitialWalkSpeed { get; } = 30f;
    private float InitialIdleResetSpeed { get; } = 4f;
    private float StepAngle { get; } = 80f;
    private float walkSpeed;
    private float idleResetSpeed;

    //debugging
    private Vector3 gizmosTargetRed;
    private Vector3 gizmosTargetBlue;
    private Vector3 gizmosTargetWhite;
    private Vector3 handleStart;
    private Vector3 handleEnd;


    private void Start()
    {
        armature = transform.parent.gameObject;
        transform.localPosition = transform.localPosition*.8f;


        //create safe zone where leg doesnt have to reposition
        safeZone = new GameObject("safeZone");
        safeZone.transform.position = transform.position;
        safeZone.transform.parent = transform.parent;

        
        anchorPosition = transform.position;

        isMoving = false;

    }

    void Update()
    {
        //time stuff
        walkSpeed = InitialWalkSpeed * SpeedController.instance.factor;
        idleResetSpeed = InitialIdleResetSpeed * SpeedController.instance.factor;

        //check if this leg is moving
        if (!isMoving)
            transform.position = anchorPosition;
        
        //checks if foot is outside the safe zone
        var safePosition = safeZone.transform.position;
        var distFromSafe = Vector3.Distance(safePosition, anchorPosition);
        // isSafe = distFromSafe <= SafeRadius;

        //check if another leg is moving
        if (!checkIfSafe() && (oppFoot.isMoving || !LegManager.Instance.GetAnyLegMoving()))
        {
            //find point inside safe zone biased towards target direction
            var tVector = (SpiderController.instance.target);
            var targetPosition = safePosition + (tVector).normalized * (.6f * SafeRadius);
            //find thing to step on
            LegManager.Instance.SetTime(0);
            CastRay(targetPosition, walkSpeed);
        }

        //return to idle position
        if (LegManager.Instance.GetAnyLegMoving()) return;
        if (LegManager.Instance.GetTime() > .2f && !SpiderController.instance.isWalking && distFromSafe > IdleRadius)
        {
            CastRay(safePosition, idleResetSpeed);
        }

    }


    private bool checkIfSafe()
    {
        var arm = armature.transform.position;
        var toFoot = (transform.position - arm).ProjectOntoPlane(armature.transform.up);
        var toSafeZone = (safeZone.transform.position - arm).ProjectOntoPlane(armature.transform.up);

        var angle = Vector3.Angle(toFoot, toSafeZone);

        var distToArmature = Vector3.Distance(transform.position, arm);
        
        return angle < SafeAngle && (toSafeZone.magnitude-SafeRadius < distToArmature && distToArmature < toSafeZone.magnitude+SafeRadius);
    }
    

    private void CastRay(Vector3 raySource, float legSpeed)
    {
        var toTarget = raySource - armature.transform.position;
        var toAnchor = anchorPosition - armature.transform.position;

        //clamp angle so the leg doest turn too far
        if(Vector3.Angle(toTarget, toAnchor) > StepAngle)
        {
            raySource = (toTarget - toAnchor) * .5f + anchorPosition;
        }

        //find ground to step on

        var horizontalDir=
            (armature.transform.position - safeZone.transform.position).ProjectOntoPlane(armature.transform.up).normalized;
        var horizontalPos = (safeZone.transform.position - armature.transform.position)*1.2f + armature.transform.position - armature.transform.up*2;
        handleStart = horizontalPos;
        handleEnd = handleStart + (horizontalDir)*5;
        
        if (Physics.Raycast(raySource + armature.transform.up*3, -armature.transform.up, out var hit, 5, layer) ||
           Physics.Raycast(horizontalPos, horizontalDir, out hit, 5, layer)) //horizontal ray
        {
            LegManager.Instance.SetMoving(true);
            isMoving = true;
            gizmosTargetRed = hit.point;
            StartCoroutine(MoveLeg(hit.point, legSpeed));
        }
        else
        {
            var pos = (armature.transform.position + safeZone.transform.position)/2 - armature.transform.up*1.5f;
            StartCoroutine(MoveLeg(pos, legSpeed));
        }
    }



    //change later to add sin wave?
    private IEnumerator MoveLeg(Vector3 targetPosition, float legSpeed)
    {

        var startPosition = transform.position;


        var elapsedTime = 0f;

        while (elapsedTime < 1)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime);
            elapsedTime += Time.deltaTime * legSpeed;
            yield return null;
        }

        anchorPosition = transform.position;
        LegManager.Instance.lastMoved = Int32.Parse(name.Substring(name.Length-1));
        LegManager.Instance.SetMoving(false);
        isMoving = false;
    }

    //for debugging: show safe zones
    private void OnDrawGizmosSelected()
    {
        if (safeZone == null) return;
        
        // Gizmos.color = Color.yellow;
        // Gizmos.DrawWireSphere(safeZone.transform.position, safeRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(safeZone.transform.position, IdleRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gizmosTargetRed, .5f);

        Handles.color = Color.magenta;
        Handles.DrawAAPolyLine(handleStart, handleEnd);
    }
}
