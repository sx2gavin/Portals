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
    private List<PortalTraveller> lstPortalTravellers = new List<PortalTraveller>();
    private Dictionary<PortalTraveller, GameObject> travellerCopies = new Dictionary<PortalTraveller, GameObject>();

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
                    portalPosition.z = -portalPosition.z;
                    target.PortalAdjustment(portalPosition, portalDisplay.transform.localScale);

                    TransformToTarget(playerTraveller.transform);
                }
            }

            foreach (PortalTraveller traveller in lstPortalTravellers)
            {
                GameObject clone = null;
                if (travellerCopies.ContainsKey(traveller))
                {
                    clone = travellerCopies[traveller];
                    TransformToTarget(traveller.transform, out Vector3 position, out Quaternion rotation);
                    clone.transform.position = position;
                    clone.transform.rotation = rotation;
                }
            }
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
        if (portalTraveller != null && portalTraveller.enabled)
        {
            if (other.CompareTag("Player"))
            {
                playerTraveller = portalTraveller;
                var playerVecFromPortal = other.transform.position - transform.position;
                enteredFromBack = Vector3.Dot(playerVecFromPortal, transform.forward) < 0;
                var localPosition = portalDisplay.transform.localPosition;
                var portalAdjustment = (enteredFromBack ? 1 : -1) * (portalDisplayExpandFactor * 0.1f / 2f);

                var portalScale = new Vector3(portalDisplayExpandFactor, originalPortalScale.y, originalPortalScale.z);
                var portalPosition = new Vector3(localPosition.x, localPosition.y, portalAdjustment);

                PortalAdjustment(portalPosition, portalScale);
            }
            else
            {
                CreateTravellerClone(portalTraveller);
            }
        }
    }

    private void CreateTravellerClone(PortalTraveller portalTraveller)
    {
        lstPortalTravellers.Add(portalTraveller);
        TransformToTarget(portalTraveller.transform, out Vector3 newPos, out Quaternion newRot);
        PortalTraveller travellerCopy = Instantiate(portalTraveller, newPos, newRot);

        travellerCopy.enabled = false;

        SimpleMove simpleMoveComponent = travellerCopy.GetComponent<SimpleMove>();
        if (simpleMoveComponent)
        {
            simpleMoveComponent.enabled = false;
        }

        Sliceable slicable = portalTraveller.GetComponent<Sliceable>();
        // slicable.Slice = gameObject;

        slicable = travellerCopy.GetComponent<Sliceable>();
        // slicable.Slice = target;

        travellerCopies.Add(portalTraveller, travellerCopy.gameObject);
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
                TransformToTarget(portalTraveller.transform);
                lstPortalTravellers.Remove(portalTraveller);
                DestroyTravellerCopy(portalTraveller);
                travellerCopies.Remove(portalTraveller);
            }
        }
    }

    public void ReceivePlayer(PortalTraveller traveller)
    {
        traveller.transform.position = portalRenderCamera.transform.position;
        Camera.main.transform.rotation = portalRenderCamera.transform.rotation;
    }

    private bool CheckTravellerPassPortal(PortalTraveller traveller)
    {
        Vector3 forward = transform.forward;
        Vector3 previousVec = traveller.LastFramePosition - transform.position;
        Vector3 currentVec = traveller.transform.position - transform.position;
        float previousSign = Mathf.Sign(Vector3.Dot(previousVec, forward));
        float currentSign = Mathf.Sign(Vector3.Dot(currentVec, forward));
        return previousSign + currentSign == 0;
    }

    private void TransformToTarget(Transform otherTransform)
    {
        if (target != null)
        {
            Vector4 position = otherTransform.position;
            position.w = 1.0f;
            var newPosition = target.gameObject.transform.localToWorldMatrix * transform.worldToLocalMatrix * position;
            otherTransform.position = newPosition;

            Quaternion rotation = otherTransform.rotation;
            var newRotation = target.gameObject.transform.rotation * Quaternion.Inverse(transform.rotation) * rotation;
            otherTransform.rotation = newRotation;
        }
    }

    private void TransformToTarget(Transform otherTransform, out Vector3 position, out Quaternion rotation)
    {
        position = Vector3.zero;
        rotation = Quaternion.identity;

        if (target != null)
        {
            Vector4 originalPosition = otherTransform.position;
            originalPosition.w = 1.0f;
            position = target.gameObject.transform.localToWorldMatrix * transform.worldToLocalMatrix * originalPosition;

            Quaternion originalRotation = otherTransform.rotation;
            rotation = target.gameObject.transform.rotation * Quaternion.Inverse(transform.rotation) * originalRotation;
        }
    }

    private void DestroyTravellerCopy(PortalTraveller traveller)
    {
        if (travellerCopies != null && travellerCopies.ContainsKey(traveller))
        {
            GameObject clone = travellerCopies[traveller];
            if (clone != null)
            {
                Destroy(clone);
            }
        }
    }

}
