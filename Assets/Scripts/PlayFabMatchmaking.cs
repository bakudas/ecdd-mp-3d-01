using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.MultiplayerModels;
using System;
using Unity.VisualScripting;

public class PlayFabMatchmaking : MonoBehaviour
{

    public string entityId;
    public string ticketId;
    public string nomeFila = "mpecddqueue";
    private IEnumerator coroutine;

    public void StartPlayFabMatchmaking()
    {
        Debug.Log("[Matchmaker] Começando o emparalhamento!");

        entityId = PlayFabLogin.PFL.EntityID;

        CreateMatchmakingTicket(entityId);
    }

    public void CreateMatchmakingTicket(string entityId)
    {
        var request = new CreateMatchmakingTicketRequest
        {
            Creator = new MatchmakingPlayer
            {
                Entity = new EntityKey
                {
                    Id = entityId,
                    Type = "title_player_account"
                }
            },
            QueueName = nomeFila,
            GiveUpAfterSeconds = 120
        };

        PlayFabMultiplayerAPI.CreateMatchmakingTicket(request, OnMatchmakingTicketCreated, OnMatchmakingError);
    }

    // callback sucesso criação ticket
    private void OnMatchmakingTicketCreated(CreateMatchmakingTicketResult result)
    {
        // captura o ticket token
        ticketId = result.TicketId;
        
        // mensagem de status
        Debug.Log($"[Matchmaker] Ticket criado com sucesso: {ticketId}");
        
        // inicializa a coroutine
        coroutine = WaitAndGetMatchmakingTicket(5.0f);
        
        // ativa a rotina
        StartCoroutine(coroutine);
    }

    // callback de erro em requisições do playfabs
    private void OnMatchmakingError(PlayFabError error)
    {
        Debug.LogError($"[Matchmaker] PlayFab erro: {error.Error} | {error.ErrorMessage}");
    }

    // Coroutine para o polling - verificação periódica da fila
    private IEnumerator WaitAndGetMatchmakingTicket(float waitTime)
    {
        while (true)
        {
            GetMatchmakingTicket();
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void GetMatchmakingTicket()
    {
        // requisição para monitorar a fila/tickets
        PlayFabMultiplayerAPI.GetMatchmakingTicket(
            new GetMatchmakingTicketRequest
            {
                TicketId = ticketId,
                QueueName = nomeFila
            },
            // callback de sucesso
            OnGetMatchmakingTicket,
            // callback de falha
            OnMatchmakingError
        );
    }

    private void OnGetMatchmakingTicket(GetMatchmakingTicketResult result)
    {
        Debug.Log($"[Matchmaker] Ticker status: {result.Status}");
    }

    private void OnGetMatched(GetMatchResult result)
    {
        // futura implementação
        // capturar o matchID para criar sala no Photon
    }

    public void CancelPlayFabMatchmaking()
    {

    }

    private void CancelMatchmakingTicket()
    {

    }

    // callback de sucesso ao cancelar o ticket
    private void OnTicketCanceled(CancelMatchmakingTicketResult result)
    {

    }
}

