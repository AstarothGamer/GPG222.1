using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using ChatSystem;
using TMPro;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Client : MonoBehaviour
{
    [SerializeField] GameObject connectingPanel;
    public delegate void ConnectedToServer();
    public ConnectedToServer ConnectedToServerEvent;

    private TcpClient client;
    private NetworkStream stream;

    private readonly byte[] readChunk = new byte[4096];
    private readonly List<byte> inBuffer = new();

    private readonly Queue<byte[]> outbox = new();
    private int outOffset = 0;

    private readonly List<BasePacket> packetQueue = new();

    public static Client Instance { get; private set; }

    public int serverPort = 4000;
    private string ipAddress;
    public PlayerController player;
    public PlayerData pd;
    
    public TMP_InputField chatInputField;

    private readonly Dictionary<string, RemotePlayer> otherPlayers = new();
    private readonly Dictionary<int, FoodController> foodById = new();
    private readonly Dictionary<int, SpeedBoostController> boostsById = new();

    private const int MaxPacketSize = 1 << 20;

    private const float SendRate = 1f / 40f; 
    private float lastTransformSent;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (pd == null)
            {

                var pdGo = GameObject.Find("PlayerData") ?? new GameObject("PlayerData");

                pd = pdGo.GetComponent<PlayerData>() ?? pdGo.AddComponent<PlayerData>();

                DontDestroyOnLoad(pdGo);

            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        if (player == null)
            player = FindObjectOfType<PlayerController>();

        if (client != null && client.Connected && packetQueue.Count > 0)
        {
            try
            {
                foreach (var packet in packetQueue)
                {
                    var body = PacketHandler.Encode(packet);
                    var full = new byte[4 + body.Length];
                    Buffer.BlockCopy(BitConverter.GetBytes(body.Length), 0, full, 0, 4);
                    Buffer.BlockCopy(body, 0, full, 4, body.Length);
                    outbox.Enqueue(full);
                }
                packetQueue.Clear();
            }
            catch (Exception)
            {
                Disconnection();
            }
        }

        if (stream != null && client.Connected)
        {
            try
            {
                while (outbox.Count > 0)
                {
                    var msg = outbox.Peek();
                    int remaining = msg.Length - outOffset;

                    stream.Write(msg, outOffset, remaining); 
                    outOffset = 0;
                    outbox.Dequeue();
                }
            }
            catch (IOException)
            {
                Disconnection();
            }
            catch (ObjectDisposedException)
            {
                Disconnection();
            }
        }

        if (stream != null && client.Connected)
        {
            try
            {
                while (stream.DataAvailable)
                {
                    int bytesRead = stream.Read(readChunk, 0, readChunk.Length);
                    if (bytesRead <= 0) { Disconnection(); break; }
                    inBuffer.AddRange(new ArraySegment<byte>(readChunk, 0, bytesRead));
                    ProcessIncomingData();
                }
            }
            catch (IOException)
            {
                Disconnection();
            }
            catch (ObjectDisposedException)
            {
                Disconnection();
            }
        }

        if (client != null && client.Connected && player != null)
        {
            packetQueue.Add(new PlayerTransformPacket(player.playerID, player.transform, pd.playerColor));
            if (Time.time - lastTransformSent >= SendRate)
            {
                packetQueue.Add(new PlayerTransformPacket(player.playerID, player.transform, pd.playerColor));
                lastTransformSent = Time.time;
            }
        }
    }

    private void ProcessIncomingData()
    {
        while (true)
        {
            if (inBuffer.Count < 4) break;

            int length = BitConverter.ToInt32(inBuffer.GetRange(0, 4).ToArray(), 0);
            if (length <= 0 || length > MaxPacketSize) { Disconnection(); break; }
            if (inBuffer.Count < 4 + length) break;

            byte[] body = inBuffer.GetRange(4, length).ToArray();
            inBuffer.RemoveRange(0, 4 + length);

            var packet = PacketHandler.Decode(body, 0, body.Length);
            HandlePacket(packet);
        }
    }

    private void HandlePacket(BasePacket packet)
    {
        if (packet is FoodEatenPacket food)
        {
            if (foodById.TryGetValue(food.foodId, out var foodObj))
                foodObj.Consume();
        }
        else if (packet is PlayerTransformPacket pos)
        {
            if (pos.playerId != player?.playerID)
            {
                if (!otherPlayers.ContainsKey(pos.playerId))
                {
                    GameObject prefab = Resources.Load<GameObject>("RemotePlayer");
                    if (prefab == null)
                    {
                        Debug.LogError("RemotePlayer prefab not found!");
                        return;
                    }
                    GameObject instance = Instantiate(prefab);
                    RemotePlayer remote = instance.GetComponent<RemotePlayer>();
                    remote.playerID = pos.playerId;
                    remote.ApplyName(pos.playerName);
                    instance.name = pos.playerName;
                    otherPlayers[pos.playerId] = remote;
                }
                RemotePlayer remotePlayer = otherPlayers[pos.playerId];
                remotePlayer.SetPosition(pos.position);
                remotePlayer.SetScale(new Vector3(pos.scale, pos.scale, pos.scale));
                remotePlayer.SetColor(new Color(pos.colorR, pos.colorG, pos.colorB, pos.colorA));
            }
        }
        else if (packet is PlayerKilledPacket dead)
        {
            if (player != null && player.playerID == dead.playerId && dead.canDie)
            {
                player.Die();
            }
            else if (otherPlayers.TryGetValue(dead.playerId, out RemotePlayer rp) && dead.canDie)
            {
                Destroy(rp.gameObject);
                otherPlayers.Remove(dead.playerId);
            }
        }
        else if (packet is FoodSpawnPacket spawn)
        {
            if (foodById.ContainsKey(spawn.foodId))
            {
                var fsp = foodById[spawn.foodId];
                fsp.transform.position = spawn.position;
                fsp.gameObject.SetActive(true);
            }
            else
            {
                GameObject prefab = Resources.Load<GameObject>("Food");
                if (prefab == null) return;
                GameObject obj = Instantiate(prefab, spawn.position, Quaternion.identity);
                FoodController fsp = obj.GetComponent<FoodController>();
                fsp.foodID = spawn.foodId;
                foodById[spawn.foodId] = fsp;
            }
        }
        else if (packet is TextPacket textPacket)
        {
            Debug.Log("Received text packet: " + textPacket.message);
            TextBoxManager.Instance.UpdateText(textPacket.message);
        }


        else if (packet is SpeedBoostSpawnPacket boostSpawn)
        {

            if (boostsById.ContainsKey(boostSpawn.boostId))
            {

                var b = boostsById[boostSpawn.boostId];

                b.transform.position = boostSpawn.position;

                b.gameObject.SetActive(true);

            }
            else
            {
                GameObject prefab = Resources.Load<GameObject>("SpeedBoost");
                if (prefab == null) return;
                GameObject obj = Instantiate(prefab, boostSpawn.position, Quaternion.identity);
                SpeedBoostController controller = obj.GetComponent<SpeedBoostController>();
                controller.boostID = boostSpawn.boostId;
                boostsById[boostSpawn.boostId] = controller;
            }
        }
        else if (packet is BoostCollectedPacket boost)
        {
            player?.ApplySpeedBoost();
            if (boostsById.TryGetValue(boost.boostId, out var boostObj))
                boostObj.Collect();
        }
        else if (packet is GameStatePacket state)
        {
            print("Hello World!!!");
            if (state.CanJoin)
            {
                player.enabled = true;
                GameObject.Find("LobbyPanel")?.SetActive(false);
            }
            TextMeshProUGUI playerCountText = GameObject.Find("PlayerCountText")?.GetComponent<TextMeshProUGUI>();
            if (playerCountText != null && state.MaxPlayers > 0)
            {
                playerCountText.text = $"Players Joined: {state.NumberOfPlayers}/{state.MaxPlayers}";
            }
            TextMeshProUGUI statusText = GameObject.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
            if (statusText != null && state.Timer != 0f)
            {
                statusText.text = ((int)state.Timer).ToString();
            }
        }
        else
        {
            Debug.LogWarning("Unknown packet type received: " + packet.GetType());
        }
        
    }
    public void AddChat()
    {
        if (chatInputField == null)
        {
            chatInputField = GameObject.Find("ChatInputField")?.GetComponent<TMP_InputField>();
            if (chatInputField == null)
            {
                Debug.LogError("Chat input field not found in the scene");
                return;
            }
        }
        string message = chatInputField.text.Trim();
        chatInputField.text = string.Empty;
        if (string.IsNullOrEmpty(message))
            return;
        string shortID = pd.playerID.Length > 4 ? pd.playerID.Substring(pd.playerID.Length - 4) : pd.playerID;
        string text = $"{pd.playerName}#{shortID}: {message}";
        TextBoxManager.Instance.UpdateText(text);
        packetQueue.Add(new TextPacket(text));
    }
    public async void ConnectToServer(string ip, string playerName, Color color)
    {
        ipAddress = ip;
        pd.playerColor = color;
        var cts = new CancellationTokenSource(5000);
        var flag = true;
        connectingPanel?.SetActive(true);
        StartCoroutine(WaitForConnectionEnd());
        try
        {
            client = new TcpClient();
            client.NoDelay = true;
            var connectTask = client.ConnectAsync(ipAddress, serverPort);


            var completedTask = await Task.WhenAny(connectTask, Task.Delay(5000, cts.Token));
            if (completedTask != connectTask)
            {
                client.Close();
                Debug.Log("Connection timed out.");
                client.Close();
                client = null;
                flag = false;
                return;
            }
            
            stream = client.GetStream();

            if (pd == null)
            {

                var pdGo = GameObject.Find("PlayerData") ?? new GameObject("PlayerData");

                pd = pdGo.GetComponent<PlayerData>() ?? pdGo.AddComponent<PlayerData>();

                DontDestroyOnLoad(pdGo);

            }


            pd.playerID = Guid.NewGuid().ToString();

            pd.playerName = playerName;

            ConnectedToServerEvent?.Invoke();
            Debug.Log("Connected to server");
        }
        catch (Exception e)
        {
            Debug.Log("Error connecting to server: " + e.Message);
        }
        finally
        {
            if (flag)
                StartCoroutine(EndOfConnectionAction());
            cts.Dispose();
        }
    }
    private IEnumerator WaitForConnectionEnd()
    {
        var txt = connectingPanel.GetComponentInChildren<TextMeshProUGUI>();
        
        txt.text = "Connecting...";
        yield return new WaitForSeconds(0.5f);
        txt.text = "Connecting";
        yield return new WaitForSeconds(0.5f);
        txt.text = "Connecting.";
        yield return new WaitForSeconds(0.5f);
        txt.text = "Connecting..";
        yield return new WaitForSeconds(0.5f);
        txt.text = "Connecting...";
        yield return new WaitForSeconds(0.5f);
        txt.text = "Connecting";
        yield return new WaitForSeconds(0.5f);
        txt.text = "Connecting.";
        yield return new WaitForSeconds(0.5f);
        txt.text = "Connecting..";
        yield return new WaitForSeconds(0.5f);
        txt.text = "Connecting...";
        yield return new WaitForSeconds(0.5f);
        txt.text = "Connecting";
    }
    private IEnumerator EndOfConnectionAction()
    {
        var txt = connectingPanel.GetComponentInChildren<TextMeshProUGUI>();
        txt.text = "Connection Timed out";
        yield return new WaitForSeconds(1f);
        txt.text = "Connecting...";
        try
        {
            connectingPanel?.SetActive(false);
        }
        catch (Exception e)
        {
            Debug.LogError("Error disabling connecting panel: " + e.Message);
        }
    }
    public void RegisterFood(FoodController food)
    {
        if (!foodById.ContainsKey(food.foodID))
            foodById.Add(food.foodID, food);
    }

    public void NotifyFoodEaten(int foodId) => packetQueue.Add(new FoodEatenPacket(foodId));
    public void NotifyBoostCollected(int boostId) => packetQueue.Add(new BoostCollectedPacket(boostId));
    public void NotifyPlayerShouldDie(string playerId, bool canDie) => packetQueue.Add(new PlayerKilledPacket { playerId = playerId, canDie = canDie });

    public void ClearClientState()
    {
        packetQueue.Clear();
        inBuffer.Clear();
        outbox.Clear();
        outOffset = 0;
        otherPlayers.Clear();
        foodById.Clear();
        boostsById.Clear();
        player.playerID = null;
        player = null;
        pd.playerID = null;
    }

    public void CleanupConnection()
    {
        try { stream?.Close(); } catch { }
        try { client?.Close(); } catch { }
        stream = null;
        client = null;
    }

    public void Disconnection()
    {
        if (client == null) return;
        try
        {
            CleanupConnection();
            ClearClientState();
        }
        catch (Exception e)
        {
            Debug.LogError("Error on disconection from server " + e.Message);
            CleanupConnection();
            ClearClientState();
        }
    }

    void OnDestroy() => Disconnection();
    void OnApplicationQuit() => Disconnection();
}