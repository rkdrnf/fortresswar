using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Const.Structure;
using S2C = Communication.S2C;
using C2S = Communication.C2S;
using Architecture;
using System.Collections;

namespace Architecture
{
    [RequireComponent(typeof(NetworkView))]
    public class BuildingNetwork : MonoBehaviour
    {
        static BuildingNetwork instance;

        List<int> m_fallingBuildings = new List<int>();

        public static BuildingNetwork Inst
        {
            get { return instance; }
        }

        void Awake()
        {
            instance = this;
        }

        void Update()
        {
            if (!Network.isServer) return;

            if (m_fallingBuildings.Count > 0)
            {
                S2C.BuildingFall pck = new S2C.BuildingFall(m_fallingBuildings);
                GetComponent<NetworkView>().RPC("RecvFall", RPCMode.Others, pck.SerializeToBytes());
                m_fallingBuildings.Clear();
            }
        }

        public void BroadcastBuild(Building building)
        {
            S2C.BuildingStatus pck = new S2C.BuildingStatus(building);
            GetComponent<NetworkView>().RPC("RecvBuild", RPCMode.Others, pck.SerializeToBytes());
        }

        [RPC]
        void RecvBuild(byte[] pckData, NetworkMessageInfo info)
        {
            //ServerCheck

            S2C.BuildingStatus pck = S2C.BuildingStatus.DeserializeFromBytes(pckData);
            Building building = new Building(pck);
            BuildingManager.Inst.Add(building);
        }

        public void BroadcastHealth(int ID, int health, DestroyReason reason)
        {
            S2C.SetStructureHealth pck = new S2C.SetStructureHealth(ID, health, reason);
            GetComponent<NetworkView>().RPC("RecvHealth", RPCMode.Others, pck.SerializeToBytes());
        }

        [RPC]
        void RecvHealth(byte[] pckData, NetworkMessageInfo info)
        {
            //ServerCheck
            S2C.SetStructureHealth pck = S2C.SetStructureHealth.DeserializeFromBytes(pckData);
            BuildingManager.Inst.Get(pck.m_ID).RecvHealth(pck);

            if (pck.m_health == 0)
                Debug.Log("Health Time : " + Network.time);
        }

        public void BroadcastFall(int ID)
        {
            //m_fallingBuildings.Add(ID); // Send Packet in lateUpdate
            m_fallingBuildings.Add(ID);
        }

        [RPC]
        public void RecvFall(byte[] pckData, NetworkMessageInfo info)
        {
            //CheckServer
            Debug.Log(" Recv Frame: " + Time.frameCount);

            S2C.BuildingFall pck = S2C.BuildingFall.DeserializeFromBytes(pckData);

            StartCoroutine(Fall(pck));
        }

        IEnumerator Fall(S2C.BuildingFall pck)
        {
            yield return new WaitForFixedUpdate();

            Debug.Log("Fall Frame: " + Time.frameCount);
            for (int i = 0; i < pck.m_IDs.Length; i++)
            {
                BuildingManager.Inst.Get(pck.m_IDs[i]).Fall();
            }

        }
    }
}