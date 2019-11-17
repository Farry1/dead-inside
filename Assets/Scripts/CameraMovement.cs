using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [HideInInspector] public GameObject initialCameraLookAt;
    public GameObject rotateAroundGO;

    [SerializeField] float moveSpeed = 350;
    [SerializeField] float zoomSpeed = 350;

    [SerializeField] float minDistance = 2f;
    [SerializeField] float maxDistance = 10f;

    protected float t;
    protected Vector3 startPosition;
    Quaternion startRotation;
    Quaternion targetRotation;
    protected Vector3 target;
    protected float timeToReachTarget;

    bool cameraMoves = false;

    

    private void Start()
    {
        initialCameraLookAt = rotateAroundGO;
        startPosition = target = transform.position;
    }

    void Update()
    {
        CameraControl();

        if (cameraMoves)
        {
            t += Time.deltaTime / timeToReachTarget;
            transform.position = Vector3.Lerp(startPosition, target, t);
            //transform.LookAt(rotateAroundGO.transform, transform.up);

            
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, t);


            //transform.rotation = Quaternion.RotateTowards(transform.rotation, rotateAroundGO.transform.rotation, t);
        }
    }

    public void SetMoveDestination(Transform destination, float time)
    {
        t = 0;
        startPosition = transform.position;
        startRotation = transform.rotation;
        timeToReachTarget = time;
        target = (destination.position + destination.up * 10f);
        targetRotation = Quaternion.LookRotation(- destination.up);
    }

    public void MoveCameraTo(Transform target)
    {
        SetMoveDestination(target, 0.75f);
        StartCoroutine(CameraMoves());
    }

    IEnumerator CameraMoves()
    {
        cameraMoves = true;
        yield return new WaitForSeconds(0.75f);
        cameraMoves = false;
    }

    void CameraControl()
    {
        if (Input.GetMouseButton(2))
        {
            transform.RotateAround(rotateAroundGO.transform.position, Vector3.up, ((Input.GetAxisRaw("Mouse X") * Time.deltaTime) * moveSpeed));
            transform.RotateAround(rotateAroundGO.transform.position, transform.right, -((Input.GetAxisRaw("Mouse Y") * Time.deltaTime) * moveSpeed));
        }

        ZoomCamera();
    }

    void ZoomCamera()
    {

        if (Vector3.Distance(
            transform.position, rotateAroundGO.transform.position) <= minDistance &&
            Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            return;
        }
        if (Vector3.Distance(transform.position, rotateAroundGO.transform.position) >= maxDistance &&
            Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
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
