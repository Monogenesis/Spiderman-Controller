using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{

    public CharacterController controller;
    public Transform cam;
    public Transform groundCheck;
    public LayerMask groundMask;

    public float gravity = 9.81f;
    public float groundDistance = 0.4f;

    public float speed = 6f;
    public float turnSmoothTime = 0.1f;

    private bool _isGrounded;

    float turnSmoothVelocity;
    Vector3 velocity;
    LineRenderer lineRenderer;

    // Jumping
    private bool _canDoubleJump;
    public float jumpHeight = 3f;


    public Vector3 gravityDirection = new Vector3(0, -1, 0);
    private Vector3 oldPos;


    // Swing
    public LayerMask webtargets;
    private bool isSwinging;
    public float ropeDistance;
    public float dampening = 0.05f;
    [SerializeField] private Vector3 swingVelocity;
    public GameObject anker;
    public float maxDistance;
    public float maxSwingSpeed;

    // Grappling Hook
    /*
    public float grapplingDistance;
    public float grapplingCooldown;
    public float grapplingSpeed;
    private float _currentGrapplingSpeed;
    private float _remainingGrapplingTime;
    private float _remainingGrapplingCooldown;
    private bool _isGrappling;
    private Vector3 grapplingTargetPosition;
    public LayerMask grapplingTargets;
    */
    private void Awake()
    {

    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        Material lineMaterial = new Material(Shader.Find("Standard"));
        lineMaterial.color = new Color(0, 0, 0, 1);
        lineRenderer.material = lineMaterial;
    }

    // TODO SWING gravity daption 1 or 20...
    // TODO Swing movement is buggy
    void Update()
    {
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (_isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }



        // Swing action
        if (isSwinging && Input.GetMouseButtonDown(1))
        {
            isSwinging = false;
            lineRenderer.enabled = false;
        }

        Debug.DrawLine(cam.transform.position, cam.transform.position + cam.transform.forward * ropeDistance, Color.red);
        RaycastHit hit;
        if (Input.GetKey(KeyCode.Mouse1) && Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, maxDistance, webtargets))
        {

            Debug.Log("Hit Web Target!");
            anker = new GameObject("Anker!");
            anker.transform.position = hit.point;
            ropeDistance = Vector3.Distance(transform.position, hit.point);
            isSwinging = true;
            lineRenderer.enabled = true;
            oldPos = transform.position;
        }

        if (isSwinging)
        {
            swingAction();
        }
        if (!isSwinging)
        {
            walkAction();
        }
        if (!_isGrounded && !isSwinging)
        {
            FallingAction();
        }
        // Jumping
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            jumpAction();
        }

    }

    void swingAction()
    {
        swingVelocity = (transform.position - oldPos);

        oldPos = transform.position;
        swingVelocity += gravityDirection * gravity * Time.deltaTime;
        swingVelocity += -swingVelocity.normalized * dampening * Time.deltaTime;
        swingVelocity = Vector3.ClampMagnitude(swingVelocity, maxSwingSpeed);
        if (Input.GetKey(KeyCode.W))
        {
            swingVelocity += swingVelocity.normalized * 3 * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            swingVelocity += -cam.transform.right * 1.2f * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            swingVelocity += cam.transform.right * 1.2f * Time.deltaTime;
        }
        transform.position += swingVelocity;


        float dist = (transform.position - anker.transform.position).magnitude;
        if (dist > ropeDistance)
        {
            Vector3 constrainedPos = Vector3.Normalize(transform.localPosition - anker.transform.localPosition);
            transform.position = anker.transform.localPosition + constrainedPos * ropeDistance;
        }

        transform.LookAt(transform.position - oldPos);
        lineRenderer.SetPositions(new Vector3[] { transform.position, anker.transform.position });
    }

    void FallingAction()
    {
        transform.position += gravityDirection * gravity * Time.deltaTime;
        transform.position += swingVelocity * Time.deltaTime;

    }
    void jumpAction()
    {
        _canDoubleJump = true;
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }



    void walkAction()
    {

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDirection.normalized * speed * Time.deltaTime);
        }
        velocity += gravityDirection * gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
