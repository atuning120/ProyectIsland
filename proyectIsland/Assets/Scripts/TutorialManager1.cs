using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TutorialManager : MonoBehaviour
{
    [Header("Objetos VR")]
    public XRGrabInteractable cameraGrab;
    public XRGrabInteractable diaryGrab;

    [Header("Paneles de Instrucciones")]
    public GameObject panelBienvenida;
    public GameObject panelMovimiento;     // ← NUEVO
    public GameObject panelAbrirMenu;      // ← NUEVO
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
        OcultarTodosPaneles();

        // Mostrar bienvenida
        if (panelBienvenida != null) panelBienvenida.SetActive(true);

        // Ocultar UI
        if (contadorFotosUI != null) contadorFotosUI.SetActive(false);
        if (iconoCamaraUI != null) iconoCamaraUI.SetActive(false);

        // Bloquear diario al inicio
        if (diaryGrab != null) diaryGrab.enabled = false;

        // Mostrar movimiento después de bienvenida
        Invoke("MostrarPanelMovimiento", 4.0f);
    }

    // ---------------------------
    // SECUENCIA DEL TUTORIAL
    // ---------------------------

    void MostrarPanelMovimiento()
    {
        OcultarTodosPaneles();
        if (panelMovimiento != null) panelMovimiento.SetActive(true);
        Invoke("OcultarPanelMovimiento", 5.0f);
    }
    void OcultarPanelMovimiento()
{
    if (panelMovimiento != null)
        panelMovimiento.SetActive(false);
}

    public void JugadorTomoCamara()
    {
        if (pasoActual == 0)
        {
            pasoActual = 1;
            OcultarTodosPaneles();

            if (panelCamara != null) panelCamara.SetActive(true);
            if (iconoCamaraUI != null) iconoCamaraUI.SetActive(true);

            Debug.Log("Tutorial: Cámara tomada.");
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

            Debug.Log("Tutorial: Foto correcta. Diario desbloqueado.");
        }
    }

    public void JugadorTomoDiario()
    {
        if (pasoActual == 2)
        {
            pasoActual = 3;
            OcultarTodosPaneles();

            if (panelUsoDiario != null) panelUsoDiario.SetActive(true);

            Invoke("MostrarPanelCorrer", 6.0f);
        }
    }

    void MostrarPanelCorrer()
    {
        OcultarTodosPaneles();
        if (panelCorrer != null) panelCorrer.SetActive(true);

        // NUEVO → después de correr, mostrar panel para abrir menú
        Invoke("MostrarPanelAbrirMenu", 6.0f);
    }

    void MostrarPanelAbrirMenu()
    {
        OcultarTodosPaneles();
        if (panelAbrirMenu != null) panelAbrirMenu.SetActive(true);

        // Luego del panel de menú → objetivo final
        Invoke("MostrarObjetivoFinal", 6.0f);
    }

    void MostrarObjetivoFinal()
    {
        OcultarTodosPaneles();
        if (panelFinal != null) panelFinal.SetActive(true);

        if (contadorFotosUI != null) contadorFotosUI.SetActive(true);

        Invoke("OcultarTodoFinal", 8.0f);
    }

    void OcultarTodoFinal()
    {
        if (panelFinal != null) panelFinal.SetActive(false);
    }

    // ---------------------------
    // UTILIDAD
    // ---------------------------

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
