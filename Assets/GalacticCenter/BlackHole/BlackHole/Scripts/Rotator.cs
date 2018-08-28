using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    public Vector3 RotationSpeed = Vector3.zero;

    void Update()
    {
        Vector3 eulerAngles = this.transform.localEulerAngles;
        eulerAngles += RotationSpeed * Time.deltaTime;
        this.transform.localEulerAngles = eulerAngles;
    }
}
