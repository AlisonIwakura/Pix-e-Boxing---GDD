using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class Personagemm
{
    public string personagem;
    public int raridade;      // 1=Comum, 2=Raro, 3=Épico, 4=Lendário
    public int forca;
    public int velocidade;
    public int stamina;
}

[System.Serializable]
public class PersonagensResponse
{
    public string status;
    public Personagemm[] personagens;
}

public class PersonagemUIManager : MonoBehaviour
{
    [Header("Prefab e Content do ScrollView")]
    public GameObject personagemItemPrefab;
    public Transform content;

    [Header("Lista de Personagens")]
    public List<Personagemm> personagens = new List<Personagemm>();

    private string keyPersonagemAtivo = "personagem_ativo";

    public void PopularLista()
    {
        // Limpa lista antiga
        foreach (Transform child in content)
            Destroy(child.gameObject);

        string ativo = PlayerPrefs.GetString(keyPersonagemAtivo, "");

        foreach (Personagemm p in personagens)
        {
            GameObject item = Instantiate(personagemItemPrefab, content);

            TMP_Text nomeTxt = item.transform.Find("Nome")?.GetComponent<TMP_Text>();
            TMP_Text raridadeTxt = item.transform.Find("Raridade")?.GetComponent<TMP_Text>();
            TMP_Text forcaTxt = item.transform.Find("Forca")?.GetComponent<TMP_Text>();
            TMP_Text velocidadeTxt = item.transform.Find("Velocidade")?.GetComponent<TMP_Text>();
            Slider staminaSlider = item.transform.Find("Stamina")?.GetComponent<Slider>();
            Button btn = item.GetComponent<Button>();

            if (nomeTxt != null) nomeTxt.text = p.personagem;
            if (raridadeTxt != null) raridadeTxt.text = $"Raridade: {p.raridade}";
            if (forcaTxt != null) forcaTxt.text = $"Força: {p.forca}";
            if (velocidadeTxt != null) velocidadeTxt.text = $"Velocidade: {p.velocidade}";
            if (staminaSlider != null) { staminaSlider.maxValue = 100; staminaSlider.value = p.stamina; }

            // Cor da raridade
            if (raridadeTxt != null)
            {
                switch (p.raridade)
                {
                    case 1: raridadeTxt.color = Color.gray; break;
                    case 2: raridadeTxt.color = Color.green; break;
                    case 3: raridadeTxt.color = Color.blue; break;
                    case 4: raridadeTxt.color = Color.magenta; break;
                    default: raridadeTxt.color = Color.white; break;
                }
            }

            // Destaca item ativo
            if (nomeTxt != null)
            {
                Image img = item.GetComponent<Image>();
                if (img != null)
                    img.color = (p.personagem == ativo) ? Color.yellow : Color.white;
            }

            // Clique para tornar ativo
            if (btn != null)
            {
                btn.onClick.AddListener(() =>
                {
                    PlayerPrefs.SetString(keyPersonagemAtivo, p.personagem);
                    PlayerPrefs.SetInt("forca_ativo", p.forca);
                    PlayerPrefs.SetInt("velocidade_ativo", p.velocidade);
                    PlayerPrefs.SetInt("stamina_ativo", p.stamina);
                    PlayerPrefs.SetInt("raridade_ativo", p.raridade);
                    PlayerPrefs.Save();
                    AtualizarDestaque();
                    Debug.Log("Personagem ativo: " + p.personagem);
                });
            }
        }
    }

    // Atualiza destaque no ScrollView
    public void AtualizarDestaque()
    {
        string ativo = PlayerPrefs.GetString(keyPersonagemAtivo, "");

        foreach (Transform child in content)
        {
            TMP_Text nomeTxt = child.Find("Nome")?.GetComponent<TMP_Text>();
            Image img = child.GetComponent<Image>();
            if (nomeTxt != null && img != null)
            {
                img.color = (nomeTxt.text == ativo) ? Color.yellow : Color.white;
            }
        }
    }
}
