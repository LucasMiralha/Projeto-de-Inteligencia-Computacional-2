using UnityEngine;
using UnityEngine.InputSystem;

public class CameraScript : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public float moveSpeed = 10f;
    public float sprintMultiplier = 2f;
    public float climbSpeed = 5f;

    [Header("Configurações de Câmera")]
    public float mouseSensitivity = 0.5f;
    public bool invertY = false;
    public float maxPitchAngle = 85f; // Limite para não dar "loop" vertical

    [Header("Input References (Arraste suas Actions aqui)")]
    public InputActionReference moveAction;      // Ex: WASD (Vector2)
    public InputActionReference lookAction;      // Ex: Mouse Delta (Vector2)
    public InputActionReference ascendAction;    // Ex: Space/Q (Button ou Axis)
    public InputActionReference descendAction;   // Ex: Ctrl/E (Button ou Axis)
    public InputActionReference sprintAction;    // Ex: Shift (Button)
    public InputActionReference toggleCursorAction; // Ex: ESC (Button)

    // Variáveis internas
    private float _pitch = 0f;
    private float _yaw = 0f;
    private bool _isCursorLocked = true;

    private void OnEnable()
    {
        // Habilita as ações automaticamente ao iniciar
        moveAction.action.Enable();
        lookAction.action.Enable();
        ascendAction.action.Enable();
        descendAction.action.Enable();
        sprintAction.action.Enable();
        toggleCursorAction.action.Enable();

        // Inscreve no evento do botão ESC
        toggleCursorAction.action.performed += OnToggleCursor;

        // Trava o cursor inicialmente
        LockCursor();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        lookAction.action.Disable();
        ascendAction.action.Disable();
        descendAction.action.Disable();
        sprintAction.action.Disable();
        toggleCursorAction.action.Disable();

        toggleCursorAction.action.performed -= OnToggleCursor;
    }

    private void Update()
    {
        // Se o mouse estiver solto (menu), não movemos a câmera
        if (!_isCursorLocked) return;

        HandleRotation();
        HandleMovement();
    }

    private void HandleRotation()
    {
        // Ler input do mouse (Delta)
        Vector2 mouseDelta = lookAction.action.ReadValue<Vector2>();

        // Ajustar Yaw (Horizontal) e Pitch (Vertical)
        _yaw += mouseDelta.x * mouseSensitivity;

        float pitchDelta = mouseDelta.y * mouseSensitivity;
        if (invertY) _pitch += pitchDelta;
        else _pitch -= pitchDelta;

        // Limitar o ângulo vertical (Clamp)
        _pitch = Mathf.Clamp(_pitch, -maxPitchAngle, maxPitchAngle);

        // Aplicar rotação
        transform.localEulerAngles = new Vector3(_pitch, _yaw, 0f);
    }

    private void HandleMovement()
    {
        // Ler input de movimento (WASD)
        Vector2 inputDir = moveAction.action.ReadValue<Vector2>();

        // Verificar Sprint
        float currentSpeed = moveSpeed;
        if (sprintAction.action.IsPressed())
        {
            currentSpeed *= sprintMultiplier;
        }

        // Calcular direção relativa à câmera
        Vector3 moveDirection = transform.right * inputDir.x + transform.forward * inputDir.y;

        // Movimento Vertical (Subir/Descer)
        float verticalMove = 0f;
        if (ascendAction.action.IsPressed()) verticalMove += 1f;
        if (descendAction.action.IsPressed()) verticalMove -= 1f;

        // Aplicar movimento
        transform.position += moveDirection * currentSpeed * Time.deltaTime;
        transform.position += Vector3.up * verticalMove * climbSpeed * Time.deltaTime;
    }

    private void OnToggleCursor(InputAction.CallbackContext context)
    {
        _isCursorLocked = !_isCursorLocked;

        if (_isCursorLocked)
            LockCursor();
        else
            UnlockCursor();
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _isCursorLocked = true;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _isCursorLocked = false;
    }
}