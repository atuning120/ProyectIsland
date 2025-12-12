using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class ControladorMenu : MonoBehaviour
{
    [Header("Configuración de Escenas")]
    [SerializeField] private string nombreEscenaJuego = "mapaJj";
    [SerializeField] private string nombreEscenaRelajacion = "Descanso";

    [Header("Referencias a Botones (VRButton)")]
    [SerializeField] private VRButton botonComenzar;
    [SerializeField] private VRButton botonRelajacion;
    [SerializeField] private VRButton botonSalir;

    [Header("Configuración de Input")]
    [SerializeField] private InputActionAsset inputActions;
    
    [Header("Navegación (Mano Izquierda)")]
    [SerializeField] private string leftHandMapName = "XRI LeftHand Interaction";
    [SerializeField] private string prevButtonAction = "Primary Button"; // Botón X
    [SerializeField] private string nextButtonAction = "Secondary Button"; // Botón Y

    [Header("Selección (Mano Derecha)")]
    [SerializeField] private string rightHandMapName = "XRI RightHand Interaction";
    [SerializeField] private string selectButtonAction = "Secondary Button"; // Botón B

    private InputAction navPrev;
    private InputAction navNext;
    private InputAction selectAction;

    private List<VRButton> botones;
    private int indiceActual = 0;

    void Awake()
    {
        botones = new List<VRButton>();
        // Añadimos los botones en orden
        if (botonComenzar != null) botones.Add(botonComenzar);
        if (botonRelajacion != null) botones.Add(botonRelajacion);
        if (botonSalir != null) botones.Add(botonSalir);
    }

    void Start()
    {
        ConfigurarInput();
        ConfigurarEventos();
        ActualizarVisuales();
    }

    void ConfigurarInput()
    {
        if (inputActions != null)
        {
            var leftMap = inputActions.FindActionMap(leftHandMapName);
            var rightMap = inputActions.FindActionMap(rightHandMapName);

            if (leftMap != null)
            {
                navPrev = leftMap.FindAction(prevButtonAction);
                navNext = leftMap.FindAction(nextButtonAction);
                navPrev?.Enable();
                navNext?.Enable();
            }

            if (rightMap != null)
            {
                selectAction = rightMap.FindAction(selectButtonAction);
                selectAction?.Enable();
            }
        }
        else
        {
            Debug.LogWarning("ControladorMenu: No se ha asignado el InputActionAsset.");
        }
    }

    void ConfigurarEventos()
    {
        // Asignamos la lógica a los botones programáticamente
        if (botonComenzar != null)
            botonComenzar.onPress.AddListener(CargarJuego);
            
        if (botonRelajacion != null)
            botonRelajacion.onPress.AddListener(CargarRelajacion);
            
        if (botonSalir != null)
            botonSalir.onPress.AddListener(Salir);
    }

    void Update()
    {
        if (botones.Count == 0) return;

        // Navegación con X (Anterior) e Y (Siguiente)
        if (navPrev != null && navPrev.WasPressedThisFrame())
            MoverSeleccion(-1);
        
        if (navNext != null && navNext.WasPressedThisFrame())
            MoverSeleccion(1);

        // Selección con B
        if (selectAction != null && selectAction.WasPressedThisFrame())
        {
            if (indiceActual >= 0 && indiceActual < botones.Count)
            {
                botones[indiceActual].SimulateClick();
            }
        }
    }

    void MoverSeleccion(int direccion)
    {
        // Desmarcar anterior
        if (indiceActual >= 0 && indiceActual < botones.Count)
            botones[indiceActual].SetHighlight(false);
        
        indiceActual += direccion;

        // Loop de navegación
        if (indiceActual < 0) indiceActual = botones.Count - 1;
        if (indiceActual >= botones.Count) indiceActual = 0;

        ActualizarVisuales();
    }

    void ActualizarVisuales()
    {
        // Marcar nuevo
        if (indiceActual >= 0 && indiceActual < botones.Count)
            botones[indiceActual].SetHighlight(true);
    }

    // --- Funciones de Lógica ---

    public void CargarJuego()
    {
        Debug.Log($"Cargando escena de juego: {nombreEscenaJuego}");
        if (!string.IsNullOrEmpty(nombreEscenaJuego))
            SceneManager.LoadScene(nombreEscenaJuego);
    }

    public void CargarRelajacion()
    {
        Debug.Log($"Cargando escena de relajación: {nombreEscenaRelajacion}");
        if (!string.IsNullOrEmpty(nombreEscenaRelajacion))
            SceneManager.LoadScene(nombreEscenaRelajacion);
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    
    void OnDestroy()
    {
        // Limpieza de eventos
        if (botonComenzar != null) botonComenzar.onPress.RemoveListener(CargarJuego);
        if (botonRelajacion != null) botonRelajacion.onPress.RemoveListener(CargarRelajacion);
        if (botonSalir != null) botonSalir.onPress.RemoveListener(Salir);
    }
}
