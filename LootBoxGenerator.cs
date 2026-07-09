using UnityEngine;

public static class LootBoxGenerator
{
    public static void GerarPersonagem()
    {
        // =========================
        // PERSONAGEM ALEATÓRIO
        // =========================
        PersonagemID personagem =
            (PersonagemID)Random.Range(
                0,
                System.Enum.GetValues(typeof(PersonagemID)).Length
            );

        // =========================
        // RARIDADE (CHANCE)
        // =========================
        Raridade raridade = SortearRaridade();

        // =========================
        // GERAR ATRIBUTOS
        // =========================
        int forca = RandomRangePorRaridade(raridade, 8, 15);
        int velocidade = RandomRangePorRaridade(raridade, 8, 15);
        int staminaMax = RandomRangePorRaridade(raridade, 80, 160);

        // =========================
        // SALVAR
        // =========================
        PlayerPrefs.SetString("personagem", personagem.ToString());
        PlayerPrefs.SetString("raridade", raridade.ToString());

        PlayerPrefs.SetInt("forca", forca);
        PlayerPrefs.SetInt("velocidadeAtributo", velocidade);
        PlayerPrefs.SetInt("staminaMax", staminaMax);

        PlayerPrefs.Save();

        Debug.Log($"🎁 LOOT: {personagem} | {raridade} | F:{forca} V:{velocidade} S:{staminaMax}");
    }

    // =========================
    // RARIDADE
    // =========================
    static Raridade SortearRaridade()
    {
        float roll = Random.Range(0f, 100f);

        if (roll < 1f)   return Raridade.Mitico;      // 1%
        if (roll < 4f)   return Raridade.Lendario;   // 3%
        if (roll < 12f)  return Raridade.Epico;      // 8%
        if (roll < 35f)  return Raridade.Raro;       // 23%
        return Raridade.Comum;                        // resto
    }

    // =========================
    // ATRIBUTOS POR RARIDADE
    // =========================
    static int RandomRangePorRaridade(Raridade raridade, int minBase, int maxBase)
    {
        float multiplicador = 1f;

        switch (raridade)
        {
            case Raridade.Raro:     multiplicador = 1.15f; break;
            case Raridade.Epico:    multiplicador = 1.35f; break;
            case Raridade.Lendario: multiplicador = 1.6f;  break;
            case Raridade.Mitico:   multiplicador = 2.0f;  break;
        }

        int min = Mathf.RoundToInt(minBase * multiplicador);
        int max = Mathf.RoundToInt(maxBase * multiplicador);

        return Random.Range(min, max + 1);
    }
}
