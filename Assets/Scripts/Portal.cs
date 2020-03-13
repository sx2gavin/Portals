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
    private bool enteredFromBack;

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
            var playerVecFromPortal = other.transform.position - transform.position;
            enteredFromBack = Vector3.Dot(playerVecFromPortal, transform.right) < 0;
            portalDisplay.transform.localScale = new Vector3(portalDisplayExpandFactor, originalPortalScale.y, originalPortalScale.z);
            var localPosition = portalDisplay.transform.localPosition;
            var portalAdjustment = (enteredFromBack ? 1 : -1) * portalDisplayExpandFactor * 0.1f / 2f;
            portalDisplay.transform.localPosition = new Vector3(portalAdjustment, localPosition.y, localPosition.z);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            var playerVecFromPortal = other.transform.position - transform.position;
            bool exitFromFront = Vector3.Dot(playerVecFromPortal, transform.right) > 0;
            if (enteredFromBack == exitFromFront)
            {
                target.TeleportPlayer(other.gameObject);
            }
            portalDisplay.transform.localScale = originalPortalScale;
            portalDisplay.transform.localPosition = new Vector3(0, portalDisplay.transform.localPosition.y, 0);
        }
    }

    public void TeleportPlayer(GameObject player)
    {
        player.transform.position = portalRenderCamera.transform.position;
        Camera.main.transform.rotation = portalRenderCamera.transform.rotation;
    }
}
