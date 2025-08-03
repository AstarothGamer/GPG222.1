using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using ChatSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    public delegate void ConnectedToServer();
    public ConnectedToServer ConnectedToServerEvent;

    private TcpClient client;
    private NetworkStream stream;
    private byte[] receiveBuffer = new byte[4096];
    private MemoryStream incomingData = new MemoryStream();

    private List<BasePacket> packetQueue = new();

    public static Client Instance { get; private set; }

    public int serverPort = 4000;
    private string ipAddress;
    public PlayerController player;
    public PlayerData pd;
    
    public TMP_InputField chatInputField;

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
        }
    }

    void Update()
    {
        if (player == null)
        {
            player = FindObjectOfType<PlayerController>();
        }
        else
        {
            Debug.Log("Player found: " + player.playerName);
        }

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
            packetQueue.Add(new PlayerTransformPacket(player.playerID, player.transform, pd.playerColor));
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
            if (player != null && pos.playerName != player.playerName)
            {
                if (!otherPlayers.ContainsKey(pos.playerName))
                {
                    var prefab = Resources.Load<GameObject>("RemotePlayer");
                    var instance = Instantiate(prefab);
                    var remote = instance.GetComponent<RemotePlayer>();
                    remote.playerID = pos.playerId;
                    instance.name = pos.playerName;
                    otherPlayers[pos.playerName] = remote;
                }

                var remotePlayer = otherPlayers[pos.playerName];
                remotePlayer.SetColor(new Color(pos.colorR, pos.colorG, pos.colorB, pos.colorA));
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
        else if (packet is TextPacket textPacket)
        {
            Debug.Log("Received text packet: " + textPacket.message);
            TextBoxManager.Instance.UpdateText(textPacket.message);
        }
        else
        {
            Debug.LogWarning("Unknown packet type received: " + packet.GetType());
        }
    }

    public void ConnectToServer(string ip, string playerName, Color color)
    {
        this.name = playerName;
        this.ipAddress = ip;

        try
        {
            client = new TcpClient();
            client.Connect(ipAddress, serverPort);
            stream = client.GetStream();

            pd.playerID = SystemInfo.deviceUniqueIdentifier;
            pd.playerName = playerName;
            pd.playerColor = color;
            
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

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }
}