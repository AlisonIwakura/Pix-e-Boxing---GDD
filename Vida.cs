using UnityEngine;

public class Vida : MonoBehaviour
{
    [Header("Status do Lutador")]
    public int vidaMax = 100;
    public int vidaAtual;
    public float staminaMax = 100f;
    public float staminaAtual;

    [Header("Referências de Visual (SpriteRenderer)")]
    [Tooltip("Arraste o objeto 'Life' aqui")]
    public SpriteRenderer barraHPSprite;

    [Tooltip("Arraste o objeto 'Stamina' aqui")]
    public SpriteRenderer barraStaminaSprite;

    private float larguraOriginalHP;
    private float larguraOriginalStamina;

    private PlayerNetworkSync netSync;
    private PlayerCombat2D combate;
    private Rigidbody2D rb;
    public bool morto = false;

    void Awake()
    {
        // Busca nos pais E no próprio objeto
        netSync = GetComponent<PlayerNetworkSync>() ?? GetComponentInParent<PlayerNetworkSync>();
        combate = GetComponent<PlayerCombat2D>() ?? GetComponentInParent<PlayerCombat2D>();
        rb = GetComponent<Rigidbody2D>() ?? GetComponentInParent<Rigidbody2D>();

        vidaAtual = vidaMax;
        staminaAtual = staminaMax;

        if (barraHPSprite != null) larguraOriginalHP = barraHPSprite.size.x;
        if (barraStaminaSprite != null) larguraOriginalStamina = barraStaminaSprite.size.x;

        AtualizarInterface();
    }

    void Update()
    {
        // Sincroniza stamina com o PlayerCombat2D para a barra refletir corretamente
        if (combate != null)
        {
            staminaAtual = combate.stamina;
            staminaMax = combate.staminaMax;

            // Sincroniza vida também
            vidaAtual = Mathf.RoundToInt(combate.health);
            vidaMax = Mathf.RoundToInt(combate.healthMax);

            AtualizarInterface();
        }
    }

    // Chamado externamente se necessário (ex: sistema legado)
    public void ReceberDano(int dano)
    {
        if (morto || dano <= 0) return;

        // Delega ao PlayerCombat2D que é a fonte de verdade
        if (combate != null)
        {
            combate.ReceberDano(dano);
            return;
        }

        // Fallback se não tiver PlayerCombat2D
        vidaAtual = Mathf.Max(0, vidaAtual - dano);
        AtualizarInterface();
        if (vidaAtual <= 0) Morrer();
    }

    public bool ConsumirStamina(float quantidade)
    {
        if (combate != null)
        {
            if (combate.stamina >= quantidade)
            {
                combate.stamina -= quantidade;
                return true;
            }
            return false;
        }

        if (staminaAtual >= quantidade)
        {
            staminaAtual -= quantidade;
            AtualizarInterface();
            return true;
        }
        return false;
    }

    public void AtualizarInterface()
    {
        if (barraHPSprite != null)
        {
            float pct = vidaMax > 0 ? (float)vidaAtual / vidaMax : 0f;
            pct = Mathf.Clamp01(pct);
            barraHPSprite.size = new Vector2(larguraOriginalHP * pct, barraHPSprite.size.y);
        }

        if (barraStaminaSprite != null)
        {
            float pct = staminaMax > 0 ? staminaAtual / staminaMax : 0f;
            pct = Mathf.Clamp01(pct);
            barraStaminaSprite.size = new Vector2(larguraOriginalStamina * pct, barraStaminaSprite.size.y);
        }
    }

    // Chamado pelo PlayerCombat2D quando detecta morte
    public void NotificarMorte()
    {
        if (morto) return;
        morto = true;

        Debug.Log($"💀 Vida.cs: Lutador nocauteado! isLocal={netSync?.isLocal}");

        vidaAtual = 0;
        AtualizarInterface();

        if (combate != null) combate.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        bool isLocalPlayer = netSync != null && netSync.isLocal;
        Debug.Log($"💀 NotificarMorte → isLocal={isLocalPlayer}");

        if (isLocalPlayer)
        {
            // Eu morri — reporto minha derrota (servidor credita o outro)
            // O ReportWinner envia mySocketId como vencedor — mas aqui queremos
            // reportar o OUTRO como vencedor. O servidor faz isso pelo disconnect.
            // Então apenas voltamos ao lobby.
            Debug.Log("💀 Eu morri. Aguardando resultado do servidor...");
            // Não chama ReportWinner — o servidor detecta por disconnect ou pelo outro lado
        }
        else
        {
            // Player remoto morreu na minha tela — eu sou o vencedor
            Debug.Log("🏆 Adversário morreu! Reportando vitória...");
            if (NetworkManager.Instance != null)
                NetworkManager.Instance.ReportWinner();
        }

        // Ambos voltam ao lobby após 3s (o NetworkManager também faz isso via OnMatchResult)
        Invoke(nameof(VoltarAoLobby), 3f);
    }

    void Morrer()
    {
        NotificarMorte();
    }

    void VoltarAoLobby() => UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
}