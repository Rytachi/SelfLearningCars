using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.CodeDom.Compiler;

public class GeneticControllerOneCriteria
{
    public List<NeuralNetwork1> population;
    public List<NeuralNetwork1> nextGeneration;
    public List<NeuralNetwork1> generation;
    private double populationFitness;
    public float mutationRate;
    public float averageFitness;
    int popSize;
    List<Point_Double> nonDom;
    List<Point_Double> dom;
    public int numGenomes = 400;
    public GeneticControllerOneCriteria(int popSize, float mutationRate, int[] strukt)
    {
        this.generation = new List<NeuralNetwork1>(popSize + numGenomes);
        this.population = new List<NeuralNetwork1>(popSize);
        this.populationFitness = 0f;
        this.mutationRate = mutationRate;
        this.averageFitness = 0f;
        this.popSize = popSize;

        for (int i = 0; i < popSize; i++)
        {
            NeuralNetwork1 nn = new NeuralNetwork1(strukt);
            this.population.Add(nn);
            this.generation.Add(nn);
        }

    }

    public void Crossover(List<float> mother, List<float> father)
    {
        List<float> tempM = new List<float>();
        List<float> tempF = new List<float>();
        for (int i = 0; i < mother.Count; i++)
        {

            if (UnityEngine.Random.Range(0, 1f) > .5)
            {
                tempM.Add(father[i]);
                tempF.Add(mother[i]);
            }
            else
            {
                tempM.Add(mother[i]);
                tempF.Add(father[i]);
            }


        }
        mother.RemoveRange(0, mother.Count);
        father.RemoveRange(0, mother.Count);

        mother.InsertRange(0, tempF);
        father.InsertRange(0, tempM);

    }


    public NeuralNetwork1[] Breed(NeuralNetwork1 mother, NeuralNetwork1 father)
    {
        NeuralNetwork1 child1 = new NeuralNetwork1(mother.layer);
        NeuralNetwork1 child2 = new NeuralNetwork1(mother.layer);

        List<float> motherChromosome = mother.Encode();
        List<float> fatherChromosome = father.Encode();

        Crossover(motherChromosome, fatherChromosome);

        child1.Decode(motherChromosome);
        child2.Decode(fatherChromosome);

        return new NeuralNetwork1[] { child1, child2 };
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

    public void Mutate(NeuralNetwork1 creature)
    {
        List<float> chromosome = creature.Encode();
        for (int i = 0; i < chromosome.Count; i++)
        {
            if (this.mutationRate > UnityEngine.Random.Range(0f, 1f))
            {

                chromosome[i] = chromosome[i] + chromosome[i] * UnityEngine.Random.Range(-0.5f, 0.5f);
            }
        }

        creature.Decode(chromosome);

    }

    public void NextGeneration()
    {

        this.nextGeneration = new List<NeuralNetwork1>();
        this.populationFitness = 0f;

        population.Sort((x, y) => x.fitness.CompareTo(y.fitness));

        double populationSum = popSize * (popSize + 1) / 2;

        this.population[0].fitnessRatio = 1 / (float)populationSum;
        for (int i = 1; i < this.population.Count; i++)
        {
            this.population[i].fitnessRatio = ((i + 1) * ((i + 1) + 1) / 2) / (float)populationSum;
            this.population[i].fitnessRatio = Math.Pow(population[i].fitnessRatio, 3);
        }

        for (int i = 0; i < numGenomes / 2 + 2; i += 2)
        {
            int[] parents = returnParents();
            NeuralNetwork1[] children = Breed(population[parents[0]], population[parents[1]]);

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

    public int[] returnParents()
    {
        double chance = UnityEngine.Random.Range(0f, 100000000f) / 100000000;
        double chance2 = UnityEngine.Random.Range(0f, 100000000f) / 100000000;

        int[] parents = new int[2];
        bool[] parentsPicked = new bool[2];
        parentsPicked[0] = false;
        parentsPicked[1] = false;
        for (int i = 0; i < population.Count; i++)
        {
            if (population[i].fitnessRatio > chance && !parentsPicked[0])
            {
                parents[0] = i;
                parentsPicked[0] = true;

            }
            else if (population[i].fitnessRatio > chance2 && !parentsPicked[1])
            {
                parents[1] = i;
                parentsPicked[1] = true;
            }
            if (parentsPicked[1] && parentsPicked[0])
            {
                break;
            }
        }

        return parents;
    }

    public void KidsGenerationAdding()
    {
        this.nextGeneration = new List<NeuralNetwork1>();
        generation.Sort((x, y) => x.fitness.CompareTo(y.fitness));
        for (int i = 0; i < popSize; i++)
        {

            population[i] = generation[numGenomes + i];
        }

        double populationSum = popSize * (popSize + 1) / 2;

        this.population[0].fitnessRatio = 1 / (float)populationSum;
        for (int i = 1; i < this.population.Count; i++)
        {
            this.population[i].fitnessRatio = ((i + 1) * ((i + 1) + 1) / 2) / (float)populationSum;
            this.population[i].fitnessRatio = Math.Pow(population[i].fitnessRatio, 5);

        }

        for (int i = 0; i < numGenomes / 2 + 2; i += 2)
        {
            int[] parents = returnParents();
            NeuralNetwork1[] children = Breed(population[parents[0]], population[parents[1]]);

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
            generation[i] = nextGeneration[i];
            population[i] = nextGeneration[i];
        }

    }
}
