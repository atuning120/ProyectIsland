using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VRPhotoCamera : MonoBehaviour
{
    [Header("Configuración General")]
    public float maxDistance = 100f;

    // NUEVO: Grosor del rayo (El "tubo" de detección)
    [Tooltip("El grosor del rayo. 0.05 es fino, 0.3 es grueso.")]
    public float rayRadius = 0.2f;

    [Tooltip("Ajuste de altura del rayo (local Y)")]
    public float rayHeightOffset = 0f;

    public LayerMask photographableLayer;
    public float focusTimeRequired = 2.5f;

    [Header("UI Retícula")]
    public Image reticleUI;
    public Color defaultColor = Color.white;
    public Color focusColor = Color.yellow;
    public Color readyColor = Color.green;

    [Header("Configuración del Flash")]
    public Image flashPanel;
    public AudioClip shutterSound;
    [Range(0f, 1f)] public float maxFlashAlpha = 0.65f;
    public float flashFadeSpeed = 4.0f;

    private PhotographableObject currentTarget;
    private float currentFocusTime = 0f;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (flashPanel != null) flashPanel.color = new Color(1f, 1f, 1f, 0f);
    }

    void Update()
    {
        HandleAiming();
    }

    void HandleAiming()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + (transform.up * rayHeightOffset);

        // CAMBIO PRINCIPAL: Usamos SphereCast en lugar de Raycast
        // Esto lanza una esfera hacia adelante, creando un cilindro de detección
        if (Physics.SphereCast(rayOrigin, rayRadius, transform.forward, out hit, maxDistance, photographableLayer))
        {
            // Buscamos el script en el objeto o en sus padres (para la Carpa)
            PhotographableObject hitObject = hit.collider.GetComponentInParent<PhotographableObject>();

            if (hitObject != null && !hitObject.isPhotographed)
            {
                // Si el grupo ya está completo, ignoramos
                if (ChecklistManager.Instance.IsGroupComplete(hitObject.objectName))
                {
                    ResetFocus();
                    return;
                }

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
        if (currentTarget == null || currentTarget.isPhotographed) return;
        if (ChecklistManager.Instance.IsGroupComplete(currentTarget.objectName)) return;

        Debug.Log("¡FOTO TOMADA!");

        SimulateFlash();
        PlayShutterSound();

        currentTarget.OnPhotograph();
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
        if (audioSource != null && shutterSound != null) audioSource.PlayOneShot(shutterSound);
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

    // --- ESTO ES LO NUEVO PARA VER EL RAYO ---
    // Esta función dibuja líneas en el editor para que veas qué está haciendo la cámara
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 rayOrigin = transform.position + (transform.up * rayHeightOffset);

        // 1. Dibujamos una esfera en la cámara (el inicio del rayo)
        Gizmos.DrawWireSphere(rayOrigin, rayRadius);

        // 2. Dibujamos la línea central hasta donde llega
        Gizmos.DrawRay(rayOrigin, transform.forward * maxDistance);

        // 3. Dibujamos una esfera al final para ver el grosor allá lejos
        Vector3 endPosition = rayOrigin + (transform.forward * maxDistance);
        Gizmos.DrawWireSphere(endPosition, rayRadius);
    }
}