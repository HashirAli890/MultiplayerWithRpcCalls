using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Events;

public class UiHandler : MonoBehaviourPunCallbacks
{
    //All User Interface Reference In MainMenu 

    public static UiHandler Instance;
    public GameObject StartGame;

    //Next Scene U want To load
    public Lobby.Scene NextScene;
    [Header("Room Show Aera of scroll view")]
    public GameObject RoomContext;
    [Header("Player Show Aera of scroll view")]
    public GameObject PlayerContext;
    [Header("if user want to name Room by own")]
    public bool byName;
    [ShowIf("byName", true)]
    public InputField RoomName;
    public GameObject CreateRoombtn;

    [Header("if user want to use nickname")]
    public bool UseNickName;
    [ShowIf("UseNickName", true)]
    public InputField PlayerNickname;
    //Text for user PhotonNetwork Id
    public Text OwnUserID;

    #region UI
    #region Messages
    [InfoBox("Panel Messages")]
    [FoldoutGroup("Messages")]
    public string MeassageForBusyHost;
    [FoldoutGroup("Messages")]
    public string MesaageForDeclineChallenge;
    [FoldoutGroup("Messages")]
    public string MessageForAcceptedChallenege;
    [FoldoutGroup("Messages")]
    public string NotEnoughEnergy;
    #endregion

    [Header("UI/Panels/Texts")]
    //Totatl Player in Room Text
    public Text PlayerCount;
    #region panels
    [InfoBox("Panels")]
    [FoldoutGroup("Panels Object")]
    public GameObject ChallengePopUp;
    [FoldoutGroup("Panels Object")]
    public GameObject BusyPopUP;
    [FoldoutGroup("Panels Object")]
    public GameObject ChallenedAcceptedPopUp;
    [FoldoutGroup("Panels Object")]
    public GameObject CancelRequestPopUp;
    [FoldoutGroup("Panels Object")]
    public GameObject BuiltTeam;
    [FoldoutGroup("Panels Object")]
    public GameObject WaitingPanel;
    [FoldoutGroup("Panels Object")]
    public GameObject PlayerLeft;
    [FoldoutGroup("Panels Object")]
    public GameObject BulletSelection;

    public GameObject SelectionPanel;
    #endregion

    #region Texts
    [InfoBox("PopUp Texts")]
    [FoldoutGroup("Texts")]
    public Text Challengetext;
    [FoldoutGroup("Texts")]
    public Text BusyPopUpText;
    [FoldoutGroup("Texts")]
    public Text ChallenedAcceptedPopUpText;
    [FoldoutGroup("Texts")]
    public Text CancelRequestPopUpText;
    [FoldoutGroup("Texts")]
    public Text Energy;
    [FoldoutGroup("Texts")]
    public Text Cube;
    [FoldoutGroup("Texts")]
    public Text Sphere;
    [FoldoutGroup("Texts")]
    public Text Cylinder;
    [FoldoutGroup("Texts")]
    public Text NotEnoughEnergyText;
    [FoldoutGroup("Texts")]
    public Text TimerText;
    #endregion

    #region button
    [InfoBox("Buttons")]
    [FoldoutGroup("Buttons")]
    public Button ChallengeAccepted;
    [FoldoutGroup("Buttons")]
    public Button ChallengeDeclined;
    [FoldoutGroup("Buttons")]
    public Button CancelRequest;
    [FoldoutGroup("Buttons")]
    public Button CubeBtn;
    [FoldoutGroup("Buttons")]
    public Button CyliderBtn;
    [FoldoutGroup("Buttons")]
    public Button SphereBtn;
    [FoldoutGroup("Buttons")]
    public Button ReadyBtn;
    [FoldoutGroup("Buttons")]
    public Button MultiplayerBtn;
    [FoldoutGroup("Buttons")]
    public Button Close;

   
    [HideInInspector]
    public string PlayerName;
    public Dropdown _DropDown;
    #endregion

    #region
    public string OnSelectedPlayerCube;
    public string OnSelectedPlayerSphere;
    public string OnSelectedPlayerCylinder;
    #endregion

    public GameObject LoadingImage;
    #endregion
    #region Energy Values
    [Title("Energy Handling")]
    [InfoBox("Cost per Deplyoment Energy")]
    [FoldoutGroup("Enery Cost")]
    public float CubeCost;
    [FoldoutGroup("Enery Cost")]
    public float SphereCost;
    [FoldoutGroup("Enery Cost")]
    public float CylinderCost;
    [FoldoutGroup("Enery Cost")]
    [InfoBox("Intial Energy that Player Can use To Buy paltoon")]
    public float TotalEnergy;
    #endregion

    #region Events
    [InfoBox("Events")]
    //Events You can Call on functions
    [FoldoutGroup("Events")]
    public UnityEvent OnRoomCreation;
    [FoldoutGroup("Events")]
    public UnityEvent OnLobbyJoined;
    [FoldoutGroup("Events")]
    public UnityEvent OnRoomJoined;
    [FoldoutGroup("Events")]
    [InfoBox("This Event Called When you Already Send one Request and didnt cancel Request And Try To send Another Request")]
    public UnityEvent PlayerAlredySentRquest;
    #endregion


    private void Awake()
    {
        if (UiHandler.Instance == null)
        {
            Instance = this;
        }
    }
    private void Start()
    {
        if (!GameManager.Instance.once)
        {
            Initialize();
        }
    }
    public void Initialize() {
        Lobby.Lob.RemoveAllListners();
        Lobby.Lob.OnClickListenes();
        CheckForBackToLobby();
    }
    void CheckForBackToLobby()
    {
        //when User Getting Back From Gameplay And resetting Values

        if (PhotonNetwork.IsConnectedAndReady && !GameManager.Instance.once)
        {
            if (PhotonNetwork.CurrentRoom.Name != Lobby.Lob.IntialRoomName)
            {
                RestValues(true);
                PhotonNetwork.LeaveRoom();
            }
   
        }
    }
    
 public void RestValues(bool ClearLists)
    {
        Lobby.Lob.CA.Acceptance = false;
        Lobby.Lob.CA.Challenger = false;
        Lobby.Lob.CA.win = false;
        Lobby.Lob.CA.lose = false;
        Lobby.Lob.CA.Meassage = null;
        Lobby.Lob.CA.ChallengedID = null;
        Lobby.Lob.CA.ChallangerID = null;
        Lobby.Lob.clinet = false;
        Lobby.Lob.RequestSent = false;
        Lobby.Lob._PlayerBtnInfo.Clear();
        Lobby.Lob.RoomNameToJoin = null;
        Lobby.Lob.CA.Cube = 0;
        Lobby.Lob.CA.Cylinder = 0;
        Lobby.Lob.CA.Sphere = 0;
        Lobby.Lob.CA.Ready = false;
        Lobby.Lob.CA.SpwanLayerSelected = 0;
        Lobby.Lob.CA.ReadyForMovePhase = false;
        Lobby.Lob.CA.MyTurn = false;
        Lobby.Lob.CA.OppenentTurn = false;
        Lobby.Lob.CA.FinishedAttack = false;
        Lobby.Lob.CA.AttackingPhase = false;
        if (ClearLists) 
        {
            Lobby.Lob.ClearList(RoomContext);
            Lobby.Lob.ClearList(PlayerContext);
        }
        Lobby.Lob.CR = Lobby.Lob.CA;  

    }
    #region Team Building
    //For Making Team

    //adding Sphere
    public void OnSphereClick() 
    {
        ReadyBtn.gameObject.SetActive(true);
        NotEnoughEnergyText.gameObject.SetActive(false);
        if (TotalEnergy > SphereCost)
        {
            UiHandler.Instance.BulletSelection.SetActive(true);
            Lobby.Lob.CA.Sphere++;
            Sphere.text = Lobby.Lob.CA.Sphere.ToString();
            TotalEnergy -= SphereCost;
            Energy.text = TotalEnergy.ToString();
            UiHandler.Instance.PlayerName = OnSelectedPlayerSphere;
        }
        else 
        {
            NotEnoughEnergyText.gameObject.SetActive(true);
            NotEnoughEnergyText.text = NotEnoughEnergy;
            WaitToCloseEnergyText();
        }
    }
    //adding Cube
    public void OnCubeClick()
    {
        ReadyBtn.gameObject.SetActive(true);
        NotEnoughEnergyText.gameObject.SetActive(false);
        if (TotalEnergy > CubeCost)
        {
            UiHandler.Instance.BulletSelection.SetActive(true);
            Lobby.Lob.CA.Cube++;
            Cube.text = Lobby.Lob.CA.Cube.ToString();
            TotalEnergy -= CubeCost;
            Energy.text = TotalEnergy.ToString();
            UiHandler.Instance.PlayerName = OnSelectedPlayerCube;
        }
        else
        {
            NotEnoughEnergyText.gameObject.SetActive(true);
            NotEnoughEnergyText.text = NotEnoughEnergy;
            WaitToCloseEnergyText();
        }
    }
    //adding Cylinder
    public void OnCylinderClick()
    {
        ReadyBtn.gameObject.SetActive(true);
        NotEnoughEnergyText.gameObject.SetActive(false);
        if (TotalEnergy > CylinderCost)
        {
            UiHandler.Instance.BulletSelection.SetActive(true);
            Lobby.Lob.CA.Cylinder++;
            Cylinder.text = Lobby.Lob.CA.Cylinder.ToString();
            TotalEnergy -= CylinderCost;
            Energy.text = TotalEnergy.ToString();
            UiHandler.Instance.PlayerName = OnSelectedPlayerCylinder;
        }
        else
        {
            NotEnoughEnergyText.gameObject.SetActive(true);
            NotEnoughEnergyText.text = NotEnoughEnergy;
            WaitToCloseEnergyText();
        }
    }
    #endregion
    IEnumerator WaitToCloseEnergyText() 
    {
        yield return new WaitForSeconds(2f);
        NotEnoughEnergyText.gameObject.SetActive(false);
    }
}
