using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookMarkerBehaviour : MonoBehaviour
{
    public Transform destination;
    public float rotationSpeed = 10.0f;
    private float deactivationRadius;
    private Camera cam;
    private Quaternion targetRoation;
    private Transform master;
    private bool isActivated;
    private float roationAngle;
    private MeshRenderer renderer;
    public Material normal;
    public Material highlighted;
    void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
        master = GameObject.FindGameObjectWithTag("Player").transform;
        cam = Camera.main;
        deactivationRadius = master.GetComponent<ThirdPersonMovement>().hookDistance;
        renderer = GetComponent<MeshRenderer>();
    }



    void Update()
    {
        if (isActivated)
        {
            Vector3 lookAtRotation = Quaternion.LookRotation(cam.transform.position - transform.position).eulerAngles;
            roationAngle += Time.deltaTime;
            transform.rotation = Quaternion.Euler(lookAtRotation);
            transform.RotateAround(transform.position, transform.forward, roationAngle * rotationSpeed);

            if (Vector3.Distance(transform.position, master.transform.position) > deactivationRadius)
            {
                Deactivate();
            }
        }

    }
    public void HighlightMarker()
    {
        renderer.material = highlighted;
    }
    public void Deactivate()
    {
        isActivated = false;
        GetComponent<MeshRenderer>().enabled = false;

    }
    public void Activate()
    {
        renderer.material = normal;
        // Test purpose can be deleted when hookDistance is more fixed
        deactivationRadius = master.GetComponent<ThirdPersonMovement>().hookDistance;
        isActivated = true;
        renderer.enabled = true;
    }

}
