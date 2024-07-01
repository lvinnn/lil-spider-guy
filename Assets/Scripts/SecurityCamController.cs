using System;
using Cinemachine.Utility;
using UnityEditor;
using UnityEngine;

public class SecurityCamController : MonoBehaviour
{
    private Vector3 idleForward;
    private Quaternion neckForwardOffset;
    private Quaternion headForwardOffset;
    // private Vector3 neckForward;
    
    private Transform head;
    private Transform neck;
    private Transform headEnd;

    private float lookSpeed;
    private Transform target;

    private Vector3 gizmosTarget1;

    // Start is called before the first frame update
    
    void Start()
    {
        neck = transform.GetChild(0).GetChild(0).GetChild(0);
        head = neck.GetChild(0);
        headEnd = head.GetChild(0);
        Instantiate(SecurityCamManager.instance.light, headEnd);
        

        idleForward = (headEnd.position - head.position);
        neckForwardOffset = Quaternion.AngleAxis(-Vector3.Angle(idleForward, neck.forward), Vector3.up);
        headForwardOffset = Quaternion.AngleAxis(Vector3.Angle(idleForward, head.forward), neck.forward);
        // neckForward = neck.forward;
        lookSpeed = SecurityCamManager.instance.lookSpeed;
        target = SecurityCamManager.instance.target;
    }

    // Update is called once per frames
    void Update()
    {
        if (canSee(target))
            setRotation(target.position - head.position, lookSpeed);
        else
            setRotation(idleForward, lookSpeed/4);
    }

    private bool canSee(Transform target)
    {
        var toTarget = (target.position - head.position).ProjectOntoPlane(Vector3.up);
        var d = target.position - head.position;
        
        gizmosTarget1 = d;
        return (Vector3.Angle(idleForward.ProjectOntoPlane(Vector3.up), toTarget) < 80) &&
               !Physics.Raycast(head.position, d, d.magnitude);
    }

    private void setRotation(Vector3 dir, float speed)
    {
        var neckForwardRot = Quaternion.LookRotation(dir.ProjectOntoPlane(Vector3.up), Vector3.up) * neckForwardOffset;
        neck.rotation = Quaternion.RotateTowards(neck.rotation, neckForwardRot, Time.deltaTime * speed);

        var toForward = headEnd.position - head.position;
        var d = dir.ProjectOntoPlane(neck.forward);
        var angle = Vector3.Angle(d, toForward);
        var cross = -Vector3.Cross(d, toForward);
        head.rotation = Quaternion.RotateTowards(head.rotation, Quaternion.AngleAxis(angle, cross) * head.rotation, Time.deltaTime * speed);
    }

    private void OnDrawGizmos()
    {
        // if(headEnd != null)
        //     Handles.DrawAAPolyLine(head.position, head.position + gizmosTarget1);
        
    }
}
