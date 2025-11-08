using UnityEngine;
using UnityEngine.UI;

public class ResetUI : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Panel que contiene la barra de progreso")]
    public GameObject resetProgressPanel;
    
    [Tooltip("Barra de progreso para mostrar el tiempo de hold")]
    public Image progressBar;
    
    [Tooltip("Texto con instrucciones")]
    public Text instructionText;
    
    [Header("Configuración Visual")]
    [Tooltip("Color de la barra de progreso")]
    public Color progressColor = Color.red;
    
    [Tooltip("¿Mostrar la UI en el mundo 3D?")]
    public bool worldSpaceUI = true;
    
    [Tooltip("Distancia de la UI del jugador (solo para worldSpace)")]
    public float distanceFromPlayer = 2f;
    
    [Tooltip("Altura de la UI (solo para worldSpace)")]
    public float uiHeight = 1.5f;

    private Canvas canvas;
    private Camera playerCamera;

    void Start()
    {
        SetupUI();
        FindPlayerCamera();
        
        // Ocultar el panel inicialmente
        if (resetProgressPanel != null)
        {
            resetProgressPanel.SetActive(false);
        }
    }

    void SetupUI()
    {
        canvas = GetComponent<Canvas>();
        
        if (worldSpaceUI && canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            
            // Configurar escala para UI en el mundo
            transform.localScale = Vector3.one * 0.001f; // Escala pequeña para world space
        }
        
        if (progressBar != null)
        {
            progressBar.color = progressColor;
            progressBar.fillAmount = 0f;
        }
    }

    void FindPlayerCamera()
    {
        // Buscar la cámara principal o la cámara VR
        playerCamera = Camera.main;
        
        if (playerCamera == null)
        {
            // Buscar específicamente la cámara XR
            GameObject xrOrigin = GameObject.Find("XR Origin");
            if (xrOrigin != null)
            {
                playerCamera = xrOrigin.GetComponentInChildren<Camera>();
            }
        }
    }

    void Update()
    {
        // Si es UI en el mundo, posicionarla frente al jugador
        if (worldSpaceUI && playerCamera != null && resetProgressPanel.activeInHierarchy)
        {
            PositionUIInFrontOfPlayer();
        }
    }

    void PositionUIInFrontOfPlayer()
    {
        // Posicionar la UI frente al jugador
        Vector3 playerPosition = playerCamera.transform.position;
        Vector3 playerForward = playerCamera.transform.forward;
        
        // Calcular posición frente al jugador
        Vector3 uiPosition = playerPosition + playerForward * distanceFromPlayer;
        uiPosition.y = playerPosition.y + uiHeight;
        
        transform.position = uiPosition;
        
        // Hacer que la UI mire hacia el jugador
        transform.LookAt(playerCamera.transform);
        transform.Rotate(0, 180, 0); // Rotar para que el texto se vea correctamente
    }

    // Métodos públicos para que el SceneReset los use
    public void ShowProgress()
    {
        if (resetProgressPanel != null)
        {
            resetProgressPanel.SetActive(true);
        }
    }

    public void HideProgress()
    {
        if (resetProgressPanel != null)
        {
            resetProgressPanel.SetActive(false);
        }
    }

    public void UpdateProgress(float progress)
    {
        if (progressBar != null)
        {
            progressBar.fillAmount = progress;
        }
    }

    public void SetInstructionText(string text)
    {
        if (instructionText != null)
        {
            instructionText.text = text;
        }
    }
}