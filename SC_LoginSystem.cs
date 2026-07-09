using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.SceneManagement;

public class SC_LoginSystem_UI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject registerPanel;

    [Header("Login Fields")]
    public TMP_InputField loginEmail;
    public TMP_InputField loginPassword;

    [Header("Register Fields")]
    public TMP_InputField registerEmail;
    public TMP_InputField registerUsername;
    public TMP_InputField registerPassword1;
    public TMP_InputField registerPassword2;

    [Header("Feedback")]
    public TextMeshProUGUI errorText;

    [Header("Config")]
    [Tooltip("Se ficar vazio, o script escolhe automaticamente (Editor=local, Build=VPS).")]
    public string rootURL = ""; // <-- DEIXA VAZIO

    const string ROOT_LOCAL = "http://localhost/sc_game/";
    const string ROOT_VPS   = "https://app.hustlerlife.store/sc_game/";

    [Tooltip("Timeout em segundos.")]
    public int timeoutSeconds = 12;

    void Awake()
    {
        // Se não setou no Inspector, escolhe automaticamente
        if (string.IsNullOrWhiteSpace(rootURL))
        {
#if UNITY_EDITOR
            rootURL = ROOT_LOCAL;
#else
            rootURL = ROOT_VPS;
#endif
        }

        if (!rootURL.EndsWith("/")) rootURL += "/";
    }

    void Start()
    {
        ShowLogin();
    }

    // ================== UI ==================
    public void ShowLogin()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        errorText.text = "";
    }

    public void ShowRegister()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        errorText.text = "";
    }

    // ================== BUTTONS ==================
    public void Login() => StartCoroutine(LoginEnumerator());
    public void Register() => StartCoroutine(RegisterEnumerator());

    // ================== COROUTINES ==================
    IEnumerator LoginEnumerator()
    {
        if (string.IsNullOrEmpty(loginEmail.text) || string.IsNullOrEmpty(loginPassword.text))
        {
            errorText.text = "Preencha todos os campos!";
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("email", loginEmail.text.Trim());
        form.AddField("password", loginPassword.text);

        using (UnityWebRequest www = UnityWebRequest.Post(rootURL + "login.php", form))
        {
            www.timeout = timeoutSeconds;
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                errorText.text = "Erro de conexão: " + www.error;
                Debug.LogError($"[LOGIN] {www.url} -> {www.error}");
                yield break;
            }

            string response = www.downloadHandler.text.Trim();
            Debug.Log("Resposta Login: " + response);

            if (response.StartsWith("Success"))
            {
                // Success|user_id|username|saldo
                string[] data = response.Split('|');
                if (data.Length >= 4)
                {
                    if (int.TryParse(data[1], out int userId))
                        PlayerPrefs.SetInt("user_id", userId);

                    PlayerPrefs.SetString("username", data[2]);

                    // saldo pode vir 9.90 ou 9,90
                    string saldoStr = data[3].Replace(",", ".");
                    if (float.TryParse(saldoStr, NumberStyles.Any, CultureInfo.InvariantCulture, out float saldo))
                        PlayerPrefs.SetFloat("saldo", saldo);

                    PlayerPrefs.Save();
                }

                SceneManager.LoadScene("MenuInicial");
            }
            else
            {
                errorText.text = response;
            }
        }
    }

    IEnumerator RegisterEnumerator()
    {
        if (string.IsNullOrEmpty(registerEmail.text) ||
            string.IsNullOrEmpty(registerUsername.text) ||
            string.IsNullOrEmpty(registerPassword1.text) ||
            string.IsNullOrEmpty(registerPassword2.text))
        {
            errorText.text = "Preencha todos os campos!";
            yield break;
        }

        if (registerPassword1.text != registerPassword2.text)
        {
            errorText.text = "As senhas não coincidem!";
            yield break;
        }

        WWWForm form = new WWWForm();
        form.AddField("email", registerEmail.text.Trim());
        form.AddField("username", registerUsername.text.Trim());
        form.AddField("password1", registerPassword1.text);
        form.AddField("password2", registerPassword2.text);

        using (UnityWebRequest www = UnityWebRequest.Post(rootURL + "register.php", form))
        {
            www.timeout = timeoutSeconds;
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                errorText.text = "Erro de conexão: " + www.error;
                Debug.LogError($"[REGISTER] {www.url} -> {www.error}");
                yield break;
            }

            string response = www.downloadHandler.text.Trim();
            Debug.Log("Resposta Register: " + response);

            if (response.StartsWith("Success"))
            {
                errorText.text = "Cadastro realizado!";
                ShowLogin();
            }
            else
            {
                errorText.text = response;
            }
        }
    }
}
