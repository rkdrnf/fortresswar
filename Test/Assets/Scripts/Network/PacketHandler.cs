using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

using S2C = Communication.S2C;
using C2S = Communication.C2S;
using UnityEngine;

namespace Communication
{
    public class PacketHandler
    {
        public static void RegisterClientPacketHandlers(NetworkClient client)
        {
            client.RegisterHandler((short)PacketType.SetPlayerID, PHSetPlayerID);
            client.RegisterHandler((short)PacketType.SendMapInfo, PHReceiveMap);
            client.RegisterHandler((short)PacketType.PlayerNotReady, PHPlayerNotReady);
            client.RegisterHandler((short)PacketType.BroadcastPlayerName, PHBroadcastPlayerName);
            client.RegisterHandler((short)PacketType.BroadcastPlayerTeam, PHBroadcastPlayerTeam);
            client.RegisterHandler((short)PacketType.PlayerPrepared, PHPlayerPrepared);

            Debug.Log("Client Packet Handlers registered");
        }

        public static void RegisterServerPacketHandlers()
        {
            NetworkServer.RegisterHandler((short)PacketType.AddNewPlayer, PHAddNewPlayer);
            NetworkServer.RegisterHandler((short)PacketType.UpdatePlayerName, PHUpdatePlayerName);
            NetworkServer.RegisterHandler((short)PacketType.UpdatePlayerTeam, PHUpdatePlayerTeam);
            NetworkServer.RegisterHandler((short)PacketType.RequestMapInfo, PHRequestMapInfo);

            Debug.Log("Server Packet Handlers registered");
        }

        public static bool ProcessMessage(MessageBase msg)
        {
            bool result = (bool)typeof(PacketHandler).GetMethod("Process", new Type[] { msg.GetType() }).Invoke(null, new object[] { 42, "Hello" });
// call without arguments)

            return result;
        }

        //Client Packet Handler

        public static void PHSetPlayerID(NetworkMessage msg)
        {
            var packet = msg.ReadMessage<IntegerMessage>();
            ServerGame.Inst.SetPlayerID(packet.value);
        }

        public static void PHReceiveMap(NetworkMessage msg)
        {
            var packet = msg.ReadMessage<S2C.MapInfo>();
            ServerGame.Inst.CurrentMap.ReceiveMapInfo(packet);
        }

        public static void PHPlayerNotReady(NetworkMessage msg)
        {
            var packet = msg.ReadMessage<S2C.PlayerNotReady>();
            ServerGame.Inst.PreparePlayer(packet);
        }

        public static void PHBroadcastPlayerName(NetworkMessage msg)
        {
            var packet = msg.ReadMessage<S2C.BroadcastPlayerName>();
            ServerGame.Inst.ReceivePlayerName(packet);
        }

        public static void PHBroadcastPlayerTeam(NetworkMessage msg)
        {
            var packet = msg.ReadMessage<S2C.BroadcastPlayerTeam>();
            ServerGame.Inst.ReceivePlayerTeam(packet);
        }

        public static void PHPlayerPrepared(NetworkMessage msg)
        {
            var packet = msg.ReadMessage<S2C.PlayerPrepared>();
            ServerGame.Inst.AddPlayerRequest();
        }

        //Server Packet Handler
        public static void PHAddNewPlayer(NetworkMessage msg)
        {
            var packet = msg.ReadMessage<C2S.AddNewPlayer>();
            ServerGame.Inst.OnNewPlayerJoin(msg.conn);
        }
        
        public static void PHUpdatePlayerName(NetworkMessage msg)
        {
            var packet = msg.ReadMessage<C2S.UpdatePlayerName>();
            ServerGame.Inst.UpdatePlayerNameRequest(packet);
        }

        public static void PHUpdatePlayerTeam(NetworkMessage msg)
        {
            var packet = msg.ReadMessage<C2S.UpdatePlayerTeam>();
            ServerGame.Inst.UpdatePlayerTeamRequest(packet);
        }

        public static void PHRequestMapInfo(NetworkMessage msg)
        {
            var packet = msg.ReadMessage<C2S.RequestMapInfo>();
            ServerGame.Inst.CurrentMap.SendCurrentMapData(msg.conn);
        }

        
        
    }
}
