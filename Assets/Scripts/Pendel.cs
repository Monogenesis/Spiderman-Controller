
using UnityEngine;

public class Pendel : MonoBehaviour
{
    public float L = 9.81f;             /*Lenght of the rope*/
    public float g = 0.81f;             /*Gravity force*/

    public float theta0 = (1 / 10) * Mathf.PI;/*Initial angle. Must be different from 0*/
    public float omega0 = 0;                /*Initial angular velocity*/
    public bool _________________;

    public float theta_k;                /*Theta value in step K*/
    public float omega_k;                /*Omega value in step K*/
    public float omega_k1;               /*Omega value in step K+1*/
    public float theta_k1;               /*Theta value in step K+1*/
    public Vector3 p, p0;
    Vector3 v;

    void Awake()
    {
        omega_k1 = omega0;
        theta_k1 = theta0;
        p0 = transform.position;
        p0.y += L;
    }

    void FixedUpdate()
    {
        EulerCromer();
        PolarToCartesian();
        RotateWithMotion(transform);
    }

    /*Implementation of the Euler-Cromer Method*/
    void EulerCromer()
    {
        omega_k = omega_k1;
        theta_k = theta_k1;
        omega_k1 = omega_k - (g / L) * theta_k * Time.deltaTime;
        theta_k1 = theta_k + omega_k1 * Time.deltaTime;
    }

    /*Convert Polar coordinates to Cartesian coordinates*/
    void PolarToCartesian()
    {
        p.z = p0.z + L * Mathf.Sin(theta_k1);
        p.y = p0.y + -L * Mathf.Cos(theta_k1);
        p.x = p0.x;
        transform.position = p;
    }

    float radiansToDegrees(float radians)
    {
        return radians * 180 / Mathf.PI;
    }

    /*Rotate the rope? and the transform with the motion of the pendulum*/
    void RotateWithMotion(Transform t)
    {
        Vector3 rot = t.rotation.eulerAngles;
        rot.x = radiansToDegrees(-theta_k1);
        t.rotation = Quaternion.Euler(rot);
    }

}
