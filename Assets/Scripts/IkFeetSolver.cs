using System;
using System.Collections;
using Cinemachine.Utility;
using UnityEditor;
using UnityEngine;

public class IkFeetSolver : MonoBehaviour
{

    public LayerMask layer;
    public bool isSafe;
    public bool isMoving; //this foot is moving
    public bool isHovering;
    public IkFeetSolver oppFoot;

    private GameObject safeZone;
    private Vector3 anchorPosition;
    private GameObject armature;

    //spider parameters
    private float SafeRadius { get; } = 1.9f;
    private float SafeAngle { get; } = 40f;
    private float IdleRadius { get; } = 0.5f;
    private float InitialWalkSpeed { get; } = 30f;
    private float InitialIdleResetSpeed { get; } = 4f;
    private float StepAngle { get; } = 80f;
    private float walkSpeed;
    private float idleResetSpeed;
    private float NumRays { get; } = 5f;

    //debugging
    private Vector3 gizmosTargetRed;
    private Vector3 gizmosTargetBlue;
    private Vector3 gizmosTargetBlue2;
    private Vector3 gizmosTargetWhite;
    private Vector3 handleStart;
    private Vector3 handleEnd;


    private void Start()
    {
        armature = transform.parent.gameObject;
        // transform.localPosition *= .8f;


        //create safe zone where leg doesnt have to reposition
        safeZone = new GameObject("safeZone");
        safeZone.transform.position = transform.position;
        safeZone.transform.parent = transform.parent;

        
        anchorPosition = transform.position;

        isMoving = false;

    }

    private void Update()
    {
        //time stuff
        walkSpeed = InitialWalkSpeed * SpeedController.instance.factor;
        idleResetSpeed = InitialIdleResetSpeed * SpeedController.instance.factor;

        //check if this leg is moving
        if (!isMoving && !isHovering)
            transform.position = anchorPosition;
        
        //checks if foot is outside the safe zone
        var safePosition = safeZone.transform.position;
        var distFromSafe = Vector3.Distance(safePosition, anchorPosition);

        //check if another leg is moving
        if (!CheckIfSafe() && (oppFoot.isMoving || !LegManager.instance.GetAnyLegMoving()))
        {
            //find point inside safe zone biased towards target direction
            var tVector = (SpiderController.instance.target);
            var targetPosition = safePosition + (tVector).normalized * (.6f * SafeRadius);
            //find thing to step on
            LegManager.instance.SetTime(0);
            CastRay(targetPosition, walkSpeed);
        }

        //return to idle position
        if (LegManager.instance.GetAnyLegMoving()) return;
        if (LegManager.instance.GetTime() > .2f && !SpiderController.instance.isWalking && distFromSafe > IdleRadius)
        {
            CastRay(safePosition, idleResetSpeed);
        }

    }


    private bool CheckIfSafe()
    {
        if (isHovering) return false;
        var arm = armature.transform.position;
        var toFoot = (transform.position - arm).ProjectOntoPlane(armature.transform.up);
        var toSafeZone = (safeZone.transform.position - arm).ProjectOntoPlane(armature.transform.up);

        var angle = Vector3.Angle(toFoot, toSafeZone);

        var distToArmature = Vector3.Distance(transform.position, arm);
        isSafe = angle < SafeAngle && (toSafeZone.magnitude-SafeRadius < distToArmature && distToArmature < toSafeZone.magnitude+SafeRadius);
        return isSafe;
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
        var horizontalPos = (safeZone.transform.position - armature.transform.position)*1.2f + armature.transform.position;
        handleStart = horizontalPos; //handles drawline
        handleEnd = handleStart + (horizontalDir)*5; //handles drawline

        Vector3? point = null;
        var hit = new RaycastHit();
        var source = (raySource - armature.transform.position) * .05f + armature.transform.position +
                     armature.transform.up * 2;
        var source2 = raySource + armature.transform.up * 3;
        if (FindWall(source, out point, true, 4) || //horizontal ray above
            FindWall(source2, out point, false, 5) || // straight down
            Physics.Raycast(horizontalPos, horizontalDir, out hit,
                (armature.transform.position - safeZone.transform.position).magnitude-SafeRadius, layer)) //horizontal ray underneath
        {
            LegManager.instance.SetMoving(true);
            isMoving = true;
            gizmosTargetRed = point != null ? point.Value : hit.point;
            StartCoroutine(MoveLeg(gizmosTargetRed, legSpeed));
            isHovering = false;
            //found a spot to stand on
        }
        else
        {
            var pos = armature.transform.position + (safeZone.transform.position - armature.transform.position)*.25f - armature.transform.up*1.5f;
            if ((transform.position - pos).magnitude > 1)
                StartCoroutine(MoveLeg(pos, legSpeed));
            isHovering = true;
            //LEG IS HOVERING
        }
    }

    private bool FindWall(Vector3 raySource, out Vector3? rayPoint, bool horizontal, float dist)
    {
        var startPoint = Quaternion.AngleAxis(45,armature.transform.up) * (raySource - armature.transform.position) + armature.transform.position;
        var endPoint = Quaternion.AngleAxis(-45,armature.transform.up) * (raySource - armature.transform.position) + armature.transform.position;

        var inc = 0f;
        var prev = .5f;
        var count = 0;
        while (inc < 1)
        {
            var a = ((count % 2) * 2 - 1) * inc;
            var source = Vector3.Lerp(startPoint, endPoint, prev + a);
            prev += a;
            count++;
            var dir = horizontal
                ? (source - armature.transform.position).ProjectOntoPlane(armature.transform.up) : -armature.transform.up;
            if (Physics.Raycast(source, dir, out var hit, dist, layer))
            {
                rayPoint = hit.point;
                return true;
            }
            inc += 1/NumRays;
        }
        rayPoint = null;
        return false;
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
        // LegManager.Instance.lastMoved = Int32.Parse(name.Substring(name.Length-1));
        LegManager.instance.SetMoving(false);
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
        
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(gizmosTargetWhite, .25f);

        // Gizmos.color = Color.blue;
        // Gizmos.DrawSphere(gizmosTargetBlue, .25f);
        // Gizmos.DrawSphere(gizmosTargetBlue2, .25f);
        
        Handles.color = Color.magenta;
        Handles.DrawAAPolyLine(handleStart, handleEnd);
    }
}
