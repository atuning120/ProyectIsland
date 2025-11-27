using UnityEngine;

public class abrirDifMenu : MonoBehaviour
{
    public GameObject menuPrincipal;
    public GameObject menuActual;


    public void AbrirNuevoMenu()
    {
        menuPrincipal.SetActive(false);
        menuActual.SetActive(true);
    }
}

