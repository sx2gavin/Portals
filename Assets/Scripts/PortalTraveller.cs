using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTraveller : MonoBehaviour
{
    public Vector3 LastFramePosition;

    private void LateUpdate()
    {
        LastFramePosition = transform.position;
    }
}
