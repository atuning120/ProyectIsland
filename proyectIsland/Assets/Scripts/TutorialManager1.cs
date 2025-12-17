using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TutorialManager : MonoBehaviour
{
    [Header("Objetos VR")]
    public XRGrabInteractable cameraGrab;
    public XRGrabInteractable diaryGrab;

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

        if (diaryGrab != null) diaryGrab.enabled = false;

        CancelInvoke();
    }

    // ---------------------------
    // BOTONES
    // ---------------------------

    public void IniciarTutorial()
    {
        if (panelEleccion != null) panelEleccion.SetActive(false);

        // Paso 1: Bienvenida
        if (panelBienvenida != null) panelBienvenida.SetActive(true);

        // A los 5 segundos cambiamos a Movimiento, pero Bienvenida se queda hasta entonces
        Invoke("MostrarPanelMovimiento", 5.0f);

        Debug.Log("Iniciando Tutorial...");
    }

    public void SaltarTutorial()
    {
        if (panelEleccion != null) panelEleccion.SetActive(false);

        if (contadorFotosUI != null) contadorFotosUI.SetActive(true);
        if (iconoCamaraUI != null) iconoCamaraUI.SetActive(true);

        if (diaryGrab != null) diaryGrab.enabled = true;

        pasoActual = 99;
        Debug.Log("Tutorial Saltado.");
    }

    // ---------------------------
    // FLUJO DEL TUTORIAL
    // ---------------------------

    void MostrarPanelMovimiento()
    {
        OcultarTodosPaneles(); // Se va Bienvenida
        if (panelMovimiento != null) panelMovimiento.SetActive(true);

        // NOTA: Ya no hay Invoke para ocultarlo. 
        // Se quedará activo hasta que el jugador agarre la cámara.
    }

    public void JugadorTomoCamara()
    {
        if (pasoActual == 0)
        {
            pasoActual = 1;
            OcultarTodosPaneles(); // Se va Movimiento

            if (panelCamara != null) panelCamara.SetActive(true);
            if (iconoCamaraUI != null) iconoCamaraUI.SetActive(true);

            // El panel de "Toma foto" se queda activo hasta que saque la foto
            Debug.Log("Tutorial: Cámara tomada.");
        }
    }

    public void VerificarFoto(string nombreObjeto)
    {
        if (pasoActual == 1 && nombreObjeto == "Carpa")
        {
            pasoActual = 2;
            OcultarTodosPaneles(); // Se va Panel Cámara

            if (panelDiario != null) panelDiario.SetActive(true);
            if (diaryGrab != null) diaryGrab.enabled = true;

            // El panel "Toma Diario" se queda activo hasta que agarre el diario
            Debug.Log("Tutorial: Foto correcta.");
        }
    }

    public void JugadorTomoDiario()
    {
        if (pasoActual == 2)
        {
            pasoActual = 3;
            OcultarTodosPaneles(); // Se va Panel Diario

            if (panelUsoDiario != null) panelUsoDiario.SetActive(true);

            // Como aquí no hay sensor, damos TIEMPO EXTRA (8 seg) para que lea cómo usarlo
            Invoke("MostrarPanelCorrer", 8.0f);
        }
    }

    void MostrarPanelCorrer()
    {
        OcultarTodosPaneles(); // Se va Uso Diario
        if (panelCorrer != null) panelCorrer.SetActive(true);

        // Damos 7 segundos para leer sobre correr
        Invoke("MostrarPanelAbrirMenu", 7.0f);
    }

    void MostrarPanelAbrirMenu()
    {
        OcultarTodosPaneles(); // Se va Correr
        if (panelAbrirMenu != null) panelAbrirMenu.SetActive(true);

        // Damos 7 segundos para leer sobre el menú
        Invoke("MostrarObjetivoFinal", 7.0f);
    }

    void MostrarObjetivoFinal()
    {
        OcultarTodosPaneles(); // Se va Menú
        if (panelFinal != null) panelFinal.SetActive(true);
        if (contadorFotosUI != null) contadorFotosUI.SetActive(true);

        // SOLICITUD: El final dura 10 segundos
        Invoke("OcultarTodoFinal", 10.0f);
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