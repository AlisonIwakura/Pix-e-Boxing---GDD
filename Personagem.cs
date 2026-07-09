using UnityEngine;

[System.Serializable]
public class Personagem
{
    public string nome;
    public Sprite imagem;
    public Raridade raridade;

    // Atributos de combate
    public int forca = 10;
    public int velocidade = 5;
    public int stamina = 100;
}
