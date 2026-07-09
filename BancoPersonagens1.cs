using UnityEngine;
using System.Collections.Generic;

public class BancoPersonagens : MonoBehaviour
{
    public List<Personagem> personagens;

    public Personagem SortearPersonagem()
    {
        List<Personagem> pool = new List<Personagem>();

        foreach (Personagem p in personagens)
        {
            int peso = PesoPorRaridade(p.raridade);

            for (int i = 0; i < peso; i++)
                pool.Add(p);
        }

        if (pool.Count == 0)
        {
            Debug.LogError("Banco de personagens vazio!");
            return null;
        }

        return pool[Random.Range(0, pool.Count)];
    }

    int PesoPorRaridade(Raridade r)
    {
        switch (r)
        {
            case Raridade.Comum: return 60;
            case Raridade.Raro: return 25;
            case Raridade.Epico: return 10;
            case Raridade.Lendario: return 4;
            case Raridade.Mitico: return 1;
            default: return 1;
        }
    }
}
