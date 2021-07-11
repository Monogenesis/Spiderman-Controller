using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointRing : MonoBehaviour
{

    private static List<PointRing> allRings = new List<PointRing>();
    private static int currentRingNumber = 0;
    public int ringNr = 0;

    private void Awake()
    {
        allRings.Add(this);
        if (currentRingNumber == ringNr)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    public void Activate()
    {
        gameObject.SetActive(true);
    }


    private void OnTriggerEnter(Collider other)
    {
        hitPointRing();
    }

    public PointRing FindRing(int currentNr)
    {
        foreach (PointRing ring in allRings)
        {
            if (ring.ringNr == currentNr)
            {
                return ring;
            }
        }
        return null;
    }

    public void hitPointRing()
    {
        gameObject.SetActive(false);
        currentRingNumber++;
        currentRingNumber = currentRingNumber % allRings.Count;
        FindRing(currentRingNumber).gameObject.SetActive(true);
    }
}
