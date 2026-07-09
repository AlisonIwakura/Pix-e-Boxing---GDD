using UnityEngine;
using System.Collections;

public class RevelarCard : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform areaCards;
    public BancoPersonagens banco;

    public float delayAntesFlip = 1.2f;

    public void Revelar()
    {
        StartCoroutine(RevelarComDelay());
    }

    IEnumerator RevelarComDelay()
    {
        // Remove card anterior
        foreach (Transform child in areaCards)
            Destroy(child.gameObject);

        // Cria novo card
        GameObject cardObj = Instantiate(cardPrefab, areaCards);
        RectTransform rt = cardObj.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;

        // Delay antes da revelação
        yield return new WaitForSeconds(delayAntesFlip);

        // Sorteia personagem completo
        Personagem personagem = banco.SortearPersonagem();

        if (personagem != null)
        {
            cardObj.GetComponent<CardUI>().Revelar(personagem);
        }
    }
}
