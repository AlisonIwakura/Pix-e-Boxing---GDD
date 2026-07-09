using UnityEngine;
using UnityEngine.UI;

public class BarraFillUI : MonoBehaviour
{
    [Header("Referência do Fill")]
    public Image fillImage;

    [Header("Valores")]
    public float valorAtual = 100f;
    public float valorMaximo = 100f;

    void Update()
    {
        if (fillImage == null || valorMaximo <= 0) return;

        fillImage.fillAmount = valorAtual / valorMaximo;
    }

    // Atualiza valores externamente
    public void Atualizar(float atual, float max)
    {
        valorAtual = atual;
        valorMaximo = max;
    }
}
