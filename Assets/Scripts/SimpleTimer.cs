using UnityEngine;
using TMPro; // Necessário para usar TextMeshPro

public class SimpleTimer : MonoBehaviour
{
    [Header("Configuração da UI")]
    [Tooltip("Arraste o objeto de Texto do Canvas para aqui")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI genNum;
    public CabritoController cabritoController;

    [Header("Opções")]
    public bool startOnAwake = true;

    private float elapsedTime = 0f;
    private bool isRunning = false;

    void Start()
    {
        if (startOnAwake) isRunning = true;
    }

    void Update()
    {
        if (isRunning)
        {
            elapsedTime += Time.deltaTime;
            UpdateDisplay();
        }
    }

    void UpdateDisplay()
    {
        // Calcula os valores matemáticos
        float minutes = Mathf.FloorToInt(elapsedTime / 60);
        float seconds = Mathf.FloorToInt(elapsedTime % 60);
        float milliseconds = (elapsedTime % 1) * 1000;

        // Formata a string: 
        // {0:00} = Minutos com 2 dígitos
        // {1:00} = Segundos com 2 dígitos
        // {2:000} = Milésimos com 3 dígitos
        timerText.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
        genNum.text = "Geração atual: " + cabritoController.currentGeneration.ToString();
    }

    // Funções públicas para controlar externamente (se precisar)
    public void StopTimer() => isRunning = false;
    public void ResumeTimer() => isRunning = true;

    public void ResetTimer()
    {
        elapsedTime = 0f;
        UpdateDisplay();
    }
}