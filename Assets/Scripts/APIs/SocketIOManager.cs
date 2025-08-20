using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Newtonsoft.Json;
using Best.SocketIO;
using Best.SocketIO.Events;


public class SocketIOManager : MonoBehaviour
{
    [SerializeField]
    private GameManager gameManager;

    [SerializeField] private UIManager uiManager;
    [SerializeField] private JSFunctCalls JSManager;
    internal GameData InitialData = null;
    internal UiData UIData = null;
    internal Root ResultData = null;
    internal Player PlayerData = null;
    internal Root GambleData = null;
    internal Root bonusData = new();
    internal List<List<int>> LineData = null;
    internal List<int> BonusData = null;

    // internal GambleResult gambleData = null;
    // internal Message myMessage = null;
    internal double GambleLimit = 0;
    //[SerializeField] internal List<string> bonusdata = null;
    internal bool isResultdone = false;

    private SocketManager manager;
    private Socket gameSocket;

    [SerializeField] protected string TestSocketURI = "https://sl3l5zz3-5000.inc1.devtunnels.ms/";
    protected string SocketURI = null;
    //protected string SocketURI = "https://6f01c04j-5000.inc1.devtunnels.ms/";

    [SerializeField]
    private string testToken;

    protected string gameID = "KN-MN";
    // protected string gameID = "";
    private const int maxReconnectionAttempts = 6;
    private readonly TimeSpan reconnectionDelay = TimeSpan.FromSeconds(10);

    internal bool SetInit = false;

    protected string nameSpace = "playground"; //BackendChanges

    private bool isConnected = false; //Back2 Start
    private bool hasEverConnected = false;
    private const int MaxReconnectAttempts = 5;
    private const float ReconnectDelaySeconds = 2f;

    private float lastPongTime = 0f;
    private float pingInterval = 2f;
    private float pongTimeout = 3f;
    private bool waitingForPong = false;
    private int missedPongs = 0;
    private const int MaxMissedPongs = 5;
    private Coroutine PingRoutine; //Back2 end

    // protected string nameSpace = "game";
    private void Start()
    {
        // Debug.unityLogger.logEnabled = false;
        OpenSocket();
    }

    string myAuth = null;

    internal bool isLoaded = false;

    private void Awake()
    {
        isLoaded = false;
    }

    private void OpenSocket()
    {
        SocketOptions options = new SocketOptions(); //Back2 Start
        options.AutoConnect = false;
        options.Reconnection = false;
        options.Timeout = TimeSpan.FromSeconds(3); //Back2 end
        options.ConnectWith = Best.SocketIO.Transports.TransportTypes.WebSocket;

#if UNITY_WEBGL && !UNITY_EDITOR
            JSManager.SendCustomMessage("authToken");
            StartCoroutine(WaitForAuthToken(options));
#else
        Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
        {
            return new
            {
                token = testToken
            };
        };
        options.Auth = authFunction;
        // Proceed with connecting to the server
        SetupSocketManager(options);
#endif
    }

    private IEnumerator WaitForAuthToken(SocketOptions options)
    {
        // Wait until myAuth is not null
        while (myAuth == null)
        {
            Debug.Log("My Auth is null");
            yield return null;
        }
        while (SocketURI == null)
        {
            Debug.Log("My Socket is null");
            yield return null;
        }

        Debug.Log("My Auth is not null");
        // Once myAuth is set, configure the authFunction
        Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
        {
            return new
            {
                token = myAuth
            };
        };
        options.Auth = authFunction;

        Debug.Log("Auth function configured with token: " + myAuth);

        // Proceed with connecting to the server
        SetupSocketManager(options);
    }

    private void SetupSocketManager(SocketOptions options)
    {
#if UNITY_EDITOR
        // Create and setup SocketManager for Testing
        this.manager = new SocketManager(new Uri(TestSocketURI), options);
#else
        // Create and setup SocketManager
        this.manager = new SocketManager(new Uri(SocketURI), options);
#endif
        if (string.IsNullOrEmpty(nameSpace) | string.IsNullOrWhiteSpace(nameSpace))
        {
            gameSocket = this.manager.Socket;
        }
        else
        {
            Debug.Log("Namespace used :" + nameSpace);
            gameSocket = this.manager.GetSocket("/" + nameSpace);
        }
        // Set subscriptions
        gameSocket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
        gameSocket.On(SocketIOEventTypes.Disconnect, OnDisconnected); //Back2 Start
        gameSocket.On(SocketIOEventTypes.Error, OnError); //Back2 Start
        gameSocket.On<string>("game:init", OnListenEvent);
        gameSocket.On<string>("result", OnResult);
        //gameSocket.On<string>("gamble:result", OnGameResult);
        //gameSocket.On<string>("bonus:result", OnBonusResult);
        gameSocket.On<bool>("socketState", OnSocketState);
        gameSocket.On<string>("internalError", OnSocketError);
        gameSocket.On<string>("alert", OnSocketAlert);
        gameSocket.On<string>("pong", OnPongReceived); //Back2 Start
        gameSocket.On<string>("AnotherDevice", OnSocketOtherDevice); //BackendChanges Finish
        manager.Open();
    }

    void OnBonusResult(string data)
    {
        // Handle the game result here
        Debug.Log("Bonus Result: " + data);

        ParseResponse(data);

    }
    // Connected event handler implementation
    void OnConnected(ConnectResponse resp) //Back2 Start
    {
        Debug.Log("✅ Connected to server.");

        if (hasEverConnected)
        {
            // uiManager.CheckAndClosePopups();
        }

        isConnected = true;
        hasEverConnected = true;
        waitingForPong = false;
        missedPongs = 0;
        lastPongTime = Time.time;
        SendPing();
    } //Back2 end
    private void OnError()
    {
        Debug.LogError("Socket Error");
    }
    private void OnDisconnected() //Back2 Start
    {
        Debug.LogWarning("⚠️ Disconnected from server.");
        isConnected = false;
        ResetPingRoutine();
    } //Back2 end
    private void OnPongReceived(string data) //Back2 Start
    {
        Debug.Log("✅ Received pong from server.");
        waitingForPong = false;
        missedPongs = 0;
        lastPongTime = Time.time;
        Debug.Log($"⏱️ Updated last pong time: {lastPongTime}");
        Debug.Log($"📦 Pong payload: {data}");
    } //Back2 end

    private void OnError(string response)
    {
        Debug.LogError("Error: " + response);
    }

    private void OnListenEvent(string data)
    {
        Debug.Log("Received some_event with data: " + data);
        ParseResponse(data);
    }
    void OnResult(string data)
    {
        print(data);
        ParseResponse(data);
    }
    private void OnSocketState(bool state)
    {
        if (state)
        {
            Debug.Log("my state is " + state);
            //InitRequest("AUTH");
        }
    }

    void CloseGame()
    {
        Debug.Log("Unity: Closing Game");
        StartCoroutine(CloseSocket());
    }
    private void OnSocketError(string data)
    {
        Debug.Log("Received error with data: " + data);
    }
    private void OnSocketAlert(string data)
    {
        Debug.Log("Received alert with data: " + data);
    }

    private void OnSocketOtherDevice(string data)
    {
        Debug.Log("Received Device Error with data: " + data);
        //  uiManager.ADfunction();
    }

    private void SendPing() //Back2 Start
    {
        ResetPingRoutine();
        PingRoutine = StartCoroutine(PingCheck());
    }
    void ResetPingRoutine()
    {
        if (PingRoutine != null)
        {
            StopCoroutine(PingRoutine);
        }
        PingRoutine = null;
    }

    private void AliveRequest()
    {
        SendDataWithNamespace("YES I AM ALIVE");
    }
    private IEnumerator PingCheck()
    {
        while (true)
        {
            Debug.Log($"🟡 PingCheck | waitingForPong: {waitingForPong}, missedPongs: {missedPongs}, timeSinceLastPong: {Time.time - lastPongTime}");

            if (missedPongs == 0)
            {
                uiManager.CheckAndClosePopups();
            }

            // If waiting for pong, and timeout passed
            if (waitingForPong)
            {
                if (missedPongs == 2)
                {
                    uiManager.ReconnectionPopup();
                }
                missedPongs++;
                Debug.LogWarning($"⚠️ Pong missed #{missedPongs}/{MaxMissedPongs}");

                if (missedPongs >= MaxMissedPongs)
                {
                    Debug.LogError("❌ Unable to connect to server — 5 consecutive pongs missed.");
                    isConnected = false;
                    uiManager.DisconnectionPopup();
                    yield break;
                }
            }

            // Send next ping
            waitingForPong = true;
            lastPongTime = Time.time;
            Debug.Log("📤 Sending ping...");
            SendDataWithNamespace("ping");
            yield return new WaitForSeconds(pingInterval);
        }
    } //Back2 end

    private void SendDataWithNamespace(string eventName, string json = null)
    {
        // Send the message
        if (gameSocket != null && gameSocket.IsOpen)
        {
            if (json != null)
            {
                gameSocket.Emit(eventName, json);
                Debug.Log("JSON data sent: " + json);
            }
            else
            {
                gameSocket.Emit(eventName);
            }
        }
        else
        {
            Debug.LogWarning("Socket is not connected.");
        }
    }
    public void ExtractUrlAndToken(string fullUrl)
    {
        Uri uri = new Uri(fullUrl);
        string query = uri.Query; // Gets the query part, e.g., "?url=http://localhost:5000&token=e5ffa84216be4972a85fff1d266d36d0"

        Dictionary<string, string> queryParams = new Dictionary<string, string>();
        string[] pairs = query.TrimStart('?').Split('&');

        foreach (string pair in pairs)
        {
            string[] kv = pair.Split('=');
            if (kv.Length == 2)
            {
                queryParams[kv[0]] = Uri.UnescapeDataString(kv[1]);
            }
        }

        if (queryParams.TryGetValue("url", out string extractedUrl) &&
            queryParams.TryGetValue("token", out string token))
        {
            Debug.Log("Extracted URL: " + extractedUrl);
            Debug.Log("Extracted Token: " + token);
            testToken = token;
            SocketURI = extractedUrl;
        }
        else
        {
            Debug.LogError("URL or token not found in query parameters.");
        }
    }

    private void PopulateSlotSocket(List<string> LineIds)
    {
        //         slotManager.shuffleInitialMatrix();
        //         for (int i = 0; i < LineIds.Count; i++)
        //         {
        //             slotManager.FetchLines(LineIds[i], i);
        //         }

        //         slotManager.SetInitialUI();

        //         isLoaded = true;
        // #if UNITY_WEBGL && !UNITY_EDITOR
        //         JSManager.SendCustomMessage("OnEnter");
        // #endif
        //         uiManager.RaycastBlocker.SetActive(false);
    }

    internal IEnumerator CloseSocket() //Back2 Start
    {
        //uiManager.RaycastBlocker.SetActive(true);
        ResetPingRoutine();

        Debug.Log("Closing Socket");

        manager?.Close();
        manager = null;

        Debug.Log("Waiting for socket to close");

        yield return new WaitForSeconds(0.5f);

        Debug.Log("Socket Closed");

#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("OnExit"); //Telling the react platform user wants to quit and go back to homepage
#endif
    } //Back2 end

    internal void closeSocketReactnativeCall()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        JSManager.SendCustomMessage("OnExit");
#endif
    }
    private void CloseSocketMesssage(string eventName)
    {
        SendDataWithNamespace("EXIT");
    }

    private void ParseResponse(string jsonObject)
    {
        Debug.Log(jsonObject);
        Root myData = JsonConvert.DeserializeObject<Root>(jsonObject);

        string id = myData.id;

        switch (id)
        {
            case "initData":
                {
                    InitialData = myData.gameData;
                    UIData = myData.uiData;
                    PlayerData = myData.player;
                    uiManager.SetInitData();
#if UNITY_WEBGL && !UNITY_EDITOR
        JSManager.SendCustomMessage("OnEnter");
#endif
                    break;
                }
            case "ResultData":
                {

                    ResultData = myData;
                    PlayerData = myData.player;
                    isResultdone = true;
                    gameManager.OnResultSucess();
                    break;
                }


            case "ExitUser":
                {
                    gameSocket.Disconnect();
                    if (this.manager != null)
                    {
                        Debug.Log("Dispose my Socket");
                        this.manager.Close();
                    }
#if UNITY_WEBGL && !UNITY_EDITOR
                    JSManager.SendCustomMessage("OnExit");
#endif
                    break;
                }
        }
    }




    internal void AccumulateResult(string events)
    {
        isResultdone = false;
        MessageData message = new MessageData();
        message.payload = new SentDeta();
        message.type = "DRAW";
        message.payload.Event = events;
        message.payload.betIndex = uiManager.BetAmountDropDown.value;
        message.payload.mines = gameManager.CurrentBombNo;
        message.payload.picks = gameManager.selectedOptions;
        // Serialize message data to JSON
        string json = JsonUtility.ToJson(message);

        SendDataWithNamespace("request", json);
    }


}
public class GameData
{
    public int total { get; set; }
    public bool isSpecial { get; set; }
    public int draws { get; set; }
    public int maximumPicks { get; set; }
    public List<double> bets { get; set; }
    public List<List<double>> paytable { get; set; }
}

public class Player
{
    public double balance { get; set; }
}

public class Root
{
    public string id { get; set; }
    public GameData gameData { get; set; }
    public UiData uiData { get; set; }
    public Player player { get; set; }

    public Payload payload { get; set; }
    public bool success { get; set; }
}

public class UiData
{
}
[Serializable]
public class MessageData
{
    public string type;

    public SentDeta payload;

}
[Serializable]
public class SentDeta
{
    public int betIndex;
    public string Event;
    public List<int> picks;
    public int mines;


}
public class Payload
{
    public double currentWinning { get; set; }
    public double totalBet { get; set; }
    public List<int> mines { get; set; }
    public bool isMine { get; set; }
    public bool isCashOut { get; set; }
}
