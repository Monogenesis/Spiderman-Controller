using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAroundSwing : MonoBehaviour
{
    public float speed;
    public GameObject webTarget;
    public Vector3 webTargetPosition;
    public float webDistance;
    public LayerMask grapplingTargets;
    public Camera cam;
    public GameObject player;

    private bool _isSwinging;
    float lastYPos;
    public LineRenderer lineRenderer;

    void Start()
    {
        
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        Material lineMaterial = new Material(Shader.Find("Standard"));
        lineMaterial.color = new Color(0, 0, 0, 1);
        lineRenderer.material = lineMaterial;
    }

   
    void Update()
    { 
        if (Input.GetKeyDown(KeyCode.K))
        {
            RaycastHit hit;
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, webDistance, grapplingTargets))
            {
                Destroy(webTarget);
                webTargetPosition = hit.point;
                _isSwinging = true;
                webTarget = new GameObject("target");
                webTarget.transform.position = hit.point;
                webTarget.transform.rotation = cam.transform.rotation;


            }
            else
            {
                webTargetPosition = Vector3.zero;
                _isSwinging = false;
             
            }
        }
        if (_isSwinging)
        {
            lineRenderer.SetPositions(new Vector3[] { transform.position, webTargetPosition });
            //transform.RotateAround(webTarget.transform.position, webTarget.transform.right, speed * Time.deltaTime);
            if (lastYPos > transform.position.y)
            {
                // Increase the speed
                speed += 1 * Time.deltaTime;
            }
            else if (lastYPos < transform.position.y)
            {
                speed -= 1 * Time.deltaTime;
            }
        }




        lastYPos = transform.position.y;
    }
}
