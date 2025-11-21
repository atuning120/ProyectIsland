using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class VRSliderHandle : MonoBehaviour
{
    [Header("Slider Settings")]
    [SerializeField] private Slider targetSlider;
    [SerializeField] private RectTransform sliderRect;
    [SerializeField] private RectTransform fillArea;
    
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset xriInputActions;
    
    [Header("Settings")]
    [SerializeField] private float grabRadius = 0.1f;
    [SerializeField] private bool showDebug = false;
    
    private bool isGrabbed = false;
    private Transform grabbingHand = null;
    private InputAction leftGripAction;
    private InputAction rightGripAction;
    private InputAction leftTriggerAction;
    private InputAction rightTriggerAction;
    
    private Vector3 sliderStartPos;
    private Vector3 sliderEndPos;
    private float sliderLength;

    void Start()
    {
        if (targetSlider == null)
            targetSlider = GetComponentInParent<Slider>();
        
        if (sliderRect == null && targetSlider != null)
            sliderRect = targetSlider.GetComponent<RectTransform>();
        
        // Calcular posiciones del slider en espacio mundial
        CalculateSliderBounds();
        
        // Configurar input actions
        SetupInputActions();
    }

    void SetupInputActions()
    {
        if (xriInputActions == null)
        {
            Debug.LogWarning("XRI Input Actions no asignado en VRSliderHandle");
            return;
        }

        var leftHandMap = xriInputActions.FindActionMap("XRI LeftHand Interaction");
        var rightHandMap = xriInputActions.FindActionMap("XRI RightHand Interaction");

        if (leftHandMap != null)
        {
            leftGripAction = leftHandMap.FindAction("Select");
            leftTriggerAction = leftHandMap.FindAction("Activate");
            
            if (leftGripAction != null) leftGripAction.Enable();
            if (leftTriggerAction != null) leftTriggerAction.Enable();
        }

        if (rightHandMap != null)
        {
            rightGripAction = rightHandMap.FindAction("Select");
            rightTriggerAction = rightHandMap.FindAction("Activate");
            
            if (rightGripAction != null) rightGripAction.Enable();
            if (rightTriggerAction != null) rightTriggerAction.Enable();
        }
    }

    void CalculateSliderBounds()
    {
        if (sliderRect == null || fillArea == null) return;

        // Obtener las esquinas del fill area en espacio mundial
        Vector3[] corners = new Vector3[4];
        fillArea.GetWorldCorners(corners);

        // Determinar si es horizontal o vertical
        if (targetSlider.direction == Slider.Direction.LeftToRight || 
            targetSlider.direction == Slider.Direction.RightToLeft)
        {
            sliderStartPos = corners[0]; // Esquina inferior izquierda
            sliderEndPos = corners[2];   // Esquina superior derecha
            sliderLength = Vector3.Distance(sliderStartPos, sliderEndPos);
        }
        else
        {
            sliderStartPos = corners[0];
            sliderEndPos = corners[1];
            sliderLength = Vector3.Distance(sliderStartPos, sliderEndPos);
        }
    }

    void Update()
    {
        if (isGrabbed && grabbingHand != null)
        {
            UpdateSliderValue();
            
            // Soltar si se suelta el gatillo
            if (!IsGripPressed(grabbingHand))
            {
                ReleaseSlider();
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (isGrabbed) return;
        
        if (IsHand(other) && IsGripPressed(other.transform))
        {
            GrabSlider(other.transform);
        }
    }

    void GrabSlider(Transform hand)
    {
        isGrabbed = true;
        grabbingHand = hand;
        
        if (showDebug)
            Debug.Log($"🎚️ Slider agarrado por: {hand.name}");
    }

    void ReleaseSlider()
    {
        if (showDebug)
            Debug.Log("🎚️ Slider soltado");
        
        isGrabbed = false;
        grabbingHand = null;
    }

    void UpdateSliderValue()
    {
        if (targetSlider == null || grabbingHand == null) return;
        
        CalculateSliderBounds(); // Recalcular en caso de que el canvas se haya movido
        
        Vector3 handPosition = grabbingHand.position;
        
        // Proyectar la posición de la mano en la línea del slider
        Vector3 sliderDirection = (sliderEndPos - sliderStartPos).normalized;
        Vector3 handToStart = handPosition - sliderStartPos;
        float projectionLength = Vector3.Dot(handToStart, sliderDirection);
        
        // Normalizar entre 0 y 1
        float normalizedValue = Mathf.Clamp01(projectionLength / sliderLength);
        
        // Invertir si el slider va de derecha a izquierda
        if (targetSlider.direction == Slider.Direction.RightToLeft || 
            targetSlider.direction == Slider.Direction.TopToBottom)
        {
            normalizedValue = 1f - normalizedValue;
        }
        
        // Aplicar al slider
        targetSlider.value = Mathf.Lerp(targetSlider.minValue, targetSlider.maxValue, normalizedValue);
        
        if (showDebug)
        {
            Debug.DrawLine(sliderStartPos, sliderEndPos, Color.green);
            Debug.DrawLine(handPosition, sliderStartPos, Color.yellow);
        }
    }

    bool IsGripPressed(Transform hand)
    {
        if (hand == null) return false;
        
        string handName = hand.name.ToLower();
        
        // Detectar mano izquierda
        if (handName.Contains("left"))
        {
            // Verificar trigger O grip
            bool triggerPressed = leftTriggerAction != null && leftTriggerAction.ReadValue<float>() > 0.5f;
            bool gripPressed = leftGripAction != null && leftGripAction.ReadValue<float>() > 0.5f;
            return triggerPressed || gripPressed;
        }
        // Detectar mano derecha
        else if (handName.Contains("right"))
        {
            bool triggerPressed = rightTriggerAction != null && rightTriggerAction.ReadValue<float>() > 0.5f;
            bool gripPressed = rightGripAction != null && rightGripAction.ReadValue<float>() > 0.5f;
            return triggerPressed || gripPressed;
        }
        
        return false;
    }

    bool IsHand(Collider other)
    {
        try
        {
            if (other.CompareTag("hand"))
                return true;
        }
        catch { }
        
        string name = other.gameObject.name.ToLower();
        return name.Contains("hand") || name.Contains("controller") || name.Contains("pointer");
    }

    void OnDrawGizmos()
    {
        if (!showDebug) return;
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, grabRadius);
        
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(sliderStartPos, sliderEndPos);
            
            if (isGrabbed && grabbingHand != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(grabbingHand.position, transform.position);
            }
        }
    }

    void OnDestroy()
    {
        if (leftGripAction != null) leftGripAction.Disable();
        if (rightGripAction != null) rightGripAction.Disable();
        if (leftTriggerAction != null) leftTriggerAction.Disable();
        if (rightTriggerAction != null) rightTriggerAction.Disable();
    }
}