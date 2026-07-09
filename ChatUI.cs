using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ChatUI : MonoBehaviour
{
    public TMP_InputField input;
    public Button sendButton;

    public Transform content;
    public TMP_Text linePrefab;

    public int maxLines = 60;

    void Start()
    {
        if (sendButton != null)
            sendButton.onClick.AddListener(Send);

        if (input != null)
            input.ActivateInputField();
    }

    void Update()
    {
        if (input == null) return;

        if (input.isFocused && Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
            Send();
    }

    public void Send()
    {
        if (input == null) return;

        string msg = input.text;
        input.text = "";
        input.ActivateInputField();

        if (string.IsNullOrWhiteSpace(msg)) return;

        Debug.Log("ChatUI.Send -> EnviarChat: " + msg);

        if (NetworkManager.Instance != null)
            NetworkManager.Instance.EnviarChat(msg);
    }

    public void AddLine(string nameOrId, string msg)
    {
        Debug.Log($"ChatUI.AddLine: content={(content!=null)} linePrefab={(linePrefab!=null)}");

        if (content == null || linePrefab == null) return;

        var line = Instantiate(linePrefab, content);
        line.text = $"{nameOrId}: {msg}";

        while (content.childCount > maxLines)
            Destroy(content.GetChild(0).gameObject);
    }
}
