using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TutorialManager : MonoBehaviour
{
    [Header("Objetos VR")]
    public XRGrabInteractable cameraGrab;
    public XRGrabInteractable diaryGrab;

    // Referencia al script de la cámara para bloquearla
    [Tooltip("Arrastra aquí el objeto que tiene el script VRPhotoCamera")]
    public VRPhotoCamera photoCameraScript;

    [Header("Panel de Decisión Inicial")]
    public GameObject panelEleccion;

    [Header("Paneles de Instrucciones")]
    public GameObject panelBienvenida;
    public GameObject panelMovimiento;
    public GameObject panelAbrirMenu;
    public GameObject panelCamara;
    public GameObject panelDiario;
    public GameObject panelUsoDiario;
    public GameObject panelCorrer;
    public GameObject panelFinal;

    [Header("UI del Juego")]
    public GameObject contadorFotosUI;
    public GameObject iconoCamaraUI;

    private int pasoActual = 0;

    void Start()
    {
        ReiniciarEstado();

        if (panelEleccion != null)
        {
            panelEleccion.SetActive(true);
        }
        else
        {
            IniciarTutorial();
        }
    }

    void ReiniciarEstado()
    {
        pasoActual = 0;
        OcultarTodosPaneles();

        if (contadorFotosUI != null) contadorFotosUI.SetActive(false);
        if (iconoCamaraUI != null) iconoCamaraUI.SetActive(false);

        // Bloquear agarre del diario
        if (diaryGrab != null) diaryGrab.enabled = false;

        // Bloquear funcionalidad de la cámara
        if (photoCameraScript != null) photoCameraScript.enabled = false;

        CancelInvoke();
    }

    // ---------------------------
    // BOTONES
    // ---------------------------

    public void IniciarTutorial()
    {
        if (panelEleccion != null) panelEleccion.SetActive(false);

        // Aseguramos que la cámara siga bloqueada al iniciar el tutorial
        if (photoCameraScript != null) photoCameraScript.enabled = false;

        // Paso 1: Bienvenida
        if (panelBienvenida != null) panelBienvenida.SetActive(true);

        // CAMBIO AQUÍ: Cambiado de 15.0f a 5.0f solo para la bienvenida
        Invoke("MostrarPanelMovimiento", 5.0f);

        Debug.Log("Iniciando Tutorial...");
    }

    public void SaltarTutorial()
    {
        if (panelEleccion != null) panelEleccion.SetActive(false);

        if (contadorFotosUI != null) contadorFotosUI.SetActive(true);
        if (iconoCamaraUI != null) iconoCamaraUI.SetActive(true);

        if (diaryGrab != null) diaryGrab.enabled = true;

        // Si salta el tutorial, activamos la cámara inmediatamente
        if (photoCameraScript != null) photoCameraScript.enabled = true;

        pasoActual = 99;
        Debug.Log("Tutorial Saltado.");
    }

    // ---------------------------
    // FLUJO DEL TUTORIAL
    // ---------------------------

    void MostrarPanelMovimiento()
    {
        OcultarTodosPaneles();
        if (panelMovimiento != null) panelMovimiento.SetActive(true);
        // Este panel NO tiene tiempo, espera a que el usuario agarre la cámara
    }

    public void JugadorTomoCamara()
    {
        if (pasoActual == 0)
        {
            pasoActual = 1;
            OcultarTodosPaneles();

            if (panelCamara != null) panelCamara.SetActive(true);
            if (iconoCamaraUI != null) iconoCamaraUI.SetActive(true);

            // Activamos la cámara para que pueda sacar fotos
            if (photoCameraScript != null) photoCameraScript.enabled = true;

            Debug.Log("Tutorial: Cámara tomada y ACTIVADA.");
            // Este panel NO tiene tiempo, espera a que el usuario saque la foto
        }
    }

    public void VerificarFoto(string nombreObjeto)
    {
        if (pasoActual == 1 && nombreObjeto == "Carpa")
        {
            pasoActual = 2;
            OcultarTodosPaneles();

            if (panelDiario != null) panelDiario.SetActive(true);
            if (diaryGrab != null) diaryGrab.enabled = true;

            Debug.Log("Tutorial: Foto correcta.");
            // Este panel NO tiene tiempo, espera a que el usuario agarre el diario
        }
    }

    public void JugadorTomoDiario()
    {
        if (pasoActual == 2)
        {
            pasoActual = 3;
            OcultarTodosPaneles();

            if (panelUsoDiario != null) panelUsoDiario.SetActive(true);

            // Mantenemos 15 segundos para leer sobre el uso del diario
            Invoke("MostrarPanelCorrer", 15.0f);
        }
    }

    void MostrarPanelCorrer()
    {
        OcultarTodosPaneles();
        if (panelCorrer != null) panelCorrer.SetActive(true);

        // Mantenemos 15 segundos para leer sobre correr
        Invoke("MostrarPanelAbrirMenu", 15.0f);
    }

    void MostrarPanelAbrirMenu()
    {
        OcultarTodosPaneles();
        if (panelAbrirMenu != null) panelAbrirMenu.SetActive(true);

        // Mantenemos 15 segundos para leer sobre el menú
        Invoke("MostrarObjetivoFinal", 15.0f);
    }

    void MostrarObjetivoFinal()
    {
        OcultarTodosPaneles();
        if (panelFinal != null) panelFinal.SetActive(true);
        if (contadorFotosUI != null) contadorFotosUI.SetActive(true);

        // Mantenemos 15 segundos para el mensaje final
        Invoke("OcultarTodoFinal", 15.0f);
    }

    void OcultarTodoFinal()
    {
        if (panelFinal != null) panelFinal.SetActive(false);
    }

    void OcultarTodosPaneles()
    {
        if (panelBienvenida) panelBienvenida.SetActive(false);
        if (panelMovimiento) panelMovimiento.SetActive(false);
        if (panelAbrirMenu) panelAbrirMenu.SetActive(false);
        if (panelCamara) panelCamara.SetActive(false);
        if (panelDiario) panelDiario.SetActive(false);
        if (panelUsoDiario) panelUsoDiario.SetActive(false);
        if (panelCorrer) panelCorrer.SetActive(false);
        if (panelFinal) panelFinal.SetActive(false);
    }
}