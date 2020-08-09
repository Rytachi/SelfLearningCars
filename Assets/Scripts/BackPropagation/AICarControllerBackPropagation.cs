using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using System;

public class AICarControllerBackPropagation : MonoBehaviour
{
    public CarControllerBackPropagation controller;
    private Vector3 lastPosition;
    public float distanceTraveled;
    public float avgSpeed;
    public float avgSpeed1;
    public float timeElapsed;
    public float timeElapsed1;
    public bool alive = true;
    public bool finishedLap = false;
    public bool finishedFirstLap = false;
    float movement;
    public NeuralNetwork1 nn;
    public float trainTrackXmin;
    public float trainTrackXmax;
    public float trainTrackYmin;
    public float trainTrackYmax;
    public float testTrackXmin;
    public float testTrackXmax;
    public float testTrackYmin;
    public float testTrackYmax;

    void Start(){
        timeElapsed = 0;
        timeElapsed1 = 0;
        controller = GetComponent<CarControllerBackPropagation>();
        lastPosition = this.controller.car.position;
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

            float[] a = {(float)input[0], (float)input[1], (float)input[2], (float)input[3], (float)input[4], (float)input[5]};
            float[] output = nn.FeedForward(a);
            
            
            controller.carTurn = output[0];
            controller.carDrive = output[1];
            UpdateMetrics();
            
            if (controller.car.position.x > trainTrackXmin && controller.car.position.x < trainTrackXmax && controller.car.position.y > trainTrackYmin && controller.car.position.y < trainTrackYmax && !finishedLap)
            {
                timeElapsed1 = timeElapsed;
                avgSpeed1 = avgSpeed;
                finishedFirstLap = true;
                Reset1();
            }
            if (controller.car.position.x > testTrackXmin && controller.car.position.x < testTrackXmax && controller.car.position.y > testTrackYmin && controller.car.position.y < testTrackYmax && !finishedLap)
            {
                StreamWriter write = new StreamWriter("Second_Test_Results_20nn.txt", true);

                write.WriteLine("track1time;track2time;avgspeed1;avgspeed2;");
                write.WriteLine(timeElapsed1 + ";" + timeElapsed + ";" + avgSpeed + ";" + avgSpeed1);

                write.Close();
                finishedLap = true;
                
                Stop();

            }

            if (controller.playerHitWall && finishedFirstLap)
            {
                StreamWriter write = new StreamWriter("Second_Test_Results_20nn.txt", true);

                write.WriteLine("track1time;;avgspeed1;");
                write.WriteLine(timeElapsed1 + ";" + avgSpeed1 + ";ANTROS APVAZIUOTI NEPAVYKO");

                write.Close();
                finishedLap = true;
                Stop();
            }

            if (controller.playerHitWall)
            {
                Stop();
            }
            if (controller.playerStopped)
            {
                Stop();
            }
        }
    }

    private void UpdateMetrics(){

        movement = Vector3.Distance(controller.car.position, lastPosition);

        distanceTraveled += movement;
        lastPosition = controller.car.position;

        timeElapsed += Time.deltaTime ;
        avgSpeed = distanceTraveled / timeElapsed;
        
    }

    void Stop(){
        alive = false;
        controller.carTurn = 0;
        controller.carDrive = 0;
        controller.car.isKinematic = true;
        controller.car.velocity = Vector3.zero;
    }

    public void Reset(){
        this.controller.ResetPosition();
        lastPosition = this.controller.car.position;
        distanceTraveled = 0;
        timeElapsed = 0;
        alive = true;
        finishedLap = false;
        controller.car.isKinematic = false;
        finishedFirstLap = false;
    }
    public void Reset1()
    {
        this.controller.ResetPosition1();
        lastPosition = this.controller.car.position;
        distanceTraveled = 0;
        timeElapsed = 0;
        alive = true;
        finishedLap = false;
        controller.car.isKinematic = false;

    }
}
