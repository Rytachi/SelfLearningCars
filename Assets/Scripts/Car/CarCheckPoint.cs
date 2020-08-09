using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarCheckPoint : MonoBehaviour
{
    public CarController carController;
    public TrackScript track;
    public Transform[] checkpointArray;
    public int nextCheckpoint = 1;
    public int currentLap = 0;
    public float distanceToCheckpoint;

    public Text checkPointText;

    void Start(){
        checkpointArray = track.checkpointArray;
        distanceToCheckpoint = Vector2.Distance(carController.car.position, checkpointArray[nextCheckpoint].position);
    }

    void FixedUpdate(){
        distanceToCheckpoint = Vector2.Distance(carController.car.position, checkpointArray[nextCheckpoint].position);
    }
}
