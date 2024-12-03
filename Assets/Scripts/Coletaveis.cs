using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coletaveis : MonoBehaviourPun
{

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.GetPhotonView().IsMine)
            return;

        photonView.RPC("DestroiItem", RpcTarget.MasterClient);
    }

    [PunRPC]
    public void DestroiItem()
    {
        PhotonNetwork.Destroy(this.gameObject);
    }


}
