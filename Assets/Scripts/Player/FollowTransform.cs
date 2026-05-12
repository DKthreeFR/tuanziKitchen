using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    //要跟随的目标
    private Transform targetTransfom;

    public void SetTargetTransfome(Transform targetTransform)
    {
        this.targetTransfom = targetTransform;
    }
    private void LateUpdate()
    {
        if(targetTransfom == null)
        {
            return;
        }
        transform.position = targetTransfom.position;
        transform.rotation = targetTransfom.rotation;
    }
}
