using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    //本脚本旨在让进度条等东西看向摄像机

    private enum Mode
    {
        LookAt,
        LookAtInverted,
        CameraForward,
        CameraForwardInverted,
    }
    [SerializeField] private Mode mode;
    private void LateUpdate()
    {
        //曾经有人推荐不要使用CameraMain 因为他没有缓存会在场景中遍历寻找摄像机
        //但现在已经被缓存所以使用这个没问题
        switch (mode)
        {
            case Mode.LookAt:
                transform.LookAt(Camera.main.transform);
                break;
            case Mode.LookAtInverted:
                Vector3 dirFromCamra = transform.position - Camera.main.transform.position;
                transform.LookAt(transform.position + dirFromCamra);
                break;
            case Mode.CameraForward:
                transform.forward  = Camera.main.transform.forward;
                break;
            case Mode.CameraForwardInverted:
                transform.forward = -Camera.main.transform.forward;
                break;
        }
       
    }
}
