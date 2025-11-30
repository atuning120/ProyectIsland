using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ToggleController : MonoBehaviour
{
    public Toggle myToggle;                     // El toggle invisible
    public InputActionReference toggleAction;   // Botón del control
    public GameObject diaryObject;              // Diario a activar/desactivar

    void Awake()
    {
        if (myToggle == null)
            myToggle = GetComponent<Toggle>();
    }

    void Start()
    {
        if (myToggle == null)
        {
            Debug.LogError("No se encontró el Toggle. El script debe estar en el mismo GO que el Toggle.");
            return;
        }

        // Sincroniza estado inicial del diario
        diaryObject.SetActive(myToggle.isOn);

        // Cuando cambie el toggle → abre/cierra el diario
        myToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnEnable()
    {
        toggleAction.action.performed += ctx => ToggleSwitch();
        toggleAction.action.Enable();
    }

    void OnDisable()
    {
        toggleAction.action.Disable();
        myToggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    void ToggleSwitch()
    {
        myToggle.isOn = !myToggle.isOn;
    }

    void OnToggleChanged(bool value)
    {
        diaryObject.SetActive(value);
    }
}
