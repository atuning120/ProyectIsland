using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class SceneReset : MonoBehaviour
{
    [Header("Configuración de Reset")]
    [Tooltip("Referencia al Input Action Asset que contiene las acciones de los controladores")]
    public InputActionAsset inputActionAsset;
    
    [Header("Configuración de Botones")]
    [Tooltip("¿Usar botón de la mano izquierda?")]
    public bool useLeftHand = true;
    
    [Tooltip("¿Usar botón de la mano derecha?")]
    public bool useRightHand = false;
    
    [Tooltip("¿Qué botón usar? (Select = gatillo, Activate = agarre, UI Press = botón A/X)")]
    public ButtonType buttonType = ButtonType.Activate;
    
    public enum ButtonType
    {
        Select,      // Gatillo principal
        Activate,    // Botón de agarre lateral
        UIPress      // Botón A/X
    }
    
    [Header("Configuración de Reset")]
    [Tooltip("¿Requiere mantener presionado el botón por un tiempo?")]
    public bool requireHoldTime = true;
    
    [Tooltip("Tiempo en segundos que se debe mantener presionado para reiniciar")]
    public float holdTimeRequired = 3.0f;
    
    [Header("Configuración de Spawn")]
    [Tooltip("¿Resetear posición del jugador al punto de spawn?")]
    public bool resetPlayerPosition = true;
    
    [Tooltip("Posición de spawn (si está vacío, usa la posición inicial del XR Origin)")]
    public Transform spawnPoint;
    
    [Header("Feedback Visual")]
    [Tooltip("UI para mostrar el progreso del reset (opcional)")]
    public UnityEngine.UI.Image progressBar;
    
    [Tooltip("Texto para mostrar instrucciones (opcional)")]
    public UnityEngine.UI.Text instructionText;

    // Variables privadas
    private InputAction leftHandAction;
    private InputAction rightHandAction;
    private float currentHoldTime = 0f;
    private bool isHolding = false;
    private string currentButton = "";
    private Vector3 originalPlayerPosition;
    private Quaternion originalPlayerRotation;

    void Start()
    {
        SaveInitialPlayerPosition();
        InitializeInputActions();
        SetupUI();
    }

    void SaveInitialPlayerPosition()
    {
        // Buscar primero por componente XROrigin (más confiable)
        var xrOriginComponent = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
        GameObject playerObject = null;
        
        if (xrOriginComponent != null)
        {
            playerObject = xrOriginComponent.gameObject;
        }
        else
        {
            // Buscar por nombres si no encuentra el componente
            playerObject = GameObject.Find("XR Origin");
            
            if (playerObject == null)
            {
                playerObject = GameObject.Find("XROrigin");
            }
            
            if (playerObject == null)
            {
                playerObject = GameObject.Find("Manos");
            }
        }

        if (playerObject != null)
        {
            originalPlayerPosition = playerObject.transform.position;
            originalPlayerRotation = playerObject.transform.rotation;
        }
    }

    void InitializeInputActions()
    {
        if (inputActionAsset == null)
        {
            Debug.LogError("SceneReset: No se ha asignado un Input Action Asset.");
            return;
        }

        // Determinar el nombre de la acción basado en el tipo de botón
        string actionSuffix = "";
        switch (buttonType)
        {
            case ButtonType.Select:
                actionSuffix = "Select";
                currentButton = "gatillo";
                break;
            case ButtonType.Activate:
                actionSuffix = "Activate";
                currentButton = "botón de agarre";
                break;
            case ButtonType.UIPress:
                actionSuffix = "UI Press";
                currentButton = "botón A/X";
                break;
        }

        // Configurar mano izquierda
        if (useLeftHand)
        {
            string leftActionName = $"XRI LeftHand Interaction/{actionSuffix}";
            leftHandAction = inputActionAsset.FindAction(leftActionName);
            
            if (leftHandAction != null)
            {
                leftHandAction.started += OnResetStarted;
                leftHandAction.canceled += OnResetCanceled;
                leftHandAction.performed += OnResetPerformed;
                leftHandAction.Enable();
            }
        }

        // Configurar mano derecha
        if (useRightHand)
        {
            string rightActionName = $"XRI RightHand Interaction/{actionSuffix}";
            rightHandAction = inputActionAsset.FindAction(rightActionName);
            
            if (rightHandAction != null)
            {
                rightHandAction.started += OnResetStarted;
                rightHandAction.canceled += OnResetCanceled;
                rightHandAction.performed += OnResetPerformed;
                rightHandAction.Enable();
            }
        }
    }

    void SetupUI()
    {
        // Configurar UI inicial
        if (progressBar != null)
        {
            progressBar.fillAmount = 0f;
            progressBar.gameObject.SetActive(false);
        }
        
        if (instructionText != null)
        {
            string hands = "";
            if (useLeftHand && useRightHand)
                hands = "cualquier mano";
            else if (useLeftHand)
                hands = "mano izquierda";
            else if (useRightHand)
                hands = "mano derecha";
            
            if (requireHoldTime)
            {
                instructionText.text = $"Mantén {currentButton} ({hands}) {holdTimeRequired}s para reiniciar";
            }
            else
            {
                instructionText.text = $"Presiona {currentButton} ({hands}) para reiniciar";
            }
        }
    }

    void Update()
    {
        // Si se requiere mantener presionado, actualizar el progreso
        if (requireHoldTime && isHolding)
        {
            currentHoldTime += Time.deltaTime;
            
            // Actualizar barra de progreso
            if (progressBar != null)
            {
                progressBar.fillAmount = currentHoldTime / holdTimeRequired;
            }
            
            // Si se completó el tiempo requerido, reiniciar
            if (currentHoldTime >= holdTimeRequired)
            {
                ResetScene();
            }
        }
    }

    private void OnResetStarted(InputAction.CallbackContext context)
    {
        if (requireHoldTime)
        {
            isHolding = true;
            currentHoldTime = 0f;
            
            // Mostrar barra de progreso
            if (progressBar != null)
            {
                progressBar.gameObject.SetActive(true);
                progressBar.fillAmount = 0f;
            }
        }
    }

    private void OnResetCanceled(InputAction.CallbackContext context)
    {
        if (requireHoldTime)
        {
            isHolding = false;
            currentHoldTime = 0f;
            
            // Ocultar barra de progreso
            if (progressBar != null)
            {
                progressBar.gameObject.SetActive(false);
            }
        }
    }

    private void OnResetPerformed(InputAction.CallbackContext context)
    {
        // Si no se requiere mantener presionado, reiniciar inmediatamente
        if (!requireHoldTime)
        {
            ResetScene();
        }
    }

    public void ResetScene()
    {
        // Resetear posición del jugador antes de resetear el checklist
        if (resetPlayerPosition)
        {
            ResetPlayerPosition();
        }
        
        // Resetear el ChecklistManager
        if (ChecklistManager.Instance != null)
        {
            ChecklistManager.Instance.ResetChecklist();
        }
    }

    void ResetPlayerPosition()
    {
        // Buscar primero por componente XROrigin (más confiable)
        var xrOriginComponent = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
        GameObject playerObject = null;
        
        if (xrOriginComponent != null)
        {
            playerObject = xrOriginComponent.gameObject;
        }
        else
        {
            // Buscar por nombres si no encuentra el componente
            playerObject = GameObject.Find("XR Origin");
            
            if (playerObject == null)
            {
                playerObject = GameObject.Find("XROrigin");
            }
            
            if (playerObject == null)
            {
                playerObject = GameObject.Find("Manos");
            }
        }

        if (playerObject != null)
        {
            Vector3 currentPosition = playerObject.transform.position;
            Vector3 targetPosition;
            Quaternion targetRotation;
            
            // Usar spawn point personalizado si está configurado
            if (spawnPoint != null)
            {
                targetPosition = spawnPoint.position;
                targetRotation = spawnPoint.rotation;
            }
            else
            {
                // Usar posición inicial guardada
                targetPosition = originalPlayerPosition;
                targetRotation = originalPlayerRotation;
            }
            
            // Para XR Origin, usar el método apropiado si está disponible
            var xrOrigin = playerObject.GetComponent<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin != null)
            {
                // Usar el método de XR Origin que es más confiable para VR
                xrOrigin.MoveCameraToWorldLocation(targetPosition);
                xrOrigin.transform.rotation = targetRotation;
            }
            else
            {
                // Aplicar directamente al transform
                playerObject.transform.position = targetPosition;
                playerObject.transform.rotation = targetRotation;
            }
        }
    }

    // Método público para resetear desde otros scripts o UI
    public void ResetSceneImmediate()
    {
        ResetScene();
    }

    private void OnDisable()
    {
        // Limpiar las suscripciones al deshabilitar
        if (leftHandAction != null)
        {
            leftHandAction.started -= OnResetStarted;
            leftHandAction.canceled -= OnResetCanceled;
            leftHandAction.performed -= OnResetPerformed;
            leftHandAction.Disable();
        }
        
        if (rightHandAction != null)
        {
            rightHandAction.started -= OnResetStarted;
            rightHandAction.canceled -= OnResetCanceled;
            rightHandAction.performed -= OnResetPerformed;
            rightHandAction.Disable();
        }
    }

    private void OnDestroy()
    {
        // Limpiar las suscripciones al destruir
        if (leftHandAction != null)
        {
            leftHandAction.started -= OnResetStarted;
            leftHandAction.canceled -= OnResetCanceled;
            leftHandAction.performed -= OnResetPerformed;
        }
        
        if (rightHandAction != null)
        {
            rightHandAction.started -= OnResetStarted;
            rightHandAction.canceled -= OnResetCanceled;
            rightHandAction.performed -= OnResetPerformed;
        }
    }
}