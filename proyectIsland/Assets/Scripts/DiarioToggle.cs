using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ToggleController : MonoBehaviour
{
    [Header("UI & Objects")]
    public Toggle myToggle;                 // El toggle invisible
    public GameObject diaryObject;          // Diario a activar/desactivar

    [Header("Spawn Settings")]
    [Tooltip("Distancia a la que aparece el diario frente al jugador")]
    public float spawnDistance = 1.0f;
    [Tooltip("Ajuste de rotación adicional (x, y, z) para corregir si sale al revés o de cabeza")]
    public Vector3 rotationOffset = Vector3.zero;

    [Header("Input Settings")]
    [Tooltip("Arrastra aquí tu archivo: XRI Default Input Actions.inputactions")]
    [SerializeField] private InputActionAsset inputActions;

    [Tooltip("Nombre del Mapa de Acción (ej: 'XRI RightHand Interaction')")]
    [SerializeField] private string actionMapName = "XRI RightHand Interaction";

    [Tooltip("Nombre del botón (Primary Button = A en mano derecha)")]
    [SerializeField] private string buttonName = "Primary Button";

    // Variable privada para guardar la acción encontrada
    private InputAction _toggleInput;

    void Awake()
    {
        if (myToggle == null)
            myToggle = GetComponent<Toggle>();
    }

    void Start()
    {
        // 1. Validaciones iniciales del Toggle y Diario
        if (myToggle == null)
        {
            Debug.LogError("No se encontró el Toggle. El script debe estar en el mismo GO que el Toggle.");
            return;
        }

        // Aseguramos que el diario inicie desactivado para que no aparezca al inicio
        myToggle.isOn = false;
        diaryObject.SetActive(false);

        myToggle.onValueChanged.AddListener(OnToggleChanged);

        // 2. Lógica de Input (Igual que en PauseMenuVR)
        if (inputActions != null)
        {
            // Buscamos el mapa de acciones (ej: mano derecha)
            var actionMap = inputActions.FindActionMap(actionMapName);
            if (actionMap != null)
            {
                // Buscamos el botón específico por nombre
                _toggleInput = actionMap.FindAction(buttonName);

                if (_toggleInput != null)
                {
                    _toggleInput.Enable();
                    Debug.Log($"✓ ToggleController: Botón configurado correctamente: {buttonName}");
                }
                else
                {
                    Debug.LogError($"✗ ToggleController: No se encontró el botón '{buttonName}' en el mapa '{actionMapName}'.");
                }
            }
            else
            {
                Debug.LogError($"✗ ToggleController: No se encontró el mapa '{actionMapName}'. Revisa el nombre en tu Input Action Asset.");
            }
        }
        else
        {
            Debug.LogError("✗ ToggleController: Falta asignar el 'Input Action Asset' en el inspector.");
        }
    }

    void Update()
    {
        // 3. Detección del input en Update (Polling)
        if (_toggleInput != null && _toggleInput.WasPressedThisFrame())
        {
            ToggleSwitch();
        }
    }

    void ToggleSwitch()
    {
        myToggle.isOn = !myToggle.isOn;
        // El listener OnToggleChanged se encargará de activar/desactivar el objeto
    }

    void OnToggleChanged(bool value)
    {
        if (diaryObject != null)
        {
            if (value)
            {
                PositionDiary();
            }
            diaryObject.SetActive(value);
        }
    }

    void PositionDiary()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            // 1. Calcular dirección frente al jugador (ignorando inclinación vertical/pitch)
            Vector3 forwardFlat = mainCam.transform.forward;
            forwardFlat.y = 0; 
            forwardFlat.Normalize();

            // 2. Posicionar frente a la cámara (a la misma altura de la cámara, pero sin bajar/subir por la mirada)
            diaryObject.transform.position = mainCam.transform.position + (forwardFlat * spawnDistance);
            
            // 3. Rotar para mirar hacia el jugador (solo en el eje Y)
            // Creamos un punto objetivo que está en la misma posición XZ que la cámara, pero a la altura del diario
            Vector3 targetPostition = new Vector3(mainCam.transform.position.x, diaryObject.transform.position.y, mainCam.transform.position.z);
            
            diaryObject.transform.LookAt(targetPostition);

            // 4. Aplicar corrección manual si es necesario (ej: si sale de cabeza o rotado)
            diaryObject.transform.Rotate(rotationOffset);
        }
    }

    void OnDestroy()
    {
        // Limpieza
        if (_toggleInput != null)
            _toggleInput.Disable();

        if (myToggle != null)
            myToggle.onValueChanged.RemoveListener(OnToggleChanged);
    }
}