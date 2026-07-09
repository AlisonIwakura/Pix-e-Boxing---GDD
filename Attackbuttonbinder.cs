using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Coloque este script no GameObject do botão de ataque na cena de Luta.
/// Ele busca o player local spawnado pela rede e vincula o OnClick automaticamente.
/// </summary>
[RequireComponent(typeof(Button))]
public class AttackButtonBinder : MonoBehaviour
{
    private Button button;
    private PlayerCombat2D playerLocal;

    void Awake()
    {
        button = GetComponent<Button>();
        // Remove qualquer listener antigo do Inspector para evitar duplicata
        button.onClick.RemoveAllListeners();
    }

    void Start()
    {
        // Tenta vincular imediatamente
        TentarVincular();
    }

    void Update()
    {
        // Se ainda não achou o player, continua tentando
        if (playerLocal == null)
            TentarVincular();
    }

    void TentarVincular()
    {
        // Busca todos os PlayerCombat2D na cena e pega o local
        var todos = FindObjectsByType<PlayerCombat2D>(FindObjectsSortMode.None);
        foreach (var p in todos)
        {
            var sync = p.GetComponent<PlayerNetworkSync>();
            if (sync != null && sync.isLocal)
            {
                playerLocal = p;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(playerLocal.Atacar);
                Debug.Log("⚔️ Botão de ataque vinculou ao player local na cena LUTA.");
                return;
            }
        }
    }
}