using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PersonagemManager : MonoBehaviour
{
    [Header("Configuração do PHP")]
    public string urlPHP = "https://seusite.com/listar_personagens.php";
    public int usuarioId = 1;

    [Header("Referência ao UI Manager")]
    public PersonagemUIManager uiManager;

    [Header("Lista de Personagens")]
    public List<Personagemm> personagens = new List<Personagemm>();

    void Start()
    {
        if (uiManager == null)
        {
            Debug.LogError("UI Manager não atribuído!");
            return;
        }
        StartCoroutine(BuscarPersonagens(usuarioId));
    }

    IEnumerator BuscarPersonagens(int userId)
    {
        WWWForm form = new WWWForm();
        form.AddField("usuario_id", userId);

        using (UnityWebRequest www = UnityWebRequest.Post(urlPHP, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Erro ao buscar personagens: " + www.error);
                yield break;
            }

            string json = www.downloadHandler.text;
            Debug.Log("JSON recebido: " + json);

            PersonagensResponse response = JsonUtility.FromJson<PersonagensResponse>(json);

            if (response.status == "success")
            {
                personagens.Clear();
                personagens.AddRange(response.personagens);
                uiManager.personagens = personagens;
                uiManager.PopularLista();
            }
            else if (response.status == "vazio")
            {
                Debug.Log("Nenhum personagem encontrado.");
            }
            else
            {
                Debug.LogError("Erro inesperado: " + json);
            }
        }
    }
}
