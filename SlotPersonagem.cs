using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class SlotPersonagem : MonoBehaviour
{
    [Header("Textos")]
    public TMP_Text nomeText;
    public TMP_Text forcaText;
    public TMP_Text velocidadeText;
    public TMP_Text staminaText;
    public TMP_Text raridadeText;

    [Header("Botão")]
    public Button ativarButton;

    /// <summary>
    /// Configura o slot com os dados do personagem e define a ação do botão Ativar.
    /// </summary>
    public void Configurar(
        string nome,
        Raridade raridade,
        int forca,
        int velocidade,
        int stamina,
        Action onAtivar)
    {
        // Ativa o slot inteiro
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        AtivarTodosFilhos(transform);

        // Textos
        if (nomeText != null) nomeText.text = nome;
        if (forcaText != null) forcaText.text = $"F {forca}";
        if (velocidadeText != null) velocidadeText.text = $"V {velocidade}";
        if (staminaText != null) staminaText.text = $"S {stamina}";

        // Raridade (ENUM → UI)
        if (raridadeText != null)
        {
            raridadeText.gameObject.SetActive(true);
            raridadeText.text = raridade.ToString();
            raridadeText.color = CorPorRaridade(raridade);
        }

        // Botão
        if (ativarButton != null)
        {
            ativarButton.onClick.RemoveAllListeners();
            ativarButton.onClick.AddListener(() => onAtivar?.Invoke());
        }
    }

    private Color CorPorRaridade(Raridade raridade)
    {
        switch (raridade)
        {
            case Raridade.Raro:      return Color.green;
            case Raridade.Epico:     return Color.blue;
            case Raridade.Lendario:  return Color.magenta;
            default:                 return Color.gray; // Comum
        }
    }

    private void AtivarTodosFilhos(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (!child.gameObject.activeSelf)
                child.gameObject.SetActive(true);

            if (child.childCount > 0)
                AtivarTodosFilhos(child);
        }
    }
}
