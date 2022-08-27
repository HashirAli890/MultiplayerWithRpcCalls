using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class Destroy : MonoBehaviour
{
    public float DestroyTime;


    private void Start()
    {
        GetComponent<PhotonView>().RPC("OnDestroy", RpcTarget.All, new object[] {  });
    }
    [PunRPC]
    public void OnDestroy()
    {
        if (this.gameObject.activeInHierarchy)
        {
            StartCoroutine(Wait());
        }
    }
    IEnumerator Wait() 
    {
        yield return new WaitForSeconds(DestroyTime);
        Destroy(this.gameObject);
    }

}
