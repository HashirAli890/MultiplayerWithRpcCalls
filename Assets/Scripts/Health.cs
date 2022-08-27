using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.UI;
public class Health : MonoBehaviour
{
    public float CurrentHealth = 0f;
    public float MaxHealth = 100f;
    public Image HealthBar;
    public string ExplosionParticles;
    public Transform ShootPoint;
    private void Start()
    {
        HealthBar.fillAmount = 1f;
        CurrentHealth = MaxHealth;
    }
    public void damage(float DamageAmount)
    {
        GetComponent<PhotonView>().RPC("Takedamage", RpcTarget.All, new object[] { DamageAmount });
    }
    //Damage Over the Network if bullet hit on network clone
    [PunRPC]
    void Takedamage(float DamageAmount) 
    {
        CurrentHealth -= DamageAmount;
        HealthBar.fillAmount = CurrentHealth / MaxHealth;
        if (CurrentHealth <= 0)
        {
            if (ExplosionParticles.Length > 0)
                PhotonNetwork.Instantiate(ExplosionParticles, transform.position, transform.rotation);
            if (TroopsDeployment.Instance)
                TroopsDeployment.Instance.UpdateListTroopsList(this.transform);

            GetComponent<AnimatorHandler>().AnimChangeCall(TroopsDeployment.Instance.Anim.Death);
            StartCoroutine(DeadWait());

        }
        else 
        {
            this.gameObject.GetComponent<AnimatorHandler>().WaitAfterHit();
        }
    }
    IEnumerator DeadWait() 
    {
        yield return new WaitForSeconds(2.5f);
        Destroy(this.gameObject);
    }
}
