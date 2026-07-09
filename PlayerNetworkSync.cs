using UnityEngine;

public class PlayerNetworkSync : MonoBehaviour
{
    public string socketId;
    public bool isLocal;

    private Vector2 targetPos;
    private bool targetFlip;

    private float sendTimer;
    private const float SEND_INTERVAL = 1f / 20f; // 20x por segundo

    // Referência ao visual para ler/escrever o flip corretamente
    private Transform visualTransform;
    private PlayerCombat2D combat;

    void Awake()
    {
        combat = GetComponent<PlayerCombat2D>();

        // Usa o mesmo visualTransform do PlayerCombat2D
        if (combat != null && combat.visualTransform != null)
            visualTransform = combat.visualTransform;
        else if (transform.childCount > 0)
            visualTransform = transform.GetChild(0);
        else
            visualTransform = transform;
    }

    void Start()
    {
        targetPos = transform.position;
        targetFlip = visualTransform.localScale.x >= 0; // estado inicial
    }

    void Update()
    {
        if (isLocal)
        {
            sendTimer += Time.deltaTime;
            if (sendTimer >= SEND_INTERVAL)
            {
                sendTimer = 0f;

                // Lê o flip do visualTransform (onde PlayerCombat2D aplica)
                bool flipAtual = visualTransform.localScale.x >= 0;

                NetworkManager.Instance?.EnviarMovimento(
                    transform.position,
                    flipAtual
                );
            }
            return;
        }

        // Jogador remoto: interpola posição
        transform.position = Vector2.Lerp(transform.position, targetPos, Time.deltaTime * 12f);

        // Aplica flip no visualTransform (igual ao local)
        if (combat != null)
        {
            combat.AplicarFlipVisual(targetFlip);
        }
        else
        {
            // Fallback se não tiver PlayerCombat2D
            var s = visualTransform.localScale;
            s.x = targetFlip ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
            visualTransform.localScale = s;
        }
    }

    public void SetRemoteState(Vector2 pos, bool flip)
    {
        targetPos = pos;
        targetFlip = flip;
    }
}