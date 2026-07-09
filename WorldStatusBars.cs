using UnityEngine;

public class WorldStatusBars : MonoBehaviour
{
    public BarraFillUI vidaBarra;
    public BarraFillUI staminaBarra;

    private Vida vida;
    private PlayerCombat2D combat;

    void Start()
    {
        vida = GetComponentInParent<Vida>();
        combat = GetComponentInParent<PlayerCombat2D>();
    }

    void Update()
    {
        if (vida != null && vidaBarra != null)
            vidaBarra.Atualizar(vida.vidaAtual, vida.vidaMax);

        if (combat != null && staminaBarra != null)
            staminaBarra.Atualizar(combat.stamina, combat.staminaMax);
    }
}
