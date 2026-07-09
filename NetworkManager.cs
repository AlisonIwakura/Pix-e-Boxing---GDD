using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    [Header("Prefab do Player")]
    public GameObject playerPrefab;

    [Header("SPAWN (Lobby/Luta)")]
    public Transform spawnPoint;
    public bool usarSpawnPointLocal = true;
    public bool enviarSpawnPointAoEntrar = true;

    [Header("Chat UI (log)")]
    public ChatUI chatUI;

    [Header("Match / Salas")]
    public string cenaLuta = "LUTA";
    public string cenaLobby = "Lobby";
    public bool autoTrocarCenaAoMatch = true;
    public bool voltarProLobbyAoFinalizarMatch = true;

    private readonly Dictionary<string, GameObject> players = new();
    public string mySocketId;

    public bool inMatch;
    public string currentRoomId;
    public int currentBet;
    public int saldo;

    private string pendingRoomPlayersJson;
    private bool fightSceneReady;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void SocketInit();
    [DllImport("__Internal")] private static extern void SocketEntrarLobby(string json);
    [DllImport("__Internal")] private static extern void SocketMove(string json);
    [DllImport("__Internal")] private static extern void SocketChat(string msg);
    [DllImport("__Internal")] private static extern void SocketJoinQueue(string json);
    [DllImport("__Internal")] private static extern void SocketLeaveQueue();
    [DllImport("__Internal")] private static extern void SocketReportWinner(string json);
    [DllImport("__Internal")] private static extern void SocketWalletGet();
    [DllImport("__Internal")] private static extern void SocketLeaveMatch();
#else
    private static void SocketInit() { }
    private static void SocketEntrarLobby(string json) { }
    private static void SocketMove(string json) { }
    private static void SocketChat(string msg) { }
    private static void SocketJoinQueue(string json) { Debug.Log("SocketJoinQueue(Editor): " + json); }
    private static void SocketLeaveQueue() { }
    private static void SocketReportWinner(string json) { Debug.Log("SocketReportWinner(Editor): " + json); }
    private static void SocketWalletGet() { }
    private static void SocketLeaveMatch() { }
#endif

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SocketInit();
#endif
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        spawnPoint = null;
        chatUI = null;

        try
        {
            var sp = GameObject.FindWithTag("SpawnPoint");
            if (sp != null) spawnPoint = sp.transform;
        }
        catch { spawnPoint = null; }

        chatUI = FindFirstObjectByType<ChatUI>();
        fightSceneReady = (scene.name == cenaLuta);

        if (fightSceneReady && !string.IsNullOrEmpty(pendingRoomPlayersJson))
        {
            ApplyRoomPlayers(pendingRoomPlayersJson);
            pendingRoomPlayersJson = null;
        }

        if (scene.name == cenaLobby)
        {
            ResetMatchState();
            ClearAllPlayers();
            LeaveQueue();
            LeaveMatch();

            if (!string.IsNullOrEmpty(mySocketId))
            {
                Invoke(nameof(EntrarLobby), 0.35f);
                Invoke(nameof(WalletGet), 0.45f);
            }
        }
    }

    private void ResetMatchState()
    {
        inMatch = false;
        currentRoomId = null;
        currentBet = 0;
    }

    // ===================== JS -> UNITY =====================

    public void SetMySocketId(string socketId)
    {
        mySocketId = socketId;
        Debug.Log($"✅ SetMySocketId: {socketId}");
        Invoke(nameof(EntrarLobby), 0.25f);
        Invoke(nameof(WalletGet), 0.35f);
    }

    // ===================== LOBBY & MOVIMENTO =====================

    public void EntrarLobby()
    {
        int userId = PlayerPrefs.GetInt("user_id", 0);
        string nome = PlayerPrefs.GetString("nome", "Player");
        string personagem = PlayerPrefs.GetString("personagem_ativo", PlayerPrefs.GetString("personagem", "MykeTyroson"));
        string raridade = PlayerPrefs.GetString("raridade_ativo", PlayerPrefs.GetString("raridade", "Comum"));

        int forca = PlayerPrefs.GetInt("forca_ativo", PlayerPrefs.GetInt("forca", 10));
        int velocidade = PlayerPrefs.GetInt("velocidade_ativo", PlayerPrefs.GetInt("velocidade", 10));
        int stamina = PlayerPrefs.GetInt("stamina_ativo", PlayerPrefs.GetInt("stamina", 100));

        Vector2 start = (spawnPoint != null) ? (Vector2)spawnPoint.position : Vector2.zero;

        var data = new LobbyJoinData
        {
            userId = userId, nome = nome, personagem = personagem, raridade = raridade,
            forca = forca, velocidade = velocidade, stamina = stamina,
            x = enviarSpawnPointAoEntrar ? start.x : 0f,
            y = enviarSpawnPointAoEntrar ? start.y : 0f,
            flip = false
        };

        Debug.Log($"📤 EntrarLobby → userId={userId} | nome={nome} | personagem={personagem}");
        SocketEntrarLobby(JsonUtility.ToJson(data));
    }

    public void EnviarMovimento(Vector2 pos, bool flip)
    {
        if (string.IsNullOrEmpty(mySocketId)) return;
        var data = new MoveData { x = pos.x, y = pos.y, flip = flip };
        SocketMove(JsonUtility.ToJson(data));
    }

    public void EnviarChat(string msg)
    {
        if (string.IsNullOrWhiteSpace(msg)) return;
        SocketChat(msg.Trim());
    }

    // ===================== MATCH / LUTA =====================

    public void JoinQueue(int bet)
    {
        // ── Diagnóstico: verifica pré-condições ──
        int userId = PlayerPrefs.GetInt("user_id", 0);
        Debug.Log($"🎯 JoinQueue chamado → userId={userId} | bet={bet} | socketId={mySocketId}");

        if (userId <= 0)
        {
            Debug.LogWarning("❌ JoinQueue bloqueado: user_id inválido (0). Verifique se o PlayerPrefs 'user_id' foi salvo corretamente após o login.");
            return;
        }

        if (string.IsNullOrEmpty(mySocketId))
        {
            Debug.LogWarning("❌ JoinQueue bloqueado: mySocketId está vazio. O socket ainda não foi inicializado.");
            return;
        }

        if (bet <= 0)
        {
            Debug.LogWarning("❌ JoinQueue bloqueado: aposta inválida.");
            return;
        }

        // Inclui personagem ativo para o servidor saber com quem o jogador está entrando na fila
        string personagem = PlayerPrefs.GetString("personagem_ativo", PlayerPrefs.GetString("personagem", "MykeTyroson"));
        string raridade = PlayerPrefs.GetString("raridade_ativo", PlayerPrefs.GetString("raridade", "Comum"));
        int forca = PlayerPrefs.GetInt("forca_ativo", PlayerPrefs.GetInt("forca", 10));
        int velocidade = PlayerPrefs.GetInt("velocidade_ativo", PlayerPrefs.GetInt("velocidade", 10));
        int stamina = PlayerPrefs.GetInt("stamina_ativo", PlayerPrefs.GetInt("stamina", 100));

        var data = new JoinQueueData
        {
            userId = userId,
            bet = bet,
            personagem = personagem,
            raridade = raridade,
            forca = forca,
            velocidade = velocidade,
            stamina = stamina
        };

        string json = JsonUtility.ToJson(data);
        Debug.Log($"📤 SocketJoinQueue enviando: {json}");
        SocketJoinQueue(json);
    }

    public void LeaveQueue() => SocketLeaveQueue();
    public void LeaveMatch() => SocketLeaveMatch();

    public void ReportWinner()
    {
        if (string.IsNullOrEmpty(currentRoomId))
        {
            Debug.LogWarning("❌ ReportWinner: currentRoomId está vazio.");
            return;
        }
        var data = new ReportWinnerData { roomId = currentRoomId, winnerSocketId = mySocketId };
        Debug.Log($"🏅 ReportWinner → roomId={currentRoomId} | winner={mySocketId}");
        SocketReportWinner(JsonUtility.ToJson(data));
    }

    public void WalletGet() => SocketWalletGet();

    // ================= CALLBACKS JS =================

    public void OnLobbyPlayers(string json)
    {
        if (SceneManager.GetActiveScene().name == cenaLuta) return;
        var wrapper = JsonUtility.FromJson<PlayerList>("{\"list\":" + json + "}");
        if (wrapper?.list == null) return;

        ClearAllPlayers();
        foreach (var p in wrapper.list) SpawnPlayer(p, p.socketId == mySocketId);
    }

    public void OnNovoPlayer(string json)
    {
        if (SceneManager.GetActiveScene().name == cenaLuta) return;
        var p = JsonUtility.FromJson<PlayerState>(json);
        if (p == null) return;

        // Evita duplicata: se já existe, apenas atualiza posição
        if (players.ContainsKey(p.socketId))
        {
            Debug.Log($"⚠️ OnNovoPlayer: socketId {p.socketId} já existe, ignorando spawn duplicado.");
            return;
        }

        SpawnPlayer(p, p.socketId == mySocketId);
    }

    public void OnPlayerMove(string json)
    {
        var p = JsonUtility.FromJson<PlayerMove>(json);
        if (p == null || p.socketId == mySocketId) return;
        if (players.TryGetValue(p.socketId, out var obj) && obj != null)
            obj.GetComponent<PlayerNetworkSync>()?.SetRemoteState(new Vector2(p.x, p.y), p.flip);
    }

    public void OnPlayerSaiu(string socketId)
    {
        if (players.TryGetValue(socketId, out var obj)) { Destroy(obj); players.Remove(socketId); }
    }

    public void OnMatchFound(string json)
    {
        Debug.Log($"🎮 OnMatchFound recebido: {json}");
        var m = JsonUtility.FromJson<MatchFoundMsg>(json);
        if (m == null)
        {
            Debug.LogWarning("❌ OnMatchFound: falha ao parsear JSON.");
            return;
        }
        currentRoomId = m.roomId;
        currentBet = m.bet;
        inMatch = true;
        ClearAllPlayers();
        Debug.Log($"✅ Partida encontrada → roomId={currentRoomId} | bet={currentBet}");

        // Libera o isSearching no FightQueueUI para permitir nova busca futuramente
        var fightQueueUI = FindFirstObjectByType<FightQueueUI>();
        if (fightQueueUI != null) fightQueueUI.OnMatchFound();
        else Debug.LogWarning("⚠️ FightQueueUI não encontrado na cena.");

        if (autoTrocarCenaAoMatch) SceneManager.LoadScene(cenaLuta);
    }

    public void OnRoomPlayers(string json)
    {
        if (!fightSceneReady) { pendingRoomPlayersJson = json; return; }
        ApplyRoomPlayers(json);
    }

    void ApplyRoomPlayers(string json)
    {
        var wrapper = JsonUtility.FromJson<PlayerList>("{\"list\":" + json + "}");
        if (wrapper?.list == null) return;
        ClearAllPlayers();
        foreach (var p in wrapper.list) SpawnPlayer(p, p.socketId == mySocketId);
    }

    public void OnMatchResult(string json)
    {
        var r = JsonUtility.FromJson<MatchResultMsg>(json);
        if (r == null)
        {
            Debug.LogWarning("❌ OnMatchResult: falha ao parsear JSON.");
            return;
        }
        Debug.Log($"🏆 RESULTADO: Ganhador ID {r.winnerUserId} | Prêmio: {r.prizeAmount}");

        // Atualiza saldo
        WalletGet();

        // Volta ao lobby após resultado (Vida.cs também agenda isso, mas garantimos aqui)
        if (voltarProLobbyAoFinalizarMatch)
            Invoke(nameof(VoltarAoLobbyCena), 3.5f);
    }

    void VoltarAoLobbyCena() => SceneManager.LoadScene(cenaLobby);

    public void OnWallet(string json)
    {
        var w = JsonUtility.FromJson<WalletMsg>(json);
        if (w != null)
        {
            saldo = w.saldo;
            Debug.Log($"💰 Saldo atualizado: {saldo}");
        }
    }

    public void OnChat(string json)
    {
        var msg = JsonUtility.FromJson<ChatMsg>(json);
        if (msg == null) return;
        if (chatUI != null) chatUI.AddLine(msg.nome ?? msg.socketId, msg.text);
        if (players.TryGetValue(msg.socketId, out var obj) && obj != null)
            obj.GetComponentInChildren<ChatBubble>(true)?.Show(msg.text);
    }

    // ================= SPAWN & AUX =================

    void SpawnPlayer(PlayerState p, bool isLocal)
    {
        Vector3 pos = new Vector3(p.x, p.y, 0);
        if (isLocal && usarSpawnPointLocal && spawnPoint != null) pos = spawnPoint.position;

        var obj = Instantiate(playerPrefab, pos, Quaternion.identity);
        players[p.socketId] = obj;

        var sync = obj.GetComponent<PlayerNetworkSync>();
        if (sync != null) { sync.socketId = p.socketId; sync.isLocal = isLocal; }

        var combat = obj.GetComponent<PlayerCombat2D>();
        if (combat != null) combat.enabled = isLocal;

        obj.GetComponent<PlayerSetup>()?.AplicarDados(p.personagem, p.forca, p.velocidade, p.stamina, p.raridade);
    }

    void ClearAllPlayers()
    {
        foreach (var kv in players) if (kv.Value != null) Destroy(kv.Value);
        players.Clear();
    }

    // ================= CLASSES DE DADOS =================

    [Serializable] class LobbyJoinData { public int userId; public string nome, personagem, raridade; public int forca, velocidade, stamina; public float x, y; public bool flip; }
    [Serializable] class MoveData { public float x, y; public bool flip; }
    [Serializable] class JoinQueueData { public int userId; public int bet; public string personagem, raridade; public int forca, velocidade, stamina; }
    [Serializable] class ReportWinnerData { public string roomId, winnerSocketId; }
    [Serializable] class PlayerState { public string socketId; public int userId; public string nome, personagem, raridade; public int forca, velocidade, stamina; public float x, y; public bool flip; }
    [Serializable] class PlayerMove { public string socketId; public float x, y; public bool flip; }
    [Serializable] class PlayerList { public PlayerState[] list; }
    [Serializable] class ChatMsg { public string socketId, text, nome; }
    [Serializable] class MatchFoundMsg { public string roomId; public int bet; }
    [Serializable] class MatchResultMsg { public string roomId; public int prizeAmount, winnerUserId; }
    [Serializable] class WalletMsg { public int saldo; }
    [Serializable] class ServerError { public string msg; }
}