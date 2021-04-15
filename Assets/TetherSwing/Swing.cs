using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swing : MonoBehaviour
{
    [SerializeField]
    public Pendulum pendulum;

    public Transform newTether;

    void Start()
    {
        pendulum.Initialise();
    }

   
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            pendulum.SwitchTether(newTether.transform.position);
        }



    }

    private void FixedUpdate()
    {
        transform.localPosition = pendulum.MoveBob(transform.localPosition, Time.deltaTime);
    }

}
