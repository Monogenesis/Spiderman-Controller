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
    public Material webMaterial;
    public Transform webHookHandPosition;
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


    // Jumping

    [Header("Jump settings")]

    public float jumpPower = 3f;
    public float diveJumpAngle = 40f;
    public float minDiveJumpHeight = 10f;
    private bool _canDoubleJump;

    private Vector3 oldPos;

    // Hook Move
    [Header("Rope Hook Settings")]
    public LayerMask hookTargets;
    [Range(-180, 180)]
    public float detectionAngle;
    public float minHookDistance = 5f;
    public bool isWebHooking;
    public float hookSpeed = 20.0f;
    public GameObject edgeMarkerPrefab;
    public GameObject availableHookTarget;
    private Vector3 hookDestination;
    public float hookDistance;
    private float searchStep = 0.1f;
    private float overlapThreshold = 1.0f;
    public bool hookJumpActivated;

    // Swing
    [Header("Swinging Settings")]
    public LayerMask webtargets;
    public Vector3 sideForce;
    private bool isSwinging;
    public float sidePushPower;
    public float ropeDistance;
    public float dampening = 0.01f;
    //[SerializeField] private Vector3 swingVelocity;

    public float maxDistance;
    public float maxSwingSpeed;
    public float swingSpeed;


    [Tooltip("Height value when the rope breaks")]
    public float ropeBreakHeight;
    private GameObject anker;
    [Header("Best Web Target Settings")]
    public float futureSteps = 0;
    public Vector3 WebTargetSearchBoxDimensions = new Vector3(3, 1, 3);

    [Min(0)]
    public float minHeightForTarget = 5;

    bool hasHitWebTarget;

    public float WebTargetSearchStep = 1f;
    Vector3 webTargetPosition;
    private Vector3 WebTargetSearchPosition = Vector3.zero;
    private Vector3 webHit;
    private ScaleObjectBetweenTwoPoints scaler;
    // Web Texture
    public GameObject webTexture;

    private static ActionState playerState = ActionState.Idle;

    [Header("Cosmetics")]
    public bool displayGizmos;
    public bool visualizeDiveJumpCheck;
    public float windChimeThreshold = 20f;
    public bool fastSpeed;

    bool isLanding;
    GameObject cinemachienCamera;
    private ParticleSystem windChimeParticles;
    enum ActionState
    {
        Idle,
        Running,
        Swinging,
        Falling,
        Hooking,
        Jumping,
        DiveJumping,

    }

    private void Awake()
    {

    }
    void Start()
    {
        cinemachienCamera = GameObject.Find("Third Person Camera");
        windChimeParticles = GameObject.FindGameObjectWithTag("WindSpeedParticleSystem").GetComponent<ParticleSystem>();
        Cursor.lockState = CursorLockMode.Locked;

        anker = new GameObject("Anker!");

        webTexture = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        webTexture.name = "WebString";
        webTexture.transform.localScale = new Vector3(0.013f, 1.5f, 0.013f);
        webTexture.GetComponent<CapsuleCollider>().enabled = false;
        webTexture.GetComponent<MeshRenderer>().materials = new Material[] { webMaterial };
        webTexture.transform.position = handPositionSwinging.position;
        webTexture.SetActive(false);
        webTexture.GetComponent<MeshRenderer>().materials[0].SetVector("Web_Distance_Tiling", new Vector2(2, webTexture.transform.localScale.y));
        //GetComponent<ScaleObjectBetweenTwoPoints>().scalingTransform = webTexture.transform;
        scaler = new ScaleObjectBetweenTwoPoints(handPositionSwinging, null, webTexture.transform);

    }

    public void StartSwinging()
    {
        hasHitWebTarget = true;
        sideForce = Vector3.zero;
        anker.transform.position = webHit;
        anker.transform.rotation = transform.rotation;
        scaler.SetEndPoint(anker.transform);
        //GetComponent<ScaleObjectBetweenTwoPoints>().end = anker.transform;
        ropeDistance = Vector3.Distance(transform.position, webHit);
        isSwinging = true;
        _canDoubleJump = false;
        // webTexture.SetActive(true);
        scaler.isActive = true;
        webTexture.GetComponent<MeshRenderer>().materials[0].SetVector("Web_Distance_Tiling", new Vector2(2, webTexture.transform.localScale.y));
        //Debug.Log("Current Pos: " + transform.position + ", Old Pos: " + oldPos + ", Velcoity: " + ((transform.position - oldPos) / Time.deltaTime));


    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {

            cinemachienCamera.SetActive(!cinemachienCamera.activeSelf);
        }

        if (new Vector3(velocity.x, Mathf.Min(0, velocity.y), velocity.z).magnitude >= windChimeThreshold)
        {
            windChimeParticles.Play();
            fastSpeed = true;
        }
        else
        {
            windChimeParticles.Stop();
            fastSpeed = false;
        }


        if (!isWebHooking)
            LookForHookTargets();
        if (Input.GetKeyDown(KeyCode.Mouse2) && availableHookTarget)
        {
            //FindHookTarget();
            isWebHooking = true;
            animator.SetTrigger("WebHooking");
            animator.ResetTrigger("DiveJump");
            animator.ResetTrigger("Landing");
            scaler.SetEndPoint(availableHookTarget.transform);
            webTexture.SetActive(true);
            scaler.isActive = true;
            velocity = Vector3.zero;

        }

        if (!isWebHooking)
        {

            _isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
            // Physics.CheckCapsule

            if (_isGrounded && velocity.y < 0)
            {
                // Stop sliding
                velocity = Vector3.zero;
                velocity.y = -2f;
            }

            // Mathf.Abs(velocity.y)
            //ector3 futurePoint = transform.position + new Vector3(velocity.x, 0, velocity.z).normalized * futureSteps;
            if (!isSwinging)
            {
                Vector3 futurePoint = transform.position + new Vector3(velocity.x, 0, velocity.z);

                WebTargetSearchPosition = futurePoint + Vector3.up * maxDistance;
            }

            if (!_isGrounded && !isSwinging && Input.GetKeyDown(KeyCode.Mouse1))
            {

                webHit = FindOptiomalWebTarget();
                if (webHit != Vector3.zero)
                {

                    animator.SetTrigger("StartSwinging");
                    animator.ResetTrigger("DiveJump");
                }



            }

            if (Input.GetKey(KeyCode.Mouse1) && isSwinging)
            {

                SwingAction();
                Debug.DrawLine(transform.position, transform.position + velocity.normalized * 2, Color.blue);

            }
            else
            {
                isSwinging = false;
                webTexture.SetActive(false);
                hasHitWebTarget = false;
                scaler.isActive = false;

            }

            if (!isSwinging)
            {
                //UpdateAnimation();
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
                    SwingJumpAction();
                    isSwinging = false;
                }
                else if (_isGrounded)
                {
                    JumpAction();
                    _canDoubleJump = true;
                }
                else if (_canDoubleJump && !isSwinging)
                {
                    DoubleJumpAction();
                    _canDoubleJump = false;
                }
            }
        }
        else
        {
            HookAction();

        }


        if (_isGrounded)
        {
            animator.SetFloat("GroundSpeed", new Vector3(velocity.x, 0, velocity.z).magnitude);
            isSwinging = false;
            isLanding = false;

        }
        else
        {
            animator.SetFloat("GroundSpeed", 0);
        }


        if (!_isGrounded && !isSwinging)
        {
            animator.SetBool("Falling", true);
            if (CheckForDivingAction())
                animator.SetTrigger("DiveJump");
            else
            {
                animator.ResetTrigger("DiveJump");
            }
        }
        else
        {
            animator.SetBool("Falling", false);
        }
        animator.SetBool("Grounded", _isGrounded);
        animator.SetBool("Swinging", isSwinging);

        if (!isLanding && _isGrounded && animator.GetCurrentAnimatorClipInfo(0)[0].clip.name.Equals("Diving"))
        {
            animator.SetTrigger("Landing");
            animator.ResetTrigger("DiveJump");
            animator.ResetTrigger("StartSwinging");
            isLanding = true;
        }
        else
        {
            animator.ResetTrigger("Landing");
        }



    }

    private bool CheckForDivingAction()
    {
        if (cam.transform.eulerAngles.x > diveJumpAngle && cam.transform.eulerAngles.x < 90 && velocity.magnitude > 34f && !animator.GetCurrentAnimatorClipInfo(0)[0].clip.name.Equals("Diving"))
        {
            return true;
        }
        return false;
    }

    public void LookForHookTargets()
    {
        List<HookMarkerBehaviour> hooks = new List<HookMarkerBehaviour>();
        Collider[] hitCollider = Physics.OverlapSphere(transform.position, hookDistance, hookTargets);

        foreach (Collider hitColl in hitCollider)
        {
            HookMarkerBehaviour hookMarker = hitColl.GetComponent<HookMarkerBehaviour>();
            RaycastHit hit;
            if (Physics.Linecast(transform.position, hookMarker.transform.position, out hit))
            {
                float dotProduct = Vector3.Dot(transform.forward, (hookMarker.transform.position - transform.position).normalized) * 180;
                //Debug.Log(transform.forward + "____" + (hookMarker.transform.position - transform.position).normalized + "___" + dotProduct);
                if (hit.collider.tag != "HookMarker" || Vector3.Distance(hookMarker.transform.position, transform.position) < minHookDistance || dotProduct < detectionAngle)
                {
                    // hooks.Remove(hookMarker);
                    hookMarker.Deactivate();
                }
                else
                {

                    hooks.Add(hookMarker);
                    hookMarker.Activate();
                }
            }
        }

        if (hooks.Count > 0)
        {
            HookMarkerBehaviour closestHook = hooks[0];
            for (int i = 0; i < hooks.Count; i++)
            {
                float distanceToPlayer = Vector3.Distance(hooks[i].gameObject.transform.position, transform.position);
                if (distanceToPlayer < (Vector3.Distance(closestHook.transform.position, transform.position)))
                {
                    closestHook = hooks[i];
                }
            }
            availableHookTarget = closestHook.gameObject;
            closestHook.HighlightMarker();
            hookDestination = closestHook.destination.position;
        }

        else
        {
            availableHookTarget = null;
        }

    }

    private void HookAction()
    {
        if (availableHookTarget != null)
        {
            Vector3 normDrection = (hookDestination - transform.position).normalized;
            float targetAngle = Mathf.Atan2(normDrection.x, normDrection.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, targetAngle, 0);
            controller.Move((hookDestination - transform.position).normalized * hookSpeed * Time.deltaTime);
            //transform.position = Vector3.Lerp(transform.position, targetPosition, hookSpeed * Time.deltaTime);
            //transform.position += (hookDestination - transform.position).normalized * hookSpeed * Time.deltaTime;

            if (Input.GetButtonDown("Jump") && Vector3.Distance(transform.position, hookDestination) < 8f)
            {
                hookJumpActivated = true;
            }

            if (Vector3.Distance(transform.position, hookDestination) <= 0.4f)
            {
                //playerState = ActionState.Idle;
                _isGrounded = true;
                availableHookTarget = null;
                isWebHooking = false;
                scaler.isActive = false;
                webTexture.SetActive(false);
                if (hookJumpActivated)
                {
                    velocity.y += Mathf.Sqrt(jumpPower * 4f * gravity);
                    velocity += transform.forward * velocity.magnitude * 1.2f;
                    hookJumpActivated = false;
                }
            }
        }
    }

    private void FindHookTarget()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, hookDistance, webtargets);
        foreach (var hitCollider in hitColliders)
        {
            // Find closest point
            Vector3 closestPoint = hitCollider.ClosestPointOnBounds(transform.position);
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = closestPoint;
            sphere.GetComponent<SphereCollider>().enabled = false;
            GameObject targetGameObject = hitCollider.gameObject;
            // Start raycasting
            float searchDelta = 0.0f;
            float distanceDifferenceSinceLastRaycast = Vector3.Distance(transform.position, closestPoint);
            overlapThreshold = 1.0f;
            //bool edgeFound = false;
            Vector3 lastPosition = closestPoint;
            while (searchDelta <= 30f)
            {
                RaycastHit hit;
                // Does the ray intersect any objects excluding the player layer
                if (Physics.Raycast(transform.position, ((closestPoint + Vector3.up * searchDelta) - transform.position).normalized, out hit, Mathf.Infinity))
                {

                    bool isInRange = hit.distance <= hookDistance;
                    if (!isInRange)
                    {
                        return;
                    }
                    // Maybe not neccessary
                    bool hitsSameGameObject = hit.transform.gameObject == targetGameObject;
                    bool didOverstep = hit.distance > (distanceDifferenceSinceLastRaycast + overlapThreshold);
                    if (isInRange && didOverstep)
                    {
                        Vector3 trueHit = lastPosition;
                        //edgeFound = true;
                        GameObject hitSphere = Instantiate(edgeMarkerPrefab, trueHit, Quaternion.identity);
                        hitSphere.GetComponent<SphereCollider>().enabled = false;
                        break;


                    }
                    else
                    {
                        lastPosition = hit.point;
                    }


                    distanceDifferenceSinceLastRaycast = hit.distance;

                }

                //Debug.DrawRay(transform.position, hit.point, Color.yellow);
                searchDelta += searchStep;

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
            //Vector3 closestPoint = currentCollider.ClosestPoint(WebTargetSearchPosition);
            //Vector3 closestPoint = Physics.ClosestPoint(WebTargetSearchPosition, allHits[i], allHits[i].transform.position, Quaternion.identity);

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
            if (visualizeDiveJumpCheck)
            {


                Gizmos.DrawRay(transform.position + transform.forward * 10f, Vector3.down * 20);

            }

            if (hasHitWebTarget)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(WebTargetSearchPosition, Vector3.down * (maxDistance - minHeightForTarget));
                //Gizmos.DrawSphere(WebTargetSearchPosition + Vector3.down * webHit.distance + Vector3.down * WebTargetSphereRadius, WebTargetSphereRadius);
                //Gizmos.DrawWireSphere(WebTargetSearchPosition + Vector3.down * webHit.distance, WebTargetSphereRadius);
                //Gizmos.DrawWireCube(new Vector3(WebTargetSearchPosition.x, , WebTargetSearchPosition.z)  WebTargetSearchPosition + Vector3.down * Vector3.Distance(webTargetPosition, WebTargetSearchPosition), WebTargetSearchBoxDimensions);
                Gizmos.DrawWireCube(new Vector3(WebTargetSearchPosition.x, webTargetPosition.y, WebTargetSearchPosition.z), WebTargetSearchBoxDimensions);
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(WebTargetSearchPosition, Vector3.down * (maxDistance - minHeightForTarget));
                Gizmos.DrawCube(WebTargetSearchPosition, WebTargetSearchBoxDimensions);
                // Gizmos.DrawSphere(WebTargetSearchPosition, WebTargetSphereRadius);
                //Gizmos.DrawWireSphere(WebTargetSearchPosition, WebTargetSphereRadius);
            }


        }

    }
    private void LateUpdate()
    {
        animator.SetFloat("Speed", velocity.magnitude);
        if (scaler.isActive)
            scaler.StretchObject();


    }

    void SwingJumpAction()
    {

        animator.SetTrigger("BackFlip");
        velocity.y += Mathf.Sqrt(jumpPower * 4f * gravity);
        velocity += velocity.normalized * 10;
    }
    void SwingAction()
    {


        // Check if the rope should break
        if (transform.position.y >= anker.transform.position.y + ropeBreakHeight)
        {
            isSwinging = false;
        }
        else
        {

            velocity = ((transform.position - oldPos) / Time.deltaTime);
            oldPos = transform.position;
            velocity += gravityDirection * gravity * Time.deltaTime;
            ApplyDrag();

            // velocity += velocity.normalized * Time.deltaTime * 5;
            //velocity += velocity.normalized * Time.deltaTime * swingSpeed * 0.9f;
            // velocity += cam.transform.forward * Time.deltaTime * swingSpeed;
            if (Input.GetKey(KeyCode.W))
            {
                //velocity += cam.transform.forward * Time.deltaTime * swingSpeed;
                velocity += new Vector3(cam.forward.x, velocity.normalized.y, cam.forward.z) * Time.deltaTime * swingSpeed;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                velocity -= cam.transform.forward * Time.deltaTime * swingSpeed;
                //velocity += cam.transform.forward * Time.deltaTime * swingSpeed; // Find direction between velocity and cam forward to apply force 
            }
            if (Input.GetKey(KeyCode.A))
            {
                velocity += -cam.transform.right * Time.deltaTime * swingSpeed * 1.5f;
            }
            if (Input.GetKey(KeyCode.D))
            {
                velocity += cam.transform.right * Time.deltaTime * swingSpeed * 1.5f;
            }

            ApplySideForce();

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

            transform.rotation = Quaternion.Euler(0, cam.rotation.eulerAngles.y, 0);

        }




    }
    private void ApplySideForce()
    {

        RaycastHit hit;
        float rDistance = 0f;
        float lDistance = 0f;

        if (Physics.Raycast(transform.position, transform.right, out hit, 60f))
        {
            //pointRight = hit.point;
            rDistance = hit.distance;
        }
        if (Physics.Raycast(transform.position, -transform.right, out hit, 60f))
        {
            // pointLeft = hit.point;
            lDistance = hit.distance;
        }
        Debug.DrawLine(transform.position, transform.position + transform.right * 2, Color.red);
        Debug.DrawLine(transform.position, transform.position + -transform.right * 2, Color.green);
        if (velocity.magnitude > 0 && rDistance < lDistance)
        {
            sideForce = Vector3.Lerp(sideForce, transform.right * sidePushPower, Time.deltaTime);

        }
        else if (velocity.magnitude > 0)
        {
            sideForce = Vector3.Lerp(sideForce, -transform.right * sidePushPower, Time.deltaTime);
        }
        if (rDistance == 0 && lDistance == 0)
        {
            sideForce = Vector3.Lerp(sideForce, Vector3.zero, Time.deltaTime);
        }
        velocity += sideForce * Time.deltaTime;

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
        // if (!animator.GetBool("Landing"))
        // {
        //     float speedDownwards = velocity.y;
        //     RaycastHit hit;
        //     if (Physics.Raycast(transform.position, Vector3.down, out hit, 5f))
        //     {
        //         if (hit.distance < speedDownwards)
        //         {
        //             animator.SetBool("Landing", true);
        //         }
        //     }


        // }
    }
    void JumpAction()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + transform.forward * 10f, Vector3.down, out hit, Mathf.Infinity))
        {
            if (hit.distance > minDiveJumpHeight && animator.GetFloat("GroundSpeed") > 0.1f && cam.transform.eulerAngles.x > diveJumpAngle && cam.transform.eulerAngles.x < 90)
            {
                animator.SetTrigger("DiveJump");
            }
        }
        velocity.y = Mathf.Sqrt(jumpPower * 2f * gravity);
    }

    void DoubleJumpAction()
    {
        animator.SetTrigger("DoubleJumpBackFlip");
        //animator.SetBool("Falling", false);
        velocity.y = Mathf.Sqrt(jumpPower * 2f * gravity);
    }


    void WalkAction()
    {

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        if (!isSwinging)
        {
            oldPos = transform.position;
        }

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
