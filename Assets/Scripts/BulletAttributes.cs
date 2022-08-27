using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class BulletAttributes : MonoBehaviour
{
    public LayerMask EnemyLayerMask;
    public LayerMask IgnorLayerMask;
    public float DamageToGive;
    public float Movepeed;
    public string HitEffect;
    public float DestroyTime;

   [PunRPC]
    public void SetValues(float damage,LayerMask enemylayer,LayerMask ignorlayer) 
    {
        DamageToGive = damage;
        EnemyLayerMask = enemylayer;
        IgnorLayerMask = ignorlayer;
        Debug.Log(Mathf.RoundToInt(Mathf.Log(ignorlayer.value, 2)));
        Physics.IgnoreLayerCollision(this.gameObject.layer, Mathf.RoundToInt(Mathf.Log(ignorlayer.value, 2)), true);

    }
    [PunRPC]
    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("Collide");
        if (((1 << other.gameObject.layer) & EnemyLayerMask) != 0)
        {
            other.gameObject.GetComponent<Health>().damage(DamageToGive);
            other.gameObject.GetComponent<AnimatorHandler>().AnimChangeCall(TroopsDeployment.Instance.Anim.Hit);
            


            if (HitEffect.Length > 0)
            PhotonNetwork.Instantiate(HitEffect, other.gameObject.transform.position,Quaternion.identity);
        }
        Destroy(this.gameObject);
    }
    private void Update()
    {
        transform.Translate(Vector3.forward*Movepeed*Time.deltaTime);
    }
}
