using UnityEngine;

public class PlayerHurtbox : MonoBehaviour
{
    private PlayerCombat2D combat;

    void Awake()
    {
        combat = GetComponent<PlayerCombat2D>();
        if (combat == null) combat = GetComponentInParent<PlayerCombat2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (combat == null) return;

        // Não recebe dano se já morto
        if (combat.morto) return;

        if (!other.CompareTag("Hitbox")) return;

        var atacante = other.GetComponentInParent<PlayerCombat2D>();
        if (atacante == null || atacante == combat) return;

        float dano = atacante.danoBase + (atacante.forcaAtributo * 0.5f);
        Debug.Log($"[Dano] {gameObject.name} recebeu {dano} de {atacante.gameObject.name} | HP: {combat.health} -> {combat.health - dano}");
        combat.ReceberDano(dano);
    }
}