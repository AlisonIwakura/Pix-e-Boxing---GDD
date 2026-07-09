using UnityEngine;
using TMPro;

public class ChatBubble : MonoBehaviour
{
    public GameObject root;      // painel/bubble
    public TMP_Text text;
    public float duration = 3f;

    float t;

    void Awake()
    {
        if (root != null) root.SetActive(false);
    }

    void Update()
    {
        if (root == null || !root.activeSelf) return;

        t -= Time.deltaTime;
        if (t <= 0f) root.SetActive(false);
    }

    public void Show(string msg)
    {
        if (root == null || text == null) return;

        text.text = msg;
        root.SetActive(true);
        t = duration;
    }
}
