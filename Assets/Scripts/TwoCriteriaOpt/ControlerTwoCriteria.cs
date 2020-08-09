using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics;

public class ControlerTwoCriteria : MonoBehaviour
{
    public int numGenomes;
    public int numSimulate;
    public int numGenomesSP;

    public int neuralNetworkNumber;
    public float timeElapsed;

    public Vector3 startingPos;
    public GameObject carFab;
    public TrackScript track;
    GameObject[] cars;
    AICarControllerTwoCriteria[] carController;
    public GeneticControllerTwoCriteria species;
    public TargetCamera bestCamera;
    
    public int currentGenome;
    public float bestGenomeFitness;
    public int batchSimulate;
    
    public AICarControllerTwoCriteria bestCar;
    public AICarControllerTwoCriteria savedCar;
    public bool finished = false;
    public int testNum = 1;
    public int nnNum = 1;
    List<NeuralNetwork1> finishedCars;

    public int stoppingGenerationNumber = 10;
    public int currentGeneration = 1;
    public int generationToReach = 0;
    bool startMoreCarOpt = false;

    void Start()
    {
        finishedCars = new List<NeuralNetwork1>();
        neuralNetworkNumber = 0;
        timeElapsed = 0;
        species = new GeneticControllerTwoCriteria(numGenomes, 0.06f, new int[] {6, 40, 2});
        cars = new GameObject[numSimulate];
        carController = new AICarControllerTwoCriteria[numSimulate];

        CarCheckPoint checkpoint = carFab.GetComponent<CarCheckPoint>();
        checkpoint.track = track;

        for (int i = 0; i < numSimulate; i++)
        {
            cars[i] = Instantiate(carFab, startingPos, carFab.transform.rotation);
            carController[i] = cars[i].GetComponent<AICarControllerTwoCriteria>();
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

    void FixedUpdate(){
        timeElapsed += Time.deltaTime/10;
        
        float bestCarDistance = 0;
        bool allCarsDead = true;
        foreach (AICarControllerTwoCriteria car in carController)
        {
            if (car.alive)
            {

                allCarsDead = false;

                if (car.distanceTraveled > bestCarDistance)
                {
                    bestCarDistance = car.distanceTraveled;
                    bestCamera.target = car.transform;
                    bestCar = car;
                    savedCar = car;
                    if (bestCar.distanceTraveled > bestGenomeFitness)
                    {
                        bestGenomeFitness = bestCar.distanceTraveled;
                    }
                }

            }
        }

        if (allCarsDead)
        {
            foreach (AICarControllerTwoCriteria car in carController)
            {
                if (car.finishedLap)
                {
                    finishedCars.Add(car.network);
                    if (finishedCars.Count == 1)
                    {
                        generationToReach = currentGeneration + stoppingGenerationNumber;

                    }
                    if (finishedCars.Count > 1)
                    {
                        bool a = true;
                        for (int i = finishedCars.Count - 2; i >= 0; i--)
                        {
                            if (finishedCars[finishedCars.Count - 1].fitness_metrics.y < finishedCars[i].fitness_metrics.y)
                            {
                                a = false;
                            }
                        }
                        if (a)
                        {
                            generationToReach = currentGeneration + stoppingGenerationNumber;
                        }
                    }
                    startMoreCarOpt = true;
                }
            }
               
                if (currentGeneration == 1)
            {
                if (currentGenome == numGenomes)
                {

                    species.NextGenerationTwoIndex();
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

                    species.NextGenerationKidsTwoIndex();
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

        if (startMoreCarOpt && currentGeneration == generationToReach)
        {
            finished = true;
        }

        if (finished)
        {

            
            if (nnNum == 1)
            {
                Save("./newPareto_1000_400_longer_30_30");
                species = new GeneticControllerTwoCriteria(numGenomes, 0.06f, new int[] { 6, 20, 2 });
            }
            if (nnNum == 2)
            {
                Save("./newPareto_1000_400_longer_30_30");
                species = new GeneticControllerTwoCriteria(numGenomes, 0.06f, new int[] { 6, 30, 2 });
            }

            for (int i = 0; i < numSimulate; i++)
            {
                carController[i].network = species.population[i];
                carController[i].Reset();
            }

            if (testNum == 1)
            {
                
                nnNum++;
                testNum = 0;
                if (nnNum == 5)
                {
                    
                    QuitGame();
                }
            }
            
            finishedCars = new List<NeuralNetwork1>();
            currentGeneration = 1;
            currentGenome = 0;
            neuralNetworkNumber = 0;
            finished = false;
            generationToReach = 0;
            startMoreCarOpt = false;
            timeElapsed = 0;

            testNum++;
        }
        
    }

    public void Save(string text)
    {
        int d = 0;
        double dist = finishedCars[0].fitness_metrics.y;
        for (int i = 1; i < finishedCars.Count; i++)
        {
            if(dist < finishedCars[i].fitness_metrics.y)
            {
                d = i;
                dist = finishedCars[i].fitness_metrics.y;
            }
        }

        StreamWriter write = new StreamWriter(text + ".txt", true);
        write.Write(neuralNetworkNumber + ";" + timeElapsed + "\n");
 
        write.Write(finishedCars[d].fitness_metrics.x + ";" + finishedCars[d].fitness_metrics.y + ";" + finishedCars[d].fitness_metrics.x / finishedCars[d].fitness_metrics.y + "\n");
        int[] layerStructure = finishedCars[d].layer;


        for (int j = 0; j < layerStructure.Length - 1; j++)
        {
            write.Write(layerStructure[j] + ";");
        }
        write.Write(layerStructure[layerStructure.Length - 1] + ";");

        List<float> encoded = finishedCars[d].Encode();
        for (int j = 0; j < encoded.Count - 1; j++)
        {
            write.Write(encoded[j] + ";");
        }
        write.Write(encoded[encoded.Count - 1] + "\n");
        

        write.Close();
    }
}
