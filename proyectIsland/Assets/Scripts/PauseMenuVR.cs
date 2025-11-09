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
    [Tooltip("Arrastra el Continuous Move Provider (opcional)")]
    [SerializeField] private ActionBasedContinuousMoveProvider continuousMoveProvider;
    [Tooltip("Arrastra el Snap Turn Provider (opcional)")]
    [SerializeField] private ActionBasedSnapTurnProvider snapTurnProvider;
    [Tooltip("Arrastra el Continuous Turn Provider (opcional)")]
    [SerializeField] private ActionBasedContinuousTurnProvider continuousTurnProvider;
    [Tooltip("Arrastra el Teleportation Provider (opcional)")]
    [SerializeField] private TeleportationProvider teleportationProvider;
    
    private bool isPaused = false;
    private InputAction pauseAction;

    void Start()
    {
        pauseMenuCanvas.SetActive(false);
        
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
            else
            {
                Debug.LogError("✗ No se encontró 'XRI RightHand Interaction'");
            }
        }
        else
        {
            Debug.LogError("✗ XRI Input Actions no asignado");
        }
    }

    void Update()
    {
        if (pauseAction != null && pauseAction.WasPressedThisFrame())
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenuCanvas.SetActive(isPaused);
        
        // Controlar la locomoción
        SetLocomotionEnabled(!isPaused);
        
        Debug.Log($"Menú de pausa: {(isPaused ? "ABIERTO" : "CERRADO")}");
    }

    private void SetLocomotionEnabled(bool enabled)
    {
        // Deshabilitar/habilitar el sistema de locomoción completo
        if (locomotionSystem != null)
        {
            locomotionSystem.enabled = enabled;
        }
        
        // Deshabilitar/habilitar movimiento continuo
        if (continuousMoveProvider != null)
        {
            continuousMoveProvider.enabled = enabled;
        }
        
        // Deshabilitar/habilitar rotación snap
        if (snapTurnProvider != null)
        {
            snapTurnProvider.enabled = enabled;
        }
        
        // Deshabilitar/habilitar rotación continua
        if (continuousTurnProvider != null)
        {
            continuousTurnProvider.enabled = enabled;
        }
        
        // Deshabilitar/habilitar teletransporte
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
        // Habilitar locomoción antes de reiniciar
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