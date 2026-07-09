using UnityEngine;

public class PenduloIdle : MonoBehaviour
{
    public float anguloMaximo = 25f;   // Quanto balança
    public float velocidade = 4f;      // Velocidade do balanço

    private float tempo;

    void Update()
    {
        tempo += Time.deltaTime * velocidade;
        float angulo = Mathf.Sin(tempo) * anguloMaximo;
        transform.localRotation = Quaternion.Euler(0, 0, angulo);
    }
}
