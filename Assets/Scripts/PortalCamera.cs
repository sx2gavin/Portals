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

        // disable portalCamera so we can render the camera manual using Render()
        portalCamera.enabled = false;
        // portalCamera.targetTexture = new RenderTexture(Display.main.renderingWidth, Display.main.renderingHeight, 24);
        // GetRenderTexture();
    }

    private void Update()
    {
        UpdateTexture();
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
