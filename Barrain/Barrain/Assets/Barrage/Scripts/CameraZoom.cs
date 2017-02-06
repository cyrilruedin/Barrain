using UnityEngine;
using System.Collections;

public class CameraZoom : MonoBehaviour
{
    // Public
    public float ZoomSpeed = 1f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
                GetComponent<Camera>().transform.position += 
                GetComponent<Camera>().transform.rotation * Vector3.forward;
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {

            GetComponent<Camera>().transform.position -=
                GetComponent<Camera>().transform.rotation * Vector3.forward;
        }
    }
}
