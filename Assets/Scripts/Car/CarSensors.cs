using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarSensors : MonoBehaviour{
    public Transform car;
    public float distance = 0;
    public float hitNormal = 0;

    private void Start(){
    }

    void LateUpdate(){

        Vector2 direction = gameObject.transform.position - car.position;

        int layerMask1 = 1 << 8;
        int layerMask2 = 1 << 2;
        int layerMask = layerMask1 | layerMask2;
        layerMask = ~layerMask;
        hitNormal = 1;

        RaycastHit2D hit = Physics2D.Raycast(car.position, direction, direction.magnitude, layerMask);
        if (hit.collider != null){

            hitNormal = hit.distance / direction.magnitude;

            Debug.DrawRay(car.position, direction, Color.red);


        }
        else
        {
            distance = 1;

        }

    }
}
