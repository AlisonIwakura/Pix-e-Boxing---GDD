using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

public class RevelarPersonagemUI : MonoBehaviour
{
    [Header("UI")]
    public Image spritePersonagem;
    public CardRaridadeUI cardRaridade;

    public Text nomePersonagem;
    public Text forca;
    public Text velocidade;
    public Text stamina;

    public Button botaoRevelar;
    public Button botaoConfirmar;

    [Header("Sprites")]
    public Sprite mykeTyroson;
    public Sprite madruga;
    public Sprite muhammad;
    public Sprite maguile;
    public Sprite popi;

    [Header("Config")]
    public float tempoSuspense = 1.2f;
    public string cenaMenu = "MenuInicial";

    [Header("Servidor")]
    public string urlRevelar = "http://localhost/sc_game/revelar_personagem.php";

    public SaldoUI saldoUI; // Referência opcional para atualizar saldo após revelar

    void Start()
    {
        // Desativa tudo no início
        if (spritePersonagem != null) spritePersonagem.gameObject.SetActive(false);
        if (cardRaridade != null) cardRaridade.gameObject.SetActive(false);
        if (nomePersonagem != null) nomePersonagem.gameObject.SetActive(false);
        if (forca != null) forca.gameObject.SetActive(false);
        if (velocidade != null) velocidade.gameObject.SetActive(false);
        if (stamina != null) stamina.gameObject.SetActive(false);
        if (botaoConfirmar != null) botaoConfirmar.gameObject.SetActive(false);
    }

    // 🔘 Botão REVELAR
    public void Revelar()
    {
        if (botaoRevelar == null) return;
        botaoRevelar.interactable = false;
        StartCoroutine(RevelarCoroutine());
    }

    IEnumerator RevelarCoroutine()
    {
        int userId = PlayerPrefs.GetInt("user_id", 0);

        if (userId <= 0)
        {
            Debug.LogError("Usuário não logado");
            if (botaoRevelar != null) botaoRevelar.interactable = true;
            yield break;
        }

        yield return new WaitForSeconds(tempoSuspense);

        WWWForm form = new WWWForm();
        form.AddField("usuario_id", userId);

        using (UnityWebRequest www = UnityWebRequest.Post(urlRevelar, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Erro de conexão: " + www.error);
                if (botaoRevelar != null) botaoRevelar.interactable = true;
                yield break;
            }

            string resposta = www.downloadHandler.text.Trim();
            Debug.Log("Resposta Revelar: " + resposta);

            if (resposta == "SaldoInsuficiente")
            {
                Debug.LogWarning("Saldo insuficiente para revelar personagem");
                if (botaoRevelar != null) botaoRevelar.interactable = true;
                yield break;
            }

            if (!resposta.StartsWith("Success"))
            {
                Debug.LogError("Erro ao revelar personagem: " + resposta);
                if (botaoRevelar != null) botaoRevelar.interactable = true;
                yield break;
            }

            // Success|Personagem|Raridade|Forca|Velocidade|Stamina
            string[] dados = resposta.Split('|');
            if (dados.Length < 6)
            {
                Debug.LogError("Formato de resposta inválido: " + resposta);
                if (botaoRevelar != null) botaoRevelar.interactable = true;
                yield break;
            }

            // Parse seguro dos enums
            PersonagemID personagem;
            Raridade raridade;

            if (!System.Enum.TryParse(dados[1], out personagem))
            {
                Debug.LogWarning("Personagem inválido: " + dados[1]);
                personagem = PersonagemID.MykeTyroson;
            }

            if (!System.Enum.TryParse(dados[2], out raridade))
            {
                Debug.LogWarning("Raridade inválida: " + dados[2]);
                raridade = Raridade.Comum;
            }

            int forcaAttr = int.Parse(dados[3]);
            int velocidadeAttr = int.Parse(dados[4]);
            int staminaAttr = int.Parse(dados[5]);

            // Mostra o personagem na UI
            MostrarPersonagem(personagem, raridade, forcaAttr, velocidadeAttr, staminaAttr);

            // ⚡ Salva o personagem ativo nos PlayerPrefs
            PlayerPrefs.SetString("personagem_ativo", personagem.ToString());
            PlayerPrefs.SetInt("forca_ativo", forcaAttr);
            PlayerPrefs.SetInt("velocidade_ativo", velocidadeAttr);
            PlayerPrefs.SetInt("stamina_ativo", staminaAttr);
            PlayerPrefs.SetString("raridade_ativo", raridade.ToString());
            PlayerPrefs.Save();

            Debug.Log("Personagem ativo salvo: " + personagem);

            // Atualiza saldo
            if (saldoUI != null)
            {
                saldoUI.AtualizarAgora();
            }
        }
    }

    // 🔹 PUBLIC para outros scripts
    public void MostrarPersonagem(PersonagemID personagem, Raridade raridade, int forcaAttr, int velocidadeAttr, int staminaAttr)
    {
        if (spritePersonagem != null) spritePersonagem.sprite = GetSpritePersonagem(personagem);
        if (spritePersonagem != null) spritePersonagem.gameObject.SetActive(true);

        if (nomePersonagem != null)
        {
            nomePersonagem.text = GetNomeFormatado(personagem);
            nomePersonagem.gameObject.SetActive(true);
        }

        // 🔹 Atualiza o sprite da raridade
        if (cardRaridade != null)
        {
            cardRaridade.SetRaridade(raridade); // ⬅ Certifica que o método altera a imagem
            cardRaridade.gameObject.SetActive(true);
        }

        if (forca != null) { forca.text = $"F: {forcaAttr}"; forca.gameObject.SetActive(true); }
        if (velocidade != null) { velocidade.text = $"V: {velocidadeAttr}"; velocidade.gameObject.SetActive(true); }
        if (stamina != null) { stamina.text = $"S: {staminaAttr}"; stamina.gameObject.SetActive(true); }

        if (botaoConfirmar != null) botaoConfirmar.gameObject.SetActive(true);
    }

    public void Confirmar()
    {
        SceneManager.LoadScene(cenaMenu);
    }

    Sprite GetSpritePersonagem(PersonagemID personagem)
    {
        switch (personagem)
        {
            case PersonagemID.MykeTyroson: return mykeTyroson;
            case PersonagemID.Madruga: return madruga;
            case PersonagemID.Muhammad: return muhammad;
            case PersonagemID.Maguile: return maguile;
            case PersonagemID.PoPi: return popi;
        }
        return null;
    }

    string GetNomeFormatado(PersonagemID personagem)
    {
        switch (personagem)
        {
            case PersonagemID.MykeTyroson: return "Myke Tyroson";
            case PersonagemID.PoPi: return "PoPi";
        }
        return personagem.ToString();
    }
}
