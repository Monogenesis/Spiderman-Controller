using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateController : MonoBehaviour
{

    public CharacterController controller;

    public float speed = 12f;
    public float gravity = 25f;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    public Camera cam;

    [SerializeField]
    public Vector3 gravityDirection;
    [SerializeField]
    private Vector3 moveDirection;

    private Vector3 velocity;

    private Vector3 previousPosition;

    [SerializeField]
    private bool _isGrounded;


    private LineRenderer lineRenderer;

    //Swinging
    public float maxWebDistance;
    public float webDistance;
    public LayerMask webTargets;
    [SerializeField] 
    private Vector3 _webHitPosition;

    [SerializeField]  
    private GameObject webHitGameObject;

    [SerializeField]
    private bool isSwinging;


    // Jumping
    private bool _canDoubleJump;
    public float jumpHeight = 3f;

    // 


    private void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        Material lineMaterial = new Material(Shader.Find("Standard"));
        lineMaterial.color = new Color(0, 0, 0, 1);
        lineRenderer.material = lineMaterial;
        gravityDirection = new Vector3(0, -1, 0);
    }


    void Update()
    {
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");   
        
        if (_isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
          
        }
     
        if(!isSwinging)
        moveDirection = transform.right * x + transform.forward * z;

        RaycastHit hit;
        //Swinging
        if(Input.GetMouseButtonDown(1) && Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, maxWebDistance, webTargets))
        {
            lineRenderer.enabled = true;
            _webHitPosition = hit.point;
            isSwinging = true;
            webDistance = Vector3.Distance(transform.position, hit.point);
            webHitGameObject = new GameObject("webtarget");
            webHitGameObject.transform.position = hit.point;
            webHitGameObject.transform.rotation = Quaternion.Euler(cam.transform.forward);

        }
        if (isSwinging)
        {
            SwingingAction();
        }



        // Double jump
        if (Input.GetButtonDown("Jump") && _canDoubleJump)
        {
            _canDoubleJump = false;
            velocity.y += Mathf.Sqrt(jumpHeight * 1f * -2f * -gravity);
        }

        // Jumping
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {

                _canDoubleJump = true;
                velocity.y += Mathf.Sqrt(jumpHeight * -2f * -gravity);
            isSwinging = false;

        }


        if (!isSwinging)
        {
            velocity += gravityDirection * gravity * Time.deltaTime; // Add gravity
            controller.Move(moveDirection * speed * Time.deltaTime);
            controller.Move(velocity * Time.deltaTime);
        }
     
    

    }

    public void GetConstrainedVelocity()
    {

        Vector3 swingVelocity = transform.position - previousPosition;
        previousPosition = transform.position;
        moveDirection += swingVelocity; // Apply velocity
        moveDirection += gravityDirection * gravity * Time.deltaTime; // Apply gravity

        float distance = Vector3.Distance(moveDirection, _webHitPosition);
        //float error = Mathf.Abs(distance - webDistance);

        Vector3 changeDir = Vector3.zero;

        webHitGameObject.transform.LookAt(moveDirection);
        //moveDirection = webHitGameObject.transform.forward * webDistance;
        Debug.DrawLine(webHitGameObject.transform.position, webHitGameObject.transform.forward * webDistance);
        moveDirection = (moveDirection - _webHitPosition).normalized * webDistance;

        //transform.position += moveDirection;
        controller.Move(moveDirection * Time.deltaTime);

        //Vector3 changeAmount = changeDir * error;
     


        //constrainedPos = Vector3.Normalize(currentPos - _webHitPosition) * webDistance;
        //predictedPos = (constrainedPos - previousPosition) / (Time.deltaTime * Time.deltaTime);



    }

    private void SwingingAction()
    {
        lineRenderer.SetPositions(new Vector3[] { transform.position, _webHitPosition });
        GetConstrainedVelocity();
        /*
        if (Vector3.Distance(transform.position, _webHitPosition) < webDistance)
        {
            transform.position = Vector3.Normalize(transform.position - _webHitPosition) * webDistance;
            //webDistance = Vector3.Distance(transform.position, _webHitPosition);
        }
        */
    }
}
