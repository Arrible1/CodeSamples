using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Billboard : MonoBehaviour {

    Transform cameraTransform;

    void Awake() 
    {
        cameraTransform = Camera.main.transform;
    }
    
    void Update()
    {
        transform.LookAt(transform.position + cameraTransform.forward, cameraTransform.up);
    }
}
