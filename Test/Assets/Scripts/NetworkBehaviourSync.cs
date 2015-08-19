using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityEngine.Networking
{
    public abstract class NetworkBehaviourSync : NetworkBehaviour
    {
        NetworkBehaviourMessageQueue m_messageQueue;

        protected virtual void Awake()
        {
            Debug.Log(string.Format("[{0}] Awake", this.GetType().Name));
            m_messageQueue = new NetworkBehaviourMessageQueue(this);
        }

        protected virtual void Start()
        {
            Debug.Log(string.Format("[{0}] Start", this.GetType().Name));
        }

        public override bool OnSerialize(NetworkWriter writer, bool initialState)
        {
            Debug.Log(string.Format("[{0}] OnSerialize", this.GetType().Name));

            return base.OnSerialize(writer, initialState);
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            Debug.Log(string.Format("[{0}] OnDeserialize", this.GetType().Name));

            base.OnDeserialize(reader, initialState);
        }

        public void AddOrProcessMessage<T>(T message) where T : MessageBase
        {
            m_messageQueue.AddOrProcessMessage<T>(message);
        }

        public bool ProcessMessage<T>(T message) where T : MessageBase
        {
            return m_messageQueue.ProcessMessage<T>(message);
        }

        protected void RegisterMessageHandler<T>(Func<T, bool> handlerFunc) where T : MessageBase
        {
            m_messageQueue.RegisterMessageHandler<T>(handlerFunc);
        }

        public override void OnStartServer()
        {
            Debug.Log(string.Format("[{0}] OnStartServer", this.GetType().Name));
        }

        public override void OnStartClient()
        {
            Debug.Log(string.Format("[{0}] OnStartClient", this.GetType().Name));
        }
    }
}
