using System;
using System.Collections.Generic;
using Cinemachine.Utility;
using UnityEditor;
using UnityEngine;

public class SpiderController : MonoBehaviour
{

    public static SpiderController instance;

    public bool isWalking;

    public GameObject armature;
    public Vector3 target;

    public LayerMask layer;

    public IkFeetSolver foot1;
    public IkFeetSolver foot2;
    public IkFeetSolver foot3;
    public IkFeetSolver foot4;

    //camera world up override
    public Camera mainCam;
    public GameObject worldUp;

    //movement parameters
    private float InitialWalkSpeed { get; } = 10f;
    private float InitialTurnSpeed { get; } = 200f;
    private float walkSpeed = 10f;
    private float turnSpeed = 300f;
    private float BodyHeightAdjustSpeed { get; } = 10;
    private float DistFromFeet { get; } = .5f;
    private bool inAir;
    private float JumpDist { get; } = 500;
    private Vector3 boxCastOrigin;

    private Rigidbody armatureRigidBody;
    private BoxCollider[] colliders;
    private List<IkFeetSolver> footList;
    
    //gizmos
    private Vector3 gizmosVector;
    private Vector3 gizmosVector2;
    private Vector3 gizmosVector3;
    private Vector3 gizmosVector4;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        armatureRigidBody = armature.GetComponent<Rigidbody>();
        colliders = armature.GetComponents<BoxCollider>();
        footList = LegManager.instance.footList;
    }

    // Update is called once per frame
    private void Update()
    {
        //time stuff
        walkSpeed = InitialWalkSpeed * SpeedController.instance.factor;
        turnSpeed = InitialTurnSpeed * SpeedController.instance.factor;

        AlignWorldUp();

        if (!inAir)
        {
            //adjust body rotation based on feet
            var feetUp = armature.transform.up; //default value
            var numHovering = LegManager.instance.GetNUmHovering();
            
            if (numHovering <= 1) feetUp = MatchRotationWithFeet(); // if mostly on the ground
            else if (numHovering <= 3) RotateToAnchoredLegs(numHovering);// if walked over an edge
            else Debug.Log("FREE BIRRRRDDD"); // 0 feet on the ground
            
            //adjust body's position based on feet and ground
            AdjustBodyPosition(feetUp);

            //wasd (sprint is in SpeedController maybe change that later)
            MovementInputs();

            //no jump for now
            // #region jump
            // //jump controls
            // if (Input.GetKey(KeyCode.Space))
            // {
            //     //charge up
            // }
            // if (Input.GetKeyUp(KeyCode.Space)) {
            //     //JUMP!!
            //     inAir = true;
            //     
            //     colliders[0].enabled = true;
            //     armatureRigidBody.isKinematic = false;
            //     var jumpForce = (armature.transform.forward + armature.transform.up) * JumpDist;
            //
            //     foreach (var foot in LegManager.instance.footList)
            //     {
            //         var footRigidBody = foot.GetComponent<Rigidbody>();
            //         foot.GetComponent<IkFeetSolver>().enabled = false;
            //         foot.GetComponent<BoxCollider>().enabled = true;
            //         footRigidBody.isKinematic = false;
            //         footRigidBody.AddForce(jumpForce);
            //     }
            //     
            //     armatureRigidBody.AddForce(jumpForce);
            // }
            // #endregion
            
        }

        // if(TriggerController.instance.timeOnGround > 2)
        // {
        //     TriggerController.instance.timeOnGround = 0;
        //     inAir = false;
        //     colliders[0].enabled = false;
        //     armatureRigidBody.isKinematic = true;
        //
        //     foreach (IkFeetSolver foot in new List<IkFeetSolver> { foot1, foot2, foot3, foot4 })
        //     {
        //         foot.GetComponent<BoxCollider>().enabled = false;
        //         foot.GetComponent<Rigidbody>().isKinematic = true;
        //         foot.GetComponent<IkFeetSolver>().enabled = true;
        //         foot.transform.position += Vector3.up* 0.01f;
        //     }
        //
        // }
    }

    private void MovementInputs()
    {
        var wasd = false;
        if (Input.GetKey(KeyCode.W))
        {
            LookMove(mainCam.transform.forward);
            wasd = true;
        }
        if (Input.GetKey(KeyCode.A))
        {
            LookMove(-mainCam.transform.right);
            wasd = true;
        }
        if (Input.GetKey(KeyCode.S))
        {
            LookMove(-mainCam.transform.forward);
            wasd = true;
        }
        if (Input.GetKey(KeyCode.D))
        {
            LookMove(mainCam.transform.right);
            wasd = true;
        }
        if (!wasd) isWalking = false;
    }
    
    private void AlignWorldUp()
    {
        var alignmentSpeed = Vector3.Angle(worldUp.transform.up, armature.transform.up) * 5;
        worldUp.transform.rotation = Quaternion.RotateTowards(worldUp.transform.rotation, armature.transform.rotation,
            Time.deltaTime * alignmentSpeed);
    }

    private Vector3 MatchRotationWithFeet()
    {
        var posList = new List<Vector3>(footList.Count);

        for (var i = 0; i < footList.Count; i++)
        {
            posList.Add(new Vector3());
            posList[i] = footList[i].isHovering ? footList[(i + 1)%footList.Count].transform.position : footList[i].transform.position;
        }
        
        var v1 = posList[2] - posList[0];
        var v2 = posList[3] - posList[1];
        var feetUp = -Vector3.Cross(v1, v2); //vector perpendicular to the plane created by the 4 feet

        var targetR = Quaternion.LookRotation(Vector3.Cross(armature.transform.right, feetUp), feetUp);
        //rotate faster the further u have to rotate
        armature.transform.rotation = Quaternion.RotateTowards(armature.transform.rotation, targetR, Time.deltaTime * turnSpeed);
        return feetUp;
    }

    private void RotateToAnchoredLegs(int numHovering)
    {
        var midPoint = Vector3.zero;
        foreach (var f in LegManager.instance.footList)
            if (!f.isHovering) midPoint += f.transform.position;

        midPoint /= (4-numHovering);
        midPoint -= armature.transform.position;

                
        midPoint = midPoint.normalized;
                
        var cross = Vector3.Cross(-armature.transform.up, midPoint).normalized;
        gizmosVector = cross;
        var angle = Vector3.Angle(-armature.transform.up, midPoint);
                
        armature.transform.rotation = Quaternion.RotateTowards(armature.transform.rotation,
            Quaternion.AngleAxis(angle, cross) * armature.transform.rotation, Time.deltaTime*turnSpeed*3);
        //rotate towards other legs
    }

    private void AdjustBodyPosition(Vector3 feetUp)
    {
        //find distance between feet plane and armature
            //I was gonna change this but it honestly works so I'll just leave it i guess??
            var bodyPosition = armature.transform.position;
            var toFootPlane = Vector3.zero;
            foreach (var foot in footList)
            {
                if (!foot.isHovering)
                    toFootPlane += Vector3.Project(foot1.transform.position - bodyPosition, -feetUp);
            }
            toFootPlane /= (footList.Count - LegManager.instance.GetNUmHovering());

            var pointOnFeetPlane = bodyPosition + toFootPlane;
            
            var boxSize = new Vector3(2.5f, .5f, 2.5f);
            boxCastOrigin = armature.transform.position + armature.transform.up;
            if (Physics.BoxCast(boxCastOrigin, boxSize / 2, -armature.transform.up, out var hit,
                         armature.transform.rotation, 4, layer))
            {
                pointOnFeetPlane += Vector3.Project(hit.point - pointOnFeetPlane, armature.transform.up);
            }
            //////////////boxCast ended
            
            
            var correctedPosition = pointOnFeetPlane + DistFromFeet * feetUp.normalized;

            if (Vector3.Distance(armature.transform.position, correctedPosition) > .01f)
            {
                var distanceToCorrectedPos = correctedPosition - armature.transform.position;
                armature.transform.position = armature.transform.position + distanceToCorrectedPos * (Time.deltaTime * BodyHeightAdjustSpeed * SpeedController.instance.factor);
            }
    }

    private void LookMove(Vector3 t)
    {
        //look at target
        target = Vector3.ProjectOnPlane(t, armature.transform.up);
        Quaternion targetDirection = Quaternion.LookRotation(target, armature.transform.up);
        Quaternion front = armature.transform.rotation;
        armature.transform.rotation = Quaternion.RotateTowards(front, targetDirection, Time.deltaTime * turnSpeed);
      
        //move towards target
        isWalking = true;
        armature.transform.position += target.normalized * (walkSpeed * Time.deltaTime);
    }
    
    private void OnDrawGizmos()
    {
        // Gizmos.color = Color.red;
        // Gizmos.DrawWireSphere(gizmosVector, .12f);
        // Gizmos.color = Color.green;
        // Gizmos.DrawSphere(gizmosVector2, .1f);
        // Gizmos.color = Color.blue;
        // Gizmos.DrawSphere(gizmosVector3, .1f);
        //
        // Gizmos.color = Color.red;
        // var boxSize = new Vector3(2.5f, .5f, 2.5f);
        // var rotationMatrix = Matrix4x4.TRS(boxCastOrigin, armature.transform.rotation, boxSize);
        // Gizmos.matrix = rotationMatrix;
        // Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        
        // Gizmos.color = Color.cyan;
        // Gizmos.DrawSphere(gizmosVector4, .25f);
        //
        // Handles.DrawAAPolyLine(gizmosVector4, gizmosVector4 + gizmosVector3*5);
        
        
    }

}
