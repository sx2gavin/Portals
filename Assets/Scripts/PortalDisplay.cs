using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(Renderer))]
public class PortalDisplay : MonoBehaviour
{
    private Material material;
    void Start()
    {
        material = GetComponent<Renderer>().material;
    }

    public void SetTexture(RenderTexture texture)
    {
        material.SetTexture("_MainTex", texture);
    }
}
