using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // Necesario si usas el nuevo Input System

public class ToggleController : MonoBehaviour
{
    public Toggle myToggle;             // Referencia al Toggle
    public InputActionReference toggleAction; // Acción que se usará (por ejemplo, un botón del mando)

    void Start()
    {
        if (myToggle == null)
            myToggle = GetComponent<Toggle>();

        // Suscribirse al evento del input
        toggleAction.action.performed += ctx => ToggleSwitch();
    }

    void OnEnable()
    {
        toggleAction.action.Enable();
    }

    void OnDisable()
    {
        toggleAction.action.Disable();
    }

    // 🔹 Cambia el estado del Toggle
    void ToggleSwitch()
    {
        myToggle.isOn = !myToggle.isOn;
    }
}
