using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using Sirenix.OdinInspector;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayHandler : MonoBehaviourPunCallbacks
{
    
    public static PlayHandler Instance;
    [Header("Next Scene")]
    public Lobby.Scene Scene;

    [Header("Buttons")]
    [FoldoutGroup("Button")]
    public GameObject Winbtn;
    [FoldoutGroup("Button")]
    public GameObject Losebtn;

    [FoldoutGroup("Messages")]
    [Header("Panel Messages")]
    public string WinMessage;
    [FoldoutGroup("Messages")]
    public string LoseMessage;
    [FoldoutGroup("Messages")]
    public string DisConnectMessage;

    //Raise event Bytes
    private const byte Winbyte = 6;
    private const byte Losebyte = 7;
   

    [Header("UI")]
    [FoldoutGroup("Text")]
    [InfoBox("Texts")]
    public Text WinText;
    [FoldoutGroup("Text")]
    public Text LoseText;
    [FoldoutGroup("Panels")]
    [InfoBox("Panels")]
    public GameObject WinPanel;
    [FoldoutGroup("Panels")]
    public GameObject LosePanel;

   
   
    [Header("Events")]
    [BoxGroup]
    [InfoBox("On Win")]
    public UnityEvent WinEvent;
    [BoxGroup]
    [InfoBox("On Lose")]
    public UnityEvent LoseEvent;
    [BoxGroup]
    [InfoBox("On Disconnect")]
    public UnityEvent DisconnectEvent;




    ChallenegAttributes CR;


    private void Awake()
    {
        if (PlayHandler.Instance == null)
        {
            Instance = this;
        }
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
      
        Winbtn.GetComponent<Button>().onClick.AddListener(() => { OnWin(); });
        Losebtn.GetComponent<Button>().onClick.AddListener(() => { OnLose(); });

        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
    }

    private void OnDestroy()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }
    private void NetworkingClient_EventReceived(ExitGames.Client.Photon.EventData obj)
    {
    
        switch (obj.Code)
        {
            case Winbyte:
                CR = JsonUtility.FromJson<ChallenegAttributes>((string)obj.CustomData);
                if (CR.Challenger)
                {
                    if (Lobby.Lob.ShowDebugs)
                        Debug.Log("we are in "+CR.ChallangerID);
                    if (CR.win && CR.ChallengedID == Lobby.Lob.userID)
                    {
                        if (Lobby.Lob.ShowDebugs)
                            Debug.Log("we are in in " + CR.ChallangerID);
                        LosePanel.SetActive(true);
                        LoseText.text = LoseMessage;
                        if (PhotonNetwork.IsMasterClient)
                            Invoke("SwitchScene", 5f);
                    }
                }
                else
                {
                    if (Lobby.Lob.ShowDebugs)
                        Debug.Log("we are in else" );

                    if (CR.ChallengedID == Lobby.Lob.userID)
                    {
                        LosePanel.SetActive(true);
                        LoseText.text = LoseMessage;
                        if (PhotonNetwork.IsMasterClient)
                            Invoke("SwitchScene", 5f);
                    }
                }
          
                break;
            case Losebyte:
                CR = JsonUtility.FromJson<ChallenegAttributes>((string)obj.CustomData);
                if (Lobby.Lob.ShowDebugs)
                {
                    Debug.Log("cehcking");
                    Debug.Log(CR.Challenger);
                }
                //if (CR.Challenger)
                {
                    if (CR.lose &&  (CR.ChallangerID == Lobby.Lob.userID || CR.ChallengedID == Lobby.Lob.userID))
                    {
                        WinPanel.SetActive(true);
                        WinText.text = WinMessage;
                        if (PhotonNetwork.IsMasterClient)
                            Invoke("SwitchScene", 5f);
                    }
                }
                //else
                //{
                //    if (CR.ChallengedID == Lobby.Lob.userID)
                //    {
                //        WinPanel.SetActive(true);
                //        WinText.text = WinMessage;
                //        if (PhotonNetwork.IsMasterClient)
                //            Invoke("SwitchScene", 5f);
                //    }

                //}
                break;
        }
    }
    public void OnWin()
    {
        if (Lobby.Lob.ShowDebugs)
            Debug.Log("i got clicked");
        WinEvent.Invoke();
        WinPanel.SetActive(true);
        WinText.text = WinMessage;
        Lobby.Lob.CA.win = true;
        Lobby.Lob.CR.win = true;
        string json = JsonUtility.ToJson(Lobby.Lob.CR) ;
      
        PhotonNetwork.RaiseEvent(Winbyte, json, RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendReliable);
        if (PhotonNetwork.IsMasterClient) 
        {
            Invoke("SwitchScene", 5f);
        }
    }
    public void OnLose() 
    {
        if (Lobby.Lob.ShowDebugs)
            Debug.Log("i called lose");
        LoseEvent.Invoke();
        LosePanel.SetActive(true);
        LoseText.text = LoseMessage;
        Lobby.Lob.CA.lose = true;
        Lobby.Lob.CR.lose = true;
        string json = JsonUtility.ToJson(Lobby.Lob.CR);
        PhotonNetwork.RaiseEvent(Losebyte, json, RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendReliable);
        if (PhotonNetwork.IsMasterClient)
        {
            Invoke("SwitchScene", 5f);
        }
    }
    public void OnDiscconet() 
    {
        DisconnectEvent.Invoke();
        WinPanel.SetActive(true);
        WinText.text = DisConnectMessage;
        Invoke("SwitchScene", 5f);
    }

    public void SwitchScene() 
    {
        
        Lobby.Lob.SwitchScene((int)Scene);
    }

}
