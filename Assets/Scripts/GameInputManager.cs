using UnityEngine;
using UnityEngine.InputSystem;

public class GameInputManager : MonoBehaviour
{
    [Header("Dependencies")]
    public CabritoController cabritoController;

    // Agora usamos câmeras padrões do Unity
    public Camera freeCam;
    public Camera agentCam;

    // Referência ao script de movimento da câmera livre (deve estar na FreeCam)
    public MonoBehaviour freeCamScript;

    [Header("Agent Camera Settings")]
    public Vector3 followOffset = new Vector3(0, 5, -8); // Posição atrás e acima do agente
    public float smoothSpeed = 5f; // Suavidade do movimento

    [Header("Input Settings")]
    public InputActionAsset inputAsset;

    private InputActionMap freeLookMap;
    private InputActionMap agentControlMap;

    private CabritoAgent currentTarget;
    private bool isControllingAgent = false;

    void Awake()
    {
        // Localiza os mapas
        freeLookMap = inputAsset.FindActionMap("Camera");
        agentControlMap = inputAsset.FindActionMap("Cabrito");

        // Configura callback para troca de câmera
        var switchActionFree = freeLookMap.FindAction("GlobalChange");
        var switchActionAgent = agentControlMap.FindAction("GlobalChange");

        if (switchActionFree != null) switchActionFree.performed += ctx => ToggleControlMode();
        if (switchActionAgent != null) switchActionAgent.performed += ctx => ToggleControlMode();

        // Callback de movimento do agente
        var moveAction = agentControlMap.FindAction("Move");
        if (moveAction != null)
        {
            moveAction.performed += ctx => SendInputToAgent(ctx.ReadValue<Vector2>());
            moveAction.canceled += ctx => SendInputToAgent(Vector2.zero);
        }
    }

    void OnEnable()
    {
        inputAsset.Enable();
        // Garante estado inicial (FreeCam ativa, AgentCam desativada)
        SetMode(false);
    }

    void ToggleControlMode()
    {
        SetMode(!isControllingAgent);
    }

    void SetMode(bool agentMode)
    {
        isControllingAgent = agentMode;

        if (isControllingAgent)
        {
            // Tenta obter um agente vivo
            currentTarget = cabritoController.GetFirstActiveAgent();

            if (currentTarget != null)
            {
                // 1. INPUT: Troca Mapas
                freeLookMap.Disable();
                agentControlMap.Enable();

                // 2. CÂMERA: Troca a câmera ativa
                freeCam.gameObject.SetActive(false);
                agentCam.gameObject.SetActive(true);

                // Posiciona a câmera do agente imediatamente no primeiro frame para evitar "pulo" visual
                Vector3 desiredPos = currentTarget.transform.position + currentTarget.transform.TransformDirection(followOffset); // Local Space
                                                                                                                                  // Ou se preferir offset global (sem girar com o agente): 
                                                                                                                                  // Vector3 desiredPos = currentTarget.transform.position + followOffset;

                agentCam.transform.position = desiredPos;
                agentCam.transform.LookAt(currentTarget.transform);

                // 3. LÓGICA: Desabilita IA e script de câmera livre
                currentTarget.useAI = false;
                if (freeCamScript) freeCamScript.enabled = false;

                Debug.Log($"Controle Assumido: {currentTarget.name}");
            }
            else
            {
                Debug.LogWarning("Nenhum agente disponível para controle.");
                SetMode(false); // Reverte se falhar
            }
        }
        else
        {
            // Volta para Free Look
            agentControlMap.Disable();
            freeLookMap.Enable();

            // Troca Câmeras
            agentCam.gameObject.SetActive(false);
            freeCam.gameObject.SetActive(true);

            if (freeCamScript) freeCamScript.enabled = true;

            // Retorna controle para IA
            if (currentTarget != null)
            {
                currentTarget.useAI = true;
                currentTarget.SetInput(Vector2.zero); // Zera inércia
                currentTarget = null;
            }
        }
    }

    void LateUpdate()
    {
        if (isControllingAgent && currentTarget != null)
        {
            // --- Opção 1: Câmera Fixa atrás do agente (estilo Mario Kart / Corrida) ---
            // Calcula posição desejada baseada na rotação do agente
            Vector3 desiredPosition = currentTarget.transform.position + (currentTarget.transform.rotation * followOffset);

            // --- Opção 2: Câmera com Offset Global (não gira junto com o agente, bom para RPG top-down) ---
            // Vector3 desiredPosition = currentTarget.transform.position + followOffset;

            // Suavização (Lerp)
            Vector3 smoothedPosition = Vector3.Lerp(agentCam.transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            agentCam.transform.position = smoothedPosition;

            // Faz a câmera olhar para o agente
            agentCam.transform.LookAt(currentTarget.transform.position + Vector3.up * 0.2f);
        }
        else if (isControllingAgent && currentTarget == null)
        {
            // Se o agente morrer enquanto controlamos, volta pra free cam
            SetMode(false);
        }
    }

    void SendInputToAgent(Vector2 input)
    {
        if (isControllingAgent && currentTarget != null)
        {
            currentTarget.SetInput(input);
        }
    }
}