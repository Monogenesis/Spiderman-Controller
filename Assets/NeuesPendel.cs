using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuesPendel : MonoBehaviour
{
    public float gravity = 1f;
    public float dampening = 0.05f;
    [SerializeField] private Vector3 velocity;
    public Transform anker;
    public Vector3 gravityDirection = new Vector3(0, -1, 0);
    public float distance = 3;
    private Vector3 prevPos;
    private void Awake()
    {
        prevPos = transform.position;
    }

    void Update()
    {
        // Steering horizontal transform and Vector3 forward 
        Vector3 steeringMotion = transform.right * Input.GetAxis("Horizontal") + Vector3.forward * Input.GetAxis("Vertical");
        // Calculate the velocity
        velocity = (transform.position - prevPos);
        // Old pos is not current pos
        prevPos = transform.position;
        // Add Forces: steering, velocity, gravity, dampening
        transform.position += steeringMotion * Time.deltaTime;
        transform.position += velocity;
        transform.position += gravityDirection * gravity * Time.deltaTime;
        transform.position += -velocity.normalized * dampening * Time.deltaTime;
        // Calculate the new distance from the anker
        float dist = (transform.position - anker.transform.position).magnitude;
        if (dist > distance)
        {
            // Constrain to the max distance allowed
            Vector3 constrainedPos = Vector3.Normalize(transform.localPosition - anker.localPosition);
            transform.position = anker.localPosition + constrainedPos * distance;
        }
        // Make transform look at new position to make steering more realistic
        transform.LookAt(transform.position - prevPos);
    }
}
