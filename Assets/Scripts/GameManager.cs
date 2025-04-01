using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    [Header("Player Config")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] playerSpawnerPosition;

    [Header("Game Timer")]
    [SerializeField] private float levelTempo = 30f; // tempo total da rodada
    private double startTime; // tempo em que o jogo começou
    
    private Dictionary<int, int> teamScores = new Dictionary<int, int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;

        // Inicializa os scores dos times (até 4 jogadores)
        for (int i = 1; i <= 4; i++)
        {
            teamScores[i] = 0;
        }
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startTime = PhotonNetwork.Time;
            var roomProps = new ExitGames.Client.Photon.Hashtable
        {
            { "StartTime", startTime }
        };
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
        }

        SpawnLocalPlayer();
    }

    private void SpawnLocalPlayer()
    {
        int actorIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;

        // Garante que não ultrapassamos o número de pontos de spawn
        Vector3 spawnPosition;
        if (actorIndex < playerSpawnerPosition.Length)
        {
            spawnPosition = playerSpawnerPosition[actorIndex].position;
        }
        else
        {
            // Fallback: pega posição aleatória disponível
            int randomIndex = UnityEngine.Random.Range(0, playerSpawnerPosition.Length);
            spawnPosition = playerSpawnerPosition[randomIndex].position;
            Debug.LogWarning($"Não há spawn específico para o jogador {actorIndex + 1}. Usando posição aleatória.");
        }

        PhotonNetwork.Instantiate("Prefabs/" + playerPrefab.name, spawnPosition, Quaternion.identity);
    }


    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable customProps)
    {
        if (customProps.TryGetValue("StartTime", out object startTimeFromProps))
        {
            startTime = (double)startTimeFromProps;
        }
    }

    public void RegisterKill(int killerTeamId)
    {
        if (teamScores.ContainsKey(killerTeamId))
            teamScores[killerTeamId]++;

        Debug.Log($"[SCORE] Time {killerTeamId}: {teamScores[killerTeamId]} pontos");

        // Atualiza UI de score se necessário
        //ScoreUI.Instance?.UpdateTeamScore(killerTeamId, teamScores[killerTeamId]);
    }

    private void Update()
    {
        if (PhotonNetwork.CurrentRoom == null || !PhotonNetwork.InRoom) return;

        double tempoPassado = PhotonNetwork.Time - startTime;
        double tempoRestante = levelTempo - tempoPassado;

        if (tempoRestante <= 0)
        {
            GameOver();
        }
        else
        {
            UIManager.Instance?.UpdateTimer((float)tempoRestante);
        }
    }

    public void GameOver()
    {
        Debug.Log("⏰ Fim de jogo!");
        // Aqui você pode chamar lógica de fim de jogo, mostrar painel de resultados, etc.
    }
}
