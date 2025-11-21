using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VRButton : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private UnityEvent onPress;
    [SerializeField] private float pressDepth = 0.02f;
    [SerializeField] private float releaseSpeed = 5f;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color pressColor = Color.green;
    
    private Vector3 startPosition;
    private Vector3 pressedPosition;
    private bool isPressed = false;
    private bool wasPressed = false;
    private Image buttonImage;
    
    void Start()
    {
        startPosition = transform.localPosition;
        pressedPosition = startPosition - new Vector3(0, 0, pressDepth);
        buttonImage = GetComponent<Image>();
        
        if (buttonImage != null)
            buttonImage.color = normalColor;
    }
    
    void Update()
    {
        // Volver a la posición original suavemente
        if (!isPressed && Vector3.Distance(transform.localPosition, startPosition) > 0.001f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, startPosition, Time.deltaTime * releaseSpeed);
        }
        
        // Resetear el estado de presión
        if (!isPressed && wasPressed)
        {
            wasPressed = false;
            if (buttonImage != null)
                buttonImage.color = normalColor;
        }
        
        isPressed = false;
    }
    
    private void OnTriggerStay(Collider other)
    {
        // Detectar si es una mano
        if (other.CompareTag("hand") || other.name.Contains("hand"))
        {
            isPressed = true;
            transform.localPosition = pressedPosition;
            
            if (buttonImage != null)
                buttonImage.color = pressColor;
            
            // Ejecutar el evento solo una vez
            if (!wasPressed)
            {
                onPress?.Invoke();
                wasPressed = true;
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("hand") || other.name.Contains("hand"))
        {
            if (buttonImage != null)
                buttonImage.color = hoverColor;
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("hand") || other.name.Contains("hand"))
        {
            if (buttonImage != null)
                buttonImage.color = normalColor;
        }
    }
}