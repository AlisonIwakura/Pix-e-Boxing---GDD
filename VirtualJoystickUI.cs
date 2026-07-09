using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystickUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("UI")]
    public RectTransform background; // JoystickBG
    public RectTransform handle;     // JoystickHandle

    [Header("Config")]
    [Tooltip("Se 0, usa metade do size do background.")]
    public float radiusOverride = 0f;

    [Tooltip("Deadzone (0..1). Ex: 0.08 evita tremedeira.")]
    [Range(0f, 0.5f)] public float deadZone = 0.08f;

    [Header("Target (auto)")]
    [Tooltip("Se vazio, ele tenta achar automaticamente o Player local (habilitado).")]
    public PlayerCombat2D player;

    public Vector2 InputVector { get; private set; } // (-1..+1)

    float radius;

    void Start()
    {
        if (background == null) background = GetComponent<RectTransform>();
        radius = radiusOverride > 0f ? radiusOverride : background.sizeDelta.x * 0.5f;

        if (handle != null) handle.anchoredPosition = Vector2.zero;
        InputVector = Vector2.zero;

        TryBindLocalPlayer();
    }

    void OnEnable()
    {
        // Se o player spawnar depois (ou trocar de cena), tenta rebinder
        TryBindLocalPlayer();
        CancelInvoke(nameof(TryBindLocalPlayer));
        InvokeRepeating(nameof(TryBindLocalPlayer), 0.2f, 0.5f);
    }

    void OnDisable()
    {
        CancelInvoke(nameof(TryBindLocalPlayer));
    }

    void TryBindLocalPlayer()
    {
        // Se já temos e ainda está válido, para
        if (player != null && player.isActiveAndEnabled) return;

        // 1) Melhor: acha PlayerNetworkSync isLocal=true
        var syncs = Object.FindObjectsByType<PlayerNetworkSync>(FindObjectsSortMode.None);
        foreach (var s in syncs)
        {
            if (s != null && s.isLocal)
            {
                var pc = s.GetComponent<PlayerCombat2D>();
                if (pc != null)
                {
                    player = pc;
                    Debug.Log("🎮 Joystick bindou no player local via PlayerNetworkSync.");
                    return;
                }
            }
        }

        // 2) Fallback: pega qualquer PlayerCombat2D habilitado (local geralmente é o único enabled)
        var combats = Object.FindObjectsByType<PlayerCombat2D>(FindObjectsSortMode.None);
        foreach (var c in combats)
        {
            if (c != null && c.isActiveAndEnabled)
            {
                player = c;
                Debug.Log("🎮 Joystick bindou no player local via PlayerCombat2D enabled.");
                return;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

    public void OnDrag(PointerEventData eventData)
    {
        if (background == null || handle == null) return;

        // garante que sempre temos o player certo
        if (player == null || !player.isActiveAndEnabled) TryBindLocalPlayer();

        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint))
            return;

        Vector2 v = localPoint / radius;

        if (v.magnitude > 1f) v = v.normalized;

        if (v.magnitude < deadZone) v = Vector2.zero;

        InputVector = v;
        handle.anchoredPosition = InputVector * radius;

        if (player != null)
        {
            if (InputVector == Vector2.zero) player.PararMover();
            else player.Mover(InputVector);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        InputVector = Vector2.zero;

        if (handle != null)
            handle.anchoredPosition = Vector2.zero;

        if (player != null)
            player.PararMover();
    }
}