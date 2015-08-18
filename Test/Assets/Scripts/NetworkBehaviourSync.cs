using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityEngine.Networking
{
    public abstract class NetworkBehaviourSync : NetworkBehaviour
    {
        Queue<MessageBase> m_messageQueue;
        bool m_readyForMessage;

        virtual void Awake()
        {
            Debug.Log(string.Format("[{0}] Awake", this.GetType().Name));
            m_messageQueue = new List<MessageBase>();
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

        public virtual void Ready()
        {
            m_readyForMessage = true;
            ProcessMessages(m_messageQueue);
        }

        void ProcessMessages()
        {
            if (!m_readyForMessage) return;

            while (m_messageQueue.Count > 0)
            {
                MessageBase message = m_messageQueue.First();

                if (ProcessMessage(message))
                {
                    m_messageQueue.Dequeue();
                }

                return;
            }
        }

        virtual void Start()
        {
            Debug.Log(string.Format("[{0}] Start", this.GetType().Name));
        }
    }
}
