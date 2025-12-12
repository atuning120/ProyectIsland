using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VRPhotoCamera : MonoBehaviour
{
    [Header("Configuración General")]
    public float maxDistance = 100f;
    public LayerMask photographableLayer;
    public float focusTimeRequired = 2.5f; // Ajustado a 2.5s como pediste

    [Header("UI Retícula")]
    public Image reticleUI;
    public Color defaultColor = Color.white;
    public Color focusColor = Color.yellow;
    public Color readyColor = Color.green;

    [Header("Configuración del Flash (Suavizado)")]
    public Image flashPanel;
    public AudioClip shutterSound;

    [Range(0f, 1f)]
    public float maxFlashAlpha = 0.65f;

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
            // --- MEJORA IMPORTANTE ---
            // Usamos GetComponentInParent. Esto permite que si el rayo choca con el modelo 3D de la carpa (hijo),
            // encuentre el script PhotographableObject que está en el objeto padre.
            PhotographableObject hitObject = hit.collider.GetComponentInParent<PhotographableObject>();

            if (hitObject != null && !hitObject.isPhotographed)
            {
                // --- MEJORA VISUAL ---
                // Si el checklist dice que ya completamos este grupo (ej: ya sacaste foto a un tigre),
                // impedimos que el círculo se ponga amarillo con los otros tigres.
                if (ChecklistManager.Instance.IsGroupComplete(hitObject.objectName))
                {
                    ResetFocus();
                    return;
                }

                // Si es un objetivo nuevo, reiniciamos el cronómetro
                if (hitObject != currentTarget)
                {
                    currentTarget = hitObject;
                    currentFocusTime = 0f;
                }

                currentFocusTime += Time.deltaTime;

                // Si pasaron los 2.5 segundos
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

        // Comprobación final con el Manager
        if (ChecklistManager.Instance.IsGroupComplete(currentTarget.objectName)) return;

        Debug.Log("¡FOTO TOMADA!");

        SimulateFlash();
        PlayShutterSound();

        currentTarget.OnPhotograph();

        // Reseteamos el foco inmediatamente
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
        Color flashColor = Color.white;
        flashColor.a = maxFlashAlpha;
        flashPanel.color = flashColor;

        while (flashPanel.color.a > 0)
        {
            flashColor.a -= Time.deltaTime * flashFadeSpeed;
            flashPanel.color = flashColor;
            yield return null;
        }

        flashPanel.color = new Color(1f, 1f, 1f, 0f);
    }
}