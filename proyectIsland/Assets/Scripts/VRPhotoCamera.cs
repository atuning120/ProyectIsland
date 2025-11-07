// VRPhotoCamera.cs (Versión Final con Efectos)
using System.Collections; // NUEVO: Necesario para usar Corrutinas
using UnityEngine;
using UnityEngine.UI;

public class VRPhotoCamera : MonoBehaviour
{
    [Header("Configuración")]
    public float maxDistance = 100f;
    public LayerMask photographableLayer;
    public float focusTimeRequired = 2.0f;

    [Header("UI")]
    public Image reticleUI;
    public Color defaultColor = Color.white;
    public Color focusColor = Color.yellow;
    public Color readyColor = Color.green;

    [Header("Efectos de Foto")] // NUEVO: Sección para los efectos
    public Image flashPanel; // Arrastra aquí tu "FlashPanel"
    public float flashDuration = 0.25f; // Duración total del flash
    public AudioClip shutterSound; // Arrastra aquí tu archivo de sonido

    private PhotographableObject currentTarget;
    private float currentFocusTime = 0f;
    private AudioSource audioSource; // NUEVO: Para guardar la referencia al componente de audio

    void Start() // NUEVO: Usamos Start para obtener el AudioSource al inicio
    {
        // Buscamos el componente AudioSource en este mismo objeto (la Main Camera)
        audioSource = GetComponent<AudioSource>();

        // Es buena práctica asegurarse de que el flash está invisible al empezar
        if (flashPanel != null)
        {
            flashPanel.color = new Color(1f, 1f, 1f, 0f);
        }
    }

    void Update()
    {
        // Dibuja un rayo rojo para depuración en la vista de Scena
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
        if (currentTarget == null || currentTarget.isPhotographed) return;

        Debug.Log("¡FOTO TOMADA!");

        // NUEVO: Disparamos los efectos
        SimulateFlash();
        PlayShutterSound();

        currentTarget.OnPhotograph();
    }

    void ResetFocus()
    {
        currentTarget = null;
        currentFocusTime = 0f;
        reticleUI.color = defaultColor;
    }

    // NUEVO: Método para llamar a la Corrutina del flash
    void SimulateFlash()
    {
        if (flashPanel != null)
        {
            StartCoroutine(FlashEffect());
        }
    }

    // NUEVO: Método para reproducir el sonido
    void PlayShutterSound()
    {
        if (audioSource != null && shutterSound != null)
        {
            // PlayOneShot es ideal para efectos de sonido cortos y rápidos
            audioSource.PlayOneShot(shutterSound);
        }
    }

    // NUEVO: Una Corrutina que maneja el efecto de aparecer y desaparecer del flash
    IEnumerator FlashEffect()
    {
        // Aparece
        flashPanel.color = new Color(1f, 1f, 1f, 1f);

        // Espera una fracción de segundo
        yield return new WaitForSeconds(flashDuration);

        // Desaparece
        flashPanel.color = new Color(1f, 1f, 1f, 0f);
    }
}