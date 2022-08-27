using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
public class AnimatorHandler : MonoBehaviour
{
    Animator Anim;
    bool Move;
    float Distance;
    Vector3 Dest;
    // Start is called before the first frame update
    void Start()
    {
        Anim = GetComponent<Animator>();
    }
    public void AnimChangeCall(string AnimName) 
    {
        GetComponent<PhotonView>().RPC("updateAnim", RpcTarget.All, new object[] { AnimName });
    }
    public void MoveDest(Vector3 DestPoint) 
    {
        Dest = DestPoint;
        Move = true;
    }
    // Update is called once per frame
    void Update()
    {
        if (Move) 
        {
            Distance = Vector3.Distance(this.transform.position,Dest);
            if (Distance <= 0.15f ) 
            {
                AnimChangeCall(TroopsDeployment.Instance.Anim.Idle);
                Move = false;
            }
        }   
    }
    [PunRPC]
    void updateAnim(string AnimName)
    {
        Anim.Play(AnimName);
    }

    public void WaitAfterHit() 
    {
        StartCoroutine(Wait());
    }
   IEnumerator Wait() 
    {
        yield return new WaitForSeconds(1.5f);
        AnimChangeCall(TroopsDeployment.Instance.Anim.Idle);
    }
}
