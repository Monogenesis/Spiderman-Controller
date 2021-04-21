using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{

    public CharacterController controller;
    public Transform cam;
    public Transform groundCheck;
    public LayerMask groundMask;
    public Animator animator;

    public Transform handPositionSwinging;

    public float gravity = 9.81f;
    public float groundDistance = 0.4f;

    public float speed = 6f;
    public float turnSmoothTime = 0.1f;

    private bool _isGrounded;

    float turnSmoothVelocity;

    public Vector3 velocity;
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
    public float dampening = 0.01f;
    //[SerializeField] private Vector3 swingVelocity;
    public GameObject anker;
    public float maxDistance;
    public float swingSpeed;
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
            // Stop sliding
            velocity = Vector3.zero;
            velocity.y = -2f;
        }



        // Swing action

        Debug.DrawLine(cam.transform.position, cam.transform.position + cam.transform.forward * ropeDistance, Color.red);
        RaycastHit hit;
        if (Input.GetKeyDown(KeyCode.Mouse1) && Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, maxDistance, webtargets))
        {
            Debug.Log("Hit Web Target!");
            Destroy(anker);
            anker = new GameObject("Anker!");
            anker.transform.position = hit.point;
            ropeDistance = Vector3.Distance(transform.position, hit.point);
            isSwinging = true;
            lineRenderer.enabled = true;
            oldPos = transform.position;
        }

        if (Input.GetKey(KeyCode.Mouse1) && isSwinging)
        {
            SwingAction();
            animator.SetBool("Swinging", true);
        }
        else
        {
            isSwinging = false;
            lineRenderer.enabled = false;
            animator.SetBool("Swinging", false);
        }
        if (!isSwinging || (isSwinging && _isGrounded))
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 direction = new Vector3(horizontal, 0f, vertical);
            WalkAction();
            if (_isGrounded && direction.magnitude > 0.2f)
            {
                animator.SetBool("Running", true);
            }
            else
            {
                animator.SetBool("Running", false);
            }
        }
        if (!_isGrounded && !isSwinging)
        {

            FallingAction();

        }
        // Jumping
        if (Input.GetButtonDown("Jump"))
        {
            isSwinging = false;
            if (_isGrounded)
            {
                JumpAction();
                _canDoubleJump = true;
            }
            else if (_canDoubleJump)
            {
                DoubleJumpAction();
                _canDoubleJump = false;
            }
        }

    }

    void SwingAction()
    {

        velocity = (transform.position - oldPos) / Time.deltaTime;
        oldPos = transform.position;
        velocity += gravityDirection * gravity * Time.deltaTime;
        velocity += -velocity * dampening;

        if (Input.GetKey(KeyCode.W))
        {
            velocity += velocity.normalized * 2;
        }
        if (Input.GetKey(KeyCode.A))
        {
            velocity += -cam.transform.right * 1.1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            velocity += cam.transform.right * 1.1f;
        }
        //velocity *= swingSpeed;
        velocity = Vector3.ClampMagnitude(velocity, maxSwingSpeed);
        //transform.position += velocity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        // Calculate the new distance to the anker
        float dist = (transform.position - anker.transform.position).magnitude;
        // Constrain the position to the max distance
        if (dist > ropeDistance)
        {
            //Vector3 constrainedPos = Vector3.Normalize(transform.localPosition - anker.transform.localPosition);
            transform.position = anker.transform.localPosition + (transform.localPosition - anker.transform.localPosition).normalized * ropeDistance;
        }

        transform.rotation = Quaternion.Euler(0, cam.rotation.eulerAngles.y, 0);
        lineRenderer.SetPositions(new Vector3[] { transform.position, anker.transform.position });
    }

    void FallingAction()
    {
        velocity += gravityDirection * gravity * Time.deltaTime;
        //transform.position += swingVelocity * Time.deltaTime;

    }
    void JumpAction()
    {

        velocity.y = Mathf.Sqrt(jumpHeight * 2f * gravity);

    }

    void DoubleJumpAction()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * 2f * gravity);
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("collision with " + other.gameObject.name);
    }
    void WalkAction()
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
