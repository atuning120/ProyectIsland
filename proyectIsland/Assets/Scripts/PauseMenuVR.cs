using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using UnityEngine.Audio;

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

    public static bool IsPaused { get; private set; } = false;

    private bool isPaused = false;
    private InputAction pauseAction;
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
        }
    }

    void Update()
    {
        if (pauseAction != null && pauseAction.WasPressedThisFrame())
        {
            TogglePause();
        }

        if (isPaused && pauseMenuCanvas.activeSelf)
        {
            UpdateCanvasOrientation();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        IsPaused = isPaused;

        if (isPaused)
        {
            PositionCanvasInFrontOfPlayer();
            PausePhotographableObjects(true);
            ShowMainMenu();

            Debug.Log("🎮 JUEGO PAUSADO - Objetos fotografiables congelados y muteados");
        }
        else
        {
            PausePhotographableObjects(false);

            Debug.Log("▶️ JUEGO REANUDADO - Objetos fotografiables activos con audio");
        }

        pauseMenuCanvas.SetActive(isPaused);
        SetLocomotionEnabled(!isPaused);

        Debug.Log($"Menú de pausa: {(isPaused ? "ABIERTO" : "CERRADO")}");
    }

    // NUEVO: Mostrar menú principal
    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        volumePanel.SetActive(false);
        Debug.Log("📋 Menú principal mostrado");
    }

    // NUEVO: Mostrar menú de volumen
    public void ShowVolumeMenu()
    {
        mainMenuPanel.SetActive(false);
        volumePanel.SetActive(true);
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

    public void QuitGame()
    {
        IsPaused = false;

        Debug.Log("👋 Saliendo del juego...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void OnDestroy()
    {
        if (pauseAction != null)
        {
            pauseAction.Disable();
        }

        // Guardar volumen al salir
        if (volumeSlider != null)
        {
            PlayerPrefs.SetFloat("MasterVolume", volumeSlider.value);
            PlayerPrefs.Save();
        }

        IsPaused = false;
    }
}