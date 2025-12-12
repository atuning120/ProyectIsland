using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; // Aqu� est� todo lo necesario
using TMPro; // Si usas TextMeshPro

public class TutorialManager : MonoBehaviour
{
    [Header("Objetos VR")]
    // CORREGIDO: Quitamos ".Interactables" y dejamos solo el tipo
    public XRGrabInteractable cameraGrab;
    public XRGrabInteractable diaryGrab;

    [Header("Paneles de Instrucciones")]
    public GameObject panelBienvenida;
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
        // 1. Configuraci�n Inicial
        OcultarTodosPaneles();
        if (panelBienvenida != null) panelBienvenida.SetActive(true);

        if (contadorFotosUI != null) contadorFotosUI.SetActive(false);
        if (iconoCamaraUI != null) iconoCamaraUI.SetActive(false);

        // 2. BLOQUEO F�SICO
        if (diaryGrab != null) diaryGrab.enabled = false;
    }

    // --- M�TODOS P�BLICOS ---

    public void JugadorTomoCamara()
    {
        if (pasoActual == 0)
        {
            pasoActual = 1;
            OcultarTodosPaneles();
            if (panelCamara != null) panelCamara.SetActive(true);

            if (iconoCamaraUI != null) iconoCamaraUI.SetActive(true);

            Debug.Log("Tutorial: C�mara tomada. Esperando foto a la Carpa.");
        }
    }

    public void VerificarFoto(string nombreObjeto)
    {
        // Solo importa si estamos en el paso 1 y es la Carpa
        if (pasoActual == 1 && nombreObjeto == "Carpa")
        {
            pasoActual = 2;
            OcultarTodosPaneles();
            if (panelDiario != null) panelDiario.SetActive(true);

            // DESBLOQUEO
            if (diaryGrab != null) diaryGrab.enabled = true;

            Debug.Log("Tutorial: Foto Carpa lista. Diario desbloqueado.");
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

    void OcultarTodosPaneles()
    {
        if (panelBienvenida) panelBienvenida.SetActive(false);
        if (panelCamara) panelCamara.SetActive(false);
        if (panelDiario) panelDiario.SetActive(false);
        if (panelUsoDiario) panelUsoDiario.SetActive(false);
        if (panelCorrer) panelCorrer.SetActive(false);
        if (panelFinal) panelFinal.SetActive(false);
    }
}