using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const.Structure;
using S2C = Packet.S2C;
using C2S = Packet.C2S;
using Architecture;

namespace Maps
{
    [RequireComponent(typeof(NetworkView))]
    public class BuildingNetwork : MonoBehaviour
    {
        static BuildingNetwork instance;

        public static BuildingNetwork Inst
        {
            get { return instance; }
        }

        void Awake()
        {
            instance = this;
        }

        public void BroadcastHealth(int ID, int health, DestroyReason reason)
        {
            S2C.SetStructureHealth pck = new S2C.SetStructureHealth(ID, health, reason);
            networkView.RPC("RecvHealth", RPCMode.Others, pck.SerializeToBytes());
        }

        [RPC]
        public void RecvHealth(byte[] pckData, NetworkMessageInfo info)
        {
            //ServerCheck
            S2C.SetStructureHealth pck = S2C.SetStructureHealth.DeserializeFromBytes(pckData);
            BuildingManager.Inst.Get(pck.m_ID).RecvHealth(pck);
        }

        public void BroadcastFall(int ID)
        {
            networkView.RPC("RecvFall", RPCMode.Others, ID);
        }

        [RPC]
        public void RecvFall(int ID, NetworkMessageInfo info)
        {
            BuildingManager.Inst.Get(ID).Fall();
        }
    }
}