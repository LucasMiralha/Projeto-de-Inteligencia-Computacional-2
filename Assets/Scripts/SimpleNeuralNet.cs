using System;
using System.Collections.Generic;
using UnityEngine;


public class SimpleNeuralNet : IComparable<SimpleNeuralNet>
{
    // Estrutura da rede: índices representam camadas, valores representam qtd neurônios
    private int[] layers;
    private float[][] neurons; // Matriz de valores dos neurônios
    private float[][][] weights; // Matriz tridimensional de pesos [camada][neuronio_origem][neuronio_destino]

    public float fitness; // Avaliação de desempenho do genótipo

    // Construtor para inicialização aleatória (Gênesis)
    public SimpleNeuralNet(int[] layers)
    {
        this.layers = new int[layers.Length];
        for (int i = 0; i < layers.Length; i++) this.layers[i] = layers[i];

        InitNeurons();
        InitWeights();
    }

    // Construtor de Cópia Profunda (Deep Copy) para Clonagem/Reprodução
    public SimpleNeuralNet(SimpleNeuralNet copyNetwork)
    {
        this.layers = new int[copyNetwork.layers.Length];
        for (int i = 0; i < layers.Length; i++) this.layers[i] = copyNetwork.layers[i];

        InitNeurons();
        InitWeights();
        CopyWeights(copyNetwork.weights);
    }

    private void InitNeurons()
    {
        List<float[]> neuronsList = new List<float[]>();
        for (int i = 0; i < layers.Length; i++)
        {
            neuronsList.Add(new float[layers[i]]);
        }
        neurons = neuronsList.ToArray();
    }

    private void InitWeights()
    {
        List<float[][]> weightsList = new List<float[][]>();

        // Itera de 1 até o fim, pois a camada 0 (input) não tem pesos de entrada na rede
        for (int i = 1; i < layers.Length; i++)
        {
            List<float[]> layerWeightsList = new List<float[]>();
            int neuronsInPreviousLayer = layers[i - 1];

            for (int j = 0; j < layers[i]; j++)
            {
                float[] neuronWeights = new float[neuronsInPreviousLayer];
                for (int k = 0; k < neuronsInPreviousLayer; k++)
                {
                    // Inicialização de pesos entre -0.5 e 0.5 (Xavier simplificado)
                    neuronWeights[k] = UnityEngine.Random.Range(-0.5f, 0.5f);
                }
                layerWeightsList.Add(neuronWeights);
            }
            weightsList.Add(layerWeightsList.ToArray());
        }
        weights = weightsList.ToArray();
    }

    private void CopyWeights(float[][][] copyWeights)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    weights[i][j][k] = copyWeights[i][j][k];
                }
            }
        }
    }

    // Processamento FeedForward (O "Pensamento" da rede)
    public float[] FeedForward(float[] inputs)
    {
        // Alimenta a camada de entrada
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }

        // Propaga o sinal
        for (int i = 1; i < layers.Length; i++)
        {
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float value = 0f; // Soma ponderada (Bias poderia ser adicionado aqui)
                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    value += weights[i - 1][j][k] * neurons[i - 1][k];
                }
                // Função de Ativação: Tanh para permitir saídas negativas (Direção)
                neurons[i][j] = (float)Math.Tanh(value);
            }
        }
        return neurons[neurons.Length - 1]; // Retorna camada de saída
    }

    // Operador Genético: Mutação
    public void Mutate(float chance, float val)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    if (UnityEngine.Random.value < chance)
                    {
                        weights[i][j][k] += UnityEngine.Random.Range(-val, val);
                        // Clamp opcional para evitar explosão de pesos
                        weights[i][j][k] = Mathf.Clamp(weights[i][j][k], -1f, 1f);
                    }
                }
            }
        }
    }

    // Implementação da interface IComparable para ordenação (Sort)
    public int CompareTo(SimpleNeuralNet other)
    {
        if (other == null) return 1;
        if (fitness > other.fitness) return 1;
        else if (fitness < other.fitness) return -1;
        else return 0;
    }
}