using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics;

public class ControlerOneCriteria : MonoBehaviour
{
    public int numGenomes;
    public int numGenomesSP;
    public int numSimulate;
    public Vector3 startingPos;
    public GameObject carFab;
    public TrackScript track;
    GameObject[] cars;
    AICarControllerOneCriteria[] carController;
    public GeneticControllerOneCriteria species;
    public TargetCamera bestCamera;
    public float timeElapsed;
    public int currentGenome;
    public float bestGenomeFitness;
    public int batchSimulate;
    public int neuralNetworkNumber;
    public int currentGeneration = 1;
    public AICarControllerOneCriteria bestCar;
    public AICarControllerOneCriteria savedCar;
    public bool finished = false;
    public int finishedBatch = 6;
    public int testNum = 1;
    public int nnNum = 1;
    void Start()
    {
        neuralNetworkNumber = 0;
        timeElapsed = 0;
        species = new GeneticControllerOneCriteria(numGenomes, 0.06f, new int[] { 6, 20, 2 });
        cars = new GameObject[numSimulate];
        carController = new AICarControllerOneCriteria[numSimulate];
        bestCar = carController[0];

        CarCheckPoint checkpoint = carFab.GetComponent<CarCheckPoint>();
        checkpoint.track = track;

        for (int i = 0; i < numSimulate; i++)
        {
            cars[i] = Instantiate(carFab, startingPos, carFab.transform.rotation);
            carController[i] = cars[i].GetComponent<AICarControllerOneCriteria>();
            carController[i].network = species.population[i];
        }

        currentGenome = 0;
        batchSimulate = numSimulate;
        Time.timeScale = 10;
    }

    public void QuitGame()
    {
    #if UNITY_EDITOR
                 // Application.Quit() does not work in the editor so
                 // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
                 UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif
    }

    void FixedUpdate()
    {
        timeElapsed += Time.deltaTime / 10;
        float bestCarFitness = 0;
        bool allCarsDead = true;
        foreach (AICarControllerOneCriteria car in carController)
        {
            if (car.alive)
            {
                if (car.finishedLap)
                {
                    finished = true;
                    savedCar = car;
                    allCarsDead = false;
                    break;
                }
                allCarsDead = false;
                if (car.overallFitness > bestCarFitness)
                {
                    bestCarFitness = car.overallFitness;
                    bestCamera.target = car.transform;
                    bestCar = car;
                    savedCar = car;
                    if (bestCar.overallFitness > bestGenomeFitness)
                    {
                        bestGenomeFitness = bestCar.overallFitness;
                    }
                }

            }
        }

        if (allCarsDead)
        {
            if (currentGeneration == 1)
            {
                if (currentGenome == numGenomes)
                {

                    species.NextGeneration();
                    for (int i = 0; i < numSimulate; i++)
                    {
                        carController[i].network = species.population[i];
                        carController[i].Reset();

                    }
                    currentGeneration++;
                    currentGenome = 0;
                    neuralNetworkNumber += batchSimulate;
                }
                else 
                {
                    if (currentGenome + numSimulate <= numGenomes)
                    {
                        batchSimulate = numSimulate;
                    }
                    else
                    {
                        batchSimulate = numGenomes - currentGenome;
                    }

                    for (int i = 0; i < batchSimulate; i++)
                    {
                        carController[i].network = species.population[currentGenome + i];
                        carController[i].Reset();
                    }
                    currentGenome += batchSimulate;
                    neuralNetworkNumber += batchSimulate;
                }
            }
            else
            {
                if (currentGenome == numGenomesSP)
                {

                    species.KidsGenerationAdding();
                    for (int i = 0; i < numSimulate; i++)
                    {
                        carController[i].network = species.population[i];
                        carController[i].Reset();

                    }
                    currentGeneration++;
                    currentGenome = 0;
                    neuralNetworkNumber += batchSimulate;
                }
                else 
                {
                    if (currentGenome + numSimulate <= numGenomesSP)
                    {
                        batchSimulate = numSimulate;
                    }
                    else
                    {
                        batchSimulate = numGenomesSP - currentGenome;
                    }

                    for (int i = 0; i < batchSimulate; i++)
                    {
                        carController[i].network = species.population[currentGenome + i];
                        carController[i].Reset();
                    }
                    currentGenome += batchSimulate;
                    neuralNetworkNumber += batchSimulate;
                }
            }

        }

        if (finished)
        {

            if (testNum == 5)
            {
                if (nnNum == 1)
                {
                    Save("./newNoPareto_1000_400_BACK_50_track3");
                }
                if (nnNum == 2)
                {
                    Save("./newNoPareto_1000_400_BACK_60_track3");
                }


                nnNum++;
                testNum = 0;
                if (nnNum == 3)
                {

                    QuitGame();
                }
            }
            if (nnNum == 1)
            {
                Save("./newNoPareto_1000_400_BACK_50_track3");
                species = new GeneticControllerOneCriteria(numGenomes, 0.06f, new int[] { 6, 50, 2 });
            }
            else if (nnNum == 2)
            {
                Save("./newNoPareto_1000_400_BACK_60_track3");
                species = new GeneticControllerOneCriteria(numGenomes, 0.06f, new int[] { 6, 60, 2 });
            }
            for (int i = 0; i < numSimulate; i++)
            {
                carController[i].network = species.population[i];
                carController[i].Reset();
            }
            currentGeneration = 1;
            currentGenome = 0;
            neuralNetworkNumber = 0;
            finished = false;


            timeElapsed = 0;

            testNum++;
        }

    }

    public void Save(string text)
    {
        StreamWriter write = new StreamWriter(text + ".txt", true);

        write.Write(neuralNetworkNumber + ";" + timeElapsed/60 + ";" + savedCar.avgSpeed + ";" + 330/ savedCar.avgSpeed + "\n");

        int[] layerStructure = savedCar.network.layer;

        for (int i = 0; i < layerStructure.Length - 1; i++)
        {
            write.Write(layerStructure[i] + ";");
        }
        write.Write(layerStructure[layerStructure.Length - 1] + ";");

        List<float> encoded = savedCar.network.Encode();
        for (int i = 0; i < encoded.Count - 1; i++)
        {
            write.Write(encoded[i] + ";");
        }
        write.Write(encoded[encoded.Count - 1] + "\n");

        write.Close();
    }
}
