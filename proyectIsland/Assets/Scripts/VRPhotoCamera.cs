// VRPhotoCamera.cs
using UnityEngine;
using UnityEngine.UI; // Necesario para manipular la UI

public class VRPhotoCamera : MonoBehaviour
{
    [Header("Configuración")]
    public float maxDistance = 100f; // Distancia máxima para fotografiar
    public LayerMask photographableLayer; // Para que el rayo solo detecte la capa "Photographable"
    public float focusTimeRequired = 2.0f; // Segundos necesarios para enfocar

    [Header("UI")]
    public Image reticleUI; // Asigna aquí la imagen de tu círculo/retícula desde el editor
    public Color defaultColor = Color.white;
    public Color focusColor = Color.yellow;
    public Color readyColor = Color.green;

    [Header("Input (Ejemplo con el sistema antiguo)")]
    public string fireButton = "Fire1"; // Cambia esto por tu botón de VR (e.g., "XRI_Right_TriggerButton")

    private PhotographableObject currentTarget;
    private float currentFocusTime = 0f;

    void Update()
    {
        HandleAiming();
        //HandleShooting();
    }

    void HandleAiming()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance, photographableLayer))
        {
            PhotographableObject hitObject = hit.collider.GetComponent<PhotographableObject>();

            if (hitObject != null && !hitObject.isPhotographed) // Añadimos la condición de que no haya sido ya fotografiado
            {
                if (hitObject != currentTarget)
                {
                    currentTarget = hitObject;
                    currentFocusTime = 0f;
                }

                currentFocusTime += Time.deltaTime;

                if (currentFocusTime >= focusTimeRequired)
                {
                    reticleUI.color = readyColor; // ¡Listo!
                    TakePhoto(); // ¡Llamamos a TakePhoto automáticamente!
                }
                else
                {
                    reticleUI.color = focusColor; // Enfocando...
                }
                return;
            }
        }

        ResetFocus();
    }


    void HandleShooting()
    {
        // Comprobamos si el foco está listo y si el jugador presiona el botón
        if (currentTarget != null && currentFocusTime >= focusTimeRequired)
        {
            // Revisa la documentación de tu SDK de VR para el nombre exacto del botón
            if (Input.GetButtonDown(fireButton))
            {
                TakePhoto();
            }
        }
    }

    void TakePhoto()
    {
        if (currentTarget == null || currentTarget.isPhotographed) return; // Seguridad para no tomar fotos dobles

        Debug.Log("¡FOTO TOMADA!");
        currentTarget.OnPhotograph();

        // Aquí podrías añadir un efecto de flash y un sonido de obturador

        // No reseteamos el foco inmediatamente para que el jugador vea el círculo verde un instante
        // ResetFocus() se llamará automáticamente en el siguiente frame porque hitObject.isPhotographed será true
    }

    void ResetFocus()
    {
        currentTarget = null;
        currentFocusTime = 0f;
        reticleUI.color = defaultColor;
    }
}