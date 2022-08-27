using Photon.Pun;

public class SpawnHandlingOverNetwork : MonoBehaviourPunCallbacks
{

    public void RPCCall(string tag,int layer) 
    {
        
        GetComponent<PhotonView>().RPC("AssignAttributes",RpcTarget.All,new object[] {tag,layer});
    }
    //setting Values over the network to the clone
    [PunRPC]
     void AssignAttributes(string Tag,int layer) 
     {
        this.tag = Tag;
        this.gameObject.layer = layer;
        this.transform.GetChild(0).gameObject.tag = Tag;
        this.transform.GetChild(0).gameObject.SetActive(false);
     }
}
