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

    [Header("Gravity Settings")]
    public Vector3 gravityDirection = new Vector3(0, -1, 0);
    public float gravity = 9.81f;
    public float groundDistance = 0.4f;

    public float speed = 6f;
    public float turnSmoothTime = 0.1f;
    [SerializeField] private bool _isGrounded;

    float turnSmoothVelocity;

    public Vector3 velocity;
    LineRenderer lineRenderer;

    // Jumping

    [Header("Jump settings")]
    private bool _canDoubleJump;
    public float jumpHeight = 3f;


    private Vector3 oldPos;


    // Swing
    [Header("Swinging Settings")]
    public LayerMask webtargets;
    private bool isSwinging;
    public float ropeDistance;
    public float dampening = 0.01f;
    //[SerializeField] private Vector3 swingVelocity;

    public float maxDistance;
    public float maxSwingSpeed;
    private float swingSpeed;
    private GameObject anker;

    [Header("Best Web Target Settings")]
    public bool displayGizmos;
    public float futureSteps = 0;
    public Vector3 WebTargetSearchBoxDimensions = new Vector3(3, 1, 3);
    public float WebTargetSphereRadius = 10;
    [Range(0, 50)]
    public float minHeightForTarget = 5;

    bool hasHitWebTarget;

    RaycastHit webHit;

    private Vector3 WebTargetSearchPosition = Vector3.zero;

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
        // Physics.CheckCapsule

        if (_isGrounded && velocity.y < 0)
        {
            // Stop sliding
            velocity = Vector3.zero;
            velocity.y = -2f;
        }

        // Swing action
        //Debug.DrawLine(cam.transform.position, cam.transform.position + cam.transform.forward * ropeDistance, Color.red);
        //Debug.DrawLine(transform.position, transform.position + velocity * futureSteps, Color.red);
        //Debug.DrawLine(transform.position, transform.position + new Vector3(velocity.x, Mathf.Abs(velocity.y), velocity.z) * futureSteps, Color.red);

        Vector3 futurePoint = transform.position + new Vector3(velocity.x, Mathf.Abs(velocity.y), velocity.z) * futureSteps;
        WebTargetSearchPosition = futurePoint + Vector3.up * maxDistance;

        RaycastHit[] allHits = new RaycastHit[0];
        //allHits = Physics.SphereCastAll(WebTargetSearchPosition, WebTargetSphereRadius, Vector3.down * (maxDistance - WebTargetSphereRadius), maxDistance - minHeightForTarget, webtargets);
        hasHitWebTarget = Physics.BoxCast(WebTargetSearchPosition, WebTargetSearchBoxDimensions / 2, Vector3.down * maxDistance, out webHit,
          transform.rotation, maxDistance - minHeightForTarget, webtargets, QueryTriggerInteraction.UseGlobal); // Vector3.Distance(transform.position, WebTargetSearchPosition)

        Debug.Log(webHit.point);

        if (hasHitWebTarget)
        {
            Debug.Log(webHit.point);
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {

            Vector3 webHit = FindOptiomalWebTarget();
            if (webHit != Vector3.zero)
            {
                Debug.Log("Hit Web Target!");
                Destroy(anker);
                anker = new GameObject("Anker!");
                anker.transform.position = webHit;
                ropeDistance = Vector3.Distance(transform.position, webHit);
                isSwinging = true;
                lineRenderer.enabled = true;
                oldPos = transform.position;
            }

        }

        if (Input.GetKey(KeyCode.Mouse1) && isSwinging)
        {
            SwingAction();
        }
        else
        {
            isSwinging = false;
            lineRenderer.enabled = false;

        }

        if (!isSwinging)
        {
            UpdateAnimation();
            WalkAction();
        }
        if (!_isGrounded && !isSwinging)
        {

            FallingAction();

        }
        // Jumping
        if (Input.GetButtonDown("Jump"))
        {
            if (isSwinging)
            {
                isSwinging = false;
                SwingJumpAction();
            }
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

    private Vector3 FindOptiomalWebTarget()
    {



        //Physics.CheckBox(checkBoxPoint, WebTargetSearchBoxDimensions,Quaternion.identity, webtargets);
        if (hasHitWebTarget)
        {
            Debug.Log("Target: " + webHit.point);
            return webHit.point;
        }

        return Vector3.zero;
    }

    private void OnDrawGizmos()
    {
        if (displayGizmos)
        {
            if (hasHitWebTarget)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(WebTargetSearchPosition, Vector3.down * (maxDistance - minHeightForTarget));
                //Gizmos.DrawSphere(WebTargetSearchPosition + Vector3.down * webHit.distance + Vector3.down * WebTargetSphereRadius, WebTargetSphereRadius);
                //Gizmos.DrawWireSphere(WebTargetSearchPosition + Vector3.down * webHit.distance, WebTargetSphereRadius);
                Gizmos.DrawWireCube(WebTargetSearchPosition + Vector3.down * webHit.distance, WebTargetSearchBoxDimensions);
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(WebTargetSearchPosition, Vector3.down * (maxDistance - minHeightForTarget));
                Gizmos.DrawWireCube(WebTargetSearchPosition, WebTargetSearchBoxDimensions);
                // Gizmos.DrawSphere(WebTargetSearchPosition, WebTargetSphereRadius);
                //Gizmos.DrawWireSphere(WebTargetSearchPosition, WebTargetSphereRadius);
            }


        }

    }
    private void LateUpdate()
    {
        UpdateAnimation();
    }
    void UpdateAnimation()
    {
        if (isSwinging)
        {
            animator.SetBool("Swinging", true);
        }
        else
        {
            animator.SetBool("Swinging", false);
        }
        if (_isGrounded || (isSwinging && _isGrounded))
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 direction = new Vector3(horizontal, 0f, vertical);
            if (_isGrounded && (direction.magnitude > 0.2f || new Vector3(velocity.x, 0, velocity.z).magnitude > 0.2f))
            {
                animator.SetBool("Running", true);
                animator.SetBool("Swinging", false);
            }
            else
            {
                animator.SetBool("Running", false);

            }

        }
        else
        {
            animator.SetBool("Running", false);

        }
        if (!_isGrounded && !isSwinging)
        {
            animator.SetBool("Falling", true);

        }
        else
        {
            animator.SetBool("Falling", false);
        }

    }
    void SwingJumpAction()
    {

        velocity.y += Mathf.Sqrt(jumpHeight * 5f * gravity);
        velocity += velocity.normalized * 15;
    }
    void SwingAction()
    {

        velocity = ((transform.position - oldPos) / Time.deltaTime);
        oldPos = transform.position;
        velocity += gravityDirection * gravity * Time.deltaTime;
        ApplyDrag();


        if (Input.GetKey(KeyCode.W))
        {
            velocity += velocity.normalized * 1.5f;
            //velocity += cam.transform.forward; // Find direction between velocity and cam forward to apply force 
        }
        if (Input.GetKey(KeyCode.A))
        {
            velocity += -cam.transform.right * 1.1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            velocity += cam.transform.right * 1.1f;
        }

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
        lineRenderer.SetPositions(new Vector3[] { handPositionSwinging.position, anker.transform.position });
    }

    void ApplyDrag()
    {
        velocity += -velocity * dampening;
    }
    void FallingAction()
    {
        velocity += gravityDirection * gravity * Time.deltaTime;
        ApplyDrag();
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
        //Debug.Log("collision with " + other.gameObject.name);
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
            velocity += moveDirection.normalized * speed * Time.deltaTime;
        }
        velocity += gravityDirection * gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

    }
}
