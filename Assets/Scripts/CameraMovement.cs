using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] GameObject cameraLookAt;

    [SerializeField] float moveSpeed = 350;
    [SerializeField] float zoomSpeed = 350;

    [SerializeField] float minDistance = 2f;
    [SerializeField] float maxDistance = 10f;

    void Update()
    {
        CameraControl();
    }

    void CameraControl()
    {
        if (Input.GetMouseButton(2))
        {
            transform.RotateAround(cameraLookAt.transform.position, Vector3.up, ((Input.GetAxisRaw("Mouse X") * Time.deltaTime) * moveSpeed));
            transform.RotateAround(cameraLookAt.transform.position, transform.right, -((Input.GetAxisRaw("Mouse Y") * Time.deltaTime) * moveSpeed));
        }

        ZoomCamera();
    }

    void ZoomCamera()
    {
        if (Vector3.Distance(
            transform.position, cameraLookAt.transform.position) <= minDistance && 
            Input.GetAxis("Mouse ScrollWheel") > 0f) {
            return;
        }
        if (Vector3.Distance(transform.position, cameraLookAt.transform.position) >= maxDistance && 
            Input.GetAxis("Mouse ScrollWheel") < 0f) {
            return;
        }

        transform.Translate(
            0f,
            0f,
            (Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime) * zoomSpeed,
            Space.Self
        );
    }




}
