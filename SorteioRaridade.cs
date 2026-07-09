using UnityEngine;

public class SorteioRaridade
{
    public static Raridade Sortear()
    {
        int roll = Random.Range(0, 100);

        if (roll < 60) return Raridade.Comum;
        if (roll < 85) return Raridade.Raro;
        if (roll < 95) return Raridade.Epico;
        if (roll < 99) return Raridade.Lendario;
        return Raridade.Mitico;
    }
}
