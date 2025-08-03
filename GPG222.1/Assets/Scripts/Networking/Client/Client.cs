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
    RemotePlayer rp;
    private byte[] receiveBuffer = new byte[4096];
    private MemoryStream incomingData = new MemoryStream();

    private List<BasePacket> packetQueue = new();

    public static Client Instance { get; private set; }

    public int serverPort = 4000;
    private string ipAddress;
    public PlayerController player;
    public PlayerData pd;

    private Dictionary<string, RemotePlayer> otherPlayers = new();
    private Dictionary<int, FoodController> foodById = new();

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
            foreach (var packet in packetQueue)
            {
                byte[] data = PacketHandler.Encode(packet);
                stream.Write(BitConverter.GetBytes(data.Length), 0, 4);
                stream.Write(data, 0, data.Length);
            }
            packetQueue.Clear();
        }

        if (stream != null && stream.DataAvailable)
        {
            int bytesRead = stream.Read(receiveBuffer, 0, receiveBuffer.Length);
            incomingData.Write(receiveBuffer, 0, bytesRead);
            ProcessIncomingData();
        }

        if (client != null && client.Connected && player != null)
        {
            packetQueue.Add(new PlayerTransformPacket(player.playerID, player.transform));
        }
    }

    void ProcessIncomingData()
    {
        incomingData.Position = 0;

        while (incomingData.Length - incomingData.Position >= 4)
        {
            long startPos = incomingData.Position;

            int length = BitConverter.ToInt32(incomingData.GetBuffer(), (int)startPos);

            if (incomingData.Length - startPos < 4 + length)
            {
                break;
            }

            incomingData.Position += 4;

            BasePacket packet = PacketHandler.Decode(incomingData.GetBuffer(), (int)incomingData.Position, length);
            incomingData.Position += length;

            HandlePacket(packet);
        }

        long remaining = incomingData.Length - incomingData.Position;
        if (remaining > 0)
        {
            byte[] leftover = new byte[remaining];
            Array.Copy(incomingData.GetBuffer(), incomingData.Position, leftover, 0, remaining);
            incomingData.SetLength(0);
            incomingData.Position = 0;
            incomingData.Write(leftover, 0, leftover.Length);
        }
        else
        {
            incomingData.SetLength(0);
            incomingData.Position = 0;
        }
    }

    void HandlePacket(BasePacket packet)
    {
        if (packet is FoodEatenPacket food)
        {
            if (foodById.TryGetValue(food.foodId, out var foodObj))
                foodObj.Consume();
        }
        else if (packet is PlayerTransformPacket pos)
        {
            if (pos.playerId != player.playerID)
            {
                if (!otherPlayers.ContainsKey(pos.playerId))
                {
                    var prefab = Resources.Load<GameObject>("RemotePlayer");
                    var instance = Instantiate(prefab);
                    var remote = instance.GetComponent<RemotePlayer>();
                    remote.playerID = pos.playerId;
                    instance.name = pos.playerName;
                    otherPlayers[pos.playerId] = remote;
                }

                var remotePlayer = otherPlayers[pos.playerId];
                remotePlayer.SetPosition(pos.position);
                remotePlayer.SetScale(pos.scale);
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
                var prefab = Resources.Load<GameObject>("Food");
                var obj = Instantiate(prefab, spawn.position, Quaternion.identity);
                var fsp = obj.GetComponent<FoodController>();
                fsp.foodID = spawn.foodId;
                foodById[spawn.foodId] = fsp;
            }
        }
        else if (packet is PlayerKilledPacket dead)
        {
            Debug.LogError("Packet came Killer came");
            if (player != null && player.playerID == dead.playerId && dead.canDie)
            {
                Debug.Log("I should die");
                player.Die();
            }
            else if (otherPlayers.TryGetValue(dead.playerId, out RemotePlayer remote) && dead.canDie)
            {
                Debug.Log("Something should be destroied");
                Destroy(remote.gameObject);
                otherPlayers.Remove(dead.playerId);
            }
        }
    }

    public void ConnectToServer(string ip, string playerName)
    {
        this.name = playerName;
        this.ipAddress = ip;

        try
        {
            client = new TcpClient();
            client.Connect(ipAddress, serverPort);
            stream = client.GetStream();

            pd.playerID = Guid.NewGuid().ToString();;
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

    public void NotifyFoodEaten(int foodId)
    {
        packetQueue.Add(new FoodEatenPacket(foodId));
    }

    public void NotifyPlayerShouldDie(string playerId, bool canDie)
    {
        PlayerKilledPacket packet = new PlayerKilledPacket { playerId = playerId, canDie = canDie };
        packetQueue.Add(packet); 
    }


    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }
}