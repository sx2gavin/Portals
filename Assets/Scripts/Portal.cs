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

    private PortalTraveller playerTraveller;
    private List<PortalTraveller> lstPortalTravellers = new List<PortalTraveller>();
    private Dictionary<PortalTraveller, GameObject> travellerCopies = new Dictionary<PortalTraveller, GameObject>();

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
            
            if (playerTraveller != null)
            {
                if (CheckTravellerPassPortal(playerTraveller))
                {
                    // pre-render target camera before player is teleported to smooth the transition.
                    target.ManuallyRenderCamera(mainCamera.transform.position, mainCamera.transform.rotation);

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

    public void OnTriggerEnter(Collider other)
    {
        PortalTraveller portalTraveller = other.GetComponent<PortalTraveller>();
        if (portalTraveller != null && portalTraveller.enabled)
        {
            var playerVecFromPortal = other.transform.position - transform.position;
            enteredFromBack = Vector3.Dot(playerVecFromPortal, transform.forward) < 0;
            if (other.CompareTag("Player"))
            {
                playerTraveller = portalTraveller;
                var localPosition = portalDisplay.transform.localPosition;
                var portalAdjustment = (enteredFromBack ? 1 : -1) * (portalDisplayExpandFactor * 0.1f / 2f);

                var portalScale = new Vector3(portalDisplayExpandFactor, originalPortalScale.y, originalPortalScale.z);
                var portalPosition = new Vector3(localPosition.x, localPosition.y, portalAdjustment);

                PortalAdjustment(portalPosition, portalScale);
            }
            else
            {
                CreateTravellerClone(portalTraveller, enteredFromBack);
            }
        }
    }

    private void CreateTravellerClone(PortalTraveller portalTraveller, bool enteredFromBack)
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

        Sliceable sliceable = portalTraveller.GetComponent<Sliceable>();
        sliceable.SlicePosition = transform.position;
        sliceable.SliceNormal = transform.forward;
        sliceable.IsSliceable = true;
        sliceable.Flip = !enteredFromBack;
        sliceable.UpdateMaterialSlice();

        if (target)
        {
            sliceable = travellerCopy.GetComponent<Sliceable>();
            sliceable.SlicePosition = target.transform.position;
            sliceable.SliceNormal = target.transform.forward;
            sliceable.IsSliceable = true;
            sliceable.Flip = enteredFromBack;
            sliceable.UpdateMaterialSlice();
        }

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
                Sliceable sliceable = portalTraveller.GetComponent<Sliceable>();
                sliceable.IsSliceable = false;
                sliceable.UpdateMaterialSlice();
                TransformToTarget(portalTraveller.transform);
                lstPortalTravellers.Remove(portalTraveller);
                DestroyTravellerCopy(portalTraveller);
                travellerCopies.Remove(portalTraveller);
            }
        }
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
