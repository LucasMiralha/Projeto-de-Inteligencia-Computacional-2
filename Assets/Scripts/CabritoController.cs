using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Essencial para LINQ (OrderBy, etc)
using System.IO;   // Para CSV

public class CabritoController : MonoBehaviour
{
    [Header("Population Config")]
    public GameObject agentPrefab;
    public int populationSize = 50;
    public List<Transform> spawnPoints; // Pontos fixos de spawn

    [Header("Evolution Parameters")]
    public float mutationRate = 0.05f;
    public float mutationStrength = 0.5f;
    public int elitismCount = 5; // Quantos melhores sobrevivem puros


    public int maxGenerations = 100;
    public float timeScale = 1.0f;
    public int currentGeneration = 0;


    public ObstacleSpawner obstacleSpawner;

    // Topologia: 5 inputs, 2 hidden layers de 10, 2 outputs
    private int[] networkStructure = new int[] { 5, 10, 10, 2 };

    private List<SimpleNeuralNet> population;
    private List<CabritoAgent> activeAgents;

    // Caminho do CSV
    private string csvPath;

    void Start()
    {
        // Setup CSV
        csvPath = Application.dataPath + "/evolution_data.csv";
        // Cria cabeçalho se arquivo não existe ou sobrescreve
        File.WriteAllText(csvPath, "Generation,BestFitness,AvgFitness,WorstFitness\n");

        InitPopulation();
    }

    void InitPopulation()
    {
        population = new List<SimpleNeuralNet>();
        activeAgents = new List<CabritoAgent>();

        // Validação de segurança de spawn
        if (populationSize > spawnPoints.Count)
        {
            Debug.LogError("Erro Crítico: População maior que Spawn Points disponíveis!");
            populationSize = spawnPoints.Count; // Ajusta automaticamente
        }

        for (int i = 0; i < populationSize; i++)
        {
            SimpleNeuralNet net = new SimpleNeuralNet(networkStructure);
            population.Add(net);
        }

        CreateAgents();
    }

    void CreateAgents()
    {
        // Limpa agentes antigos
        foreach (var agent in activeAgents)
        {
            if (agent != null) Destroy(agent.gameObject);
        }
        activeAgents.Clear();

        obstacleSpawner.SpawnObstacles();

        // Embaralha spawn points para aleatoriedade de posição
        // Usamos uma lista temporária embaralhada
        List<Transform> shuffledSpawns = spawnPoints.OrderBy(x => UnityEngine.Random.value).ToList();

        for (int i = 0; i < populationSize; i++)
        {
            Transform spawn = shuffledSpawns[i];
            GameObject go = Instantiate(agentPrefab, spawn.position, spawn.rotation);
            CabritoAgent agent = go.GetComponent<CabritoAgent>();

            // Injeta o cérebro
            agent.brain = population[i];
            agent.ResetAgent(spawn.position, spawn.rotation);

            activeAgents.Add(agent);
        }
    }

    void Update()
    {
        // Controle de Tempo da Engine
        Time.timeScale = timeScale;

        // Verifica se todos morreram
        bool anyoneAlive = false;
        foreach (var agent in activeAgents)
        {
            if (agent.isAlive)
            {
                anyoneAlive = true;
                break;
            }
        }

        if (!anyoneAlive)
        {
            Evolve();
        }
    }

    void Evolve()
    {
        currentGeneration++;
        if (currentGeneration >= maxGenerations)
        {
            Debug.Log("Experimento finalizado.");
            Time.timeScale = 0; // Pausa
            return;
        }

        // 1. Avaliação e Ordenação (Decrescente: Melhor -> Pior)
        population.Sort();
        population.Reverse();

        // 2. Coleta de Dados e CSV
        float bestFit = population[0].fitness;
        float worstFit = population[population.Count - 1].fitness;
        float avgFit = 0f;
        foreach (var n in population) avgFit += n.fitness;
        avgFit /= population.Count;

        LogToCSV(currentGeneration, bestFit, avgFit, worstFit);
        Debug.Log($"Gen {currentGeneration}: Best={bestFit:F2} Avg={avgFit:F2}");

        // 3. Seleção e Reprodução
        List<SimpleNeuralNet> newPop = new List<SimpleNeuralNet>();

        // Elitismo: Mantém os N melhores sem alteração
        for (int i = 0; i < elitismCount; i++)
        {
            newPop.Add(new SimpleNeuralNet(population[i]));
        }

        // Preenche o resto
        while (newPop.Count < populationSize)
        {
            // Seleção por Torneio (Pega 2 aleatórios, vence o melhor)
            SimpleNeuralNet parent1 = TournamentSelection();
            SimpleNeuralNet parent2 = TournamentSelection();

            // Crossover simples (neste caso, copia Parent1 e muta. Pode ser expandido)
            SimpleNeuralNet child = new SimpleNeuralNet(parent1);

            // Mutação
            child.Mutate(mutationRate, mutationStrength);

            newPop.Add(child);
        }

        population = newPop;

        // Reinicia obstáculos (para garantir que agentes não decorem apenas um layout fixo)
        FindAnyObjectByType<ObstacleSpawner>()?.SpawnObstacles();

        // Recria agentes físicos
        CreateAgents();
    }

    SimpleNeuralNet TournamentSelection()
    {
        // Pega 3 aleatórios, retorna o melhor
        int k = 3;
        SimpleNeuralNet best = null;
        for (int i = 0; i < k; i++)
        {
            SimpleNeuralNet candidate = population[UnityEngine.Random.Range(0, population.Count)];
            if (best == null || candidate.fitness > best.fitness)
                best = candidate;
        }
        return best;
    }

    void LogToCSV(int gen, float best, float avg, float worst)
    {
        string line = $"{gen},{best.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)},{avg.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)},{worst.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}\n";
        File.AppendAllText(csvPath, line);
    }

    // Métodos para o Input Switcher (Câmera)
    public CabritoAgent GetFirstActiveAgent()
    {
        // Retorna o primeiro da lista que ainda está vivo, ou o primeiro da lista geral
        var alive = activeAgents.FirstOrDefault(a => a.isAlive);
        if (alive != null) return alive;
        return activeAgents.Count > 0 ? activeAgents[0] : null;
    }
}