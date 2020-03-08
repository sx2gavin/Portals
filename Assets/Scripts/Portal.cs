using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private Portal target;
    [SerializeField] private PortalCamera portalRenderCamera;
    [SerializeField] private PortalDisplay portalDisplay;
    [SerializeField] private float portalDisplayExpandFactor = 15.0f;

    private Vector3 originalPortalScale;

    // Start is called before the first frame update
    void Start()
    {
        if (target)
        {
            var texture = target.GetPortalCameraRenderTexture();
            SetPortalDisplayTexture(texture);
        }

        originalPortalScale = portalDisplay.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (target)
        {
            Camera mainCamera = Camera.main;
            var cameraLocalPosition = this.transform.InverseTransformPoint(mainCamera.transform.position);
            var cameraLocalRotation = Quaternion.Inverse(transform.rotation) * mainCamera.transform.rotation;

            target.SetPortalCameraLocalTransform(cameraLocalPosition, cameraLocalRotation);
        }
    }

    public void SetPortalCameraLocalTransform(Vector3 localPosition, Quaternion localRotation)
    {
        if (portalRenderCamera)
        {
            portalRenderCamera.transform.localPosition = localPosition;
            portalRenderCamera.transform.localRotation = localRotation;
        }
    }

    public RenderTexture GetPortalCameraRenderTexture()
    {
        return portalRenderCamera.GetRenderTexture();
    }

    public void SetPortalDisplayTexture(RenderTexture texture)
    {
        portalDisplay.SetTexture(texture);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            portalDisplay.transform.localScale = new Vector3(portalDisplayExpandFactor, originalPortalScale.y, originalPortalScale.z);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            target.TeleportPlayer(other.gameObject);
            portalDisplay.transform.localScale = originalPortalScale;
        }
    }

    public void TeleportPlayer(GameObject player)
    {
        player.transform.position = portalRenderCamera.transform.position;
        Camera.main.transform.rotation = portalRenderCamera.transform.rotation;
    }
}
