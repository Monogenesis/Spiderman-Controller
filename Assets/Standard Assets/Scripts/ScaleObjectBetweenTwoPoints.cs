using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleObjectBetweenTwoPoints
{
    private Transform start;
    private Transform scalingTransform;
    private Transform end;
    public bool isActive;
    public ScaleObjectBetweenTwoPoints(Transform start, Transform end, Transform scalingTransform)
    {
        this.start = start;
        this.end = end;
        this.scalingTransform = scalingTransform;
    }
    public void SetStartPoint(Transform startPoint)
    {
        start = startPoint;
    }
    public void SetEndPoint(Transform endPoint)
    {
        end = endPoint;
    }
    public void StretchObject()
    {
        if (isActive && scalingTransform && start && end)
        {
            float dist = (start.transform.position - end.transform.position).magnitude;
            scalingTransform.transform.localScale = new Vector3(0.1f, dist * 0.5f, 0.1f);
            scalingTransform.transform.position = (start.transform.position + end.transform.position) * 0.5f;
            scalingTransform.transform.rotation = Quaternion.LookRotation((start.transform.position - end.transform.position), Vector3.up);
            Vector3 rotation = scalingTransform.transform.rotation.eulerAngles;
            rotation.x += 90f;
            scalingTransform.transform.rotation = Quaternion.Euler(rotation);
        }
    }
}
