using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server : MonoBehaviour
{
    private Socket server;
    private int port = 4000;
    private List<Socket> clients = new();
    private byte[] buffer = new byte[4096];
    private List<FoodState> foodList = new();
    private int foodCount = 40;
    private float mapSize = 10f;

    private List<BoostState> boostList = new();

    private int maxBoosts = 3;

    void Start()
    {
        server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        server.Bind(new IPEndPoint(IPAddress.Any, port));
        server.Listen(10);
        server.Blocking = false;

        Debug.Log("Server started on port " + port);

        for (int i = 0; i < foodCount; i++)
        {
            Vector3 position = new Vector3(
                UnityEngine.Random.Range(-mapSize, mapSize),
                UnityEngine.Random.Range(-mapSize, mapSize),
                0
            );

            foodList.Add(new FoodState
            {
                foodId = i,
                position = position,
                isActive = true
            });
        }

        for (int i = 0; i < maxBoosts; i++)
        {

            Vector2 position = new Vector2(UnityEngine.Random.Range(-mapSize, mapSize), UnityEngine.Random.Range(-mapSize, mapSize));

            boostList.Add(new BoostState
            {
                boostId = i,
                position = position,
                isActive = true

            });




        }
    }

    void Update()
    {
        try
        {
            Socket newClient = server.Accept();
            newClient.Blocking = false;
            clients.Add(newClient);
            Debug.Log("New client connected");

            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            foreach (var food in foodList)
            {
                if (food.isActive)
                {
                    var packet = new FoodSpawnPacket { foodId = food.foodId, position = food.position };
                    byte[] data = PacketHandler.Encode(packet);
                    writer.Write(data.Length);
                    writer.Write(data);
                }
            }

            foreach (var boost in boostList)
            {

                if (boost.isActive)
                {

                    var packet = new SpeedBoostSpawnPacket { boostId = boost.boostId, position = boost.position };

                    byte[] data = PacketHandler.Encode(packet);

                    writer.Write(data.Length);

                    writer.Write(data);


                }    




            }
                newClient.Send(stream.ToArray());
        }
        catch (SocketException) { }

        List<Socket> disconnected = new();

        foreach (Socket client in clients)
        {
            try
            {
                if (client.Available > 0)
                {
                    int bytes = client.Receive(buffer);
                    if (bytes == 0)
                    {
                        disconnected.Add(client);
                        continue;
                    }

                    int offset = 0;
                    while (offset < bytes)
                    {
                        int length = BitConverter.ToInt32(buffer, offset);
                        offset += 4;

                        BasePacket packet = PacketHandler.Decode(buffer, offset, length);
                        offset += length;

                        if (packet is FoodEatenPacket eaten)
                        {
                            var food = foodList.Find(f => f.foodId == eaten.foodId);
                            if (food != null && food.isActive)
                            {
                                food.isActive = false;
                                BroadcastToAllClients(new FoodEatenPacket(food.foodId));
                            }
                        }
                        else if (packet is BoostCollectedPacket collected)
                        {

                            var boost = boostList.Find(b => b.boostId == collected.boostId);

                            if (boost != null && boost.isActive)
                            {
                                boost.isActive = false;

                                BroadcastToAllClients(new BoostCollectedPacket(boost.boostId));

                            }

                        }
                        else if (packet != null)
                        {
                            BroadcastToAllClients(packet);
                        }
                    }
                }
                else if (client.Poll(0, SelectMode.SelectRead) && client.Available == 0)
                {
                    disconnected.Add(client);
                }
            }
            catch (SocketException)
            {
                disconnected.Add(client);
            }
        }

        foreach (Socket d in disconnected)
        {
            Debug.Log("Client disconnected");
            d.Close();
            clients.Remove(d);
        }
    }

    void BroadcastToAllClients(BasePacket packet)
    {
        byte[] body = PacketHandler.Encode(packet);
        byte[] full = new byte[4 + body.Length];
        Buffer.BlockCopy(BitConverter.GetBytes(body.Length), 0, full, 0, 4);
        Buffer.BlockCopy(body, 0, full, 4, body.Length);

        foreach (Socket client in clients)
        {
            if (client.Connected)
                client.Send(full);
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