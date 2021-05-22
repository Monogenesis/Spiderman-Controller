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
    public float swingSpeed;


    [Header("Best Web Target Settings")]
    private GameObject anker;
    public bool displayGizmos;
    public float futureSteps = 0;
    public Vector3 WebTargetSearchBoxDimensions = new Vector3(3, 1, 3);

    [Min(0)]
    public float minHeightForTarget = 5;

    bool hasHitWebTarget;

    public float WebTargetSearchStep = 1f;
    Vector3 webTargetPosition;
    Vector3 velocityBeforeSwing;

    private Vector3 WebTargetSearchPosition = Vector3.zero;

    // Web Texture
    GameObject webTexture;

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
        webTexture = new GameObject("WebString");
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

        Vector3 futurePoint = transform.position + new Vector3(velocity.x, Mathf.Abs(velocity.y), velocity.z).normalized * futureSteps;
        //Vector3 futurePoint = transform.position + velocity.normalized * futureSteps;
        WebTargetSearchPosition = futurePoint + Vector3.up * maxDistance;

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {

            Vector3 webHit = FindOptiomalWebTarget();
            if (webHit != Vector3.zero)
            {
                velocityBeforeSwing = velocity;
                Destroy(anker);
                anker = new GameObject("Anker!");
                anker.transform.position = webHit;
                anker.transform.rotation = transform.rotation;
                ropeDistance = Vector3.Distance(transform.position, webHit);
                isSwinging = true;
                lineRenderer.enabled = true;
                //oldPos = transform.position;
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


        Collider[] allHits = new Collider[0];


        Vector3 SearchBox = WebTargetSearchBoxDimensions;
        for (float tempDistance = 0; tempDistance < (maxDistance - minHeightForTarget); tempDistance += WebTargetSearchStep)
        {
            allHits = Physics.OverlapBox(WebTargetSearchPosition, WebTargetSearchBoxDimensions / 2, transform.rotation, webtargets, QueryTriggerInteraction.UseGlobal);
            if (allHits.Length > 0)
            {
                break;
            }
            else
            {
                WebTargetSearchPosition += Vector3.down * WebTargetSearchStep;
                //SearchBox += new Vector3(0.5f, 0, 0.5f);

            }
        }


        int bestHit = 0;
        for (int i = 0; i < allHits.Length; i++)
        {
            Collider currentCollider = allHits[i];
            Vector3 closestPoint = currentCollider.ClosestPointOnBounds(WebTargetSearchPosition);

            if (Vector3.Distance(closestPoint, transform.position) > Vector3.Distance(allHits[bestHit].ClosestPointOnBounds(WebTargetSearchPosition), transform.position))
            {
                bestHit = i;
            }

            webTargetPosition = allHits[bestHit].ClosestPointOnBounds(WebTargetSearchPosition);
        }
        hasHitWebTarget = allHits.Length > 0;
        //Physics.CheckBox(checkBoxPoint, WebTargetSearchBoxDimensions,Quaternion.identity, webtargets);
        if (hasHitWebTarget)
        {
            return webTargetPosition;
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
                Gizmos.DrawWireCube(WebTargetSearchPosition + Vector3.down * Vector3.Distance(webTargetPosition, WebTargetSearchPosition), WebTargetSearchBoxDimensions);
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
            velocity += velocity.normalized * Time.deltaTime * swingSpeed;
            //velocity += cam.transform.forward * Time.deltaTime * swingSpeed; // Find direction between velocity and cam forward to apply force 
        }
        if (Input.GetKey(KeyCode.A))
        {
            velocity += -cam.transform.right * Time.deltaTime * swingSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            velocity += cam.transform.right * Time.deltaTime * swingSpeed;
        }

        velocity = Vector3.ClampMagnitude(velocity, maxSwingSpeed);
        //transform.position += velocity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        // Calculate the new distance to the anker
        // float dist = (transform.position - anker.transform.position).magnitude;
        float dist = Vector3.Distance(transform.position, anker.transform.position);
        // Constrain the position to the max distance
        if (dist != ropeDistance)
        {
            //Vector3 constrainedPos = Vector3.Normalize(transform.localPosition - anker.transform.localPosition);
            transform.position = anker.transform.localPosition + (transform.localPosition - anker.transform.localPosition).normalized * ropeDistance;
            ropeDistance = Vector3.Distance(transform.position, anker.transform.localPosition);

        }
        else if (dist < ropeDistance && !_isGrounded)
        {
            transform.position = anker.transform.localPosition + (transform.localPosition - anker.transform.localPosition).normalized * ropeDistance;
            ropeDistance = Vector3.Distance(transform.position, anker.transform.localPosition);
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
        //velocity += gravityDirection * gravity * Time.deltaTime;
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
