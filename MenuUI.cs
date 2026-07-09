using UnityEngine;
using TMPro;

public class MenuUI : MonoBehaviour
{
    public TextMeshProUGUI textoSaldo;

    void Start()
    {
        if (textoSaldo != null)
        {
            float saldo = PlayerPrefs.GetFloat("saldo", 0);
            textoSaldo.text = $" {saldo:F2}";
        }
        else
        {
            Debug.LogError("TextoSaldo não foi atribuído no Inspector!");
        }
    }
}
