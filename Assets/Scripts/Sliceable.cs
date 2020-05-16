using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

public class Sliceable : MonoBehaviour
{
    private MeshRenderer mesh;
    private Material material;

    private Vector3 slicePosition;
    public Vector3 SlicePosition
    {
        get => slicePosition;
        set
        {
            slicePosition = value;
            if (material)
            {
                material.SetVector("sliceCentre", slicePosition);
            }
        }
    }

    private Vector3 sliceNormal;
    public Vector3 SliceNormal
    {
        get => sliceNormal;
        set
        {
            sliceNormal = value;
            if (material)
            {
                material.SetVector("sliceNormal", sliceNormal);
            }
        }
    }

    private bool isSliceable;
    public bool IsSliceable
    {
        get => isSliceable;
        set
        {
            isSliceable = value;
            if (material)
            {
                material.SetInt("isSliceable", isSliceable ? 1 : 0);
            }
        }
    }

    private bool flip;
    public bool Flip
    {
        get => flip;
        set
        {
            flip = value;
            if (material)
            {
                material.SetInt("flip", flip ? 1 : 0);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        material = mesh.material;
        UpdateMaterialSlice();
    }

    public void UpdateMaterialSlice()
    {
        if (material)
        {
            material.SetInt("isSliceable", isSliceable ? 1 : 0);
            material.SetInt("flip", flip ? 1 : 0);
            material.SetVector("sliceCentre", slicePosition);
            material.SetVector("sliceNormal", sliceNormal);
        }
    }
}
