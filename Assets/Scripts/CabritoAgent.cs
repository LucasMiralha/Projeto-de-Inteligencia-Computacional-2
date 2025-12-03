using UnityEngine;
using UnityEngine.InputSystem; // Requer New Input System package

public class CabritoAgent : MonoBehaviour
{
    [Header("Network & AI")]
    public SimpleNeuralNet brain;
    public bool useAI = true;


    public float acceleration = 20f;
    public float rotationSpeed = 100f;
    public LayerMask obstacleLayer;


    public Transform sensorOrigin;
    public float sensorRange = 15f;
    // Ângulos dos 5 sensores
    private float[] sensorAngles = new float[] { -60, -30, 0, 30, 60 };


    public bool isAlive = true;
    public float distanceTravelled = 0f;
    private Vector3 startPos;
    private Vector3 lastPos;
    private Rigidbody rb;

    
    private Vector2 manualInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
        lastPos = startPos;
    }

    public void ResetAgent(Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        isAlive = true;
        distanceTravelled = 0f;
        lastPos = pos;

        // Garante que a física e renderização estejam ativas
        GetComponent<Collider>().enabled = true;
        foreach (var mesh in GetComponentsInChildren<MeshRenderer>()) mesh.enabled = true;
    }

    void FixedUpdate()
    {
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
            else if (outputs.Length == 1)
            {
                // Fallback se a rede só tiver 1 saída (apenas vira, aceleração constante)
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

    private float[] GetSensorReadings()
    {
        float[] readings = new float[sensorAngles.Length];
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
        return readings;
    }

    private void Move(float steer, float gas)
    {
        transform.Rotate(Vector3.up * steer * rotationSpeed * Time.fixedDeltaTime);
        rb.AddForce(transform.forward * gas * acceleration, ForceMode.Acceleration);
    }

    private void CalculateFitness()
    {
        float distDelta = Vector3.Distance(transform.position, lastPos);

        // Penalidade simples para girar parado
        // Se girou muito mas não saiu do lugar, não ganha pontos
        if (distDelta > 0.01f)
        {
            distanceTravelled += distDelta;
        }

        lastPos = transform.position;

        if (brain != null)
        {
            // Função de Fitness: Distância + Bônus de Sobrevivência
            brain.fitness = distanceTravelled;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isAlive) return;

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Die();
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

    public void SetInput(Vector2 input)
    {
        manualInput = input;
    }
}