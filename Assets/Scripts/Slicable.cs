using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slicable : MonoBehaviour
{
    public GameObject Slice;
    private MeshRenderer mesh;
    private Material material;
    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        material = mesh.material;
    }

    // Update is called once per frame
    void Update()
    {
        if (material != null)
        {
            material.SetVector("sliceCentre", Slice.transform.position);
            material.SetVector("sliceNormal", Slice.transform.forward);
        }
    }
}
