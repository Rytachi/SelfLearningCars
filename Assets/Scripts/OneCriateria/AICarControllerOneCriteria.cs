using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AICarControllerOneCriteria : MonoBehaviour
{
    public NeuralNetwork1 network = null;

    // Car properties
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
    public bool alive = true;
    public bool finishedLap = false;

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
                network.fitness = overallFitness;

                finishedLap = true;
            }
            if (controller.carCheckPoint.nextCheckpoint < 2 && timeElapsed >= 2.0f && controller.carCheckPoint.currentLap == 0)
            {
                Stop();
                network.fitness = overallFitness * 0.1f;
            }
            if (controller.carCheckPoint.nextCheckpoint < 3 && timeElapsed >= 7.0f && controller.carCheckPoint.currentLap == 0)
            {
                Stop();
                network.fitness = overallFitness * 0.1f;
            }

        }
    }

    private void CalculateFitness(){
        movement = Vector3.Distance(controller.car.position, lastPosition) * -1;
        if (controller.carCheckPoint.distanceToCheckpoint < lastCheckpointDistance){
            movement *= -1;
        }

        distanceTraveled += movement;

        lastCheckpointDistance = controller.carCheckPoint.distanceToCheckpoint;
        lastPosition = controller.car.position;

        timeElapsed += Time.deltaTime/10 ;
        avgSpeed = distanceTraveled / timeElapsed;

        overallFitness = (distanceTraveled); 
    }

    void Stop(){
        alive = false;
        controller.carTurn = 0;
        controller.carDrive = 0;
        controller.car.isKinematic = true;
        controller.car.velocity = Vector3.zero;
        network.fitness = overallFitness;
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
