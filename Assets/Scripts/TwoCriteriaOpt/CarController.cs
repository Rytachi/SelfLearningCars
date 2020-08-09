using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CarController : MonoBehaviour
{

    public float acceleration = 5f;
    public float deacceleration = 3f;
    public float turnSpeed = 100f;
    public float speed;
    float torqueForce = 0;
    public CarCheckPoint carCheckPoint;
    Vector3 startingPos;
    Quaternion carRotation;
    float idleTime = 5f;
    float timeLeft = 0;
    int firstCheckpoint;

    float driftSpeedMoving = .9f;
    float driftSpeedStatic = .9f;
    float maxSideways = .5f;

    public Rigidbody2D car;
    public List<CarSensors> sensors;
    public bool playerStopped;
    public bool playerHitWall;
    public bool hitCheckPoint;
    bool timerStarted;

    public float carDrive;
    public float carTurn;

    void Start(){
        carCheckPoint = gameObject.GetComponent<CarCheckPoint>();
        playerStopped = false;
        playerHitWall = false;
        hitCheckPoint = false;
        startingPos = gameObject.transform.position;
        carRotation = gameObject.transform.rotation;
        timerStarted = false;
        firstCheckpoint = carCheckPoint.nextCheckpoint;


    }

    void FixedUpdate(){
        speed = car.velocity.magnitude;

        if (car.velocity.magnitude < .05f){
            if (timerStarted){
                timeLeft += Time.deltaTime;
                if (timeLeft > idleTime){

                    playerStopped = true;
                    timerStarted = false;
                    timeLeft = 0;
                }
            }
            else{
                timerStarted = true;
            }
        }

        float driftFactor = driftSpeedStatic;
        if (ForwardVelocity().magnitude > maxSideways){
            driftFactor = driftSpeedMoving;
        }
        car.velocity = ForwardVelocity() + SideVelocity() * driftFactor;

        if (carDrive >0){
            car.AddForce(transform.up * acceleration);

        }
        if (Input.GetKey(KeyCode.S) || carDrive <= 0){
            car.AddForce(transform.up * deacceleration);
        }

        torqueForce = Mathf.Lerp(0, turnSpeed, car.velocity.magnitude / 2);

        car.angularVelocity = (float)((carTurn) * torqueForce);

    }


    Vector2 ForwardVelocity(){

        return transform.up * Vector2.Dot(GetComponent<Rigidbody2D>().velocity, transform.up);
    }

    Vector2 SideVelocity(){
        return transform.right * Vector2.Dot(GetComponent<Rigidbody2D>().velocity, transform.right);
    }

    private void OnTriggerEnter2D(Collider2D other){
        if (other.gameObject.tag == "CheckPoint"){
            if (other.transform == carCheckPoint.checkpointArray[carCheckPoint.nextCheckpoint].transform)
            {
                if (carCheckPoint.nextCheckpoint + 1 == carCheckPoint.checkpointArray.Length)
                {
                    carCheckPoint.nextCheckpoint = 0;
                    carCheckPoint.currentLap += 1;
                }
                else
                {
                    carCheckPoint.nextCheckpoint += 1;
                    hitCheckPoint = true;
                }
            }
            return;
        }else if(other.gameObject.tag == "Player")
        {
            return;
        }

        playerHitWall = true;
        
    }

    public void ResetPosition(){
        this.car.velocity = Vector2.zero;
        this.car.position = startingPos;
        gameObject.transform.rotation = carRotation;
        this.carCheckPoint.nextCheckpoint = firstCheckpoint;
        timeLeft = 0;

        playerStopped = false;
        playerHitWall = false;
        hitCheckPoint = false;
        timerStarted = false;

    }

}
