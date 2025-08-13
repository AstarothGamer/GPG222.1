using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
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
            if (Time.time - lastTransformSent >= SendRate)
            {
                packetQueue.Add(new PlayerTransformPacket(player.playerID, player.transform));
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
                    instance.name = pos.playerName;
                    otherPlayers[pos.playerId] = remote;
                }

                RemotePlayer remotePlayer = otherPlayers[pos.playerId];
                remotePlayer.SetPosition(pos.position);
                remotePlayer.SetScale(new Vector3(pos.scale, pos.scale, pos.scale));
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
    }

    public void ConnectToServer(string ip, string playerName)
    {
        ipAddress = ip;
        try
        {
            client = new TcpClient();
            client.NoDelay = true;
            client.Connect(ipAddress, serverPort);
            stream = client.GetStream();

            pd.playerID = Guid.NewGuid().ToString();
            pd.playerName = playerName;

            ConnectedToServerEvent?.Invoke();
            Debug.Log("Connected to server");
        }
        catch (Exception e)
        {
            Debug.Log("Error connecting to server: " + e.Message);
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
        catch
        {
            CleanupConnection();
            ClearClientState();
        }
    }

    void OnDestroy() => Disconnection();
    void OnApplicationQuit() => Disconnection();
}