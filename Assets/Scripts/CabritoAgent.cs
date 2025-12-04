using UnityEngine;
using UnityEngine.InputSystem; // Requer New Input System package
using System.Collections.Generic;

public class CabritoAgent : MonoBehaviour
{
    [Header("Network & AI")]
    public SimpleNeuralNet brain;
    public bool useAI = true;


    [Header("Survival Settings")]
    public float maxTimeWithoutCheckpoint = 180f; //tempo em segundos
    private float timeSinceLastCheckpoint = 0f;


    private List<Transform> checkpoints;
    private int currentCheckpointIndex = 0;
    private Transform currentTarget;


    public float acceleration = 20f;
    public float rotationSpeed = 100f;
    public LayerMask obstacleLayer;


    public Transform sensorOrigin;
    public float sensorRange = 15f;
    // Ângulos dos 5 sensores
    private float[] sensorAngles = new float[] { -60, -30, 0, 30, 60 };


    public bool isAlive = true;
    public bool reachedTarget = false;


    public float fitness = 0f; // Variável local para visualização
    private float initialDistanceToTarget;
    private float fitnessBonus = 0f;


    private Rigidbody rb;
    private Vector2 manualInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void ResetAgent(Vector3 pos, Quaternion rot, List<Transform> levelCheckpoints)
    {
        transform.position = pos;
        transform.rotation = rot;


        this.checkpoints = new List<Transform>(levelCheckpoints);


        currentCheckpointIndex = 0;
        fitnessBonus = 0f;
        fitness = 0f;


        timeSinceLastCheckpoint = 0f;


        if (checkpoints != null && checkpoints.Count > 0)
        {
            currentTarget = checkpoints[currentCheckpointIndex];
            initialDistanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
        }


        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        isAlive = true;
        reachedTarget = false;


        // Garante que a física e renderização estejam ativas
        GetComponent<Collider>().enabled = true;
        foreach (var mesh in GetComponentsInChildren<MeshRenderer>()) mesh.enabled = true;
    }

    void FixedUpdate()
    {
        if (!isAlive || reachedTarget) return;

        CheckSurvivalTimer();
        if (!isAlive) return;

        float steer = 0f;
        float gas = 0f;

        float[] sensorReadings = GetSensorReadings();

        if (useAI && brain != null)
        {
            float[] outputs = brain.FeedForward(sensorReadings);

            if (outputs.Length >= 2)
            {
                steer = outputs[0];
                gas = Mathf.Clamp01(outputs[1]);
            }
            else if (outputs.Length == 1) //fallback
            {
                steer = outputs[0];
                gas = 1f;
            }
        }
        else
        {
            // Cloning Behaviour
            steer = manualInput.x;
            gas = Mathf.Clamp01(manualInput.y);
        }

        Move(steer, gas);

        CalculateFitness();
    }

    private void CheckSurvivalTimer()
    {
        timeSinceLastCheckpoint += Time.fixedDeltaTime;

        if (timeSinceLastCheckpoint >= maxTimeWithoutCheckpoint)
        {
            // Opcional: Penalidade extra no fitness por morrer de "velhice/preguiça"
            if (brain != null) brain.fitness -= 50f;

            Debug.Log($"{name} morreu por inatividade (Time Out).");
            Die();
        }
    }

    private float[] GetSensorReadings()
    {
        float[] readings = new float[sensorAngles.Length + 2];
        for (int i = 0; i < sensorAngles.Length; i++)
        {
            Vector3 dir = Quaternion.Euler(0, sensorAngles[i], 0) * transform.forward;
            Ray ray = new Ray(sensorOrigin.position, dir);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, sensorRange, obstacleLayer))
            {
                // Normaliza: 1.0 (Muito perto) a 0.0 (Longe)
                readings[i] = 1f - (hit.distance / sensorRange);
                Debug.DrawLine(sensorOrigin.position, hit.point, Color.red);
            }
            else
            {
                readings[i] = 0f;
                Debug.DrawLine(sensorOrigin.position, sensorOrigin.position + dir * sensorRange, Color.green);
            }
        }

        if (currentTarget != null)
        {
            Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
            Vector3 localDir = transform.InverseTransformDirection(directionToTarget);

            readings[5] = localDir.x; // O alvo está à esquerda ou direita?
            readings[6] = localDir.z; // O alvo está na frente ou atrás?
        }
        else
        {
            readings[5] = 0f;
            readings[6] = 0f;
        }
        return readings;
    }

    private void Move(float steer, float gas)
    {
        transform.Rotate(Vector3.up * steer * rotationSpeed * Time.fixedDeltaTime);
        rb.AddForce(transform.forward * gas * acceleration, ForceMode.Acceleration);
    }

    private void CalculateFitness()
    {
        if (currentTarget == null) return;

        float currentDistance = Vector3.Distance(transform.position, currentTarget.position);
        float progressToCurrent = initialDistanceToTarget - currentDistance;

        // Fitness Total = Bônus acumulado dos checkpoints passados + progresso para o atual
        // Penalidade de tempo reduzida para não matar agentes que demoram em labirintos
        float velocityPenalty = rb.linearVelocity.magnitude < 1f ? 0.1f : 0f;

        fitness = fitnessBonus + Mathf.Max(0, progressToCurrent) - (Time.fixedDeltaTime * 0.01f) - velocityPenalty;

        if (brain != null)
        {
            brain.fitness = fitness;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isAlive && !reachedTarget) return;

        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Obstacle"))
        {
            Die();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Verifica se bateu em um checkpoint ou na linha de chegada
        if (other.CompareTag("Checkpoint") || other.CompareTag("FinishLine"))
        {
            // Verifica se é o objeto do alvo ATUAL (para evitar pular checkpoints sem querer)
            if (other.transform == currentTarget)
            {
                NextCheckpoint();
            }
        }
    }

    private void NextCheckpoint()
    {
        // 1. Dá uma recompensa grande por ter chegado aqui
        fitnessBonus += 500f;

        // 2. Avança o índice
        currentCheckpointIndex++;
        timeSinceLastCheckpoint = 0f;

        // 3. Verifica se acabou a lista
        if (currentCheckpointIndex >= checkpoints.Count)
        {
            Win();
        }
        else
        {
            // 4. Define o novo alvo e reseta a distância de referência
            currentTarget = checkpoints[currentCheckpointIndex];
            initialDistanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
            Debug.Log("Checkpoint alcançado! Indo para o próximo...");
        }
    }

    public void Die()
    {
        isAlive = false;
        // Congela física
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        // morte (ficar de cabeça pra baixo)
        Vector3 tempPosition = transform.position;
        tempPosition.y = 0.25f;
        transform.position = tempPosition;
        Vector3 currentRotation = transform.localEulerAngles;
        currentRotation.x = 180f;
        transform.localEulerAngles = currentRotation;
    }

    public void Win()
    {
        if (reachedTarget) return;
        reachedTarget = true;
        isAlive = false;

        // Bônus final massivo
        if (brain != null) brain.fitness += 2000f;

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;

        Debug.Log("Circuito Finalizado!");
    }

    public void SetInput(Vector2 input)
    {
        manualInput = input;
    }
}