using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(Renderer))]
public class PortalDisplay : MonoBehaviour
{
    private Material material;
    private Vector3[] displayVertices;
    private Vector2[] uvs;

    private Mesh mesh;
    void Start()
    {
        // material = new Material(Shader.Find("Sprites/Default"));
        material = GetComponent<Renderer>().material;
        
        mesh = GetComponent<MeshFilter>().mesh;


        displayVertices = mesh.vertices;
        
        uvs = new Vector2[displayVertices.Length];
    }

    void Update()
    {
        // CalculateUVs();
    }

    public void SetTexture(RenderTexture texture)
    {
        material.SetTexture("_MainTex", texture);
    }

    public void CalculateUVs()
    {
        for (int i = 0; i < displayVertices.Length; i++)
        {
            Vector3 viewportPt = Camera.main.WorldToViewportPoint(transform.TransformPoint(displayVertices[i]));
            uvs[i] = viewportPt;
        }

        mesh.uv = uvs;
    }

    void OnDrawGizmosSelected()
    {
        mesh = GetComponent<MeshFilter>().sharedMesh;
        displayVertices = mesh.vertices;
        // Gizmos.(transform.TransformPoint(displayVertices[0]), 0.1f);
        Handles.Label(transform.TransformPoint(displayVertices[0]), "1");
    }

    void OnGUI()
    {
        for (int i = 0; i < displayVertices.Length; i++)
        {
            Vector3 viewportPt = Camera.main.WorldToViewportPoint(transform.TransformPoint(displayVertices[i]));
            
            Handles.Label(transform.TransformPoint(displayVertices[i]), "viewport: " + viewportPt.ToString() + "\nuv: " + mesh.uv[i]);
            
        }
    }
}
