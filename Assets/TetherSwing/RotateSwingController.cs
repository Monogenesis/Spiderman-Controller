using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSwingController : MonoBehaviour
{
    public CharacterController controller;
    public float speed;

    void Start()
    {
        
    }

   
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);
    }
}
