using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AICarControllerTwoCriteria : MonoBehaviour
{
    public NeuralNetwork1 network = null;

    public CarController controller;
    private Vector3 lastPosition;
    public float distanceTraveled;
    public float avgSpeed;
    public float timeElapsed;
    private float avgSensor;
    float timer = 0;
    public float bestFitness = 0;
    public float bestPopFitness = 0;
    public float overallFitness;
    public float distanceMultiplier;
    public float speedMultiplier;
    public float sensorMultiplier;
    public bool alive = true;
    public bool finishedLap = false;
    public float maxDistance = 340;
    float lastCheckpointDistance;
    float startinglastCheckpointDistance;
    float movement;

    void Start(){
        timeElapsed = 0;
        controller = GetComponent<CarController>();
        lastPosition = this.controller.car.position;
        lastCheckpointDistance = controller.carCheckPoint.distanceToCheckpoint;
        startinglastCheckpointDistance = controller.carCheckPoint.distanceToCheckpoint;
    }

    void FixedUpdate(){
        if (alive)
        {
            List<double> input = new List<double>();
            
            for (int i = 0; i < controller.sensors.Count; i++)
            {
                input.Add(controller.sensors[i].hitNormal);
                  
            }
            input.Add(controller.speed / controller.acceleration);
            float[] a = { (float)input[0], (float)input[1], (float)input[2], (float)input[3], (float)input[4], (float)input[5] };
            float[] output = network.FeedForward(a);

           
            controller.carTurn = (float)output[0];
            controller.carDrive = (float)output[1];

            CalculateFitness();
            

            if (controller.playerHitWall)
            {
                Stop();
            }
            if (controller.playerStopped)
            {
                Stop();
            }
            if(controller.carCheckPoint.nextCheckpoint >= 22)
            {
                network.fitness_metrics = new Point_Double(distanceTraveled, distanceTraveled / timeElapsed, controller.acceleration);
                network.fitness_metrics.finished = true;
                finishedLap = true;
                Stop();
            }
            if(controller.carCheckPoint.nextCheckpoint < 3 && timeElapsed >= 15.0f && controller.carCheckPoint.currentLap == 0)
            {  
                Stop();
            }
        }
    }

    private void CalculateFitness(){
        movement = Vector3.Distance(controller.car.position, lastPosition) * -1;
        if (controller.carCheckPoint.distanceToCheckpoint < lastCheckpointDistance){
            movement *= -1;
        }

        distanceTraveled += movement;

        for (int i = 0; i < controller.sensors.Count; i++)
        {
            avgSensor += controller.sensors[i].hitNormal;
        }
        avgSpeed /= controller.sensors.Count;

        lastCheckpointDistance = controller.carCheckPoint.distanceToCheckpoint;
        lastPosition = controller.car.position;

        timeElapsed += Time.deltaTime ;
        avgSpeed = distanceTraveled / timeElapsed;

        overallFitness = (distanceTraveled * distanceMultiplier) + (avgSpeed * speedMultiplier) + (avgSensor * sensorMultiplier);

    }

    void Stop(){
        alive = false;
        controller.carTurn = 0;
        controller.carDrive = 0;
        controller.car.isKinematic = true;
        controller.car.velocity = Vector3.zero;
        network.fitness_metrics = new Point_Double(distanceTraveled, distanceTraveled / timeElapsed, controller.acceleration);

    }

    public void Reset(){
        this.controller.ResetPosition();
        lastPosition = this.controller.car.position;
        distanceTraveled = 0;
        timeElapsed = 0;
        lastCheckpointDistance = controller.carCheckPoint.distanceToCheckpoint;
        timer = 10f;
        avgSensor = 0;
        if (overallFitness > bestPopFitness){
            bestPopFitness = overallFitness;
        }
        bestFitness = 0f;
        alive = true;
        finishedLap = false;
        controller.car.isKinematic = false;
    }
}
