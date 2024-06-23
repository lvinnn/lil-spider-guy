using System.Collections.Generic;
using UnityEngine;

public class SpiderController : MonoBehaviour
{

    public static SpiderController instance;

    public bool isWalking;

    public GameObject armature;
    public Vector3 target;

    //public CinemachineFreeLook cam;
    //public GameObject worldUp;
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
    
    //gizmos
    private Vector3 gizmosVector;
    private Vector3 gizmosVector2;
    private Vector3 gizmosVector3;

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
    }

    // Update is called once per frame
    private void Update()
    {
        //time stuff
        walkSpeed = InitialWalkSpeed * SpeedController.instance.factor;
        turnSpeed = InitialTurnSpeed * SpeedController.instance.factor;

        if (!inAir)
        {
            #region match body's rotation with the feet's positions
            var f13 = foot3.transform.position - foot1.transform.position;
            var f24 = foot4.transform.position - foot2.transform.position;
            var feetUp = -Vector3.Cross(f13, f24); //vector perpendicular to the plane created by the 4 feet

            Quaternion targetR = Quaternion.LookRotation(Vector3.Cross(armature.transform.right, feetUp), feetUp);
            //rotate faster the further u have to rotate
            var rotateSpeed = Quaternion.Angle(armature.transform.rotation, targetR) * 15 * SpeedController.instance.factor; 
            armature.transform.rotation = Quaternion.RotateTowards(armature.transform.rotation, targetR, Time.deltaTime * rotateSpeed);
            Quaternion.Angle(armature.transform.rotation, targetR);

            #endregion

            #region match body's position with the floor under it

            //bunch of math to find distance between feet plane and armature
            var a = feetUp.x;
            var b = feetUp.y;
            var c = feetUp.z;

            var f1 = foot1.transform.position;
            var d = -(a * f1.x + b * f1.y + c * f1.z);
            var bodyPosition = armature.transform.position;
            var distance = Mathf.Abs(a * bodyPosition.x + b * bodyPosition.y + c * bodyPosition.z + d) / feetUp.magnitude;
            var projectedPoint = bodyPosition - distance * feetUp.normalized;
            //blue sphere
            gizmosVector3 = projectedPoint; //projected point on feet plane
            
            
            
            // box cast
            var boxSize = new Vector3(2.5f, .5f, 2.5f);
            boxCastOrigin = armature.transform.position + armature.transform.up;
            var numOverlaps = Physics.OverlapBoxNonAlloc(boxCastOrigin, boxSize / 2, new Collider[1], armature.transform.rotation, layer);
            Vector3 contact;
            if (numOverlaps > 0)
            {
                contact = boxCastOrigin;
                gizmosVector2 = contact; //green sphere
            }
            else if (Physics.BoxCast(boxCastOrigin, boxSize / 2, -armature.transform.up, out var hit,
                         armature.transform.rotation, 5, layer))
            {
                gizmosVector2 = hit.point; //green sphere
                contact = hit.point;
            }
            else contact = projectedPoint;
            projectedPoint += Vector3.Project(contact - projectedPoint, armature.transform.up);
            
            
            gizmosVector = projectedPoint; //red sphere
            var correctedPosition = projectedPoint + DistFromFeet * feetUp.normalized;

            if (Vector3.Distance(armature.transform.position, correctedPosition) > .01f)
            {
                var distanceToCorrectedPos = correctedPosition - armature.transform.position;
                armature.transform.position = armature.transform.position + distanceToCorrectedPos * (Time.deltaTime * BodyHeightAdjustSpeed * SpeedController.instance.factor);
            }
            #endregion

            
            
            
            #region wasd controls
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
            #endregion

            //jump controls
            if (Input.GetKey(KeyCode.Space))
            {
                //charge up
            }
            if (Input.GetKeyUp(KeyCode.Space)) {
                //JUMP!!
                inAir = true;
                
                colliders[0].enabled = true;
                armatureRigidBody.isKinematic = false;
                var jumpForce = (armature.transform.forward + armature.transform.up) * JumpDist;

                foreach (var foot in new List<IkFeetSolver>{foot1, foot2, foot3, foot4})
                {
                    var footRigidBody = foot.GetComponent<Rigidbody>();
                    foot.GetComponent<IkFeetSolver>().enabled = false;
                    foot.GetComponent<BoxCollider>().enabled = true;
                    footRigidBody.isKinematic = false;
                    footRigidBody.AddForce(jumpForce);
                }
                
                armatureRigidBody.AddForce(jumpForce);
            }
            
        }

        return;
        if(TriggerController.instance.timeOnGround > 2)
        {
            TriggerController.instance.timeOnGround = 0;
            inAir = false;
            colliders[0].enabled = false;
            armatureRigidBody.isKinematic = true;

            foreach (IkFeetSolver foot in new List<IkFeetSolver> { foot1, foot2, foot3, foot4 })
            {
                foot.GetComponent<BoxCollider>().enabled = false;
                foot.GetComponent<Rigidbody>().isKinematic = true;
                foot.GetComponent<IkFeetSolver>().enabled = true;
                foot.transform.position += Vector3.up* 0.01f;
            }

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
        
        
    }

}
