using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCamera : MonoBehaviour
{
    public Transform target;
    public float height = -1f;

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        transform.position = new Vector3(target.position.x, target.position.y, height);
    }
}
