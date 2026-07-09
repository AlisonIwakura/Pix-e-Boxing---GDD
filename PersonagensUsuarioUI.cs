using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

public class PersonagensUsuarioUI : MonoBehaviour
{
    [Header("UI")]
    public Transform content;
    public GameObject slotPrefab;

    [Header("Referências")]
    public RevelarPersonagemUI revelarPersonagemUI;

    [Header("Servidor")]
    public string urlListar = "http://localhost/sc_game/listar_personagens.php";

    private readonly List<GameObject> slotsAtivos = new List<GameObject>();

    void OnEnable()
    {
        Limpar();
        StartCoroutine(CarregarPersonagens());
    }

    // =========================
    // LIMPAR
    // =========================
    void Limpar()
    {
        foreach (Transform t in content)
            Destroy(t.gameObject);

        slotsAtivos.Clear();
    }

    // =========================
    // CARREGAR PERSONAGENS
    // =========================
    IEnumerator CarregarPersonagens()
    {
        int userId = PlayerPrefs.GetInt("user_id", 0);
        if (userId <= 0)
        {
            Debug.LogError("Usuário não logado");
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("usuario_id", userId);

        using (UnityWebRequest www = UnityWebRequest.Post(urlListar, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Erro ao listar personagens: " + www.error);
                yield break;
            }

            string resposta = www.downloadHandler.text.Trim();
            Debug.Log("Resposta Lista: " + resposta);

            PersonagensResponse data =
                JsonUtility.FromJson<PersonagensResponse>(resposta);

            if (data == null || data.personagens == null)
            {
                Debug.LogError("Resposta inválida do servidor");
                yield break;
            }

            foreach (PersonagemData p in data.personagens)
            {
                GameObject slotObj = Instantiate(slotPrefab, content);
                slotsAtivos.Add(slotObj);

                SlotPersonagem slot = slotObj.GetComponent<SlotPersonagem>();
                if (slot == null) continue;

                // 🔥 STRING → ENUM (CORRETO)
                Raridade raridadeEnum = ConverterRaridade(p.raridade);

                slot.Configurar(
                    p.personagem,
                    raridadeEnum,
                    p.forca,
                    p.velocidade,
                    p.stamina,
                    () => TrocarPersonagem(p)
                );
            }

            AtualizarDestaque(
                PlayerPrefs.GetString("personagem_ativo", "")
            );
        }
    }

    // =========================
    // TROCAR PERSONAGEM
    // =========================
    void TrocarPersonagem(PersonagemData p)
    {
        // 🔹 PERSONAGEM
        PersonagemID personagemEnum = PersonagemID.MykeTyroson;
        Enum.TryParse(p.personagem, out personagemEnum);

        // 🔹 RARIDADE
        Raridade raridadeEnum = ConverterRaridade(p.raridade);

        // 🔹 SALVAR
        PlayerPrefs.SetString("personagem_ativo", p.personagem);
        PlayerPrefs.SetString("raridade_ativo", raridadeEnum.ToString());
        PlayerPrefs.SetInt("forca_ativo", p.forca);
        PlayerPrefs.SetInt("velocidade_ativo", p.velocidade);
        PlayerPrefs.SetInt("stamina_ativo", p.stamina);
        PlayerPrefs.Save();

        Debug.Log($"Personagem trocado: {p.personagem} | {raridadeEnum}");

        // 🔹 Atualiza UI de revelação
        if (revelarPersonagemUI != null)
        {
            revelarPersonagemUI.MostrarPersonagem(
                personagemEnum,
                raridadeEnum,
                p.forca,
                p.velocidade,
                p.stamina
            );
        }

        // 🔹 Atualiza menu inicial
        if (MenuInicialUI.Instance != null)
            MenuInicialUI.Instance.CarregarPersonagemAtivo();

        AtualizarDestaque(p.personagem);
    }

    // =========================
    // CONVERSÃO CORRETA (STRING → ENUM)
    // =========================
    Raridade ConverterRaridade(string raridade)
    {
        if (Enum.TryParse(raridade, true, out Raridade r))
            return r;

        Debug.LogWarning("Raridade inválida: " + raridade);
        return Raridade.Comum;
    }

    // =========================
    // DESTAQUE VISUAL
    // =========================
    void AtualizarDestaque(string ativo)
    {
        foreach (GameObject slotObj in slotsAtivos)
        {
            SlotPersonagem slot = slotObj.GetComponent<SlotPersonagem>();
            if (slot == null) continue;

            Image bg = slot.GetComponent<Image>();
            if (bg == null) continue;

            bg.color = (slot.nomeText.text == ativo)
                ? Color.yellow
                : Color.white;
        }
    }

    // =========================
    // DTOs
    // =========================
    [Serializable]
    public class PersonagensResponse
    {
        public string status;
        public List<PersonagemData> personagens;
    }

    [Serializable]
    public class PersonagemData
    {
        public string personagem;
        public string raridade; // 🔥 STRING (ENUM DO BANCO)
        public int forca;
        public int velocidade;
        public int stamina;
    }
}
