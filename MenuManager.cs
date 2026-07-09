using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public string cenaLobby = "LUTA";

    public void IrParaLobby()
    {
        // 1) troca para a cena Lobby
        SceneManager.LoadScene(cenaLobby);

        // 2) força entrar no lobby no servidor após a cena carregar
        Invoke(nameof(EntrarLobby), 0.3f);
    }

    void EntrarLobby()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.EntrarLobby();
            NetworkManager.Instance.WalletGet();
        }
    }
}
