using System.Collections;
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

    [Header("Efectos de Foto")]
    public Image flashPanel;
    public float flashDuration = 0.25f;
    public AudioClip shutterSound;

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
        // NUEVO: No hacer nada si el juego está pausado
        if (PauseMenuVR.IsPaused)
        {
            ResetFocus();
            return;
        }

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
        // NUEVO: No tomar foto si el juego está pausado
        if (PauseMenuVR.IsPaused) return;
        
        if (currentTarget == null || currentTarget.isPhotographed) return;

        Debug.Log("¡FOTO TOMADA!");

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

    void SimulateFlash()
    {
        if (flashPanel != null)
        {
            StartCoroutine(FlashEffect());
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
        flashPanel.color = new Color(1f, 1f, 1f, 1f);
        yield return new WaitForSeconds(flashDuration);
        flashPanel.color = new Color(1f, 1f, 1f, 0f);
    }
}