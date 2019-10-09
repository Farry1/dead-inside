using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitFacing : MonoBehaviour
{
    // Object always faces towards camera on y-axis
    void Update()
    {
        // Look at Camera
        transform.LookAt(Camera.main.transform.position, -Vector3.up);
        // And Only Rotate on Y
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
    }
}
