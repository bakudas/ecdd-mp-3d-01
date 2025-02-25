using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.DataModels;
using PlayFab.ProfilesModels;
using PlayFab.ClientModels;
using TMPro;
using Unity.VisualScripting;
using ExitGames.Client.Photon;

public class PlayFabLogin : MonoBehaviour
{

    public static string PlayFabID;
    public string Nickname;

    public TMP_Text statusText;

    public string userEmail;
    public string userPassword;
    public string username;

    // campos utilizados para efetuar o login do jogador
    public TMP_InputField inputUserEmailLogin;
    public TMP_InputField inputUserPasswordLogin;

    // campos utilizados para criar uma nova conta para o jogador
    public TMP_InputField inputUsername;
    public TMP_InputField inputEmail;
    public TMP_InputField inputPassword;

    public GameObject loginPanel;

    public CarregamentoEConexao loadManager;

    public static PlayFabLogin PFL;

    private void Awake()
    {
        if (PFL  != null && PFL != this)
        {
            Destroy(PFL);
        }
        PFL = this;
        DontDestroyOnLoad(this.gameObject);
    }

    #region Login

    public void Login()
    {
        if (string.IsNullOrEmpty(inputUserEmailLogin.text) || string.IsNullOrEmpty(inputUserPasswordLogin.text))
        {
            Debug.Log("Preencha os dados corretamente!");
            statusText.text = "Preencha os dados corretamente!";
        }
        else
        {
            // credenciais para autentica��o
            userEmail = inputUserEmailLogin.text;
            userPassword = inputUserPasswordLogin.text;
            //payload de requisi��o
            var request = new LoginWithEmailAddressRequest { Email = userEmail, Password = userPassword };
            // Requisi��o
            PlayFabClientAPI.LoginWithEmailAddress(request, SucessoLogin, FalhaLogin);
        }
    }

    public void CriarConta()
    {
        if (string.IsNullOrEmpty(inputUsername.text) || string.IsNullOrEmpty(inputEmail.text) || string.IsNullOrEmpty(inputPassword.text))
        {
            Debug.Log("Preencha os dados corretamente!");
            statusText.text = "Preencha os dados corretamente!";
        }
        else
        {
            username = inputUsername.text;
            userEmail = inputEmail.text;
            userPassword = inputPassword.text;

            // payload da requisi��o
            var request = new RegisterPlayFabUserRequest { Email = userEmail, Password = userPassword, Username = username };
            // Requisi��o
            PlayFabClientAPI.RegisterPlayFabUser(request, SucessoCriarConta, FalhaCriarConta);
        }
    }

    public void SucessoLogin(LoginResult resulto)
    {
        Debug.Log("Login foi feito com sucesso!");
        statusText.text = "Login foi feito com sucesso!";
        loginPanel.SetActive(false);
        loadManager.Connect();
    }

    public void FalhaLogin(PlayFabError error)
    {
        Debug.Log("N�o foi poss�vel fazer login!");
        statusText.text = "N�o foi poss�vel fazer login!";

        switch (error.Error)
        {
            case PlayFabErrorCode.AccountNotFound:
                statusText.text = "N�o foi poss�vel efetuar o login!\nConta n�o existe.";
                break;
            case PlayFabErrorCode.InvalidEmailOrPassword:
                statusText.text = "N�o foi poss�vel efetuar o login!\nE-mail ou senha inv�lidos.";
                break;
            default:
                statusText.text = "N�o foi poss�vel efetuar o login!\nVerifique os dados infomados.";
                break;

        }
    }

    public void FalhaCriarConta(PlayFabError error)
    {
        Debug.Log("Falhou a tentativa de criar uma conta nova");
        statusText.text = "Falhou a tentativa de criar uma conta nova";

        switch (error.Error)
        {
            case PlayFabErrorCode.InvalidEmailAddress:
                statusText.text = "J� possui um conta com esse email!";
                break;
            case PlayFabErrorCode.InvalidUsername:
                statusText.text = "Username j� est� em uso.";
                break;
            case PlayFabErrorCode.InvalidParams:
                statusText.text = "N�o foi poss�vel criar um conta! \nVerifique os dados informados";
                break;
            default:
                statusText.text = "N�o foi poss�vel efetuar o login!\nVerifique os dados infomados.";
                Debug.Log(error.ErrorMessage);
                break;
        }
    }

    public void SucessoCriarConta(RegisterPlayFabUserResult result)
    {
        Debug.Log("Sucesso ao criar uma conta nova!");
        statusText.text = "Sucesso ao criar uma conta nova!";
    }

    #endregion

}
