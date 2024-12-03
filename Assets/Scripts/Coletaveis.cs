using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coletaveis : MonoBehaviourPun
{
    [SerializeField] private int quatidade = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.GetPhotonView().IsMine)
            return;

        other.GetComponent<PlayerController>().UpdateScore(quatidade);
        photonView.RPC("DestroiItem", RpcTarget.MasterClient);
    }

    [PunRPC]
    public void DestroiItem()
    {
        PhotonNetwork.Destroy(this.gameObject);
    }

}
