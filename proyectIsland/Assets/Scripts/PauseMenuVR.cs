using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenuVR : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseMenuCanvas;
    
    [Header("XR Input Settings")]
    [Tooltip("Arrastra: XRI Default Input Actions.inputactions")]
    [SerializeField] private InputActionAsset xriInputActions;
    
    [Header("Button Configuration")]
    [Tooltip("Select = Grip | Activate = Trigger | UI Press = UI Button")]
    [SerializeField] private string buttonName = "Select";
    
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
        Debug.Log($"Menú de pausa: {(isPaused ? "ABIERTO" : "CERRADO")}");
    }

    public void ResumeGame()
    {
        isPaused = false;
        pauseMenuCanvas.SetActive(false);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
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