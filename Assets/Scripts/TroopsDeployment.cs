using Photon.Pun;
using Photon.Realtime;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


[System.Serializable]
public class AnimationStrings
{
    public string Death;
    public string Attack;
    public string Hit;
    public string Move;
    public string Idle;
}
[System.Serializable]
public class Data_Info
{
    public Transform _Transform;
    public string Type;
    public float Health;
    public bool used;
    public string BulletName;
}
[System.Serializable]
public class DeploymentPointsLayer
{
    public int layer;
    public string ColorName;
}
public class TroopsDeployment : MonoBehaviourPunCallbacks
{
    public static TroopsDeployment Instance;
    public AnimationStrings Anim;
    //Main camera 
    public Camera Cam;

    #region Layers & Tags
    public DeploymentPointsLayer[] layers;
    [InfoBox("Grtting Range Active And deactive by getchild 0")]
    public int RangeLayerNo;


    [InfoBox("Tags For Sender And Reciver && Layers")]
    [FoldoutGroup("Tags")]
    public string TagForChallengerPlayers;
    [FoldoutGroup("Tags")]
    public string TagForChallenedPlayers;
    [FoldoutGroup("Layers")]
    public int RequestSenderlayer;
    [FoldoutGroup("Layers")]
    public int RequestReciverLayer;
    [FoldoutGroup("Layers")]
    [InfoBox("For Reset the collision matrix")]
    public int BulletLayerNo;
    [FoldoutGroup("Layers")]
    public LayerMask RequestSenderlayermask;
    [FoldoutGroup("Layers")]
    public LayerMask RequestReciverLayerMask;
    #endregion

    #region UI

    [FoldoutGroup("Text")]
    public Text CubeText;
    [FoldoutGroup("Text")]
    public Text SphereText;
    [FoldoutGroup("Text")]
    public Text CylinderText;
    [FoldoutGroup("Text")]
    public Text chosenPointForSpwan;
    [FoldoutGroup("Text")]
    public Text WaitingText;

    [FoldoutGroup("Buttons")]
    public Button Deploybtn;
    [FoldoutGroup("Buttons")]
    public Button Cubebtn;
    [FoldoutGroup("Buttons")]
    public Button Spherebtn;
    [FoldoutGroup("Buttons")]
    public Button cylinderbtn;
    [FoldoutGroup("Buttons")]
    public Button ReadyForMove;
    [FoldoutGroup("Buttons")]
    public Button ReadyForAttack;
    [FoldoutGroup("Buttons")]
    public Button FinishAttack;

    [FoldoutGroup("Panels")]
    [InfoBox("Panel For Deploy Troops")]
    public GameObject Panel;
    [FoldoutGroup("Panels")]
    [InfoBox("Panel For Movement")]
    public GameObject MovePanel;

    [FoldoutGroup("Messages")]
    [InfoBox("Infrom You that you have to deploy green area or red aera")]
    public string MessageForDeployAera;
    [FoldoutGroup("Messages")]
    [InfoBox("if Waiting For other Player to get Ready for movement Phase")]
    public string MeaasgeOfWaiting;
    [FoldoutGroup("Messages")]
    [InfoBox("if other Player is ready and waiting for u get Ready")]
    public string MeaasgeToHurryUp;
    [FoldoutGroup("Messages")]
    public string OppenetTurnMessage;
    [FoldoutGroup("Messages")]
    public string PlayerTurnMessage;
    [FoldoutGroup("Messages")]
    public string PlacingPlayerMessage;
    #endregion

    #region Prefab Names for Instantiate
    [FoldoutGroup("Prafabs Name For Instantiate")]
    public string CubePrefabName;
    [FoldoutGroup("Prafabs Name For Instantiate")]
    public string CylinderPrefabName;
    [FoldoutGroup("Prafabs Name For Instantiate")]
    public string SpherePrefabName;
    [FoldoutGroup("Prafabs Name For Instantiate")]
    public string Bullets;
    #endregion
    public GameObject Plane;

    #region Debugs
    [FoldoutGroup("Debugs")]
    [ReadOnly]
    public List<Data_Info> Info;
    [FoldoutGroup("Debugs")]
    [ShowInInspector]
    [ReadOnly]
    bool Deploy;
    [FoldoutGroup("Debugs")]
    [ShowInInspector]
    [ReadOnly]
    bool Move;
    [FoldoutGroup("Debugs")]
    [ShowInInspector]
    [ReadOnly]
    bool Attack;
    [FoldoutGroup("Debugs")]
    [ShowInInspector]
    [ReadOnly]
    int SelectedLayer;
    [FoldoutGroup("Debugs")]
    [ShowInInspector]
    [ReadOnly]
    public string ObjectToInstanitate = "";
    [FoldoutGroup("Debugs")]
    [ShowInInspector]
    [ReadOnly]
    Vector3 spwanposition;
    [FoldoutGroup("Debugs")]
    [ShowInInspector]
    [ReadOnly]
    int TotalCubeCanDeploy;
    [FoldoutGroup("Debugs")]
    [ShowInInspector]
    [ReadOnly]
    int TotalSphereCanDeploy;
    [FoldoutGroup("Debugs")]
    [ShowInInspector]
    [ReadOnly]
    int TotalCylinderCanDeploy;
    //For Private Move Object For Storing Refrences
    [FoldoutGroup("Debugs")]
    [ShowInInspector]
    [ReadOnly]
    GameObject Obj;
    [FoldoutGroup("Debugs")]
    [ShowInInspector]
    [ReadOnly]
    NavMeshAgent Agent;
    [FoldoutGroup("Debugs")]
    [ShowInInspector]
    [ReadOnly]
    bool once = true;
    #endregion

    #region Constant Byte
    //Bytes For Troops Deployment
    private const byte AssignPosToSpwan = 11;
    private const byte WaitingForMove = 12;
    private const byte BeginMovePhase = 13;
    private const byte BeginAttackPhase = 14;
    private const byte AttackOppnonent = 15;
    private const byte ReadyAgain = 16;
    #endregion


    private void Awake()
    {
        if (TroopsDeployment.Instance == null)
        {
            Instance = this;
        }
    }
    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
        AssignSpwanLayer();
    }
    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
    }
    // Assiging Layers / Spwan Positions To Players
    void AssignSpwanLayer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int i = Random.Range(0, layers.Length);
            SelectedLayer = layers[i].layer;
            chosenPointForSpwan.text = layers[i].ColorName + MessageForDeployAera;
            Lobby.Lob.CA.SpwanLayerSelected = layers[i].layer;
            string json = JsonUtility.ToJson(Lobby.Lob.CA);
            PhotonNetwork.RaiseEvent(AssignPosToSpwan, json, RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendReliable);
        }
    }


    private void Start()
    {

        addListers();
        TotalCubeCanDeploy = Lobby.Lob.CA.Cube;
        TotalSphereCanDeploy = Lobby.Lob.CA.Sphere;
        TotalCylinderCanDeploy = Lobby.Lob.CA.Cylinder;
        CubeText.text = TotalCubeCanDeploy.ToString();
        SphereText.text = TotalSphereCanDeploy.ToString();
        CylinderText.text = TotalCylinderCanDeploy.ToString();
    }
    //buton Listener 
    public void addListers()
    {
        //Selecting Prefabs to Deploy
        Cubebtn.onClick.AddListener(() =>
        {
            ObjectToInstanitate = CubePrefabName;
        });
        Spherebtn.onClick.AddListener(() =>
        {
            ObjectToInstanitate = SpherePrefabName;
        });
        cylinderbtn.onClick.AddListener(() =>
        {
            ObjectToInstanitate = CylinderPrefabName;
        });
        //Done Deploying
        Deploybtn.onClick.AddListener(() =>
        {
            Deploy = true;
            Deploybtn.gameObject.SetActive(false);
            Panel.SetActive(true);
        });
        //For Moving
        ReadyForMove.onClick.AddListener(() =>
        {

            MoveCall();
        });
        //For Attacking
        ReadyForAttack.onClick.AddListener(() =>
        {
            Move = false;
            once = true;
            ReadyForAttack.transform.gameObject.SetActive(false);
            Lobby.Lob.CA.AttackingPhase = true;
            if (Lobby.Lob.CR.AttackingPhase)
            {
               
                if (PhotonNetwork.IsMasterClient)
                {
                    DecideAttackTurn();
                }
                else
                {
                   
                    string json = JsonUtility.ToJson(Lobby.Lob.CR);
                    PhotonNetwork.RaiseEvent(BeginAttackPhase, json, RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendReliable);
                }
            }
            else
            {

                Lobby.Lob.CR.AttackingPhase = true;
                string json = JsonUtility.ToJson(Lobby.Lob.CR);
                PhotonNetwork.RaiseEvent(BeginAttackPhase, json, RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendReliable);
            }
            for (int i = 0; i < Info.Count; i++)
            {
                Info[i].used = false;
            }
        });
        //Finish Attacking Back to Move Phase
        FinishAttack.onClick.AddListener(() =>
        {

            Lobby.Lob.CR.MyTurn = false;
            Lobby.Lob.CR.OppenentTurn = true;
            Lobby.Lob.CA.FinishedAttack = true;
            FinishAttack.gameObject.SetActive(false);
            string json = JsonUtility.ToJson(Lobby.Lob.CR);
            PhotonNetwork.RaiseEvent(AttackOppnonent, json, RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendReliable);
            Attack = false;

        });
    }
    private void Update()
    {
        //Can Deploy If in Deployment phase
        #region Deploy
        if (Deploy)
        {
            Ray ray = Cam.ScreenPointToRay(Input.mousePosition);
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(ray, out hit))
                {

                    spwanposition = hit.point;
                    if (hit.transform.gameObject.layer == SelectedLayer)
                    {
                        if (ObjectToInstanitate == CubePrefabName && TotalCubeCanDeploy > 0)
                        {
                            TotalCubeCanDeploy--;
                            CubeText.text = TotalCubeCanDeploy.ToString();
                            Deploying(spwanposition, ObjectToInstanitate);
                        }
                        if (ObjectToInstanitate == CylinderPrefabName && TotalCylinderCanDeploy > 0)
                        {
                            TotalCylinderCanDeploy--;
                            CylinderText.text = TotalCylinderCanDeploy.ToString();
                            Deploying(spwanposition, ObjectToInstanitate);
                        }
                        if (ObjectToInstanitate == SpherePrefabName && TotalSphereCanDeploy > 0)
                        {
                            TotalSphereCanDeploy--;
                            SphereText.text = TotalSphereCanDeploy.ToString();
                            Deploying(spwanposition, ObjectToInstanitate);
                        }
                    }
                }

            }
        }
        #endregion
        //Can Move If in Move phase
        #region Move
        if (Move)
        {
            Ray ray = Cam.ScreenPointToRay(Input.mousePosition);
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(ray, out hit))
                {

                    if (hit.transform.gameObject.layer == RangeLayerNo && Obj != null)
                    {
                        MovingToPoint(hit.point, Agent);
                        Obj.transform.GetChild(0).gameObject.SetActive(false);
                        return;
                    }
                    if (Lobby.Lob.CA.Challenger)
                    {
                        if (hit.transform.tag == TagForChallengerPlayers && Lobby.Lob.CA.ChallangerID == Lobby.Lob.userID)
                        {

                            if (Obj != null)
                            {
                                Obj.transform.GetChild(0).gameObject.SetActive(false);
                                Obj = null;
                            }
                            Obj = hit.transform.gameObject;
                            Agent = Obj.GetComponent<NavMeshAgent>();

                            Obj.transform.GetChild(0).gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        if (hit.transform.tag == TagForChallenedPlayers && Lobby.Lob.CR.ChallengedID == Lobby.Lob.userID)
                        {
                            if (Obj != null)
                            {
                                Obj.transform.GetChild(0).gameObject.SetActive(false);
                                Obj = null;
                            }
                            Obj = hit.transform.gameObject;
                            Agent = Obj.GetComponent<NavMeshAgent>();
                            Obj.transform.GetChild(0).gameObject.SetActive(true);
                        }
                    }

                }
            }
        }
        #endregion
        //Can Attack If in Move phase
        #region Attack
        if (Attack)
        {
            Ray ray = Cam.ScreenPointToRay(Input.mousePosition);
            if (Lobby.Lob.CR.MyTurn == true && Lobby.Lob.CR.OppenentTurn == false)
            {

                if (Input.GetMouseButtonDown(0))
                {
                    RaycastHit hit = new RaycastHit();
                    if (Physics.Raycast(ray, out hit))
                    {
                        if (Lobby.Lob.ShowDebugs)
                            Debug.Log(hit.transform.gameObject.name);
                        if (Lobby.Lob.CA.ChallangerID == Lobby.Lob.userID)
                        {

                            if (hit.transform.tag == TagForChallengerPlayers)
                            {
                                if (Obj != null)
                                {
                                    Obj = null;
                                }
                                Obj = hit.transform.gameObject;
                            }
                            if (Obj != null && hit.transform.tag == TagForChallenedPlayers)
                            {
                                for (int i = 0; i < Info.Count; i++)
                                {
                                    if (Obj.transform == Info[i]._Transform)
                                    {
                                        if (!Info[i].used)
                                        {
                                            Obj.transform.LookAt(hit.transform);
                                          
                                            Shoot(Obj.GetComponent<Health>().ShootPoint, RequestReciverLayerMask, Random.Range(50, 80), RequestSenderlayermask,hit.transform,Info[i].BulletName);
                                            Obj.GetComponent<AnimatorHandler>().AnimChangeCall(Anim.Attack);
                                            Info[i].used = true;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {

                            if (hit.transform.tag == TagForChallenedPlayers)
                            {
                                if (Obj != null)
                                {
                                    Obj = null;
                                }
                                Obj = hit.transform.gameObject;
                            }
                            if (Obj != null && hit.transform.tag == TagForChallengerPlayers)
                            {
                                for (int i = 0; i < Info.Count; i++)
                                {
                                    if (Obj.transform == Info[i]._Transform)
                                    {
                                        if (!Info[i].used)
                                        {
                                            Obj.transform.LookAt(hit.transform);
                                          
                                            Shoot(Obj.GetComponent<Health>().ShootPoint, RequestSenderlayermask, Random.Range(50, 80), RequestReciverLayerMask,hit.transform, Info[i].BulletName);
                                            Obj.GetComponent<AnimatorHandler>().AnimChangeCall(Anim.Attack);
                                            Info[i].used = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }

    public void MoveCall()
    {
        Lobby.Lob.CA.ReadyForMovePhase = true;
        Panel.SetActive(false);
        if (Lobby.Lob.CR.ReadyForMovePhase)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Deploy = false;
                Move = true;
                MovePanel.SetActive(true);
                if (Lobby.Lob.ShowDebugs)
                    Debug.Log("Master Ready For ATtack Phase");
                string json = JsonUtility.ToJson(Lobby.Lob.CA);
                PhotonNetwork.RaiseEvent(WaitingForMove, json, RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendReliable);
                StartCoroutine(StartMovingPhase());
            }
            else
            {
                MovePanel.SetActive(true);
                WaitingText.gameObject.SetActive(false);
                if (Lobby.Lob.ShowDebugs)
                    Debug.Log("Move To Attack Phase");
                string json = JsonUtility.ToJson(Lobby.Lob.CR);
                PhotonNetwork.RaiseEvent(WaitingForMove, json, RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendReliable);

            }
        }
        else
        {

            WaitingText.gameObject.SetActive(true);
            WaitingText.text = MeaasgeOfWaiting;
            ReadyForMove.gameObject.SetActive(false);
            string json = JsonUtility.ToJson(Lobby.Lob.CA);
            PhotonNetwork.RaiseEvent(WaitingForMove, json, RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendReliable);

        }
    }
    //Deploying Troops On given Layer points / spawn points
    public void Deploying(Vector3 _pos, string objName)
    {

        if (objName.Length > 0)
        {
            ReadyForMove.gameObject.SetActive(true);
            {
                GameObject Troop = PhotonNetwork.Instantiate("Players/" + ObjectToInstanitate, _pos, Quaternion.identity);
                Data_Info Datainit = new Data_Info();
                Datainit.Type = ObjectToInstanitate;
                Datainit._Transform = Troop.transform;
                Datainit.BulletName = WeaponController.instance.GetBulletType(ObjectToInstanitate);
                Datainit.Health = Troop.GetComponent<Health>().CurrentHealth;
                Info.Add(Datainit);
                if (Lobby.Lob.CA.Challenger)
                {
                    Troop.GetComponent<SpawnHandlingOverNetwork>().RPCCall(TagForChallengerPlayers, RequestSenderlayer);

                }
                else
                {
                    Troop.GetComponent<SpawnHandlingOverNetwork>().RPCCall(TagForChallenedPlayers, RequestReciverLayer);

                }
            }
        }
        else
        {
            if (Lobby.Lob.ShowDebugs)
            {
                Debug.Log("no objecct selected");
            }
        }
    }
    //Moving Troops To click Points
    public void MovingToPoint(Vector3 MovePosition, NavMeshAgent _Agent)
    {
        _Agent.SetDestination(MovePosition);
        Obj.GetComponent<AnimatorHandler>().AnimChangeCall(Anim.Move);
        Obj.GetComponent<AnimatorHandler>().MoveDest(MovePosition);
        ReadyForAttack.transform.gameObject.SetActive(true);
    }
    //Events Based On scenario
    private void NetworkingClient_EventReceived(ExitGames.Client.Photon.EventData obj)
    {
        switch (obj.Code)
        {
            case AssignPosToSpwan:
                Lobby.Lob.CR = JsonUtility.FromJson<ChallenegAttributes>((string)obj.CustomData);
                if (Lobby.Lob.CR.ChallengedID == Lobby.Lob.userID)
                {
                    for (int i = 0; i < layers.Length; i++)
                    {
                        if (layers[i].layer != Lobby.Lob.CR.SpwanLayerSelected)
                        {
                            SelectedLayer = layers[i].layer;
                            Lobby.Lob.CA.SpwanLayerSelected = SelectedLayer;
                            chosenPointForSpwan.text = layers[i].ColorName + MessageForDeployAera;
                            break;
                        }
                    }
                }
                break;
            case WaitingForMove:
                Lobby.Lob.CR = JsonUtility.FromJson<ChallenegAttributes>((string)obj.CustomData);
                if (Lobby.Lob.CR.ChallengedID == Lobby.Lob.userID || Lobby.Lob.CR.ChallangerID == Lobby.Lob.userID)
                {
                    if (Lobby.Lob.ShowDebugs)
                        Debug.Log("One Player is ready for Move Phase");
                    if (!Lobby.Lob.CA.ReadyForMovePhase)
                    {
                        WaitingText.gameObject.SetActive(true);
                        WaitingText.text = MeaasgeToHurryUp;
                    }
                    if (Lobby.Lob.CR.ReadyForMovePhase && Lobby.Lob.CA.ReadyForMovePhase)
                    {
                        if (Lobby.Lob.ShowDebugs)
                            Debug.Log("All Players Ready");
                        MovePanel.SetActive(true);
                        WaitingText.gameObject.SetActive(false);
                        StartCoroutine(StartMovingPhase());
                    }
                }
                break;
            case BeginMovePhase:
                Lobby.Lob.CR = JsonUtility.FromJson<ChallenegAttributes>((string)obj.CustomData);
                if (Lobby.Lob.CR.ChallengedID == Lobby.Lob.userID || Lobby.Lob.CR.ChallangerID == Lobby.Lob.userID)
                {
                    MOvePhase();
                }
                break;
            case BeginAttackPhase:
              
                Lobby.Lob.CR = JsonUtility.FromJson<ChallenegAttributes>((string)obj.CustomData);
                if (Lobby.Lob.CR.ChallengedID == Lobby.Lob.userID || Lobby.Lob.CR.ChallangerID == Lobby.Lob.userID)
                {
                   
                    if (Lobby.Lob.CR.AttackingPhase && !Lobby.Lob.CA.AttackingPhase)
                    {

                        WaitingText.gameObject.SetActive(true);
                        WaitingText.text = MeaasgeOfWaiting;
                    }
                    else
                    {
                      
                        if (PhotonNetwork.IsMasterClient)
                        {

                            DecideAttackTurn();
                        }
                    }
                }
                break;
            case AttackOppnonent:
                Lobby.Lob.CR = JsonUtility.FromJson<ChallenegAttributes>((string)obj.CustomData);
                if (Lobby.Lob.CR.ChallengedID == Lobby.Lob.userID || Lobby.Lob.CR.ChallangerID == Lobby.Lob.userID)
                {
                    if (!Lobby.Lob.CA.FinishedAttack)
                    {
                      
                        if (Lobby.Lob.CR.OppenentTurn)
                        {
                            Move = false;
                            WaitingText.gameObject.SetActive(true);
                            WaitingText.text = PlayerTurnMessage;
                            Lobby.Lob.CR.OppenentTurn = false;
                            Lobby.Lob.CR.MyTurn = true;
                            FinishAttack.gameObject.SetActive(true);
                            Attack = true;

                        }
                        else
                        {
                            WaitingText.gameObject.SetActive(true);
                            WaitingText.text = OppenetTurnMessage;
                        }
                    }
                    else
                    {
                       
                        if (once)
                        {

                            once = false;
                            MovePanel.SetActive(true);
                            WaitingText.gameObject.SetActive(false);
                            string json = JsonUtility.ToJson(Lobby.Lob.CR);
                            PhotonNetwork.RaiseEvent(ReadyAgain, json, RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendReliable);
                            MOvePhase();
                        }
                    }
                }
                break;
            case ReadyAgain:
                if (Lobby.Lob.CR.ChallengedID == Lobby.Lob.userID || Lobby.Lob.CR.ChallangerID == Lobby.Lob.userID)
                {
                    MOvePhase();
                }
                break;

        }
    }
    IEnumerator StartMovingPhase()
    {
        WaitingText.gameObject.SetActive(true);
        WaitingText.text = PlacingPlayerMessage;
        yield return new WaitForSeconds(1.5f);
        string json = JsonUtility.ToJson(Lobby.Lob.CA);
        PhotonNetwork.RaiseEvent(BeginMovePhase, json, RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendReliable);
        MOvePhase();
    }
    public void MOvePhase()
    {
        Lobby.Lob.CR.AttackingPhase = false;
        Lobby.Lob.CA.AttackingPhase = false;
        Lobby.Lob.CR.ReadyForMovePhase = false;
        Lobby.Lob.CA.ReadyForMovePhase = false;
        Lobby.Lob.CA.FinishedAttack = false;
        Lobby.Lob.CR.FinishedAttack = false;
        if (ReadyForMove.gameObject.activeInHierarchy)
            ReadyForMove.gameObject.SetActive(false);
        MovePanel.SetActive(false);
        for (int i = 0; i < Plane.transform.childCount; i++)
        {
            Plane.transform.GetChild(i).gameObject.SetActive(false);
        }
        Move = true;
        Deploy = false;
    }
    //Attck Turn
    public void DecideAttackTurn()
    {
        Obj = null;
        Lobby.Lob.CR.MyTurn = true;
        Lobby.Lob.CR.OppenentTurn = false;
        Attack = true;
        Move = false;
        FinishAttack.gameObject.SetActive(true);
        WaitingText.gameObject.SetActive(true);
        WaitingText.text = PlayerTurnMessage;
        string json = JsonUtility.ToJson(Lobby.Lob.CR);
        PhotonNetwork.RaiseEvent(AttackOppnonent, json, RaiseEventOptions.Default, ExitGames.Client.Photon.SendOptions.SendReliable);
    }
    //Bullet Spawning and initalizing value
    public void Shoot(Transform Shootpoint, LayerMask EnemyMask, float DamageAmount, LayerMask PlayersToIgnore,Transform lookPoint,string BulletName)
    {
        StartCoroutine(AnimWait(Shootpoint, EnemyMask, DamageAmount, PlayersToIgnore,lookPoint,BulletName));

    }
    IEnumerator AnimWait(Transform Shootpoint, LayerMask EnemyMask, float DamageAmount, LayerMask PlayersToIgnore,Transform lookpoint, string BulletName) 
    {
        yield return new WaitForSeconds(0.35f);
        GameObject Bullet = PhotonNetwork.Instantiate(BulletName, Shootpoint.position, Shootpoint.rotation);
        Bullet.transform.LookAt(lookpoint);
        Bullet.GetComponent<BulletAttributes>().SetValues(DamageAmount, EnemyMask, PlayersToIgnore);
    }
    //resetting Collision matrix
    private void OnDestroy()
    {
        Physics.IgnoreLayerCollision(BulletLayerNo, Mathf.RoundToInt(Mathf.Log(RequestSenderlayermask.value, 2)), false);
        Physics.IgnoreLayerCollision(BulletLayerNo, Mathf.RoundToInt(Mathf.Log(RequestReciverLayerMask.value, 2)), false);
    }
    public void UpdateListTroopsList(Transform Obj) 
    {
        foreach (Data_Info playerinfo in Info)
        {
            if (playerinfo._Transform == Obj)
            {
                Info.Remove(playerinfo);

                break;
            }
        }

        if (Info.Count == 0)
        {
            PlayHandler.Instance.OnLose();
        }
    }
}



