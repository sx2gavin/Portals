using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTraveler : MonoBehaviour
{
    [HideInInspector] public Vector3 LastFramePosition;

    private void LateUpdate()
    {
        LastFramePosition = transform.position;
    }
}
