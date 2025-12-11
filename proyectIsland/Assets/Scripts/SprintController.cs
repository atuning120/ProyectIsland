using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class SprintController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [Tooltip("Arrastra aquí tu componente ActionBasedContinuousMoveProvider")]
    public ActionBasedContinuousMoveProvider moveProvider;
    
    [Tooltip("Velocidad normal al caminar")]
    public float walkSpeed = 8.0f;
    
    [Tooltip("Velocidad al correr (cuando se presiona el gatillo)")]
    public float runSpeed = 12.0f;

    [Header("Configuración de Input")]
    [Tooltip("El Asset de Input Actions (generalmente XRI Default Input Actions)")]
    public InputActionAsset inputActionAsset;
    
    [Tooltip("Usar gatillo de mano izquierda")]
    public bool useLeftHand = true;
    
    [Tooltip("Usar gatillo de mano derecha")]
    public bool useRightHand = false;

    [Tooltip("Nombre de la acción a usar (Activate = Gatillo, Select = Agarre)")]
    public string actionName = "Activate";

    private InputAction leftHandRunAction;
    private InputAction rightHandRunAction;

    private void Start()
    {
        // Intentar encontrar el componente de movimiento en este mismo objeto o en la escena
        if (moveProvider == null)
        {
            moveProvider = GetComponent<ActionBasedContinuousMoveProvider>();
            if (moveProvider == null)
            {
                moveProvider = FindObjectOfType<ActionBasedContinuousMoveProvider>();
            }
        }

        // Asegurarnos de empezar con la velocidad de caminar
        if (moveProvider != null)
        {
            moveProvider.moveSpeed = walkSpeed;
        }

        InitializeInput();
    }

    private void InitializeInput()
    {
        if (inputActionAsset == null)
        {
            Debug.LogError("SprintController: Falta asignar el Input Action Asset.");
            return;
        }

        if (useLeftHand)
        {
            leftHandRunAction = inputActionAsset.FindAction($"XRI LeftHand Interaction/{actionName}");
            if (leftHandRunAction != null)
            {
                leftHandRunAction.started += OnSprintStarted;
                leftHandRunAction.canceled += OnSprintEnded;
                leftHandRunAction.Enable();
            }
            else
            {
                Debug.LogWarning($"SprintController: No se encontró la acción 'XRI LeftHand Interaction/{actionName}'");
            }
        }

        if (useRightHand)
        {
            rightHandRunAction = inputActionAsset.FindAction($"XRI RightHand Interaction/{actionName}");
            if (rightHandRunAction != null)
            {
                rightHandRunAction.started += OnSprintStarted;
                rightHandRunAction.canceled += OnSprintEnded;
                rightHandRunAction.Enable();
            }
            else
            {
                Debug.LogWarning($"SprintController: No se encontró la acción 'XRI RightHand Interaction/{actionName}'");
            }
        }
    }

    private void OnSprintStarted(InputAction.CallbackContext context)
    {
        if (moveProvider != null)
        {
            moveProvider.moveSpeed = runSpeed;
        }
    }

    private void OnSprintEnded(InputAction.CallbackContext context)
    {
        if (moveProvider != null)
        {
            moveProvider.moveSpeed = walkSpeed;
        }
    }

    private void OnDisable()
    {
        if (leftHandRunAction != null) leftHandRunAction.Disable();
        if (rightHandRunAction != null) rightHandRunAction.Disable();
    }
}
