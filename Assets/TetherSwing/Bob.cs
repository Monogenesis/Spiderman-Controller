using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Bob
{
    public Vector3 velocity;
    public float gravity = 20f;
    public Vector3 gravityDirection = new Vector3(0, 1, 0);
    public float maximumSpeed;

     Vector3 dampingDirection;

    public float drag;
    public void ApplyGravity()
    {
        velocity -= gravityDirection * gravity * Time.deltaTime;
    }

    public void ApplyDamping()
    {
        dampingDirection = -velocity;
        dampingDirection *= drag;
        velocity += dampingDirection * Time.deltaTime;
    }
    
    public void CapMaxSpeed()
    {
        velocity = Vector3.ClampMagnitude(velocity, maximumSpeed);
    }
}
