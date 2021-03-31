using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Camera cam;
    public float ropeMaxDist = 30;
    public LayerMask interactionLayer;
    public LayerMask whatIsGround;
    public float camLookSpeed = 1;
    public float XSensitivity = 2f;
    public float YSensitivity = 2f;
    public float MinimumX = -90F;
    public float MaximumX = 90F;

    private float lastYPos;
    private bool swingBack;

    public float speed = 10;
    public float jumpPower = 100;
    public float groundDistance = 0.1f;
    private float ropeDistance;
    private Quaternion characterTargetRot;
    private Quaternion cameraTargetRot;
    Rigidbody m_Rigidbody;
    private GameObject ropeTarget;
    private LineRenderer rope ;
  

    public float velocity = 0.0f;

    // Rope
    public float ropeSpeed = 400.0f;
    public float minRopeSpeed = 500.0f;
    public float ropeVelocity = 1.0f;
    private float minRopeVelocity = 0.0f;
    public float maxRopeVelocity = 1.0f;

    public float ropeAngle = Mathf.PI * 0.25f;

    public float normalRopeTilt;
    public float minRopeTilt;
    public float maxRopeTilt;

    public float ropeTiltSpeed;
    private Vector3 ropeRotationAxis;
    private Vector3 ropeTiltAxis;
    private Vector3 ropeRestingRotation;

    bool falling;

    public bool onGround = true;
    void Start()
    {

        m_Rigidbody = GetComponent<Rigidbody>();
        cam = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rope = GetComponent<LineRenderer>();
        rope.positionCount = 2;
        rope.startWidth = 0.1f;
        rope.endWidth = 0.1f;
    }


    void Update()
    {

        // Camera
        characterTargetRot = transform.localRotation;
        cameraTargetRot = cam.transform.localRotation;

        float xRot = Input.GetAxis("Mouse X") * XSensitivity;
        float yRot = Input.GetAxis("Mouse Y") * YSensitivity;

        characterTargetRot *= Quaternion.Euler(0f, xRot, 0f);
        cameraTargetRot *= Quaternion.Euler(-yRot, 0f, 0f);

        cameraTargetRot = ClampRotationAroundXAxis(cameraTargetRot);

        transform.localRotation = characterTargetRot;
        cam.transform.localRotation = cameraTargetRot;


        if (!onGround && lastYPos > transform.position.y)
        {
          
            falling = true;
        }
        else if (lastYPos < transform.position.y)
        {
            falling = false;
        }
        else
        {
            falling = false;
        }
        lastYPos = transform.position.y;
        Debug.Log(falling);
        if (Physics.Raycast(transform.position, Vector3.down, transform.localScale.y * 0.5f + groundDistance, whatIsGround))
        {
            onGround = true;
            destroyHook();
        }

        if (ropeTarget == null)
        {
            m_Rigidbody.useGravity = true;
            if (true) // onGround
            {
                transform.position += transform.forward * speed * Time.deltaTime * Input.GetAxis("Vertical");
                transform.position += transform.right * speed * Time.deltaTime * Input.GetAxis("Horizontal");
            }

        }else
        {

            float xPos = ropeTarget.transform.position.x;
            float yPos = ropeTarget.transform.position.y + ropeDistance * Mathf.Cos(ropeAngle);
            float zPos = ropeTarget.transform.position.z + ropeDistance * Mathf.Sin(ropeAngle);

            Vector3 nextPos = new Vector3(xPos, yPos, zPos).normalized;

            transform.position += nextPos;
            m_Rigidbody.useGravity = false;

            /*
            // speed up or slow down
            if (ropeVelocity < 0.1)
            {
                ropeVelocity = 0.15f;
                swingBack = !swingBack;
            }
            
            if (falling)
            {
                ropeVelocity += 0.75f * Time.deltaTime;
          
            }else if (!falling)
            {
                ropeVelocity -= 0.75f * Time.deltaTime;
         
            }
            else
            {
               
                ropeVelocity = 0.1f;
            }
       
          
            if(Input.GetAxis("Horizontal") > 0.1f || Input.GetAxis("Horizontal") < -0.1f)
            {
                //ropeTiltAxis = ropeTarget.transform.up;
                ropeTarget.transform.Rotate(ropeTarget.transform.up, -Input.GetAxis("Horizontal") * ropeTiltSpeed  * ropeSpeed * Time.deltaTime * Mathf.Clamp(ropeVelocity, 0, maxRopeVelocity));           
            }
            //ropeTarget.transform.rotation = Quaternion.Slerp(ropeTarget.transform.rotation, Quaternion.Euler(ropeTiltAxis), 0.01f);
            //ropeTarget.transform.rotation = Quaternion.Euler(ropeTarget.transform.rotation.x, Mathf.Lerp(ropeTarget.transform.rotation.y, ropeTiltAxis.y, 0.1f * Time.deltaTime), ropeTarget.transform.rotation.z);

            m_Rigidbody.useGravity = false;
            ropeRotationAxis = ropeTarget.transform.right;
            if (swingBack)
            ropeTarget.transform.RotateAround(ropeTarget.transform.position, -ropeRotationAxis, ropeSpeed * (1 / ropeDistance) * Time.deltaTime * Mathf.Clamp(ropeVelocity, 0, maxRopeVelocity));
            else
            {
                ropeTarget.transform.RotateAround(ropeTarget.transform.position, ropeRotationAxis, ropeSpeed * (1 / ropeDistance) * Time.deltaTime * Mathf.Clamp(ropeVelocity, 0, maxRopeVelocity));
            }
             */
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
               
        }



        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Debug.DrawLine(ray.origin, ray.GetPoint(ropeMaxDist));

        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_Rigidbody.AddForce(Vector3.up * jumpPower);
            onGround = false;
            destroyHook();
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (ropeTarget != null)
            {
                destroyHook();
            }
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, ropeMaxDist, interactionLayer))
            {
                //transform.position = hit.point;
                if (ropeTarget != null)
                {
                    destroyHook();


                }
                shootRope(ray, hit.point);
            }
        }

    

    }

    private void LateUpdate()
    {
        if(ropeTarget != null)
        {
            // Draw Rope
            rope.SetPosition(1, transform.position + transform.forward);
        }
        
    }


    private void destroyHook()
    {
        gameObject.transform.parent = null;
        Destroy(ropeTarget);
        rope.positionCount = 0;


    }

    public void shootRope(Ray ray, Vector3 targetPoint)
    {
        m_Rigidbody.velocity = Vector3.zero;
        ropeSpeed = minRopeSpeed;
        ropeTarget = new GameObject("rope target");
        ropeTarget.transform.position = targetPoint;
        ropeTarget.transform.rotation = transform.rotation;
        ropeDistance = Vector3.Distance(transform.position, targetPoint);
        gameObject.transform.parent = ropeTarget.gameObject.transform;
        ropeRotationAxis = ropeTarget.transform.right;
        ropeTiltAxis = ropeTarget.transform.up;
        ropeRestingRotation = new Vector3(cam.transform.rotation.x, cam.transform.rotation.y, -180.0f);
        ropeVelocity = 0;

        // Draw Rope
        rope.positionCount = 2;
        rope.SetPosition(0, ropeTarget.transform.position);
        rope.SetPosition(1, transform.position);
       
        
    }

 

    void OnDrawGizmos()
    {

        if (ropeTarget != null) { 
            
            Gizmos.color = new Color(0, 0, 0, 0.2F);
            Gizmos.DrawSphere(ropeTarget.transform.position, ropeDistance);
           
        }
    }

    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }

}
