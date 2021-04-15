using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Pendulum 
{
    public Transform bob_tr;
    public Tether tether;
    public Arm arm;
    public Bob bob;

    Vector3 previousPos;

    public void Initialise()
    {
        bob_tr.transform.parent = tether.tether_tr;
        arm.length = Vector3.Distance(bob_tr.transform.localPosition, tether.position);

    }

    public Vector3 MoveBob(Vector3 pos, float time)
    {
        bob.velocity += GetConstrainedVelocity(pos, previousPos, time);
        bob.ApplyGravity();
        bob.ApplyDamping();
        bob.CapMaxSpeed();
        pos += bob.velocity * time;
        if (Vector3.Distance(pos, tether.position) < arm.length)
        {
            pos = Vector3.Normalize(pos - tether.position) * arm.length;
            arm.length = Vector3.Distance(pos, tether.position);
        }
  
        previousPos = pos;
        return pos;
    }

    public Vector3 MoveBob(Vector3 pos, Vector3 prevPos, float time)
    {
        bob.velocity += GetConstrainedVelocity(pos, prevPos, time);
        bob.ApplyGravity();
        bob.ApplyDamping();
        bob.CapMaxSpeed();
        pos += bob.velocity * time;
        if (Vector3.Distance(pos, tether.position) < arm.length)
        {
            pos = Vector3.Normalize(pos - tether.position) * arm.length;
            arm.length = Vector3.Distance(pos, tether.position);
        }

        previousPos = pos;
        return pos;
    }

    public Vector3 GetConstrainedVelocity(Vector3 currentPos, Vector3 previousPos, float time)
    {
        float distanceToTether;
        Vector3 constrainedPos;
        Vector3 predictedPos;
        distanceToTether = Vector3.Distance(currentPos, tether.position);

        if(distanceToTether > arm.length)
        {
            constrainedPos = Vector3.Normalize(currentPos - tether.position) * arm.length;
            predictedPos = (constrainedPos - previousPos) / (time);
            return predictedPos;
        }

        return Vector3.zero;
    }

    public void SwitchTether(Vector3 newPosition)
    {
        bob_tr.transform.parent = null;
        tether.tether_tr.position = newPosition;
        bob_tr.transform.parent = tether.tether_tr;
        tether.position = tether.tether_tr.InverseTransformPoint(newPosition);
        arm.length = Vector3.Distance(bob_tr.transform.localPosition, tether.position);
    }

    public Vector3 Fall(Vector3 pos, float time)
    {
        bob.ApplyGravity();
        bob.ApplyDamping();
        bob.CapMaxSpeed();

        pos += bob.velocity * time;
        return pos;
    }

}
