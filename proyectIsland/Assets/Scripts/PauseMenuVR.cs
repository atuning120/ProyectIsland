using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

public class PauseMenuVR : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pauseMenuCanvas;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject volumePanel;

    [Header("Volume Settings")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private AudioMixer audioMixer; // Opcional: si usas AudioMixer
    [Tooltip("Nombre del parámetro de volumen en el AudioMixer (ej: 'MasterVolume')")]
    [SerializeField] private string volumeParameter = "MasterVolume";
    
    [Header("Audio Control")]
    [Tooltip("Arrastra aquí el AudioSource del Narrador o música de fondo para pausarlo")]
    [SerializeField] private AudioSource mindfulnessAudioSource;

    [Header("XR Input Settings")]
    [Tooltip("Arrastra: XRI Default Input Actions.inputactions")]
    [SerializeField] private InputActionAsset xriInputActions;

    [Header("Button Configuration")]
    [Tooltip("Secondary Button = Botón B | Select = Grip | Activate = Trigger")]
    [SerializeField] private string buttonName = "Secondary Button";

    [Header("Menu Navigation (Left Hand)")]
    [SerializeField] private string leftHandMapName = "XRI LeftHand Interaction";
    [SerializeField] private string leftLocomotionMapName = "XRI LeftHand Locomotion";
    [Tooltip("Botón para opción anterior (ej: X - Primary Button)")]
    [SerializeField] private string prevOptionButton = "Primary Button";
    [Tooltip("Botón para opción siguiente (ej: Y - Secondary Button)")]
    [SerializeField] private string nextOptionButton = "Secondary Button";
    [Tooltip("Joystick para mover sliders (ej: Move)")]
    [SerializeField] private string sliderMoveAxis = "Move";

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

    public static bool IsPaused { get; private set; } = false;

    private bool isPaused = false;
    private InputAction pauseAction;
    private InputAction navPrevAction;
    private InputAction navNextAction;
    private InputAction sliderAction;
    
    // Usamos Component para poder almacenar tanto VRButton como Slider
    private System.Collections.Generic.List<Component> currentInteractables = new System.Collections.Generic.List<Component>();
    private int currentSelectionIndex = 0;

    private float currentVolume = 1f;

    void Start()
    {
        pauseMenuCanvas.SetActive(false);
        mainMenuPanel.SetActive(true);
        volumePanel.SetActive(false);
        IsPaused = false;

        // Configurar el slider de volumen
        if (volumeSlider != null)
        {
            currentVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            volumeSlider.value = currentVolume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            SetVolume(currentVolume);
        }

        if (vrCamera == null)
        {
            vrCamera = Camera.main.transform;
            if (vrCamera == null)
            {
                Debug.LogWarning("No se encontró la cámara VR. Asigna manualmente en el Inspector.");
            }
        }

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

            // Configuración de navegación (Mano Izquierda)
            var leftHandMap = xriInputActions.FindActionMap(leftHandMapName);
            var leftLocomotionMap = xriInputActions.FindActionMap(leftLocomotionMapName);

            if (leftHandMap != null)
            {
                navPrevAction = leftHandMap.FindAction(prevOptionButton);
                navNextAction = leftHandMap.FindAction(nextOptionButton);
                
                if (navPrevAction != null) 
                {
                    navPrevAction.Enable();
                    Debug.Log($"✓ Acción de navegación 'Anterior' encontrada: {prevOptionButton}");
                }
                else
                {
                    Debug.LogError($"✗ No se encontró la acción '{prevOptionButton}' en el mapa '{leftHandMapName}'. Asegúrate de haberla creado en el Input Action Asset.");
                }

                if (navNextAction != null) 
                {
                    navNextAction.Enable();
                    Debug.Log($"✓ Acción de navegación 'Siguiente' encontrada: {nextOptionButton}");
                }
                else
                {
                    Debug.LogError($"✗ No se encontró la acción '{nextOptionButton}' en el mapa '{leftHandMapName}'. Asegúrate de haberla creado en el Input Action Asset.");
                }
            }
            else
            {
                Debug.LogError($"✗ No se encontró el mapa de acciones '{leftHandMapName}'. Verifica el nombre en el Inspector.");
            }

            if (leftLocomotionMap != null)
            {
                sliderAction = leftLocomotionMap.FindAction(sliderMoveAxis);
                if (sliderAction != null) 
                {
                    sliderAction.Enable();
                    Debug.Log($"✓ Acción de Slider encontrada: {sliderMoveAxis}");
                }
                else
                {
                    Debug.LogError($"✗ No se encontró la acción '{sliderMoveAxis}' en el mapa '{leftLocomotionMapName}'.");
                }
            }
            else
            {
                Debug.LogError($"✗ No se encontró el mapa de locomoción '{leftLocomotionMapName}'.");
            }
        }
    }

    void Update()
    {
        if (pauseAction != null && pauseAction.WasPressedThisFrame())
        {
            if (!isPaused)
            {
                TogglePause();
            }
            else
            {
                // Si está pausado, el botón B actúa como "Seleccionar"
                SelectCurrentOption();
            }
        }

        if (isPaused)
        {
            if (pauseMenuCanvas.activeSelf)
            {
                UpdateCanvasOrientation();
                HandleMenuNavigation();
            }
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        IsPaused = isPaused;

        // IMPORTANTE: Activar el Canvas PRIMERO para que 'activeInHierarchy' sea true en los botones
        pauseMenuCanvas.SetActive(isPaused);

        if (isPaused)
        {
            PositionCanvasInFrontOfPlayer();
            PausePhotographableObjects(true);
            ShowMainMenu(); 
            
            // Pausar audio de mindfulness
            if (mindfulnessAudioSource != null && mindfulnessAudioSource.isPlaying)
            {
                mindfulnessAudioSource.Pause();
            }
            
            Debug.Log("🎮 JUEGO PAUSADO - Objetos fotografiables congelados y muteados");
        }
        else
        {
            PausePhotographableObjects(false);

            // Reanudar audio de mindfulness
            if (mindfulnessAudioSource != null)
            {
                mindfulnessAudioSource.UnPause();
            }

            Debug.Log("▶️ JUEGO REANUDADO - Objetos fotografiables activos con audio");
        }

        SetLocomotionEnabled(!isPaused);

        if (isPaused)
        {
            // Re-habilitar acciones de navegación por si se desactivaron con la locomoción
            if (navPrevAction != null) navPrevAction.Enable();
            if (navNextAction != null) navNextAction.Enable();
            if (sliderAction != null) sliderAction.Enable();
        }

        Debug.Log($"Menú de pausa: {(isPaused ? "ABIERTO" : "CERRADO")}");
    }

    // NUEVO: Mostrar menú principal
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        volumePanel.SetActive(false);
        RefreshInteractables(); // Actualizar lista de botones
        Debug.Log("📋 Menú principal mostrado");
    }

    // NUEVO: Mostrar menú de volumen
    public void ShowVolumeMenu()
    {
        mainMenuPanel.SetActive(false);
        volumePanel.SetActive(true);
        RefreshInteractables(); // Actualizar lista de botones/sliders
        Debug.Log("🔊 Menú de volumen mostrado");
    }

    // NUEVO: Cambiar volumen desde el slider
    private void OnVolumeChanged(float value)
    {
        currentVolume = value;
        SetVolume(value);
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
        Debug.Log($"🔊 Volumen cambiado a: {Mathf.RoundToInt(value * 100)}%");
    }

    // NUEVO: Aplicar el volumen
    private void SetVolume(float value)
    {
        if (audioMixer != null)
        {
            // Si usas AudioMixer (RECOMENDADO)
            // Convertir de 0-1 a decibelios (-80 a 0)
            float dB = value > 0 ? Mathf.Log10(value) * 20 : -80f;
            audioMixer.SetFloat(volumeParameter, dB);
        }
        else
        {
            // Si no usas AudioMixer (alternativa simple)
            AudioListener.volume = value;
        }
    }

    private void PausePhotographableObjects(bool pause)
    {
        GameObject[] photographables = GameObject.FindGameObjectsWithTag("Photographable");

        foreach (GameObject obj in photographables)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                if (pause)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true;
                }
                else
                {
                    rb.isKinematic = false;
                }
            }

            Animator animator = obj.GetComponent<Animator>();
            if (animator != null)
            {
                animator.enabled = !pause;
            }

            AudioSource audioSource = obj.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.mute = pause;

                if (pause && audioSource.isPlaying)
                {
                    audioSource.Pause();
                }
                else if (!pause)
                {
                    audioSource.UnPause();
                }
            }

            WildAnimalAI animalAI = obj.GetComponent<WildAnimalAI>();
            if (animalAI != null)
            {
                animalAI.MuteAnimal(pause);
            }

            MonoBehaviour[] scripts = obj.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                if (script.GetType().Name != "PhotographableObject" &&
                    script.GetType() != typeof(AudioSource))
                {
                    script.enabled = !pause;
                }
            }
        }

        int mutedCount = photographables.Length;
        string statusIcon = pause ? "🔇" : "🔊";
        Debug.Log($"{statusIcon} {mutedCount} objetos fotografiables {(pause ? "pausados y muteados" : "reanudados con audio")}");
    }

    private void PositionCanvasInFrontOfPlayer()
    {
        if (vrCamera == null || pauseMenuCanvas == null) return;

        Vector3 cameraPosition = vrCamera.position;
        Vector3 cameraForward = vrCamera.forward;

        cameraForward.y = 0;
        cameraForward.Normalize();

        Vector3 menuPosition = cameraPosition + (cameraForward * distanceFromPlayer);
        menuPosition.y = cameraPosition.y + heightOffset;

        pauseMenuCanvas.transform.position = menuPosition;
        UpdateCanvasOrientation();
    }

    private void UpdateCanvasOrientation()
    {
        if (vrCamera == null || pauseMenuCanvas == null) return;

        Vector3 directionToCamera = vrCamera.position - pauseMenuCanvas.transform.position;
        directionToCamera.y = 0;

        if (directionToCamera.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-directionToCamera);
            pauseMenuCanvas.transform.rotation = targetRotation;
        }
    }

    private void SetLocomotionEnabled(bool enabled)
    {
        if (locomotionSystem != null)
            locomotionSystem.enabled = enabled;

        if (continuousMoveProvider != null)
            continuousMoveProvider.enabled = enabled;

        if (snapTurnProvider != null)
            snapTurnProvider.enabled = enabled;

        if (continuousTurnProvider != null)
            continuousTurnProvider.enabled = enabled;

        if (teleportationProvider != null)
            teleportationProvider.enabled = enabled;

        Debug.Log($"Locomoción: {(enabled ? "HABILITADA" : "DESHABILITADA")}");
    }

    public void ResumeGame()
    {
        isPaused = false;
        IsPaused = false;

        pauseMenuCanvas.SetActive(false);
        SetLocomotionEnabled(true);
        PausePhotographableObjects(false);

        Debug.Log("▶️ Juego reanudado desde el botón");
    }

    public void RestartGame()
    {
        IsPaused = false;
        SetLocomotionEnabled(true);

        // Resetear el ChecklistManager si existe
        if (ChecklistManager.Instance != null)
        {
            ChecklistManager.Instance.ResetChecklist();
        }

        Debug.Log("🔄 Reiniciando juego...");

        // Recargar la escena actual
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void HandleMenuNavigation()
    {
        if (navPrevAction != null && navPrevAction.WasPressedThisFrame())
        {
            Debug.Log("Botón 'Anterior' presionado");
            ChangeSelection(-1);
        }
        
        if (navNextAction != null && navNextAction.WasPressedThisFrame())
        {
            Debug.Log("Botón 'Siguiente' presionado");
            ChangeSelection(1);
        }

        // Control de Slider con Joystick
        if (sliderAction != null && currentInteractables.Count > 0)
        {
            if (currentSelectionIndex >= 0 && currentSelectionIndex < currentInteractables.Count)
            {
                Component current = currentInteractables[currentSelectionIndex];
                if (current is Slider slider)
                {
                    Vector2 input = sliderAction.ReadValue<Vector2>();
                    if (Mathf.Abs(input.x) > 0.1f)
                    {
                        slider.value += input.x * Time.unscaledDeltaTime; // Ajustar velocidad si es necesario
                    }
                }
            }
        }
    }

    private void ChangeSelection(int direction)
    {
        if (currentInteractables.Count == 0) 
        {
            Debug.LogWarning("⚠️ No hay elementos interactuables en la lista para navegar.");
            return;
        }

        currentSelectionIndex += direction;
        if (currentSelectionIndex < 0) currentSelectionIndex = currentInteractables.Count - 1;
        if (currentSelectionIndex >= currentInteractables.Count) currentSelectionIndex = 0;

        Debug.Log($"Seleccionando índice: {currentSelectionIndex} - Objeto: {currentInteractables[currentSelectionIndex].name}");
        HighlightCurrentOption();
    }

    private void HighlightCurrentOption()
    {
        if (currentInteractables.Count == 0) return;
        
        // Primero deseleccionar todo visualmente
        foreach(var item in currentInteractables)
        {
            if (item is VRButton vrb) vrb.SetHighlight(false);
            // Sliders se manejan con EventSystem, lo limpiamos abajo si es necesario
        }

        if (currentSelectionIndex >= 0 && currentSelectionIndex < currentInteractables.Count)
        {
            Component current = currentInteractables[currentSelectionIndex];
            
            if (current is VRButton vrb)
            {
                vrb.SetHighlight(true);
                // Limpiar selección del EventSystem para que no se quede marcado un slider anterior
                if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
            }
            else if (current is Selectable s)
            {
                s.Select();
                if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(s.gameObject);
            }
        }
    }

    private void SelectCurrentOption()
    {
        if (currentInteractables.Count == 0) return;
        
        if (currentSelectionIndex >= 0 && currentSelectionIndex < currentInteractables.Count)
        {
            Component current = currentInteractables[currentSelectionIndex];
            
            if (current is VRButton vrb)
            {
                vrb.SimulateClick();
            }
            else if (current is Button btn)
            {
                btn.onClick.Invoke();
            }
            // Si es slider, ya se controla con el joystick
        }
    }

    private void RefreshInteractables()
    {
        currentInteractables.Clear();
        GameObject activePanel = null;
        
        if (mainMenuPanel.activeSelf) activePanel = mainMenuPanel;
        else if (volumePanel.activeSelf) activePanel = volumePanel;
        
        if (activePanel != null)
        {
            // 1. Buscar VRButtons
            VRButton[] vrButtons = activePanel.GetComponentsInChildren<VRButton>();
            foreach(var btn in vrButtons)
            {
                if (btn.gameObject.activeInHierarchy)
                {
                    currentInteractables.Add(btn);
                }
            }

            // 2. Buscar Sliders (u otros Selectables estándar que no sean botones VR)
            Slider[] sliders = activePanel.GetComponentsInChildren<Slider>();
            foreach(var s in sliders)
            {
                if (s.gameObject.activeInHierarchy && s.interactable)
                {
                    currentInteractables.Add(s);
                }
            }
            
            // Ordenar por posición vertical (de arriba a abajo)
            currentInteractables.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));
            
            Debug.Log($"Se encontraron {currentInteractables.Count} elementos interactuables (VRButtons + Sliders).");
        }
        
        currentSelectionIndex = 0;
        HighlightCurrentOption();
    }

    public void QuitGame()
    {
        IsPaused = false;
        SetLocomotionEnabled(true);

        Debug.Log("🔙 Volviendo al menú principal...");
        SceneManager.LoadScene("MainMenu");
    }

    void OnDestroy()
    {
        if (pauseAction != null) pauseAction.Disable();
        if (navPrevAction != null) navPrevAction.Disable();
        if (navNextAction != null) navNextAction.Disable();
        if (sliderAction != null) sliderAction.Disable();

        // Guardar volumen al salir
        if (volumeSlider != null)
        {
            PlayerPrefs.SetFloat("MasterVolume", volumeSlider.value);
            PlayerPrefs.Save();
        }

        IsPaused = false;
    }
}