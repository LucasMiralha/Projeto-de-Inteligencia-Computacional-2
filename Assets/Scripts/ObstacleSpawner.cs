using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Configurações de Spawn")]
    public GameObject obstaclePrefab; // O objeto a ser spawnado

    [Tooltip("Quantidade desejada de objetos.")]
    public int obstaclesToSpawn = 10;

    [Header("Lista de Posições")]
    [Tooltip("Arraste aqui os Transforms que marcam os locais possíveis.")]
    public List<Transform> spawnPositions;

    // Lista privada para controlar o que está em cena agora
    private List<GameObject> currentObstacles = new List<GameObject>();

    public void SpawnObstacles()
    {
        // 1. Limpeza: "Se existir objetos, limpe. Se não, apenas crie."
        // Essa função remove tudo que estiver na lista currentObstacles antes de prosseguir.
        ClearCurrentObstacles();

        // Verificações de segurança
        if (obstaclePrefab == null)
        {
            Debug.LogError("ObstacleSpawner: Prefab do obstáculo não foi atribuído!");
            return;
        }
        if (spawnPositions == null || spawnPositions.Count == 0)
        {
            Debug.LogWarning("ObstacleSpawner: Lista de posições está vazia!");
            return;
        }

        // 2. Garante que o número não ultrapasse os lugares disponíveis
        // Se você pedir 50 objetos mas só tiver 10 lugares, ele usará 10.
        int finalCount = Mathf.Min(obstaclesToSpawn, spawnPositions.Count);

        // Cria uma lista temporária copiada da original.
        // Isso serve para removermos as posições usadas sem estragar a lista original do Inspector.
        List<Transform> availableSpots = new List<Transform>(spawnPositions);

        for (int i = 0; i < finalCount; i++)
        {
            // Escolhe um índice aleatório da lista de disponíveis
            int randomIndex = Random.Range(0, availableSpots.Count);
            Transform selectedSpot = availableSpots[randomIndex];

            // Rotação aleatória (opcional, remova se quiser rotação fixa)
            Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

            // Instancia o objeto
            GameObject newObj = Instantiate(obstaclePrefab, selectedSpot.position, rotation);

            // Adiciona à lista de controle (para podermos deletar depois)
            currentObstacles.Add(newObj);

            // Remove a posição da lista TEMPORÁRIA para não repetir o local nesta rodada
            availableSpots.RemoveAt(randomIndex);
        }

        Debug.Log($"Spawn concluído. {finalCount} objetos criados.");
    }

    private void ClearCurrentObstacles()
    {
        // Verifica se a lista tem algo e destrói os GameObjects da cena
        if (currentObstacles.Count > 0)
        {
            foreach (var obj in currentObstacles)
            {
                if (obj != null) Destroy(obj);
            }
            // Limpa a referência da lista
            currentObstacles.Clear();
        }
    }
}