using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CarControllerBackPropagation : MonoBehaviour
{
    public float acceleration = 5f;
    public float deacceleration = 3f;
    public float turnSpeed = 100f;
    public float speed;
    float torqueForce = 0;
    Vector3 startingPos;
    Quaternion carRotation;
    float idleTime = 5f;
    float timeLeft = 0;

    float driftSpeedMoving = .9f;
    float driftSpeedStatic = .9f;
    float maxSideways = .5f;
    public Rigidbody2D car;
    public List<CarSensors> sensors;

    public bool playerStopped;
    public bool playerHitWall;
    bool timerStarted;
    
    public float carDrive;
    public float carTurn;

    void Start(){
        playerStopped = false;
        playerHitWall = false;
        startingPos = gameObject.transform.position;
        carRotation = gameObject.transform.rotation;
        timerStarted = false; 
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

        if (carDrive > 0){
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            return;
        }

        playerHitWall = true;
        
    }

    public void ResetPosition(){
        this.car.velocity = Vector2.zero;
        this.car.position = startingPos;
        gameObject.transform.rotation = carRotation;
        timeLeft = 0;

        playerStopped = false;
        playerHitWall = false;
        timerStarted = false;

    }

    public void ResetPosition1()
    {
        this.car.velocity = Vector2.zero;
        this.car.position = new Vector3(72.41f, 26.76f, 0f);

        gameObject.transform.rotation = Quaternion.AngleAxis(30, Vector3.forward); ;
        timeLeft = 0;

        playerStopped = false;
        playerHitWall = false;
        timerStarted = false;

    }

}
