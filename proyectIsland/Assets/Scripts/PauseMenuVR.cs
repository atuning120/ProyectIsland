using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class PauseMenuVR : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseMenuCanvas;

    [Header("XR Input Settings")]
    [Tooltip("Arrastra: XRI Default Input Actions.inputactions")]
    [SerializeField] private InputActionAsset xriInputActions;

    [Header("Button Configuration")]
    [Tooltip("Secondary Button = Botón B | Select = Grip | Activate = Trigger")]
    [SerializeField] private string buttonName = "Secondary Button";

    [Header("Locomotion Settings")]
    [Tooltip("Arrastra el Locomotion System de tu XR Origin")]
    [SerializeField] private LocomotionSystem locomotionSystem;
    [Tooltip("Arrastra el Continuous Move Provider")]
    [SerializeField] private ActionBasedContinuousMoveProvider continuousMoveProvider;
    [Tooltip("Arrastra el Snap Turn Provider")]
    [SerializeField] private ActionBasedSnapTurnProvider snapTurnProvider;
    [Tooltip("Arrastra el Continuous Turn Provider (opcional)")]
    [SerializeField] private ActionBasedContinuousTurnProvider continuousTurnProvider;
    [Tooltip("Arrastra el Teleportation Provider (opcional)")]
    [SerializeField] private TeleportationProvider teleportationProvider;

    [Header("Canvas Positioning")]
    [Tooltip("Cámara VR (CenterEyeAnchor o Main Camera)")]
    [SerializeField] private Transform vrCamera;
    [Tooltip("Distancia del menú frente al jugador")]
    [SerializeField] private float distanceFromPlayer = 2f;
    [Tooltip("Altura adicional del menú (0 = a la altura de los ojos)")]
    [SerializeField] private float heightOffset = 0f;

    private bool isPaused = false;
    private InputAction pauseAction;

    void Start()
    {
        pauseMenuCanvas.SetActive(false);

        // Buscar la cámara VR automáticamente si no está asignada
        if (vrCamera == null)
        {
            vrCamera = Camera.main.transform;
            if (vrCamera == null)
            {
                Debug.LogWarning("No se encontró la cámara VR. Asigna manualmente en el Inspector.");
            }
        }

        // Buscar la acción en el controlador derecho
        if (xriInputActions != null)
        {
            var rightHandMap = xriInputActions.FindActionMap("XRI RightHand Interaction");
            if (rightHandMap != null)
            {
                pauseAction = rightHandMap.FindAction(buttonName);
                if (pauseAction != null)
                {
                    pauseAction.Enable();
                    Debug.Log($"✓ Botón de pausa configurado: {buttonName}");
                }
                else
                {
                    Debug.LogError($"✗ No se encontró '{buttonName}'. Botones disponibles:");
                    foreach (var action in rightHandMap.actions)
                    {
                        Debug.Log($"  - {action.name}");
                    }
                }
            }
        }
    }

    void Update()
    {
        if (pauseAction != null && pauseAction.WasPressedThisFrame())
        {
            TogglePause();
        }

        // Mantener el canvas mirando al jugador mientras está en pausa
        if (isPaused && pauseMenuCanvas.activeSelf)
        {
            UpdateCanvasOrientation();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            // Posicionar el canvas frente al jugador
            PositionCanvasInFrontOfPlayer();
        }

        pauseMenuCanvas.SetActive(isPaused);
        SetLocomotionEnabled(!isPaused);

        Debug.Log($"Menú de pausa: {(isPaused ? "ABIERTO" : "CERRADO")}");
    }

    private void PositionCanvasInFrontOfPlayer()
    {
        if (vrCamera == null || pauseMenuCanvas == null) return;

        // Obtener la posición y rotación de la cámara
        Vector3 cameraPosition = vrCamera.position;
        Vector3 cameraForward = vrCamera.forward;

        // Ignorar la inclinación vertical (solo usar rotación horizontal)
        cameraForward.y = 0;
        cameraForward.Normalize();

        // Calcular la posición frente al jugador
        Vector3 menuPosition = cameraPosition + (cameraForward * distanceFromPlayer);
        menuPosition.y = cameraPosition.y + heightOffset;

        // Posicionar el canvas
        pauseMenuCanvas.transform.position = menuPosition;

        // Hacer que el canvas mire hacia el jugador
        UpdateCanvasOrientation();
    }

    private void UpdateCanvasOrientation()
    {
        if (vrCamera == null || pauseMenuCanvas == null) return;

        // Dirección desde el canvas hacia la cámara
        Vector3 directionToCamera = vrCamera.position - pauseMenuCanvas.transform.position;

        // Ignorar la componente vertical para mantener el canvas vertical
        directionToCamera.y = 0;

        // Solo rotar si hay una dirección válida
        if (directionToCamera.sqrMagnitude > 0.001f)
        {
            // Hacer que el canvas mire hacia la cámara (invertido para que se vea de frente)
            Quaternion targetRotation = Quaternion.LookRotation(-directionToCamera); // ← Nota el signo negativo
            pauseMenuCanvas.transform.rotation = targetRotation;
        }
    }

    private void SetLocomotionEnabled(bool enabled)
    {
        if (locomotionSystem != null)
        {
            locomotionSystem.enabled = enabled;
        }

        if (continuousMoveProvider != null)
        {
            continuousMoveProvider.enabled = enabled;
        }

        if (snapTurnProvider != null)
        {
            snapTurnProvider.enabled = enabled;
        }

        if (continuousTurnProvider != null)
        {
            continuousTurnProvider.enabled = enabled;
        }

        if (teleportationProvider != null)
        {
            teleportationProvider.enabled = enabled;
        }

        Debug.Log($"Locomoción: {(enabled ? "HABILITADA" : "DESHABILITADA")}");
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuCanvas.SetActive(false);
        SetLocomotionEnabled(true);
        Debug.Log("Juego reanudado");
    }

    public void RestartGame()
    {
        SetLocomotionEnabled(true);
        Debug.Log("Reiniciando juego...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    void OnDestroy()
    {
        if (pauseAction != null)
        {
            pauseAction.Disable();
        }
    }
}