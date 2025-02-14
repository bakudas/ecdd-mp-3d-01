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
        }
        else
        {
            // credenciais para autenticação
            userEmail = inputUserEmailLogin.text;
            userPassword = inputUserPasswordLogin.text;
            //payload de requisição
            var request = new LoginWithEmailAddressRequest { Email = userEmail, Password = userPassword };
            // Requisição
            PlayFabClientAPI.LoginWithEmailAddress(request, SucessoLogin, FalhaLogin);
        }
    }

    public void CriarConta()
    {
        if (string.IsNullOrEmpty(inputUsername.text) || string.IsNullOrEmpty(inputEmail.text) || string.IsNullOrEmpty(inputPassword.text))
        {
            Debug.Log("Preencha os dados corretamente!");
        }
        else
        {
            username = inputUsername.text;
            userEmail = inputEmail.text;
            userPassword = inputPassword.text;

            // payload da requisição
            var request = new RegisterPlayFabUserRequest { Email = userEmail, Password = userPassword, Username = username };
            // Requisição
            PlayFabClientAPI.RegisterPlayFabUser(request, SucessoCriarConta, FalhaCriarConta);
        }
    }

    public void SucessoLogin(LoginResult resulto)
    {
        Debug.Log("Login foi feito com sucesso!");
    }

    public void FalhaLogin(PlayFabError error)
    {
        Debug.Log("Não foi possível fazer login!");
    }

    public void FalhaCriarConta(PlayFabError error)
    {
        Debug.Log("Falhou a tentativa de criar uma conta nova");
    }

    public void SucessoCriarConta(RegisterPlayFabUserResult result)
    {
        Debug.Log("Sucesso ao criar uma conta nova!");
    }

    #endregion

}
