using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using System.Collections.Generic;

public class PlayFabLogin : MonoBehaviour
{
    #region Singleton

    public static PlayFabLogin Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    #endregion

    #region Public References

    public CarregamentoEConexao loadManager;
    public GameObject loginPanel;
    public TMP_Text statusText;

    [Header("Login UI")]
    public TMP_InputField inputUserEmailLogin;
    public TMP_InputField inputUserPasswordLogin;

    [Header("Register UI")]
    public TMP_InputField inputUsername;
    public TMP_InputField inputEmail;
    public TMP_InputField inputPassword;

    #endregion

    #region Internal State

    public static string PlayFabID;
    public string EntityID;
    public string EntityType;
    public string Nickname;

    private string _userEmailOrUsername;
    private string _userPassword;
    private string _username;

    #endregion

    #region Public Methods

    public void Login()
    {
        if (string.IsNullOrEmpty(inputUserEmailLogin.text) || string.IsNullOrEmpty(inputUserPasswordLogin.text))
        {
            SetStatus("Preencha os dados corretamente!");
            return;
        }

        _userEmailOrUsername = inputUserEmailLogin.text;
        _userPassword = inputUserPasswordLogin.text;

        if (_userEmailOrUsername.Contains("@"))
        {
            var request = new LoginWithEmailAddressRequest
            {
                Email = _userEmailOrUsername,
                Password = _userPassword
            };

            PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginError);
        }
        else
        {
            var request = new LoginWithPlayFabRequest
            {
                Username = _userEmailOrUsername,
                Password = _userPassword
            };

            PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnLoginError);
        }
    }

    public void Register()
    {
        if (string.IsNullOrEmpty(inputUsername.text) || string.IsNullOrEmpty(inputEmail.text) || string.IsNullOrEmpty(inputPassword.text))
        {
            SetStatus("Preencha os dados corretamente!");
            return;
        }

        _username = inputUsername.text;
        _userEmailOrUsername = inputEmail.text;
        _userPassword = inputPassword.text;

        var request = new RegisterPlayFabUserRequest
        {
            Email = _userEmailOrUsername,
            Password = _userPassword,
            Username = _username
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnRegisterError);
    }

    public void SavePlayerData(string key, string value)
    {
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { key, value } }
        },
        result => Debug.Log("Dados do jogador atualizados com sucesso."),
        error => Debug.LogError(error.GenerateErrorReport()));
    }

    public void LoadPlayerData(string key)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest
        {
            PlayFabId = PlayFabID
        },
        result =>
        {
            if (result.Data != null && result.Data.ContainsKey(key))
            {
                PlayerPrefs.SetString(key, result.Data[key].Value);
            }
            else
            {
                Debug.Log("Dado não encontrado.");
            }
        },
        error => Debug.LogError(error.GenerateErrorReport()));
    }

    #endregion

    #region Login Callbacks

    private void OnLoginSuccess(LoginResult result)
    {
        PlayFabID = result.PlayFabId;
        SetStatus("Login realizado com sucesso!");
        loginPanel.SetActive(false);

        if (result.EntityToken?.Entity != null)
        {
            EntityID = result.EntityToken.Entity.Id;
            EntityType = result.EntityToken.Entity.Type;
        }

        UpdateDisplayName(_userEmailOrUsername);
        GetDisplayName(PlayFabID);
        loadManager.Connect();
    }

    private void OnLoginError(PlayFabError error)
    {
        Debug.LogError("Erro ao fazer login: " + error.GenerateErrorReport());

        switch (error.Error)
        {
            case PlayFabErrorCode.AccountNotFound:
                SetStatus("Conta não encontrada.");
                break;
            case PlayFabErrorCode.InvalidEmailOrPassword:
                SetStatus("E-mail ou senha inválidos.");
                break;
            default:
                SetStatus("Erro ao efetuar login. Verifique os dados.");
                break;
        }
    }

    #endregion

    #region Register Callbacks

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        SetStatus("Conta criada com sucesso!");
    }

    private void OnRegisterError(PlayFabError error)
    {
        Debug.LogError("Erro ao registrar: " + error.GenerateErrorReport());

        switch (error.Error)
        {
            case PlayFabErrorCode.InvalidEmailAddress:
                SetStatus("Já existe uma conta com este e-mail.");
                break;
            case PlayFabErrorCode.InvalidUsername:
                SetStatus("Nome de usuário já está em uso.");
                break;
            case PlayFabErrorCode.InvalidParams:
                SetStatus("Dados inválidos. Verifique e tente novamente.");
                break;
            default:
                SetStatus("Erro ao criar conta.");
                break;
        }
    }

    #endregion

    #region Display Name

    private void UpdateDisplayName(string displayName)
    {
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = displayName
        },
        result =>
        {
            Nickname = result.DisplayName;
            Debug.Log("Display name atualizado.");
        },
        error => Debug.LogWarning("Falha ao atualizar Display Name: " + error.GenerateErrorReport()));
    }

    private void GetDisplayName(string playFabId)
    {
        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest
        {
            PlayFabId = playFabId,
            ProfileConstraints = new PlayerProfileViewConstraints
            {
                ShowDisplayName = true
            }
        },
        result => { Nickname = result.PlayerProfile.DisplayName; },
        error => Debug.LogWarning("Erro ao obter display name: " + error.ErrorMessage));
    }

    #endregion

    #region Utilities

    private void SetStatus(string msg)
    {
        statusText.text = msg;
        Debug.Log(msg);
    }

    #endregion
}
