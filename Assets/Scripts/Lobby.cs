using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;//Custom Editor NameSpace
//[photonnetwork NameSpace]
using Photon.Pun;
using Photon.Realtime;





    [System.Serializable]
public class PlayerBtnInfo
{
    //button refernce 
    public GameObject Playersbtn;
    //contains player unique id
    public string id;
}
[System.Serializable]
public class ChallenegAttributes
{
    //Network Id OF Player Who send Challenge
    public string ChallangerID;
    //Network Id OF Player Who you challenged
    public string ChallengedID;
    //Message for to send Over Network
    public string Meassage;
    //To define Who is challenger and who get challenged
    public bool Challenger;
    //if u win match
    public bool win;
    //if u win lose match
    public bool lose;
    //if challenged player Accepted Challenge 
    public bool Acceptance = false;
    //Toal Cube To Deploy
    public int Cube;
    //Total Cylinder To Deploy
    public int Cylinder;
    //Total Sphere To Deploy
    public int Sphere;
    //Check If play Selected Team and ready for Gameplay
    public bool Ready;
    //The Layer Where usercan Deploy Troops
    public int SpwanLayerSelected;
    //Check if Depliyed All Players and ready for Move
    public bool ReadyForMovePhase;
    //Check if Depliyed All Players and ready for Attack
    public bool AttackingPhase;
    //if true then i can Attack
    public bool MyTurn;
    //if true then i oppnent Can attack
    public bool OppenentTurn;
    //if both finish Attacking
    public bool FinishedAttack;

}
public class Lobby : MonoBehaviourPunCallbacks, IInRoomCallbacks,ILobbyCallbacks
{
    #region Scenes 
    //For Scene Switching
    //Add your Scenes here 
    public enum Scene
    {

        MainMenu = 0, GamePlay = 1
    }

    
    public static Scene _Scenes;
    #endregion

    public static Lobby Lob;
    //For Intial Room Creation
    public string IntialRoomName;
    public bool ShowDebugs;//For Showing Debugs In Console and Inspector
    public bool ShowIDOnPlayers;
    public int TimerTime;
    [Header("Wait For network to ready")]
    //if photonNetwork is not ready yet 
    public float ReconnectingWait;

    #region Debugs / Store Values
    [ShowIf("ShowDebugs", true)]
    [InfoBox("debug values")]
    [ReadOnly]
    public List<PlayerBtnInfo> _PlayerBtnInfo;
    [ShowIf("ShowDebugs", true)]
    [BoxGroup]
    [ReadOnly]
    public string userID;
    [ShowIf("ShowDebugs", true)]
    [BoxGroup]
    [ReadOnly]
    public string ChallengedPlayerID;
    [ShowIf("ShowDebugs", true)]
    [BoxGroup]
    [ReadOnly]
    public ChallenegAttributes CA;
    [ShowIf("ShowDebugs", true)]
    [BoxGroup]
    [ReadOnly]
    public bool RequestSent;
    [ShowIf("ShowDebugs", true)]
    [BoxGroup]
    [ReadOnly]
    public bool clinet;
    [ShowIf("ShowDebugs", true)]
    [ReadOnly]
    [BoxGroup]
    public ChallenegAttributes CR;
    [ShowIf("ShowDebugs", true)]
    [ReadOnly]
    [BoxGroup]
    public string RoomNameToJoin;
    [ShowIf("ShowDebugs", true)]
    [ReadOnly]
    [BoxGroup]
    [ShowInInspector]
    int currentTimer = 0;
    #endregion

    public bool ShowButtonsOnnetwork;
    [ShowIf("ShowButtonsOnnetwork",true)]
    [InfoBox("Name of Prefab for Room and Players To Show if using PhotonNetwrok")]
    public string PlayerObjectPrefabname;
    [ShowIf("ShowButtonsOnnetwork", true)]
    public string RoomObjectPrefabName;
    #region Events
    [FoldoutGroup("Events")]
    [InfoBox("Events")]
    public UnityEvent OnRoomLeft;
    [FoldoutGroup("Events")]
    public UnityEvent OnOtherPlayerEnterRoom;
    [FoldoutGroup("Events")]
    public UnityEvent OnOtherPlayerLeftRoom;
    #endregion
    #region Raise Event Bytes
    //network Bytes for Raise event you can use 0-199 bytes other 199 to 254 is reserved
    private const byte Challenge_Byte = 1;
    private const byte PlayerBusyWithOtherPlayer_Byte = 2;
    private const byte ChallenegAccepted_Byte = 3;
    private const byte ChallenegDeclined_Byte = 4;
    private const byte RoomRequest = 5;
    private const byte CaneclRequest = 9;
    private const byte Ready = 10;
    #endregion

    bool once = true;
    public GameObject PlayerObjPrefab;

    private void Awake()
    {
        if (Lobby.Lob == null)
        {
            Lob = this;
        }
        else
        {
            Destroy(this.gameObject);

        }
        DontDestroyOnLoad(this.gameObject);
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        if (GameManager.Instance.once)
        {
            UiHandler.Instance.SelectionPanel.SetActive(true);
        }
        UiHandler.Instance.MultiplayerBtn.onClick.AddListener(()=> 
        {
            UiHandler.Instance.MultiplayerBtn.gameObject.SetActive(false);
            UiHandler.Instance.Initialize();
            UiHandler.Instance.LoadingImage.SetActive(true);
        });

        if (UiHandler.Instance.UseNickName)
        {
            UiHandler.Instance.PlayerNickname.gameObject.SetActive(true);
        }
        //binding the event or subscribe event
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
    }

    public void OnDisable()
    {
        //for remove binding the event or unsubscribe event
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }
    #region Adding and removing on Click listner
    //adding actions to button 
    public void OnClickListenes()
    {
        PhotonNetwork.ConnectUsingSettings();
        UiHandler.Instance.StartGame.GetComponent<Button>().onClick.AddListener(() => PlayGame());
        //Challenged Room Creation for Gameplay 
        UiHandler.Instance.CreateRoombtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            CR.Meassage = UiHandler.Instance.RoomName.text;
            string Json = JsonUtility.ToJson(CR);
            UiHandler.Instance.CancelRequest.gameObject.SetActive(false);
            RoomNameToJoin = UiHandler.Instance.RoomName.text;
            PhotonNetwork.RaiseEvent(RoomRequest, Json, RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendReliable);
            LeaveRoom();
            UiHandler.Instance.ChallenedAcceptedPopUp.SetActive(false);
        }
        );
        UiHandler.Instance.ChallengeAccepted.onClick.AddListener(() => { 
            OnChallengeAccepted();
            StopAllCoroutines();
            UiHandler.Instance.TimerText.gameObject.SetActive(false);
            UiHandler.Instance.TimerText.text = TimerTime.ToString();
        });
        UiHandler.Instance.ChallengeDeclined.onClick.AddListener(() => { OnChallengeDeclined(); });
        UiHandler.Instance.CancelRequest.onClick.AddListener(() => { 
            OnCacelRequest();

            StopAllCoroutines();
            UiHandler.Instance.TimerText.text = TimerTime.ToString();
            UiHandler.Instance.TimerText.gameObject.SetActive(false);
        });
        UiHandler.Instance.RoomName.onEndEdit.AddListener(delegate { EnterRoomName(); }); ;
        UiHandler.Instance.CubeBtn.onClick.AddListener(()=> {
            UiHandler.Instance.OnCubeClick();
            
        });
        UiHandler.Instance.CyliderBtn.onClick.AddListener(() => { UiHandler.Instance.OnCylinderClick(); });
        UiHandler.Instance.SphereBtn.onClick.AddListener(() => { UiHandler.Instance.OnSphereClick(); });
        UiHandler.Instance.ReadyBtn.onClick.AddListener(() => { ReadyPlayer(); });
        UiHandler.Instance.Close.onClick.AddListener(() => {
            UiHandler.Instance.BulletSelection.SetActive(false);
           WeaponController.instance.WeaponValueChanged();
            }
        );
    }
    //adding actions from button 
    public void RemoveAllListners()
    {
        UiHandler.Instance.StartGame.GetComponent<Button>().onClick.RemoveAllListeners();
        UiHandler.Instance.CreateRoombtn.GetComponent<Button>().onClick.RemoveAllListeners();
        UiHandler.Instance.ChallengeAccepted.onClick.RemoveAllListeners();
        UiHandler.Instance.ChallengeDeclined.onClick.RemoveAllListeners();
        UiHandler.Instance.CancelRequest.onClick.RemoveAllListeners();
        UiHandler.Instance.RoomName.onEndEdit.RemoveAllListeners();
        UiHandler.Instance.CubeBtn.onClick.RemoveAllListeners();
        UiHandler.Instance.SphereBtn.onClick.RemoveAllListeners();
        UiHandler.Instance.CyliderBtn.onClick.RemoveAllListeners();
        UiHandler.Instance.ReadyBtn.onClick.RemoveAllListeners();
    }
    #endregion
    #region Pun Call backs
    public override void OnConnectedToMaster()
    {
        if (ShowDebugs)
            Debug.Log(PhotonNetwork.CloudRegion + " Region");


        if (GameManager.Instance.once)
        {
            UiHandler.Instance.SelectionPanel.SetActive(false);
            UiHandler.Instance.StartGame.gameObject.SetActive(true);
            GameManager.Instance.once = false;
        }
        else
        {
           
            CreateNewRoom();
        }
    }
    public override void OnJoinedLobby()
    {
        if (ShowDebugs)
        {
            Debug.Log(PhotonNetwork.CurrentLobby.Name);
            Debug.Log("We are In lobby");
            Debug.Log(PhotonNetwork.CountOfRooms + " On join lobby count");
            
         
        }
        IntialConnectionToRoom();
     UiHandler.Instance.OnLobbyJoined.Invoke();
    }
    //Call back If new Room Added to list this function only work if u in lobby
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("jhfgiuldsfigh");

        foreach (RoomInfo Info in roomList)
        {
            if (ShowDebugs)
            {
                Debug.Log(Info.Name);
            }
        }
    }
    //Call Back for Room created
    public override void OnCreatedRoom()
    {
        if (ShowDebugs)
        {
            Debug.Log("Room created");
            Debug.Log(PhotonNetwork.CountOfRooms + " Room Count");
        }
    UiHandler.Instance.OnRoomCreation.Invoke();
    }
    //Call Back for Room Joined
    public override void OnJoinedRoom()
    {
        userID = PhotonNetwork.LocalPlayer.UserId;
        UiHandler.Instance.OwnUserID.text = PhotonNetwork.LocalPlayer.UserId;
        if (PhotonNetwork.CurrentRoom.Name != IntialRoomName) 
        {
            UiHandler.Instance.BuiltTeam.SetActive(true);
        }
        if (ShowDebugs)
        {
            Debug.Log(UiHandler.Instance.PlayerNickname.text);
            Debug.Log("Room joined " + PhotonNetwork.CurrentRoom.Name);
        }

        
        
        UiHandler.Instance.PlayerCount.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString();
        //All The Players Already in Room 
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            //Excluding my Own Player
            if (p.UserId != PhotonNetwork.LocalPlayer.UserId)
            {
                //Showinf Other Players To user
                OnNew_Room_player_Add(p.UserId, UiHandler.Instance.PlayerContext, false, PlayerObjectPrefabname);
            }

        }

        //Showing my room Currenlty Connected With
        OnNew_Room_player_Add(PhotonNetwork.CurrentRoom.Name, UiHandler.Instance.RoomContext, true, RoomObjectPrefabName);
       UiHandler.Instance.OnRoomJoined.Invoke();


    }
    //On UserPlayer Left The Room
    public override void OnLeftRoom()
    {
        if (ShowDebugs)
            Debug.Log("Room left");
        

        OnRoomLeft.Invoke();
    }
    //Call Back Room Creation Failed
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        if (ShowDebugs)
            Debug.Log("Room creation Failed");
        CreateNewRoom();
    }
    //Call Back on New Player Enter or joined Room
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        OnNew_Room_player_Add(newPlayer.UserId, UiHandler.Instance.PlayerContext, false, PlayerObjectPrefabname);
        UiHandler.Instance.PlayerCount.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString();

        if (PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount && PhotonNetwork.CurrentRoom.Name != IntialRoomName)
        {
            if (once)
            {
                UiHandler.Instance.BuiltTeam.SetActive(true);
                if (ShowDebugs)
                    Debug.Log("All Player Joined");
                //SwitchScene((int)UiHandler.Instance.NextScene);
                once = false;
            }
        }
        OnOtherPlayerEnterRoom.Invoke();
    }
    //Call back When Some Player Leaved the Room
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        
        int Sceneno = SceneManager.GetActiveScene().buildIndex;
        if (Sceneno == (int)Lobby.Scene.MainMenu)
        {

            UiHandler.Instance.PlayerCount.text = PhotonNetwork.CurrentRoom.PlayerCount.ToString();
            //Comparing to -1 Because if no Player with id Exist 
            if (indexno(otherPlayer.UserId) != -1)
            {
                Destroy(_PlayerBtnInfo[indexno(otherPlayer.UserId)].Playersbtn);
                _PlayerBtnInfo.Remove(_PlayerBtnInfo[indexno(otherPlayer.UserId)]);
            }
            if (otherPlayer.UserId == CA.ChallangerID || otherPlayer.UserId == CA.ChallengedID)
            {
                if (CA.Acceptance)
                    CA.Acceptance = false;
                if (RequestSent)
                    RequestSent = false;
                UiHandler.Instance.CancelRequest.gameObject.SetActive(false);
                clinet = false;
               
            }
            if (PhotonNetwork.CurrentRoom.Name != IntialRoomName)
            {

                UiHandler.Instance.RestValues(false);
                UiHandler.Instance.BuiltTeam.SetActive(false);
                UiHandler.Instance.Sphere.text = "0";
                UiHandler.Instance.Cube.text = "0";
                UiHandler.Instance.Cylinder.text = "0";
                UiHandler.Instance.PlayerLeft.SetActive(true);
                PhotonNetwork.LeaveRoom();
            }
        }
        else
        {
            //GamePlay Discconectivity Handlling
            if (PlayHandler.Instance != null)
            {
                PlayHandler.Instance.OnDiscconet();
            }
        }
        OnOtherPlayerLeftRoom.Invoke();

    }
    #endregion
    //After Connecting to Master Play Game On ButtonClick
    void PlayGame()
    {
        if (UiHandler.Instance.UseNickName)
        {
            if (UiHandler.Instance.PlayerNickname.text.Length > 0)
            {
                PhotonNetwork.NickName = UiHandler.Instance.PlayerNickname.text;
            }
            else
            {
                PhotonNetwork.NickName = Random.Range(0, 10000).ToString();
            }
        }
        UiHandler.Instance.StartGame.SetActive(false);
        UiHandler.Instance.PlayerNickname.gameObject.SetActive(false);
        PhotonNetwork.JoinLobby();
    }
    void IntialConnectionToRoom()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            RoomOptions RoomOpt = new RoomOptions()
            {
                //first room for all players no limits how many players will join this room is supposed as a lobby  
                //in this room you can see players and 0 mean no limit
                //for getting players id publishuserid is true on false u wont be able to get playersid
                IsVisible = true,
                IsOpen = true,
                MaxPlayers = 0,
                PublishUserId = true,
                EmptyRoomTtl = 5

            };
            PhotonNetwork.JoinOrCreateRoom(IntialRoomName, RoomOpt, null);
        }
        else
        {
            StartCoroutine(WaitToReconnect());
        }

    }
    IEnumerator WaitToReconnect()
    {
        yield return new WaitForSeconds(ReconnectingWait);
        IntialConnectionToRoom();
    }
    //in id any string can be sent as header this id show on button text 
    //context is the object which is gonna be your parent object
    //bool room is using to check weather this is using for room or player
    void OnNew_Room_player_Add(string Id, GameObject COntext, bool Room, string PrefabName)
    {
        if (COntext != null)
        {
            GameObject Btn;
            if (!ShowButtonsOnnetwork)
            {
                Btn = Instantiate(PlayerObjPrefab);
            }
            else 
            {
                Btn = PhotonNetwork.Instantiate(PrefabName, transform.position, transform.rotation);
            }
               
            Btn.transform.SetParent(COntext.transform);
            Btn.transform.position = Vector3.zero;
            Btn.transform.localScale = Vector3.one;
            if(ShowIDOnPlayers)
                Btn.transform.GetChild(0).GetComponent<Text>().text = Id;

            if (!Room)
            {
                PlayerBtnInfo _info = new PlayerBtnInfo();
                _info.Playersbtn = Btn;
                _info.id = Id;
                _PlayerBtnInfo.Add(_info);
                Btn.transform.GetComponent<Button>().onClick.AddListener(() =>
                {
                    
                    UiHandler.Instance.CancelRequest.gameObject.SetActive(true);
                    ChallengedPlayerID = _info.id;
                    CA.ChallengedID = _info.id;
                    CA.ChallangerID = userID;
                    SendChallenge();
                });
            }
        }

    }

    //Returns The Index Number of Id From List (PlayerBtnInfo) Which stores button refernce and id of player
    public int indexno(string id)
    {
        for (int i = 0; i < _PlayerBtnInfo.Count; i++)
        {
            if (_PlayerBtnInfo[i].id == id)
            {
                return i;
            }
        }
        return -1;
    }
    //Removing all Prefabs/ Buttons /players in the Slider View Context
    public void ClearList(GameObject Context)
    {
    
        for (int i = 0; i < Context.transform.childCount; i++)
        {
            Destroy(UiHandler.Instance.RoomContext.transform.GetChild(i));
        }

    }
    //Creating New Room For Gameplay 
    public void CreateNewRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            RoomOptions opt = new RoomOptions()
            {
                IsOpen = true,
                IsVisible = true,
                MaxPlayers = 2,
                EmptyRoomTtl = 5
            };
            if (!clinet && RoomNameToJoin == null)
            {
                if (UiHandler.Instance.byName)
                {
                    if (UiHandler.Instance.RoomName.text.Length > 0)
                    {
                        PhotonNetwork.CreateRoom(UiHandler.Instance.RoomName.text, opt, null);
                    }
                    else
                    {
                        IntialConnectionToRoom();
                        if (ShowDebugs)
                            Debug.Log("No name given for room");
                    }
                }
                else
                {
                    if (ShowDebugs)
                        Debug.Log("Random Room Creating");
                    int num = Random.Range(0, 999);
                    PhotonNetwork.CreateRoom("num", opt, null);
                }
            }
            else
            {
                PhotonNetwork.JoinOrCreateRoom(RoomNameToJoin, opt, null);
            }
        }
    }
    //For Leaving Room
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        OnRoomLeft.Invoke();
    }
    //Changing Scene
    public void SwitchScene(int Index)
    {
        if (ShowDebugs)
            Debug.Log(Index);
        PhotonNetwork.LoadLevel(Index);
        once = true;
    }
    //For Send Challeneg To Other Player
    void SendChallenge()
    {
       
        //player once Sent Request To Player not Able to request Othe Player
        if (!RequestSent)
        {
            CA.ChallangerID = userID;
            CA.ChallengedID = ChallengedPlayerID;
            string json = JsonUtility.ToJson(CA);
            PhotonNetwork.RaiseEvent(Challenge_Byte, json, RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendReliable);
            RequestSent = true;
            currentTimer = TimerTime;
            UiHandler.Instance.TimerText.gameObject.SetActive(true);
            StartCoroutine(Timer());
        }
        else 
        {
            if(ShowDebugs)
                Debug.Log("Already Busy With One Host");

        UiHandler.Instance.PlayerAlredySentRquest.Invoke();
        }
    }
    IEnumerator Timer() 
    {
        
        yield return new WaitForSeconds(1f);
        currentTimer--;
        UiHandler.Instance.TimerText.text = currentTimer.ToString();
        if (currentTimer <= 0)
        {
            UiHandler.Instance.TimerText.gameObject.SetActive(false);
            OnCacelRequest();
        }
        else 
        {
            StartCoroutine(Timer());
        }

    }
    //On Event Raise in MainMenu Requests
    private void NetworkingClient_EventReceived(ExitGames.Client.Photon.EventData obj)
    {
        switch (obj.Code)
        {
            case Challenge_Byte://On Chalenge receive
                CR = JsonUtility.FromJson<ChallenegAttributes>((string)obj.CustomData);
              
                if (!RequestSent)
                {
                    if (CR.ChallengedID == userID)
                    {
                        if (!UiHandler.Instance.ChallengePopUp.activeInHierarchy)
                        {

                            UiHandler.Instance.ChallengePopUp.SetActive(true);
                            currentTimer = TimerTime;
                            UiHandler.Instance.TimerText.gameObject.SetActive(true);
                            StartCoroutine(Timer());
                             UiHandler.Instance.Challengetext.text = "Someone Challenged you with the id  " + CR.ChallangerID;
                         

                        }
                        else
                        {


                            CR.Meassage = UiHandler.Instance.MeassageForBusyHost;
                            string json = JsonUtility.ToJson(CR);
                            PhotonNetwork.RaiseEvent(PlayerBusyWithOtherPlayer_Byte, json, RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendReliable);

                        }
                    }
                }
                else
                {
                    if (CR.ChallengedID == userID)
                    {
                        CR.Meassage = UiHandler.Instance.MeassageForBusyHost;
                        string json = JsonUtility.ToJson(CR);
                        PhotonNetwork.RaiseEvent(PlayerBusyWithOtherPlayer_Byte, json, RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendReliable);
                    }
                }
                break;
            case PlayerBusyWithOtherPlayer_Byte:

                CR = JsonUtility.FromJson<ChallenegAttributes>((string)obj.CustomData);
                if (CR.ChallangerID == userID && RequestSent == true && CR.ChallengedID == CA.ChallengedID)
                {
                    UiHandler.Instance.BusyPopUP.SetActive(true);
                    UiHandler.Instance.BusyPopUpText.text = CR.Meassage;
                    UiHandler.Instance.CancelRequest.gameObject.SetActive(false);
                    RequestSent = false;
                }
                break;
            case ChallenegAccepted_Byte:
                CR = JsonUtility.FromJson<ChallenegAttributes>((string)obj.CustomData);
               
                if (CR.ChallangerID == userID)
                {
                    StopAllCoroutines();
                    UiHandler.Instance.TimerText.text = TimerTime.ToString();
                    UiHandler.Instance.TimerText.gameObject.SetActive(false);
                    UiHandler.Instance.ChallenedAcceptedPopUp.SetActive(true);
                    UiHandler.Instance.ChallenedAcceptedPopUpText.text = UiHandler.Instance.MessageForAcceptedChallenege + " " + CR.ChallengedID;
                    CA.Challenger = true;
                    if (!UiHandler.Instance.byName)
                    {
                        UiHandler.Instance.CreateRoombtn.SetActive(true);
                        UiHandler.Instance.RoomName.gameObject.SetActive(false);
                    }
                    if (Lobby.Lob.ShowDebugs)
                        Debug.Log("Challeng Accept by challenged Player");
                }
                break;
            case ChallenegDeclined_Byte:
                CR = JsonUtility.FromJson<ChallenegAttributes>((string)obj.CustomData);
                if (CR.ChallangerID == userID)
                {
                    RequestSent = false;
                    UiHandler.Instance.CancelRequest.gameObject.SetActive(false);
                    if (Lobby.Lob.ShowDebugs)
                        Debug.Log("Challeng Declined by challenger");
                }
                break;
            case RoomRequest:
                

                CR = JsonUtility.FromJson<ChallenegAttributes>((string)obj.CustomData);
                if (CR.ChallengedID == userID)
                {
                    UiHandler.Instance.CancelRequest.gameObject.SetActive(false);
                    RoomNameToJoin = CR.Meassage;
                    StartCoroutine(Wait());
                }
                break;
            case CaneclRequest:
                if (Lobby.Lob.ShowDebugs)
                    Debug.Log("Cancel Accepted Request");
                CR = JsonUtility.FromJson<ChallenegAttributes>((string)obj.CustomData);

                if (CR.ChallengedID == CA.ChallangerID && CR.ChallangerID == CA.ChallangerID) 
                {

                    StopAllCoroutines();

                    UiHandler.Instance.TimerText.text = TimerTime.ToString();
                    UiHandler.Instance.TimerText.gameObject.SetActive(false);
                    RequestSent = false;
                    CA.Acceptance = false;
                    UiHandler.Instance.CancelRequestPopUp.SetActive(true);
                    UiHandler.Instance.CancelRequestPopUpText.text = UiHandler.Instance.MesaageForDeclineChallenge;
                    UiHandler.Instance.ChallengePopUp.SetActive(false);
                    UiHandler.Instance.CancelRequest.gameObject.SetActive(false);
                    
                    if (PhotonNetwork.CurrentRoom.Name != IntialRoomName)
                    {
                        LeaveRoom();
                    }
                }
                else
                {
                   
                    if (UiHandler.Instance.ChallengePopUp.activeInHierarchy && CR.ChallengedID == userID)
                    {
                        
                        UiHandler.Instance.ChallengePopUp.SetActive(false);
                        StopAllCoroutines();
                        UiHandler.Instance.TimerText.text = TimerTime.ToString();
                    }
                    else 
                    {
                        
                        if (CR.Acceptance && UiHandler.Instance.ChallenedAcceptedPopUp.activeInHierarchy)
                        {
                            StopAllCoroutines();
                            UiHandler.Instance.TimerText.text = TimerTime.ToString();
                            UiHandler.Instance.CancelRequest.gameObject.SetActive(false);
                            UiHandler.Instance.ChallenedAcceptedPopUp.SetActive(false);
                            UiHandler.Instance.CancelRequestPopUp.SetActive(true);
                            UiHandler.Instance.CancelRequestPopUpText.text = UiHandler.Instance.MesaageForDeclineChallenge;
                            RequestSent = false;
                        }
                        else 
                        {
                            if (CR.Acceptance && CR.ChallengedID == userID) 
                            {
                                // StopCoroutine("Timer");
                                StopAllCoroutines();
                                UiHandler.Instance.TimerText.text = TimerTime.ToString();
                                CA.Acceptance = false;
                                UiHandler.Instance.CancelRequest.gameObject.SetActive(false);
                                UiHandler.Instance.CancelRequestPopUp.SetActive(true);
                                UiHandler.Instance.CancelRequestPopUpText.text = UiHandler.Instance.MesaageForDeclineChallenge;
                                RequestSent = false;
                            }
                        }
                    }
                }
                break;
            case Ready:
                if (ShowDebugs)
                    Debug.Log("Player Ready");
                CR = JsonUtility.FromJson<ChallenegAttributes>((string)obj.CustomData);
                Debug.Log(CR.ChallangerID);
                if (CR.ChallangerID == CA.ChallangerID || CR.ChallangerID == CA.ChallengedID)
                {
                    if(CA.Ready && CR.Ready)
                    {
                      
                        SwitchScene((int)UiHandler.Instance.NextScene);
                    }
                }
                break;
        }
    }
    //On ButtonClick Challenge Accepted
    void OnChallengeAccepted()
    {
        CR.Acceptance = true;
        CA.Acceptance = true;
        UiHandler.Instance.ChallengePopUp.SetActive(false);
        UiHandler.Instance.CancelRequest.gameObject.SetActive(true);
        clinet = true;
        CR.Meassage = UiHandler.Instance.MessageForAcceptedChallenege;
        string json = JsonUtility.ToJson(CR);
        PhotonNetwork.RaiseEvent(ChallenegAccepted_Byte, json, RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendReliable);
    }
    //On ButtonClick Challenged Declined
    void OnChallengeDeclined()
    {
        UiHandler.Instance.ChallengePopUp.SetActive(false);
        string json = JsonUtility.ToJson(CR);
        PhotonNetwork.RaiseEvent(ChallenegDeclined_Byte, json, RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendReliable);
    }
    //If user have option to Enter Room Name by own
    //Making sure that Room is not null
    public void EnterRoomName()
    {
        if (UiHandler.Instance.RoomName.text.Length > 0)
        {
            UiHandler.Instance.CreateRoombtn.SetActive(true);
            
        }
        else
        {
            UiHandler.Instance.CreateRoombtn.SetActive(false);
        }
    }
    //Wait Before Leaving Room
    public IEnumerator Wait()
    {

        yield return new WaitForSeconds(3f);
        LeaveRoom();
    }
    //Cancel Request On Button
    public void OnCacelRequest()
    {
        RequestSent = false;
        CA.Acceptance = false;
        clinet = false;
        UiHandler.Instance.ChallenedAcceptedPopUp.SetActive(false);
        UiHandler.Instance.ChallengePopUp.SetActive(false);
        UiHandler.Instance.CancelRequest.gameObject.SetActive(false);
        string json = JsonUtility.ToJson(CR);
        PhotonNetwork.RaiseEvent(CaneclRequest, json, RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendReliable);
        if (PhotonNetwork.CurrentRoom.Name != IntialRoomName)
        {
            UiHandler.Instance.RoomName.text = null;
            LeaveRoom();
            RequestSent = false;
            clinet = false;
        }
    }
    public void ReadyPlayer() 
    {
        CA.Ready = true;
        if (CA.Ready && CR.Ready && PhotonNetwork.IsMasterClient)
        {
            SwitchScene((int)UiHandler.Instance.NextScene);
        }
        else
        {
            CR.Ready = true;
            string json = JsonUtility.ToJson(CR);
            PhotonNetwork.RaiseEvent(Ready, json, RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendReliable);
            UiHandler.Instance.WaitingPanel.SetActive(true);
        }
       
       
    }

}
