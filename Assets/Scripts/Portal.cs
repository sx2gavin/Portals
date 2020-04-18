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

    private PortalTraveller playerTraveller;
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
            
            if (playerTraveller != null)
            {
                if (CheckTravellerPassPortal(playerTraveller))
                {
                    // pre-render target camera before player is teleported to smooth the transition.
                    SetPortalCameraLocalTransform(cameraLocalPosition, cameraLocalRotation);
                    portalRenderCamera.Render();

                    var portalPosition = portalDisplay.transform.localPosition;
                    portalPosition.x = -portalPosition.x;
                    target.PortalAdjustment(portalPosition, portalDisplay.transform.localScale);

                    TeleportTravellerToTarget(playerTraveller);
                }
            }

            foreach (PortalTraveller traveller in lstPortalTravellers)
            {
                if (CheckTravellerPassPortal(traveller))
                {
                    TeleportTravellerToTarget(traveller);
                }
            }
        }
    }

    private bool CheckTravellerPassPortal(PortalTraveller traveller)
    {
        Vector3 forward = transform.right;
        Vector3 previousVec = traveller.LastFramePosition - transform.position;
        Vector3 currentVec = traveller.transform.position - transform.position;
        float previousSign = Mathf.Sign(Vector3.Dot(previousVec, forward));
        float currentSign = Mathf.Sign(Vector3.Dot(currentVec, forward));
        return previousSign + currentSign == 0;
    }

    private void TeleportTravellerToTarget(PortalTraveller traveller)
    {
        if (target != null)
        {
            Vector4 position = traveller.gameObject.transform.position;
            position.w = 1.0f;
            var newPosition = target.gameObject.transform.localToWorldMatrix * transform.worldToLocalMatrix * position;
            traveller.gameObject.transform.position = newPosition;

            Quaternion rotation = traveller.gameObject.transform.rotation;
            var newRotation = target.gameObject.transform.rotation * Quaternion.Inverse(transform.rotation) * rotation;
            traveller.gameObject.transform.rotation = newRotation;
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
        PortalTraveller portalTraveller = other.GetComponent<PortalTraveller>();
        if (portalTraveller != null)
        {
            if (other.CompareTag("Player"))
            {
                playerTraveller = portalTraveller;
                var playerVecFromPortal = other.transform.position - transform.position;
                enteredFromBack = Vector3.Dot(playerVecFromPortal, transform.right) < 0;
                var localPosition = portalDisplay.transform.localPosition;
                var portalAdjustment = (enteredFromBack ? 1 : -1) * (portalDisplayExpandFactor * 0.1f / 2f);

                var portalScale = new Vector3(portalDisplayExpandFactor, originalPortalScale.y, originalPortalScale.z);
                var portalPosition = new Vector3(portalAdjustment, localPosition.y, localPosition.z);

                PortalAdjustment(portalPosition, portalScale);
            }
            else
            {
                lstPortalTravellers.Add(portalTraveller);
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
            if (other.CompareTag("Player"))
            {
                playerTraveller = null;
                var originalPosition = new Vector3(0, portalDisplay.transform.localPosition.y, 0);

                PortalAdjustment(originalPosition, originalPortalScale);
                target.PortalAdjustment(originalPosition, originalPortalScale);
            }
            else
            {
                lstPortalTravellers.Remove(portalTraveller);
            }
        }
    }

    public void ReceivePlayer(PortalTraveller traveller)
    {
        traveller.transform.position = portalRenderCamera.transform.position;
        Camera.main.transform.rotation = portalRenderCamera.transform.rotation;
    }

}
