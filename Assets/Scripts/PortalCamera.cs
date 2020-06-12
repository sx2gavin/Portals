using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PortalCamera : MonoBehaviour
{
    public delegate void ViewTextureUpdated(RenderTexture newTexture);
    public event ViewTextureUpdated TextureUpdated;
    private Camera portalCamera;
    private RenderTexture viewTexture;

    // Start is called before the first frame update
    void Start()
    {
        portalCamera = GetComponent<Camera>();
        if (Application.isPlaying)
        {
            // disable portalCamera so we can render the camera manual using Render()
            portalCamera.enabled = false;
        }
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            UpdateTexture();
        }
    }

    public void UpdateNearClippingPlane(Transform nearClippingPlane)
    {
        int flip = Math.Sign(Vector3.Dot(nearClippingPlane.forward, (nearClippingPlane.position - transform.position)));

        Vector3 cameraSpaceClipPlaneOrigin = portalCamera.worldToCameraMatrix.MultiplyPoint(nearClippingPlane.position);
        Vector3 cameraSpaceClipPlaneNormal = portalCamera.worldToCameraMatrix.MultiplyVector(nearClippingPlane.forward) * flip;

        float planeDistance = -Vector3.Dot(cameraSpaceClipPlaneOrigin, cameraSpaceClipPlaneNormal);

        portalCamera.projectionMatrix = portalCamera.CalculateObliqueMatrix(new Vector4(cameraSpaceClipPlaneNormal.x, cameraSpaceClipPlaneNormal.y, cameraSpaceClipPlaneNormal.z, planeDistance));
    }

    private void UpdateTexture()
    {
        if (viewTexture == null || viewTexture.width != Screen.width || viewTexture.height != Screen.height)
        {
            if (viewTexture != null)
            {
                viewTexture.Release();
            }

            viewTexture = new RenderTexture(Screen.width, Screen.height, 0);
            portalCamera.targetTexture = viewTexture;
            TextureUpdated?.Invoke(viewTexture);
        }
    }

    public RenderTexture GetRenderTexture()
    {
        UpdateTexture();
        return viewTexture;
    }

    public void Render()
    {
        portalCamera.Render();
    }
}
