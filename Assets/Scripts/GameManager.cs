using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{

    public static GameManager Instance;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Transform playerSpawnerPosition;

    [SerializeField] float levelTempo = 30f;
    private double startTime;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startTime = PhotonNetwork.Time;
            ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable();
            roomProps["StartTime"] = startTime;
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
        }

        if (PlayerController.LocalPlayerInstance == null)
        {
            PhotonNetwork.Instantiate("Prefabs/" + playerPrefab.name, playerSpawnerPosition.position, Quaternion.identity);
        }

    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable customProps)
    {
        if (customProps.ContainsKey("StartTime"))
        {
            startTime = (double)PhotonNetwork.CurrentRoom.CustomProperties["StartTime"];
        }
    }

    // Update is called once per frame
    void Update()
    {
        // quanto de tempo já passou
        double tempoPassado = PhotonNetwork.Time - startTime;
        // quanto de tempo resta para acabar o jogo
        double tempoRestante = levelTempo - tempoPassado;

        if (tempoRestante <= 0)
        {
            // rotina de acabar o jogo
            GameOver();
        }

        // Atualizar a UI com o tempo de jogo
        UIManager.Instance.UpdateTimer((float)tempoRestante);
    }

    public void GameOver()
    {
        Debug.Log("ACABOU O JOGO");
    }
}
