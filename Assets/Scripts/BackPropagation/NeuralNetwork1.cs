using System;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork1
{

    public int[] layer;
    Layer1[] layers;
    public Point_Double fitness_metrics;
    public float fitness;
    public double fitnessRatio;

    public NeuralNetwork1(int[] layer)
    {
        this.layer = new int[layer.Length];
        for (int i = 0; i < layer.Length; i++)
            this.layer[i] = layer[i];

        layers = new Layer1[layer.Length - 1];

        for (int i = 0; i < layers.Length; i++)
        {
            layers[i] = new Layer1(layer[i], layer[i + 1]);
        }

    }

    public List<float> Encode()
    {
        List<float> weights = new List<float>();

        for (int i = 0; i < layers.Length; i++)
        {
            List<float> list = layers[i].returnWeights();
            foreach(var j in list)
            {
                weights.Add(j);
            }
        }
        return weights;
    }

    public void Decode(List<float> weights)
    {
        layers[0].InitilizeWeights(weights, 0);
        layers[1].InitilizeWeights(weights, layer[0]*layer[1]);
    }

    public float[] FeedForward(float[] inputs)
    {
        layers[0].FeedForwardSig(inputs);
        for (int i = 1; i < layers.Length; i++)
        {
            layers[i].FeedForwardTan(layers[i - 1].outputs);
        }

        return layers[layers.Length - 1].outputs;
    }

    public void BackProp(float[] expected)
    {
        for (int i = layers.Length - 1; i >= 0; i--)
        {
            if (i == layers.Length - 1)
            {
                layers[i].BackPropOutput(expected); 
            }
            else
            {
                layers[i].BackPropHidden(layers[i + 1].gamma, layers[i + 1].weights);
            }
        }

        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].UpdateWeights();
        }
    }

    public class Layer1
    {
        int numberOfInputs;
        int numberOfOuputs;


        public float[] outputs;
        public float[] inputs;
        public float[,] weights;
        public float[,] weightsDelta;
        public float[] gamma;
        public float[] error;

        public static System.Random random = new System.Random();

        public Layer1(int numberOfInputs, int numberOfOuputs)
        {
            this.numberOfInputs = numberOfInputs;
            this.numberOfOuputs = numberOfOuputs;

            outputs = new float[numberOfOuputs];
            inputs = new float[numberOfInputs];
            weights = new float[numberOfOuputs, numberOfInputs];
            weightsDelta = new float[numberOfOuputs, numberOfInputs];
            gamma = new float[numberOfOuputs];
            error = new float[numberOfOuputs];

            InitilizeWeights();
        }

        public void InitilizeWeights()
        {
            for (int i = 0; i < numberOfOuputs; i++)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weights[i, j] = (float)random.NextDouble() - 0.5f;
                }
            }
        }

        public void InitilizeWeights(List<float> weights, int k)
        {
            for (int i = 0; i < numberOfOuputs; i++)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    this.weights[i, j] = weights[k];
                    k++;
                }
            }
        }

        public List<float> returnWeights()
        {
            List<float> list = new List<float>();
            for (int i = 0; i < numberOfOuputs; i++)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    list.Add(weights[i, j]);
                }
            }
            return list;
        }

        public float[] FeedForwardTan(float[] inputs)
        {
            this.inputs = inputs;

            for (int i = 0; i < numberOfOuputs; i++)
            {
                outputs[i] = 0;
                for (int j = 0; j < numberOfInputs; j++)
                {
                    outputs[i] += inputs[j] * weights[i, j];
                }

                outputs[i] = (float)Math.Tanh(outputs[i]);
            }

            return outputs;
        }

        public float[] FeedForwardSig(float[] inputs)
        {
            this.inputs = inputs;

            for (int i = 0; i < numberOfOuputs; i++)
            {
                outputs[i] = 0;
                for (int j = 0; j < numberOfInputs; j++)
                {
                    outputs[i] += inputs[j] * weights[i, j];
                }

                outputs[i] = (float)( 1.0f / (1.0f + (float)Math.Exp(-outputs[i]))); ;
            }

            return outputs;
        }

        public float TanHDer(float value)
        {
            return 1 - (value * value);
        }

        public static float SigDer(float value)
        {
            return value * (1 - value);
        }

        public void BackPropOutput(float[] expected)
        {
            for (int i = 0; i < numberOfOuputs; i++)
            {
                error[i] = outputs[i] - expected[i];
                
            }

            for (int i = 0; i < numberOfOuputs; i++)
                gamma[i] = error[i] * TanHDer(outputs[i]);

            for (int i = 0; i < numberOfOuputs; i++)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weightsDelta[i, j] = gamma[i] * inputs[j];
                }
            }
        }

        public void BackPropHidden(float[] gammaForward, float[,] weightsFoward)
        {
            for (int i = 0; i < numberOfOuputs; i++)
            {
                gamma[i] = 0;

                for (int j = 0; j < gammaForward.Length; j++)
                {
                    gamma[i] += gammaForward[j] * weightsFoward[j, i];
                }

                gamma[i] *= SigDer(outputs[i]);
            }

            for (int i = 0; i < numberOfOuputs; i++)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weightsDelta[i, j] = gamma[i] * inputs[j];
                }
            }
        }

        public void UpdateWeights()
        {
            for (int i = 0; i < numberOfOuputs; i++)
            {
                for (int j = 0; j < numberOfInputs; j++)
                {
                    weights[i, j] -= weightsDelta[i, j] * 0.033f;
                }
            }
        }
    }
}
