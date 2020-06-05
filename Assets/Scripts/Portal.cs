using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private Portal target;
    [SerializeField] private PortalCamera targetPortalRenderCamera;
    [SerializeField] private PortalDisplay portalDisplay;
    [SerializeField] private float portalDisplayExpandFactor = 15.0f;

    private Vector3 originalPortalScale;
    private bool enteredFromBack;
    private Bounds displayBounds;

    private PortalTraveler playerTraveler;
    private List<PortalTraveler> lstPortalTravelers = new List<PortalTraveler>();
    private Dictionary<PortalTraveler, GameObject> travelerCopies = new Dictionary<PortalTraveler, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        targetPortalRenderCamera.TextureUpdated += PortalRenderCameraTextureUpdated;
        if (target)
        {
            var texture = targetPortalRenderCamera.GetRenderTexture();
            portalDisplay.SetTexture(texture);
        }

        originalPortalScale = portalDisplay.transform.localScale;

        if (portalDisplay && portalDisplay.GetComponent<MeshRenderer>())
        {
            displayBounds = portalDisplay.GetComponent<MeshRenderer>().bounds;
        }
    }

    private void OnDestroy()
    {
        targetPortalRenderCamera.TextureUpdated -= PortalRenderCameraTextureUpdated;
    }

    // Update is called once per frame
    void Update()
    {
        if (target)
        {
            Camera mainCamera = Camera.main;

            if (GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(mainCamera), displayBounds))
            {
                TransformToTarget(mainCamera.transform, out Vector3 targetPosition, out Quaternion targetRotation);
                ManuallyRenderCamera(targetPosition, targetRotation);
            }
            
            if (playerTraveler != null)
            {
                if (CheckTravelerPassPortal(playerTraveler))
                {
                    // pre-render target camera before player is teleported to smooth the transition.
                    target.ManuallyRenderCamera(mainCamera.transform.position, mainCamera.transform.rotation);

                    var portalPosition = portalDisplay.transform.localPosition;
                    portalPosition.z = -portalPosition.z;
                    target.PortalAdjustment(portalPosition, portalDisplay.transform.localScale);

                    TransformToTarget(playerTraveler.transform);
                }
            }

            foreach (PortalTraveler traveler in lstPortalTravelers)
            {
                GameObject clone = null;
                if (travelerCopies.ContainsKey(traveler))
                {
                    clone = travelerCopies[traveler];
                    TransformToTarget(traveler.transform, out Vector3 position, out Quaternion rotation);
                    clone.transform.position = position;
                    clone.transform.rotation = rotation;
                }
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        PortalTraveler portalTraveler = other.GetComponent<PortalTraveler>();
        if (portalTraveler != null && portalTraveler.enabled)
        {
            var playerVecFromPortal = other.transform.position - transform.position;
            enteredFromBack = Vector3.Dot(playerVecFromPortal, transform.forward) < 0;
            if (other.CompareTag("Player"))
            {
                playerTraveler = portalTraveler;
                var localPosition = portalDisplay.transform.localPosition;
                var portalAdjustment = (enteredFromBack ? 1 : -1) * (portalDisplayExpandFactor * 0.1f / 2f);

                var portalScale = new Vector3(portalDisplayExpandFactor, originalPortalScale.y, originalPortalScale.z);
                var portalPosition = new Vector3(localPosition.x, localPosition.y, portalAdjustment);

                PortalAdjustment(portalPosition, portalScale);
            }
            else
            {
                CreateTravelerClone(portalTraveler, enteredFromBack);
            }
        }
    }

    private void CreateTravelerClone(PortalTraveler portalTraveler, bool enteredFromBack)
    {
        lstPortalTravelers.Add(portalTraveler);
        TransformToTarget(portalTraveler.transform, out Vector3 newPos, out Quaternion newRot);
        PortalTraveler travelerCopy = Instantiate(portalTraveler, newPos, newRot);

        travelerCopy.enabled = false;

        SimpleMove simpleMoveComponent = travelerCopy.GetComponent<SimpleMove>();
        if (simpleMoveComponent)
        {
            simpleMoveComponent.enabled = false;
        }

        Sliceable sliceable = portalTraveler.GetComponent<Sliceable>();
        sliceable.SlicePosition = transform.position;
        sliceable.SliceNormal = transform.forward;
        sliceable.IsSliceable = true;
        sliceable.Flip = !enteredFromBack;
        sliceable.UpdateMaterialSlice();

        if (target)
        {
            sliceable = travelerCopy.GetComponent<Sliceable>();
            sliceable.SlicePosition = target.transform.position;
            sliceable.SliceNormal = target.transform.forward;
            sliceable.IsSliceable = true;
            sliceable.Flip = enteredFromBack;
            sliceable.UpdateMaterialSlice();
        }

        travelerCopies.Add(portalTraveler, travelerCopy.gameObject);
    }

    public void PortalAdjustment(Vector3 localPosition, Vector3 localScale)
    {
        portalDisplay.transform.localScale = localScale;
        portalDisplay.transform.localPosition = localPosition;
    }

    public void OnTriggerExit(Collider other)
    {
        PortalTraveler portalTraveler = other.GetComponent<PortalTraveler>();
        if (portalTraveler != null)
        {
            if (other.CompareTag("Player"))
            {
                playerTraveler = null;
                var originalPosition = new Vector3(0, portalDisplay.transform.localPosition.y, 0);

                PortalAdjustment(originalPosition, originalPortalScale);
                target.PortalAdjustment(originalPosition, originalPortalScale);
            }
            else
            {
                Sliceable sliceable = portalTraveler.GetComponent<Sliceable>();
                sliceable.IsSliceable = false;
                sliceable.UpdateMaterialSlice();
                TransformToTarget(portalTraveler.transform);
                lstPortalTravelers.Remove(portalTraveler);
                DestroyTravelerCopy(portalTraveler);
                travelerCopies.Remove(portalTraveler);
            }
        }
    }

    private bool CheckTravelerPassPortal(PortalTraveler traveler)
    {
        Vector3 forward = transform.forward;
        Vector3 previousVec = traveler.LastFramePosition - transform.position;
        Vector3 currentVec = traveler.transform.position - transform.position;
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

    private void DestroyTravelerCopy(PortalTraveler traveler)
    {
        if (travelerCopies != null && travelerCopies.ContainsKey(traveler))
        {
            GameObject clone = travelerCopies[traveler];
            if (clone != null)
            {
                Destroy(clone);
            }
        }
    }

    private void PortalRenderCameraTextureUpdated(RenderTexture newTexture)
    {
        portalDisplay.SetTexture(newTexture);
    }

    public void ManuallyRenderCamera(Vector3 position, Quaternion rotation)
    {
        targetPortalRenderCamera.transform.position = position;
        targetPortalRenderCamera.transform.rotation = rotation;
        targetPortalRenderCamera.Render();
    }

}
