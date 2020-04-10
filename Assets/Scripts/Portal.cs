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

    private PortalTraveller traveller;
    // private Vector3 previousTravellerPosition;
    private List<PortalTraveller> lstPortalTravellers = new List<PortalTraveller>();

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
            if (traveller != null)
            {
                Vector3 forward = transform.right;
                Vector3 previousVec = traveller.LastFramePosition - transform.position;
                Vector3 currentVec = traveller.transform.position - transform.position;
                float previousSign = Mathf.Sign(Vector3.Dot(previousVec, forward));
                float currentSign = Mathf.Sign(Vector3.Dot(currentVec, forward));
                if (previousSign + currentSign == 0)
                {
                    TeleportTraveller(cameraLocalPosition, cameraLocalRotation);
                }
            }
        }
    }

    private void TeleportTraveller(Vector3 cameraLocalPosition, Quaternion cameraLocalRotation)
    {
        SetPortalCameraLocalTransform(cameraLocalPosition, cameraLocalRotation);
        portalRenderCamera.Render();

        var portalPosition = portalDisplay.transform.localPosition;
        portalPosition.x = -portalPosition.x;
        target.PortalAdjustment(portalPosition, portalDisplay.transform.localScale);

        Debug.Log("Teleported.");
        target.ReceivePlayer(traveller);
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
        PortalTraveller portalTraveller = other.GetComponent<PortalTraveller>();
        if (portalTraveller != null)
        {
            lstPortalTravellers.Add(portalTraveller);
            if (other.CompareTag("Player"))
            {
                traveller = portalTraveller;
                var playerVecFromPortal = other.transform.position - transform.position;
                enteredFromBack = Vector3.Dot(playerVecFromPortal, transform.right) < 0;
                var localPosition = portalDisplay.transform.localPosition;
                var portalAdjustment = (enteredFromBack ? 1 : -1) * (portalDisplayExpandFactor * 0.1f / 2f);

                var portalScale = new Vector3(portalDisplayExpandFactor, originalPortalScale.y, originalPortalScale.z);
                var portalPosition = new Vector3(portalAdjustment, localPosition.y, localPosition.z);

                PortalAdjustment(portalPosition, portalScale);

            }
        }
    }

    public void PortalAdjustment(Vector3 localPosition, Vector3 localScale)
    {
        portalDisplay.transform.localScale = localScale;
        portalDisplay.transform.localPosition = localPosition;
    }

    public void OnTriggerExit(Collider other)
    {
        PortalTraveller portalTraveller = other.GetComponent<PortalTraveller>();
        if (portalTraveller != null)
        {
            traveller = null;
            var originalPosition = new Vector3(0, portalDisplay.transform.localPosition.y, 0);

            PortalAdjustment(originalPosition, originalPortalScale);
            target.PortalAdjustment(originalPosition, originalPortalScale);
        }
    }

    public void ReceivePlayer(PortalTraveller traveller)
    {
        traveller.transform.position = portalRenderCamera.transform.position;
        Camera.main.transform.rotation = portalRenderCamera.transform.rotation;
    }
}
