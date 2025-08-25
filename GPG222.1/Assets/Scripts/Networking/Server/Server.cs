using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Collections;

public class Server : MonoBehaviour
{
    private Socket server;
    private int port = 4000;

    private readonly List<ClientState> clients = new();

    private readonly List<FoodState> foodList = new();
    private readonly List<BoostState> boostList = new();

    [SerializeField] private int foodCount = 40;
    [SerializeField] private int maxBoosts = 3;
    [SerializeField] private float mapSize = 10f;
    [SerializeField] private int maxPlayers = 5;
    
    private float countdownToStart = 10f;
    private bool gameStarted = false;

    private const int MaxPacketSize = 1 << 20;

    void Start()
    {
        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        server.Bind(new IPEndPoint(IPAddress.Any, port));
        server.Listen(64);
        server.Blocking = false;
        Debug.Log("Server started on port " + port);

        for (int i = 0; i < foodCount; i++)
        {
            Vector2 position = new Vector2(
                UnityEngine.Random.Range(-mapSize, mapSize),
                UnityEngine.Random.Range(-mapSize, mapSize)
            );
            foodList.Add(new FoodState { foodId = i, position = position, isActive = true });
        }

        for (int i = 0; i < maxBoosts; i++)
        {
            Vector2 position = new Vector2(
                UnityEngine.Random.Range(-mapSize, mapSize),
                UnityEngine.Random.Range(-mapSize, mapSize)
            );
            boostList.Add(new BoostState { boostId = i, position = position, isActive = true });
        }
    }

    void Update()
    {
        AcceptNewClients();
        ReceiveFromClients();
        FlushOutgoing();
        CleanupDisconnected();
        StartGame();
        
        if (gameStarted && clients.Count == 0)
        {
            server.Close();
            GetComponent<RestartServer>().Restart();
        }
    }

    private void StartGame()
    {
        if ((clients.Count >= maxPlayers || countdownToStart <= 0f) && !gameStarted)
        {
            SendStartGame();
        }

        if (!gameStarted)
        {
            BroadcastToAllClients(new GameStatePacket {Timer = countdownToStart});
            if (clients.Count >= 2)
            {
                countdownToStart -= Time.deltaTime;
                if (countdownToStart <= 0f)
                {
                    SendStartGame();
                }
            }
            else
            {
                countdownToStart = 10f;
            }
        }
        BroadcastToAllClients(new GameStatePacket {NumberOfPlayers = clients.Count, MaxPlayers = maxPlayers} );
    }

    private void SendStartGame()
    {
        gameStarted = true;
        Debug.Log("Game Started with " + clients.Count + " players.");
        BroadcastToAllClients(new GameStatePacket { CanJoin = true });
    }

    private void AcceptNewClients()
    {
        while (true)
        {
            try
            {
                Socket newClient = server.Accept();
                if (newClient == null) return;
                newClient.Blocking = false;
                newClient.NoDelay = true;

                var state = new ClientState { socket = newClient };
                clients.Add(state);
                Debug.Log("New client connected");

                using var ms = new MemoryStream();
                using var bw = new BinaryWriter(ms);
                
                foreach (var food in foodList)
                {
                    if (!food.isActive) continue;
                    var p = new FoodSpawnPacket { foodId = food.foodId, position = food.position };
                    EnqueuePacketRaw(bw, p);
                }
                foreach (var boost in boostList)
                {
                    if (!boost.isActive) continue;
                    var p = new SpeedBoostSpawnPacket { boostId = boost.boostId, position = boost.position };
                    EnqueuePacketRaw(bw, p);
                }
                state.outbox.Enqueue(ms.ToArray());
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.WouldBlock || ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                    return;
                Debug.LogWarning("Accept error: " + ex.SocketErrorCode);
                return;
            }
        }
    }

    private void EnqueuePacketRaw(BinaryWriter bw, BasePacket packet)
    {
        var body = PacketHandler.Encode(packet);
        bw.Write(body.Length);
        bw.Write(body);
    }

    private void ReceiveFromClients()
    {
        foreach (var c in clients)
        {
            if (!c.socket.Connected) { c.markDisconnected = true; continue; }

            if (c.socket.Poll(0, SelectMode.SelectRead) && c.socket.Available == 0)
            {
                c.markDisconnected = true;
                continue;
            }

            try
            {
                byte[] temp = new byte[4096];
                while (c.socket.Available > 0)
                {
                    int toRead = Math.Min(temp.Length, c.socket.Available);
                    int read = c.socket.Receive(temp, 0, toRead, SocketFlags.None);
                    if (read <= 0) { c.markDisconnected = true; break; }
                    c.inBuffer.AddRange(new ArraySegment<byte>(temp, 0, read));
                }

                ParseIncoming(c);
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode is SocketError.WouldBlock or SocketError.IOPending or SocketError.NoBufferSpaceAvailable)
                    continue;
                c.markDisconnected = true;
            }
            catch (Exception)
            {
                c.markDisconnected = true;
            }
        }
    }

    private void ParseIncoming(ClientState c)
    {
        byte[] header = new byte[4];

        while (true)
        {
            if (c.inBuffer.Count < 4) break;

            c.inBuffer.CopyTo(0, header, 0, 4);
            int length = BitConverter.ToInt32(header, 0);

            if (length <= 0 || length > MaxPacketSize)
            {
                c.markDisconnected = true;
                break;
            }

            if (c.inBuffer.Count < 4 + length) break;

            byte[] packetData = new byte[length];
            c.inBuffer.CopyTo(4, packetData, 0, length);

            c.inBuffer.RemoveRange(0, 4 + length);

            var packet = PacketHandler.Decode(packetData, 0, packetData.Length);
            HandleServerPacket(c, packet);
        }
    }

    private void HandleServerPacket(ClientState sender, BasePacket packet)
    {
        switch (packet)
        {
            case FoodEatenPacket eaten:
                {
                    var food = foodList.Find(f => f.foodId == eaten.foodId);
                    if (food != null && food.isActive)
                    {
                        food.isActive = false;
                        StartCoroutine(Respawn(food));
                        BroadcastToAllClients(packet);
                    }
                    break;
                }
            case BoostCollectedPacket collected:
                {
                    var boost = boostList.Find(b => b.boostId == collected.boostId);
                    if (boost != null && boost.isActive)
                    {
                        boost.isActive = false;
                        BroadcastToAllClients(packet);
                    }
                    break;
                }
            case PlayerTransformPacket pt:
                {
                    if (string.IsNullOrEmpty(sender.playerId))
                        sender.playerId = pt.playerId;

                    BroadcastToAllClients(packet, excludeId: pt.playerId);
                    break;
                }
            case TextPacket textPacket:
                { 
                    Debug.LogError("Received text: " + textPacket.message);
                    BroadcastToAllClients(textPacket /*, new(){client}*/); // Remove reference to undefined 'client'
                    break;
                }
            default:
                BroadcastToAllClients(packet);
                break;
        }
    }

    private void BroadcastToAllClients(BasePacket packet, string excludeId = null)
    {
        var body = PacketHandler.Encode(packet);
        var full = new byte[4 + body.Length];
        Buffer.BlockCopy(BitConverter.GetBytes(body.Length), 0, full, 0, 4);
        Buffer.BlockCopy(body, 0, full, 4, body.Length);

        foreach (var c in clients)
        {
            if (!c.socket.Connected) { c.markDisconnected = true; continue; }
            if (excludeId != null && c.playerId == excludeId) continue;
            c.outbox.Enqueue(full);
        }
    }

    private void FlushOutgoing()
    {
        foreach (var c in clients)
        {
            if (!c.socket.Connected) { c.markDisconnected = true; continue; }

            while (c.outbox.Count > 0)
            {
                var msg = c.outbox.Peek();
                try
                {
                    int remaining = msg.Length - c.outOffset;
                    int sent = c.socket.Send(msg, c.outOffset, remaining, SocketFlags.None);
                    if (sent <= 0) { c.markDisconnected = true; break; }
                    c.outOffset += sent;

                    if (c.outOffset >= msg.Length)
                    {
                        c.outbox.Dequeue();
                        c.outOffset = 0;
                    }
                    else
                    {
                        break;
                    }
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode is SocketError.WouldBlock or SocketError.IOPending or SocketError.NoBufferSpaceAvailable)
                        break;
                    c.markDisconnected = true;
                    break;
                }
                catch
                {
                    c.markDisconnected = true;
                    break;
                }
            }
        }
    }

    private void CleanupDisconnected()
    {
        for (int i = clients.Count - 1; i >= 0; i--)
        {
            var c = clients[i];
            if (!c.markDisconnected) continue;
            try { c.socket?.Close(); } catch { }
            clients.RemoveAt(i);
            Debug.Log("Client disconnected");
        }
    }

    private IEnumerator Respawn(FoodState food)
    {
        yield return new WaitForSeconds(5f);
        Vector2 randomPos = new Vector2(
            UnityEngine.Random.Range(-mapSize, mapSize),
            UnityEngine.Random.Range(-mapSize, mapSize));

        food.position = randomPos;
        food.isActive = true;
        
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        var p = new FoodSpawnPacket { foodId = food.foodId, position = food.position };
        EnqueuePacketRaw(bw, p);

        foreach(var state in clients)
        {
            state.outbox.Enqueue(ms.ToArray());
        }
    }
}

public class FoodState
{
    public int foodId;
    public Vector2 position;
    public bool isActive;
}

public class BoostState
{
    public int boostId;
    public Vector2 position;
    public bool isActive;
}

public class ClientState
{
    public Socket socket;
    public string playerId;                  
    public List<byte> inBuffer = new();      
    public Queue<byte[]> outbox = new();      
    public int outOffset = 0;                 
    public bool markDisconnected = false;
}
