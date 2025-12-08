using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VRPhotoCamera : MonoBehaviour
{
    [Header("Configuración General")]
    public float maxDistance = 100f;
    public LayerMask photographableLayer;
    public float focusTimeRequired = 2.0f;

    [Header("UI Retícula")]
    public Image reticleUI;
    public Color defaultColor = Color.white;
    public Color focusColor = Color.yellow;
    public Color readyColor = Color.green;

    [Header("Configuración del Flash (Suavizado)")]
    public Image flashPanel;
    public AudioClip shutterSound;

    // NUEVO: Control deslizante para la intensidad máxima (0 es invisible, 1 es sólido)
    [Range(0f, 1f)]
    public float maxFlashAlpha = 0.65f; // Recomendado entre 0.4 y 0.7 para VR

    // NUEVO: Qué tan rápido desaparece el flash (número más alto = más rápido)
    public float flashFadeSpeed = 4.0f;

    private PhotographableObject currentTarget;
    private float currentFocusTime = 0f;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (flashPanel != null)
        {
 
            flashPanel.color = new Color(1f, 1f, 1f, 0f);

        }
    }

    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * maxDistance, Color.red);
        HandleAiming();
    }

    void HandleAiming()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxDistance, photographableLayer))
        {
            PhotographableObject hitObject = hit.collider.GetComponent<PhotographableObject>();

            if (hitObject != null && !hitObject.isPhotographed)
            {
                if (hitObject != currentTarget)
                {
                    currentTarget = hitObject;
                    currentFocusTime = 0f;
                }

                currentFocusTime += Time.deltaTime;

                if (currentFocusTime >= focusTimeRequired)
                {
                    reticleUI.color = readyColor;
                    TakePhoto();
                }
                else
                {
                    reticleUI.color = focusColor;
                }
                return;
            }
        }

        ResetFocus();
    }

    void TakePhoto()
    {
        // Doble chequeo de seguridad
        if (currentTarget == null || currentTarget.isPhotographed) return;

        // Comprobación final con el Manager (por si acaso el layer no se actualizó a tiempo en el frame anterior)
        if (ChecklistManager.Instance.IsGroupComplete(currentTarget.objectName)) return;

        Debug.Log("¡FOTO TOMADA!");

        SimulateFlash();
        PlayShutterSound();

        currentTarget.OnPhotograph();

        // Reseteamos el foco inmediatamente para evitar disparos múltiples accidentales en el mismo frame
        ResetFocus();
    }

    void ResetFocus()
    {
        currentTarget = null;
        currentFocusTime = 0f;
        reticleUI.color = defaultColor;
    }

    void SimulateFlash()
    {
        if (flashPanel != null)
        {
            // Detenemos cualquier flash anterior si estuviera ocurriendo
            StopCoroutine("FlashEffect");
            StartCoroutine("FlashEffect");
        }
    }

    void PlayShutterSound()
    {
        if (audioSource != null && shutterSound != null)
        {
            audioSource.PlayOneShot(shutterSound);
        }
    }

    IEnumerator FlashEffect()
    {
        // 1. Configurar el color base (Blanco)
        Color flashColor = Color.white;

        // 2. INICIO INSTANTÁNEO: Poner el Alpha al máximo configurado (ej: 0.6)
        flashColor.a = maxFlashAlpha;
        flashPanel.color = flashColor;

        // 3. DESVANECIMIENTO GRADUAL (Loop)
        // Mientras la transparencia sea mayor a 0...
        while (flashPanel.color.a > 0)
        {
            // Restamos alpha basado en el tiempo y la velocidad
            flashColor.a -= Time.deltaTime * flashFadeSpeed;
            flashPanel.color = flashColor;

            // Esperamos al siguiente frame
            yield return null;
        }

        // 4. Asegurarnos de que quede totalmente invisible al final
        flashPanel.color = new Color(1f, 1f, 1f, 0f);
    }
}