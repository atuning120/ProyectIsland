using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;


public class EquipOnGrab : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    private MeshRenderer[] meshRenderers;
    private Collider[] colliders;

    [Tooltip("Si es True, el objeto se vuelve invisible al agarrarlo.")]
    public bool hideVisualsOnGrab = true;

    [Tooltip("Marca esto para que el objeto se DESTRUYA de la escena permanentemente al agarrarlo.")]
    public bool destroyOnGrab = false; // <-- CAMBIO AQUI: NUEVA VARIABLE

    // Opcional: Si necesitas que el objeto haga algo después de destruirse,
    // puedes tener una referencia a un Manager que se encargue, pero usualmente no es necesario.
    // public TutorialManager tutorialManager; // Descomenta si lo necesitas

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        colliders = GetComponentsInChildren<Collider>();
    }

    void OnEnable()
    {
        // Nos suscribimos SOLO al evento de "cuando se agarra"
        grabInteractable.selectEntered.AddListener(OnGrab);

        // NO necesitamos el OnRelease si se va a destruir o esconder permanentemente
        // grabInteractable.selectExited.AddListener(OnRelease); // Descomentar si necesitas OnRelease para otra cosa
    }

    void OnDisable()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        // grabInteractable.selectExited.RemoveListener(OnRelease); // Descomentar si necesitas OnRelease para otra cosa
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        // Si no queremos que se vea, lo ocultamos primero
        if (hideVisualsOnGrab)
        {
            ToggleVisuals(false);
        }

        // Opcional: Desactivamos las colisiones para que no estorbe en la mano
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        // --- ¡EL CAMBIO CLAVE AQUÍ! ---
        if (destroyOnGrab)
        {
            // Primero, aseguramos que ya no pueda ser agarrado de nuevo si lo destruimos
            grabInteractable.enabled = false;


            Destroy(gameObject, 0.1f); // Destruye el objeto 0.1 segundos después
        }
      
    }

   

    void ToggleVisuals(bool isActive)
    {
        foreach (var mesh in meshRenderers)
        {
            mesh.enabled = isActive;
        }
    }
}