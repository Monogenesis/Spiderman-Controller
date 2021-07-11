using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShakeController : MonoBehaviour
{

    public ThirdPersonMovement player;
    private CinemachineFreeLook freeLook;
    private CinemachineBasicMultiChannelPerlin cameraShakeTopRig;
    private CinemachineBasicMultiChannelPerlin cameraShakeMiddleRig;
    private CinemachineBasicMultiChannelPerlin cameraShakeBottomRig;

    float cameraShakePower = 0;
    public float maxShakePower = 5;
    public float step = 1;
    private void Awake()
    {
        freeLook = GetComponent<CinemachineFreeLook>();
        cameraShakeTopRig = freeLook.GetRig(0).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        cameraShakeMiddleRig = freeLook.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        cameraShakeBottomRig = freeLook.GetRig(2).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

    }

    // Update is called once per frame
    void Update()
    {
        if (player.fastSpeed)
        {
            cameraShakePower = Mathf.Lerp(cameraShakePower, maxShakePower, step * Time.deltaTime);
            cameraShakeTopRig.m_FrequencyGain = cameraShakePower;
            cameraShakeMiddleRig.m_FrequencyGain = cameraShakePower;
            cameraShakeBottomRig.m_FrequencyGain = cameraShakePower;

        }
        else
        {
            cameraShakePower = Mathf.Lerp(cameraShakePower, 0, Time.deltaTime);
            cameraShakeTopRig.m_FrequencyGain = cameraShakePower;
            cameraShakeMiddleRig.m_FrequencyGain = cameraShakePower;
            cameraShakeBottomRig.m_FrequencyGain = cameraShakePower;
        }
    }
}
