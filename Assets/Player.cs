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

    public float speed = 10;
    public float jumpPower = 100;
    public float groundDistance = 0.1f;
    private float ropeDistance;
    private Quaternion characterTargetRot;
    private Quaternion cameraTargetRot;
    Rigidbody m_Rigidbody;
    private GameObject ropeTarget;
    public float ropeSpeed = 50.0f;
    private Vector3 ropeVelocity;
    private Vector3 ropeRotationAxis;
    public bool onGround = true;
    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        cam = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    void Update()
    {

        characterTargetRot = transform.localRotation;
        cameraTargetRot = cam.transform.localRotation;

        float xRot = Input.GetAxis("Mouse X") * XSensitivity;
        float yRot = Input.GetAxis("Mouse Y") * YSensitivity;

        characterTargetRot *= Quaternion.Euler(0f, xRot, 0f);
        cameraTargetRot *= Quaternion.Euler(-yRot, 0f, 0f);

        cameraTargetRot = ClampRotationAroundXAxis(cameraTargetRot);

        transform.localRotation = characterTargetRot;
        cam.transform.localRotation = cameraTargetRot;

        if (Physics.Raycast(transform.position, Vector3.down, transform.localScale.y * 0.5f + groundDistance, whatIsGround))
        {
            onGround = true;
            Destroy(ropeTarget);
        }

        if (ropeTarget == null)
        {
            GetComponent<Rigidbody>().useGravity = true;
            if (true) // onGround
            {
                transform.position += transform.forward * speed * Time.deltaTime * Input.GetAxis("Vertical");
                transform.position += transform.right * speed * Time.deltaTime * Input.GetAxis("Horizontal");
            }

        }else
        {
            GetComponent<Rigidbody>().useGravity = false;
            transform.RotateAround(ropeTarget.transform.position, -ropeTarget.transform.right, ropeSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }



        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Debug.DrawLine(ray.origin, ray.GetPoint(ropeMaxDist));

        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetComponent<Rigidbody>().AddForce(Vector3.up * jumpPower);
            onGround = false;
            Destroy(ropeTarget);
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (ropeTarget != null)
            {
                Destroy(ropeTarget);
                ropeTarget = null;
            }
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, ropeMaxDist, interactionLayer))
            {
                Debug.Log("Hit Wall!");
                //transform.position = hit.point;
                if (ropeTarget != null)
                {
                    Destroy(ropeTarget);
                }
                shootRope(hit.point);
            }
        }



    }

    public void shootRope(Vector3 targetPoint)
    {
        Debug.Log("new Rope");
        ropeTarget = new GameObject("rope target");
        ropeTarget.transform.position = targetPoint;
        ropeTarget.transform.rotation = cam.transform.rotation;
        ropeDistance = Vector3.Distance(transform.position, targetPoint);
        


    }

    void OnDrawGizmos()
    {

        if (ropeTarget != null) { 
            Gizmos.color = new Color(1, 1, 0, 0.75F);
            Gizmos.DrawWireSphere(ropeTarget.transform.position, ropeDistance);
           
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
