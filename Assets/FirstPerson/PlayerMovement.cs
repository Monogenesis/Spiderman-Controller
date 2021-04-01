using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    public float speed = 12f;
    public float gravity = -9.81f;


    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    public Camera cam;

    // Dashing
    private bool _dashing;
    private float _remainingDashTime;
    private float _remainingDashCooldown;
    public float dashTime;
    public float dashSpeed;
    public float dashCooldown;

    // Jumping
    private bool _canDoubleJump;
    public float jumpHeight = 3f;

    // Grappling Hook
    public float grapplingDistance;
    public float grapplingCooldown;
    public float grapplingSpeed;
    private float _currentGrapplingSpeed;
    private float _remainingGrapplingTime;
    private float _remainingGrapplingCooldown;
    private bool _isGrappling;
    private Vector3 grapplingTargetPosition;
    public LayerMask grapplingTargets;

    // Trampoline
    public LayerMask trampolineMask;
    public float trampolineVelocity;
    private bool _isOnTrampoline;

    // Web-Swinging
    public Transform webTarget;
    public float maxWebDistance;
    public float swingSpeed;
    private bool _isSwinging;
    

    private bool _isGrounded;
    Vector3 velocity;
    LineRenderer lineRenderer;
    private void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        Material lineMaterial = new Material(Shader.Find("Standard"));
        lineMaterial.color = new Color(0, 0, 0, 1);
        lineRenderer.material = lineMaterial;
    }

    private void LateUpdate()
    {

        if (_isSwinging)
        {              
                webTarget.LookAt(transform.position);
                Vector3 limit = webTarget.position + webTarget.forward * maxWebDistance;
                transform.position = limit;      
        }
    }



    void Update()
    {
        _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        _isOnTrampoline = Physics.CheckSphere(groundCheck.position, groundDistance, trampolineMask);

        if (_isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;


        // Find best spider contact point
        if (Input.GetMouseButtonDown(1))
        {
            _isSwinging = !_isSwinging;
            velocity += Vector3.down * swingSpeed;
        }
        if (_isSwinging)
        {
            velocity.y -= gravity * Time.deltaTime * 0.99f;
        }


        // Grappling Hook
            RaycastHit hit;
        if (Input.GetMouseButtonDown(2) && !_isGrappling && _remainingGrapplingCooldown <= 0 && Physics.Raycast(cam.transform.position,cam.transform.forward, out hit, grapplingDistance, grapplingTargets))
        {
            grapplingTargetPosition = hit.point;
            lineRenderer.enabled = true;
            _remainingGrapplingCooldown = grapplingCooldown;
            _isGrappling = true;
            _remainingGrapplingTime = 0.1f * Vector3.Distance(hit.point, transform.position);
           
        }
        else if (Input.GetMouseButtonDown(2) && _isGrappling)
        {
            _isGrappling = false;
        }
        if (_isGrappling && _remainingGrapplingTime > 0) // && Vector3.Distance(transform.position, grapplingTargetPosition) > 2f
        {
            if (Input.GetButtonDown("Jump")){
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

        // Dashing
        if (Input.GetKeyDown(KeyCode.LeftShift) && !_dashing && _remainingDashCooldown <= 0)
        {
            _remainingDashCooldown = dashCooldown;
            _remainingDashTime = dashTime;
            _dashing = true;

        }
        if (_remainingDashTime > 0)
        {
            _remainingDashTime -= Time.deltaTime;
            controller.Move(move.normalized * dashSpeed * Time.deltaTime);
        }
        else
        {
            _remainingDashCooldown -= Time.deltaTime;
            _dashing = false;
        }




        // Jumping
        if (Input.GetButtonDown("Jump"))
        {
            if (_isSwinging)
            {
                _isSwinging = false;
                _canDoubleJump = true;
            }

            // Double Jump
            if (_canDoubleJump)
            {
                _canDoubleJump = false;
                velocity.y = Mathf.Sqrt(jumpHeight * 1.5f * -2f * gravity);
            }

            if (_isGrounded)
            {

                if (_isOnTrampoline)
                {
                    _canDoubleJump = true;
                    velocity.y = Mathf.Sqrt(trampolineVelocity * -2f * gravity);
                }
                else
                {
                    _canDoubleJump = true;
                    velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }
            }

   
        }

        controller.Move(move * speed * Time.deltaTime);
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

    }
}
