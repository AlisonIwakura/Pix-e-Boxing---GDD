using UnityEngine;

public class HitboxAtaque : MonoBehaviour
{
    public int dano = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return; // não acerta a si mesmo

        Vida vida = other.GetComponent<Vida>();
        if (vida != null)
        {
            vida.ReceberDano(dano);
            Debug.Log("Acertou! Dano: " + dano);
        }
    }
}
