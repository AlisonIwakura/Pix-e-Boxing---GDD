using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerCombat2D : MonoBehaviour
{
    [Header("Vida")]
    public float health = 100f;
    public float healthMax = 100f;
    public bool morto = false;

    [Header("Movimento")]
    public float velocidade = 5f;
    public float custoStaminaMovimento = 6f;
    public float staminaMinParaMover = 5f;

    [Header("Combate")]
    public GameObject hitbox;
    public float duracaoAtaque = 0.2f;
    public float custoAtaque = 8f;
    public int danoBase = 10;

    [Header("Atributos Base")]
    public float stamina = 100f;
    public float staminaMax = 100f;
    public float StaminaAtual => stamina;
    public float StaminaMax => staminaMax;

    [Header("Atributos Avançados")]
    [Range(1, 100)] public int velocidadeAtributo = 10;
    [Range(1, 100)] public int forcaAtributo = 10;
    public Raridade raridade = Raridade.Comum;

    [Header("Regeneração")]
    public float regenParado = 80f;
    public float regenAndando = 25f;
    public float atrasoRegen = 0.25f;

    [Header("Direção e Visual")]
    public Transform visualTransform;
    [Tooltip("Marque se o sprite original já está virado para a esquerda")]
    public bool spriteOriginarioEsquerda = false;

    [Header("Input")]
    public bool usarInputDoTeclado = true;
    public float prioridadeMobilePor = 0.08f;

    private Rigidbody2D rb;
    private bool atacando;
    private Vector2 direcaoMovimento;
    private Vector2 direcaoMobile;
    private float mobilePriorityTimer;
    private Vector3 escalaInicialVisual;
    private float tempoDesdeUltimoGasto;
    private PlayerNetworkSync net;

    // Guarda o último flip para não resetar quando parar
    private bool ultimoFlipDireita = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        net = GetComponent<PlayerNetworkSync>();

        if (visualTransform == null && transform.childCount > 0)
            visualTransform = transform.GetChild(0);

        // Sempre salva a escala com x positivo como referência
        // Independente de como o sprite está no prefab
        Vector3 escalaRef = visualTransform != null ? visualTransform.localScale : transform.localScale;
        escalaRef.x = Mathf.Abs(escalaRef.x);
        escalaInicialVisual = escalaRef;

        if (hitbox != null)
            hitbox.SetActive(false);

        staminaMax = Mathf.Max(1f, staminaMax);
        stamina = Mathf.Clamp(stamina, 0f, staminaMax);
        health = healthMax;

        // Aplica flip inicial correto (direita por padrão)
        AplicarFlipVisual(ultimoFlipDireita);
    }

    void Update()
    {
        if (morto) return;

        CapturarInputPC();
        ResolverFonteDeMovimento();
        ProcessarStamina();
        AplicarFlip();
    }

    void FixedUpdate()
    {
        if (morto) return;
        ProcessarMovimentoFisico();
    }

    // ================= DANO =================

    public void ReceberDano(float dano)
    {
        if (morto) return;

        health -= dano;
        Debug.Log($"{gameObject.name} HP: {health}");

        if (health <= 0)
        {
            health = 0;
            Morrer();
        }
    }

    void Morrer()
    {
        if (morto) return;
        morto = true;

        Debug.Log("💀 Morte detectada!");

        if (hitbox != null) hitbox.SetActive(false);
        rb.linearVelocity = Vector2.zero;

        // Notifica o Vida.cs para atualizar a barra e gerenciar o fluxo de morte
        var vida = GetComponentInChildren<Vida>();
        if (vida != null)
        {
            vida.NotificarMorte();
            return; // Vida.cs cuida do ReportWinner e VoltarAoLobby
        }

        // Fallback se não tiver Vida.cs
        if (net != null && net.isLocal)
        {
            if (NetworkManager.Instance != null)
                NetworkManager.Instance.ReportWinner();

            Invoke(nameof(VoltarParaMenu), 3f);
        }
    }

    void VoltarParaMenu()
    {
        SceneManager.LoadScene("Lobby");
    }

    // Detecção de dano movida para PlayerHurtbox.cs
    // que funciona mesmo com PlayerCombat2D desabilitado (player remoto)

    // ================= MOVIMENTO =================

    void ProcessarMovimentoFisico()
    {
        if (rb == null || stamina < staminaMinParaMover) return;

        float fatorStamina = Mathf.Clamp(stamina / staminaMax, 0.5f, 1f);
        Vector2 delta = direcaoMovimento * (velocidade * fatorStamina * Time.fixedDeltaTime);
        rb.MovePosition(rb.position + delta);
    }

    void ProcessarStamina()
    {
        if (direcaoMovimento != Vector2.zero && stamina >= staminaMinParaMover)
        {
            float custoFinal = custoStaminaMovimento * (1f + velocidadeAtributo * 0.015f);
            stamina -= custoFinal * Time.deltaTime;
            tempoDesdeUltimoGasto = 0f;
        }
        else
        {
            tempoDesdeUltimoGasto += Time.deltaTime;
        }

        if (tempoDesdeUltimoGasto >= atrasoRegen)
            RegenerarStamina();

        stamina = Mathf.Clamp(stamina, 0f, staminaMax);
    }

    void RegenerarStamina()
    {
        float regenBase = direcaoMovimento == Vector2.zero ? regenParado : regenAndando;
        stamina += regenBase * GetMultiplicadorRaridade() * Time.deltaTime;
    }

    float GetMultiplicadorRaridade()
    {
        switch (raridade)
        {
            case Raridade.Raro: return 1.15f;
            case Raridade.Epico: return 1.35f;
            case Raridade.Lendario: return 1.6f;
            case Raridade.Mitico: return 2f;
            default: return 1f;
        }
    }

    // ================= ATAQUE =================

    public void Atacar()
    {
        if (atacando || morto || stamina < custoAtaque) return;

        atacando = true;
        stamina -= custoAtaque;
        tempoDesdeUltimoGasto = 0f;

        if (hitbox != null) hitbox.SetActive(true);
        Invoke(nameof(FinalizarAtaque), duracaoAtaque);
    }

    void FinalizarAtaque()
    {
        if (hitbox != null) hitbox.SetActive(false);
        atacando = false;
    }

    // ================= FLIP =================

    void AplicarFlip()
    {
        if (direcaoMovimento.x > 0f)
            ultimoFlipDireita = true;
        else if (direcaoMovimento.x < 0f)
            ultimoFlipDireita = false;

        AplicarFlipVisual(ultimoFlipDireita);
    }

    // Público para que PlayerNetworkSync possa aplicar no jogador remoto
    public void AplicarFlipVisual(bool paraDireita)
    {
        Transform alvo = visualTransform != null ? visualTransform : transform;
        Vector3 escala = alvo.localScale;
        float absX = Mathf.Abs(escala.x);

        // Se o sprite padrão olha para a ESQUERDA:
        // paraDireita=true  → escala.x negativa (espelha o sprite)
        // paraDireita=false → escala.x positiva (sprite no estado original)
        escala.x = paraDireita ? -absX : absX;
        alvo.localScale = escala;
    }

    // ================= INPUT =================

    void CapturarInputPC()
    {
        if (!usarInputDoTeclado || Keyboard.current == null) return;

        float x = 0f, y = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x = 1f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y = 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y = -1f;

        if (mobilePriorityTimer <= 0f)
            direcaoMovimento = new Vector2(x, y).normalized;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            Atacar();
    }

    void ResolverFonteDeMovimento()
    {
        if (mobilePriorityTimer > 0f)
        {
            mobilePriorityTimer -= Time.deltaTime;
            direcaoMovimento = direcaoMobile;
        }
    }

    public void Mover(Vector2 direcao)
    {
        direcaoMobile = direcao.normalized;
        if (direcaoMobile != Vector2.zero)
            mobilePriorityTimer = prioridadeMobilePor;
    }

    public void PararMover()
    {
        direcaoMobile = Vector2.zero;
        mobilePriorityTimer = 0f;
        direcaoMovimento = Vector2.zero;
    }

    public void OnMove(InputValue value) => Mover(value.Get<Vector2>());
    public void OnAttack() => Atacar();
}