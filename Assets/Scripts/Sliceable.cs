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
            UpdateMaterialSlice();
        }
    }

    private Vector3 sliceNormal;
    public Vector3 SliceNormal
    {
        get => sliceNormal;
        set
        {
            sliceNormal = value;
            UpdateMaterialSlice();
        }
    }

    private bool isSliceable;
    public bool IsSliceable
    {
        get => isSliceable;
        set
        {
            isSliceable = value;
            UpdateMaterialSlice();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        material = mesh.material;
    }

    private void UpdateMaterialSlice()
    {
        material.SetVector("sliceCentre", slicePosition);
        material.SetVector("sliceNormal", sliceNormal);
    }
}
