using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using System;
using System.Globalization;

public class BackPropagationControler : MonoBehaviour
{
    public Vector3 startingPos;
    public GameObject carFab;
    GameObject cars;
    AICarControllerBackPropagation carController;
    public TargetCamera bestCamera;
    public float timeElapsed;
    public bool finished = false;
    public int testNum;
    NeuralNetwork1 nns;
    List<DataSet> dataSets = new List<DataSet>();
    int [] strukt;

    void Start()
    {
        strukt = new int[] { 6, 5, 2 };
        readDataSet();
        nns = new NeuralNetwork1(strukt);
        for (int j = 0; j < dataSets.Count; j++)
        {
            nns.FeedForward(dataSets[j].Values);
            nns.BackProp(dataSets[j].Targets);

        }

        timeElapsed = 0;
        cars = new GameObject();
        carController = new AICarControllerBackPropagation();

        cars = Instantiate(carFab, startingPos, carFab.transform.rotation);
        carController = cars.GetComponent<AICarControllerBackPropagation>();
        carController.nn = nns;

        Time.timeScale = 10;
    }

    public class DataSet
    {
        public float[] Values { get; set; }
        public float[] Targets { get; set; }

        public DataSet(float[] values, float[] targets)
        {
            Values = values;
            Targets = targets;
        }
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR

        #else
                Application.Quit();
        #endif
    }
    void readDataSet()
    {
        string[] lines = System.IO.File.ReadAllLines(@"D:\Studijos\7 Semestras\Projektinis darbas\RytisPetrauskasProject\inputDatatrack3Norm.txt");
        string[] lines1 = System.IO.File.ReadAllLines(@"D:\Studijos\7 Semestras\Projektinis darbas\RytisPetrauskasProject\outputDatatrack3Norm.txt");
        string[] splitString;
        int i = 0;
        foreach (string line in lines)
        {
            
            splitString = line.Split(';');
            float[] C = { float.Parse(splitString[0], CultureInfo.InvariantCulture.NumberFormat), float.Parse(splitString[1], CultureInfo.InvariantCulture.NumberFormat), float.Parse(splitString[2], CultureInfo.InvariantCulture.NumberFormat), float.Parse(splitString[3], CultureInfo.InvariantCulture.NumberFormat), float.Parse(splitString[4], CultureInfo.InvariantCulture.NumberFormat), float.Parse(splitString[5], CultureInfo.InvariantCulture.NumberFormat) };
            splitString = lines1[i].Split(';');
            float[] D = { float.Parse(splitString[0], CultureInfo.InvariantCulture.NumberFormat), float.Parse(splitString[1], CultureInfo.InvariantCulture.NumberFormat) };
            dataSets.Add(new DataSet(C, D));
            i++;
        }
    }

    void FixedUpdate()
    {
        timeElapsed += Time.deltaTime;

        if (carController != null)
        {
            bestCamera.target = carController.transform;
        }
        if (!carController.alive)
        {

            if (carController.finishedFirstLap)
            {
                finished = true;
            }
            else
            {
                UnityEngine.Debug.Log("ASD");
                trainOneEpoch();
            }
            
        }

        if (finished)
        {
            if (testNum == 5)
            {
                QuitGame();
            }

            finished = false;
            testNum++;
        }
    }

    void trainOneEpoch()
    {

        
        for (int j = 0; j < dataSets.Count; j++)
        {
            nns.FeedForward(dataSets[j].Values);
            nns.BackProp(dataSets[j].Targets);
        }
        carController.nn = nns;
        carController.Reset();
        

    }

    void resetNeuralNetwork()
    {
        
        nns = new NeuralNetwork1(strukt);
        for (int j = 0; j < dataSets.Count; j++)
        {
            nns.FeedForward(dataSets[j].Values);
            nns.BackProp(dataSets[j].Targets);

        }

        carController.nn = nns;
        carController.Reset();
        
    }
}
