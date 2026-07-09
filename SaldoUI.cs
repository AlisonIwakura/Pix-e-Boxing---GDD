using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class SaldoUI : MonoBehaviour
{
    [Header("UI")]
    public Text textoSaldo; // arraste seu Text UI aqui

    [Header("Servidor")]
    public string urlSaldo = "http://localhost/sc_game/get_saldo.php";

    [Header("Configuração")]
    public float intervaloAtualizacao = 3f; // segundos

    void Start()
    {
        // Checa se o Text está atribuído
        if (textoSaldo == null)
        {
            Debug.LogError("TextoSaldo não foi atribuído no Inspector!");
            return;
        }

        // Sempre mostra algo no início
        textoSaldo.text = "💰 0";

        // Se usuário está logado, inicia atualização
        int userId = PlayerPrefs.GetInt("user_id", 0);
        if (userId > 0)
        {
            // Atualiza imediatamente
            StartCoroutine(AtualizarSaldo());
            // E inicia loop contínuo
            StartCoroutine(AtualizarSaldoLoop());
        }
    }

    IEnumerator AtualizarSaldoLoop()
    {
        while (true)
        {
            yield return AtualizarSaldo();
            yield return new WaitForSeconds(intervaloAtualizacao);
        }
    }

    IEnumerator AtualizarSaldo()
    {
        int userId = PlayerPrefs.GetInt("user_id", 0);

        if (userId <= 0)
        {
            textoSaldo.text = "💰 0"; // garante valor padrão
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("usuario_id", userId);

        using (UnityWebRequest www = UnityWebRequest.Post(urlSaldo, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Erro ao buscar saldo: " + www.error);
                yield break;
            }

            string resposta = www.downloadHandler.text.Trim();

            if (!resposta.StartsWith("Success"))
            {
                Debug.LogError("Retorno inválido do servidor: " + resposta);
                textoSaldo.text = "💰 0";
                yield break;
            }

            // Ex: Success|150.50
            string[] dados = resposta.Split('|');
            if (dados.Length >= 2)
            {
                textoSaldo.text = $"💰 {dados[1]}";
            }
            else
            {
                Debug.LogError("Formato de resposta inválido: " + resposta);
                textoSaldo.text = "💰 0";
            }
        }
    }

    // 🔄 Atualização manual (botão "Atualizar Agora")
    public void AtualizarAgora()
    {
        if (textoSaldo != null)
            StartCoroutine(AtualizarSaldo());
    }
}
