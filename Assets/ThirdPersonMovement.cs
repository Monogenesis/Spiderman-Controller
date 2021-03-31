using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
  
    public CharacterController controller;
    public Transform cam;
    public Transform groundCheck;
    public LayerMask groundMask;

    public float gravity = -9.81f;
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

        if (_isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }


        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if(direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDirection.normalized * speed * Time.deltaTime);
        }

        // Find best rope contact
        if (Input.GetMouseButtonDown(1))
        {


        }

        // Grappling Hook
            /*
            RaycastHit hit;
            if (Input.GetMouseButtonDown(1) && !_isGrappling && _remainingGrapplingCooldown <= 0 && Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, grapplingDistance, grapplingTargets))
            {
                grapplingTargetPosition = hit.point;
                lineRenderer.enabled = true;
                _remainingGrapplingCooldown = grapplingCooldown;
                _isGrappling = true;
                _remainingGrapplingTime = 0.1f * Vector3.Distance(hit.point, transform.position);

            }
            else if (Input.GetMouseButtonDown(1) && _isGrappling)
            {
                _isGrappling = false;
            }
            if (_isGrappling && _remainingGrapplingTime > 0) // && Vector3.Distance(transform.position, grapplingTargetPosition) > 2f
            {
                if (Input.GetButtonDown("Jump"))
                {
                    _isGrappling = false;
                    _canDoubleJump = true;
                    _isGrounded = true;
                }
                _remainingGrapplingTime -= Time.deltaTime;
                controller.Move((grapplingTargetPosition - transform.position).normalized * Mathf.Lerp(_currentGrapplingSpeed, grapplingSpeed, 1f) * Time.deltaTime);
                lineRenderer.SetPositions(new Vector3[] { transform.position, grapplingTargetPosition });
                velocity.y -= gravity * Time.deltaTime; // cancel out gravity
            }
            else
            {
                _remainingGrapplingCooldown -= Time.deltaTime;
                _isGrappling = false;
                lineRenderer.enabled = false;
            }
            */

            // Jumping
            if (Input.GetButtonDown("Jump") && _isGrounded)
        {         
                _canDoubleJump = true;
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
