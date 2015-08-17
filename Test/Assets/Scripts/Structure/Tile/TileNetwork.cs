using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const.Structure;
using S2C = Communication.S2C;
using C2S = Communication.C2S;
using Architecture;

namespace Maps
{ 
    [RequireComponent(typeof(NetworkView))]
    public class TileNetwork : MonoBehaviour
    {
        static TileNetwork instance;

        public static TileNetwork Inst
        {
            get { return instance; }
        }

        void Awake()
        {
            instance = this;
        }

        public void BroadcastHealth(ushort ID, short health, DestroyReason reason)
        {
            S2C.SetStructureHealth pck = new S2C.SetStructureHealth(ID, health, reason);
            GetComponent<NetworkView>().RPC("RecvHealth", RPCMode.Others, pck.SerializeToBytes());
        }

        [RPC]
        public void RecvHealth(byte[] pckData, NetworkMessageInfo info)
        {
            //ServerCheck
            S2C.SetStructureHealth pck = new S2C.SetStructureHealth();
            pck.DeserializeFromBytes(pckData);
            TileManager.Inst.Get(pck.m_ID).RecvHealth(pck);
        }
    }
}