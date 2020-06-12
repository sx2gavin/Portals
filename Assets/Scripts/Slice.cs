using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slice : MonoBehaviour
{
    [SerializeField] Sliceable sliceable;
    [SerializeField] Text position;
    [SerializeField] Text normal;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        sliceable.SlicePosition = transform.position;
        sliceable.SliceNormal = transform.forward;
        position.text = transform.position.ToString();
        normal.text = transform.forward.ToString();
    }
}
