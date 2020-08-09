using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelController : MonoBehaviour{
    Vector3 wheelAngle;
    float steerAngle, maxSteerAngle = 30f;
    public Rigidbody2D car;
    public CarController aicar;

    void FixedUpdate(){
        steerAngle = (maxSteerAngle * aicar.carTurn + car.rotation) * Time.deltaTime;
    }

    void LateUpdate(){

        wheelAngle = transform.eulerAngles;
        wheelAngle.z = steerAngle;
        transform.eulerAngles = wheelAngle;
    }
}
