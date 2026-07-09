// Arquivo: HitboxAtaqueNovo.cs
using UnityEngine;

public class HitboxAtaqueNovo : MonoBehaviour
{
    public int dano = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Vida vida = other.GetComponent<Vida>();
        if (vida != null)
        {
            vida.ReceberDano(dano);
            Debug.Log("Acertou! Dano: " + dano);
        }
    }
}
