using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateLigth : MonoBehaviour
{
    public GameObject sun;
    public float velocity;

    // Update is called once per frame
    void Update()
    {
        sun.transform.Rotate(Vector3.right * velocity);
    }
}
