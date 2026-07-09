using UnityEngine;

public class AbrirTrocarLutador : MonoBehaviour
{
    public GameObject painel;

    public void Abrir()
    {
        painel.SetActive(true);
    }

    public void Fechar()
    {
        painel.SetActive(false);
    }
}
