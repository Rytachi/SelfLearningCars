using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.CodeDom.Compiler;

public class GeneticControllerTwoCriteria
{
    public List<NeuralNetwork1> population;
    public List<NeuralNetwork1> generation;
    public List<NeuralNetwork1> nextGeneration;
    private double populationFitness;
    public float mutationRate;
    public float averageFitness;
    int popSize;
    int numGenomes;
    List<Point_Double> nonDom;
    List<Point_Double> dom;
    public GeneticControllerTwoCriteria(int popSize, float mutationRate, int [] strukt){
        this.population = new List<NeuralNetwork1>(popSize);
        
        this.populationFitness = 0f;
        this.mutationRate = mutationRate;
        this.averageFitness = 0f;
        this.popSize = popSize;
        this.numGenomes = 20;
        this.generation = new List<NeuralNetwork1>(popSize + numGenomes);
        for (int i = 0; i < popSize; i++){
            NeuralNetwork1 nn = new NeuralNetwork1(strukt);
            this.population.Add(nn);
            this.generation.Add(nn);
        }

    }

    public void Crossover(List<float> mother, List<float> father){
        List<float> tempM = new List<float>();
        List<float> tempF = new List<float>();
        for (int i = 0; i < mother.Count; i++){

            if (UnityEngine.Random.Range(0, 1f) > .5){
                tempM.Add(father[i]);
                tempF.Add(mother[i]);
            }else{ 
                tempM.Add(mother[i]);
                tempF.Add(father[i]);
            }


        }
        mother.RemoveRange(0, mother.Count);
        father.RemoveRange(0, mother.Count);

        mother.InsertRange(0, tempF);
        father.InsertRange(0, tempM);

    }


    public NeuralNetwork1[] Breed(NeuralNetwork1 mother, NeuralNetwork1 father){
        NeuralNetwork1 child1 = new NeuralNetwork1(mother.layer);
        NeuralNetwork1 child2 = new NeuralNetwork1(mother.layer);

        List<float> motherChromosome = mother.Encode();
        List<float> fatherChromosome = father.Encode();

        Crossover(motherChromosome, fatherChromosome);

        child1.Decode(motherChromosome);
        child2.Decode(fatherChromosome);

        return new NeuralNetwork1[] {child1, child2};
    }

    public NeuralNetwork1[] MutateTwoMembers(NeuralNetwork1 mother, NeuralNetwork1 father)
    {
        NeuralNetwork1 child1 = new NeuralNetwork1(mother.layer);
        NeuralNetwork1 child2 = new NeuralNetwork1(mother.layer);

        List<float> motherChromosome = mother.Encode();
        List<float> fatherChromosome = father.Encode();

        child1.Decode(motherChromosome);
        child2.Decode(fatherChromosome);

        Mutate(child1);
        Mutate(child2);
        return new NeuralNetwork1[] { child1, child2 };
    }

    public void Mutate(NeuralNetwork1 creature){
        List<float> chromosome = creature.Encode();
        for (int i = 0; i < chromosome.Count; i++) {
            if (this.mutationRate > UnityEngine.Random.Range(0f, 1f)){

                chromosome[i] = chromosome[i] + chromosome[i] * UnityEngine.Random.Range(-0.5f, 0.5f);
            }
        }

        creature.Decode(chromosome);

    }

    public void NextGenerationTwoIndex()
    {
        this.nextGeneration = new List<NeuralNetwork1>();
        this.populationFitness = 0f;
        calculateFitness();
        for (int i = 0; i < numGenomes/2 + 2; i += 2) 
        {
            int[] parents = returnParents();
            NeuralNetwork1[] children = Breed(population[parents[0]], population[parents[1]]);

            Mutate(children[0]);
            Mutate(children[1]);

            nextGeneration.Add(children[0]);
            nextGeneration.Add(children[1]);
        }

        for (int i = 0; i < numGenomes / 2 + 2; i += 2)
        {
            int[] parents = returnParents();
            NeuralNetwork1[] children = MutateTwoMembers(population[parents[0]], population[parents[1]]);

            nextGeneration.Add(children[0]);
            nextGeneration.Add(children[1]);
        }

        for (int i = 0; i < numGenomes; i++)
        {
            generation.Add(nextGeneration[i]);
            population[i] = nextGeneration[i];
        }
    }

    public void calculateFitness()
    {
        nonDom = new List<Point_Double>();
        dom = new List<Point_Double>();
        var is_effiecient = Enumerable.Repeat(true, population.Count).ToList();

        for (int i = 0; i < population.Count; i++)
        {
            
            if (is_effiecient[i])
            {
               
                for (int j = 0; j < population.Count; j++)
                {
                    
                    if (population[i].fitness_metrics.x >= population[j].fitness_metrics.x && population[i].fitness_metrics.y >= population[j].fitness_metrics.y)
                    {
                        is_effiecient[j] = false;
                    }
                }
                is_effiecient[i] = true;
            }
        }
        
        for (int i = 0; i < is_effiecient.Count; i++)
        {
            if (is_effiecient[i])
            {
                nonDom.Add(population[i].fitness_metrics);
                nonDom[nonDom.Count - 1].eucDis = 0;
                nonDom[nonDom.Count - 1].number = i;
            }
            else
            {
                dom.Add(population[i].fitness_metrics);
                dom[dom.Count - 1].number = i;
            }
        }
        
        double eucD;
        for (int i = 0; i < dom.Count; i++)
        {
            dom[i].eucDis = GetDistance(dom[i].x/340, nonDom[0].x/340, dom[i].y/dom[i].timeElapsed, nonDom[0].y / nonDom[0].timeElapsed);
            for (int j = 1; j < nonDom.Count; j++)
            {
                eucD = GetDistance(dom[i].x / 340, nonDom[j].x / 340, dom[i].y / dom[i].timeElapsed, nonDom[j].y / nonDom[j].timeElapsed);
                if (eucD < dom[i].eucDis)
                {
                    dom[i].eucDis = eucD;
                }
            }
        }
        
        dom.Sort();

        nonDom.Sort((x, y) => x.y.CompareTo(y.y));
        
        int popNum = population.Count;

        float populationSum = popNum * (popNum + 1) / 2;
       
        int popIterate = 1;
        dom[0].fitness_Ratio = 1 / populationSum;
        for (int i = 1; i < dom.Count; i++)
        {
            dom[i].fitness_Ratio = ((i + 1) * ((i + 1) + 1) / 2) / populationSum;
            dom[i].fitness_Ratio = Math.Pow(dom[i].fitness_Ratio, 3);
            popIterate++;
        }
        for (int i = 0; i < nonDom.Count; i++)
        {
            nonDom[i].fitness_Ratio = ((popIterate + 1) * ((popIterate + 1) + 1) / 2) / populationSum;
            nonDom[i].fitness_Ratio = Math.Pow(nonDom[i].fitness_Ratio,3);
            popIterate++;
        }
        
    }

    public int [] returnParents()
    {
        double chance = UnityEngine.Random.Range(0f, 100000000f) / 100000000;
        double chance2 = UnityEngine.Random.Range(0f, 100000000f) / 100000000;

        int [] parents = new int[2];
        bool [] parentsPicked = new bool[2];
        parentsPicked[0] = false;
        parentsPicked[1] = false;
        for (int i = 0; i < dom.Count; i++)
        {
            if(dom[i].fitness_Ratio > chance && !parentsPicked[0])
            {
                parents[0] = dom[i].number;
                parentsPicked[0] = true;
                
            } else if(dom[i].fitness_Ratio > chance2 && !parentsPicked[1])
            {
                parents[1] = dom[i].number;
                parentsPicked[1] = true;
            }
            if(parentsPicked[1] && parentsPicked[0])
            {
                break;
            }
        }
        if (!parentsPicked[0])
        {
            for(int i = 0; i < nonDom.Count; i++)
            {
                if (nonDom[i].fitness_Ratio > chance)
                {
                    parents[0] = nonDom[i].number;
                    parentsPicked[0] = true;
                    break;
                }
            }
        }
        if (!parentsPicked[1])
        {
            for (int i = 0; i < nonDom.Count; i++)
            {
                if (nonDom[i].fitness_Ratio > chance)
                {
                    parents[1] = nonDom[i].number;
                    parentsPicked[0] = true;
                    break;
                }
            }
        }

        return parents;
    }

    public void calculateFitness2()
    {
        nonDom = new List<Point_Double>();
        dom = new List<Point_Double>();
        var is_effiecient = Enumerable.Repeat(true, generation.Count).ToList();

        for (int i = 0; i < generation.Count; i++)
        {

            if (is_effiecient[i])
            {
                for (int j = 0; j < generation.Count; j++)
                {

                    if (generation[i].fitness_metrics.x >= generation[j].fitness_metrics.x && generation[i].fitness_metrics.y >= generation[j].fitness_metrics.y)
                    {
                        is_effiecient[j] = false;
                    }
                }
                is_effiecient[i] = true;
            }
        }

        for (int i = 0; i < is_effiecient.Count; i++)
        {

            if (is_effiecient[i])
            {
                nonDom.Add(generation[i].fitness_metrics);
                nonDom[nonDom.Count - 1].eucDis = 0;
                nonDom[nonDom.Count - 1].number = i;
            }
            else
            {
                dom.Add(generation[i].fitness_metrics);
                dom[dom.Count - 1].number = i;
            }
        }
        double eucD;
        for (int i = 0; i < dom.Count; i++)
        {
            dom[i].eucDis = GetDistance(dom[i].x / 340, nonDom[0].x / 340, dom[i].y / dom[i].timeElapsed, nonDom[0].y / nonDom[0].timeElapsed);
            for (int j = 1; j < nonDom.Count; j++)
            {
                eucD = GetDistance(dom[i].x / 340, nonDom[j].x / 340, dom[i].y / dom[i].timeElapsed, nonDom[j].y / nonDom[j].timeElapsed);
                if (eucD < dom[i].eucDis)
                {
                    dom[i].eucDis = eucD;
                }
            }
        }
        dom.Sort();

        nonDom.Sort((x, y) => x.y.CompareTo(y.y));

        for(int i = 0; i < numGenomes; i++)
        {
            dom[i].fitness_Ratio = 0;
        }

        int popNum = population.Count;

        float populationSum = popNum * (popNum + 1) / 2;

        int popIterate = 1;
        dom[numGenomes].fitness_Ratio = 1 / populationSum;
        for (int i = numGenomes + 1; i < dom.Count; i++)
        {
            dom[i].fitness_Ratio = ((popIterate + 1) * ((popIterate + 1) + 1) / 2) / populationSum;
            dom[i].fitness_Ratio = Math.Pow(dom[i].fitness_Ratio, 3);
            popIterate++;
        }
        for (int i = 0; i < nonDom.Count; i++)
        {
            nonDom[i].fitness_Ratio = ((popIterate + 1) * ((popIterate + 1) + 1) / 2) / populationSum;
            nonDom[i].fitness_Ratio = Math.Pow(nonDom[i].fitness_Ratio, 3);
            popIterate++;
        }
    }

    public void NextGenerationKidsTwoIndex()
    {
        this.nextGeneration = new List<NeuralNetwork1>();
        this.populationFitness = 0f;
        calculateFitness2();
        for (int i = 0; i < numGenomes / 2; i += 2)
        {
            int[] parents = returnParents();
            NeuralNetwork1[] children = Breed(generation[parents[0]], generation[parents[1]]);

            Mutate(children[0]);
            Mutate(children[1]);

            nextGeneration.Add(children[0]);
            nextGeneration.Add(children[1]);
        }

        for (int i = 0; i < numGenomes / 2; i += 2)
        {
            int[] parents = returnParents();
            NeuralNetwork1[] children = MutateTwoMembers(generation[parents[0]], generation[parents[1]]);

            nextGeneration.Add(children[0]);
            nextGeneration.Add(children[1]);
        }

        for (int i = 0; i < numGenomes; i++)
        {
            generation[dom[i].number] = nextGeneration[i];
            population[i] = nextGeneration[i];
        }

    }

    public void Save(string text)
    {
        StreamWriter write = new StreamWriter(text + ".txt", true);

        int[] layerStructure = population[0].layer;

        for (int i = 0; i < layerStructure.Length - 1; i++)
        {
            write.Write(layerStructure[i] + ";");
        }
        write.Write(layerStructure[layerStructure.Length - 1] + ";\n");

        List<float> encoded = population[0].Encode();
        for (int i = 0; i < encoded.Count - 1; i++)
        {
            write.Write(encoded[i] + ";");
        }
        write.Write(encoded[encoded.Count - 1] + "\n");

        write.Close();
    }

    private static double GetDistance(double x1, double x2, double y1, double y2)
    {
        return Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
    }

}
