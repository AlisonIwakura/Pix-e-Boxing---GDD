using UnityEngine;

public class OpenLink : MonoBehaviour
{
    public string url = "https://seusite.com/recarga";

    public void AbrirLink()
    {
        Application.OpenURL(url);
    }
}