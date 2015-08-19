using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityEngine.Networking
{
    public class NetworkBehaviourMessageQueue
    {
        Queue<MessageBase> m_messageQueue;
        bool m_readyForMessage;
        NetworkBehaviour m_behaviour;
        Dictionary<Type, object> m_handlers;
        System.Diagnostics.Stopwatch m_stopWatch;

        public NetworkBehaviourMessageQueue(NetworkBehaviour behaviour)
        {
            m_messageQueue = new Queue<MessageBase>();
            m_readyForMessage = false;
            m_behaviour = behaviour;
            m_handlers = new Dictionary<Type, object>();
            m_stopWatch = new System.Diagnostics.Stopwatch();

            Debug.Log(string.Format("[{0} NBMessageQueue] Created", m_behaviour.gameObject.name));
        }

        public virtual void Ready()
        {
            m_readyForMessage = true;
            ProcessMessages();
        }

        public bool IsReady()
        {
            return m_readyForMessage;
        }

        void ProcessMessages()
        {
            if (!m_readyForMessage) return;

            Debug.Log(string.Format("[{0} NBMessageQueue] Message Queue Count : {1}"
                , m_behaviour.gameObject.name, m_messageQueue.Count));

            while (m_messageQueue.Count > 0)
            {
                MessageBase message = m_messageQueue.First();
                Type messageType = message.GetType();

                bool result = (bool)typeof(NetworkBehaviourMessageQueue).GetMethod("ProcessMessage", new Type[] { messageType }).MakeGenericMethod(messageType).Invoke(this, new object[] { message });
                if (result)
                {
                    m_messageQueue.Dequeue();
                }
                else
                {
                    Debug.Log(string.Format("[{0} NBMessageQueue] Processing Message Failed : {1}"
                        , m_behaviour.gameObject.name, message.GetType().Name));
                }

                return;
            }
        }

        public void AddOrProcessMessage<T>(T message) where T : MessageBase
        {
            if (m_messageQueue.Count == 0)
            {
                Type messageType = typeof(T);
                bool result = (bool)typeof(NetworkBehaviourMessageQueue).GetMethod("ProcessMessage", new Type[] { messageType }).MakeGenericMethod(messageType).Invoke(this, new object[] { message });

                if (result == false)
                {
                    AddMessage(message);
                }
            }
            else
            {
                AddMessage(message);
            }
        }

        void AddMessage(MessageBase message)
        {
            m_messageQueue.Enqueue(message);

            Debug.Log(string.Format("[{0} NBMessageQueue] Message Added : {1}"
                        , m_behaviour.gameObject.name, message.GetType().Name));
        }

        public virtual bool ProcessMessage<T>(T message)
        {
            

            Type messageType = typeof(T);
            if (m_handlers.ContainsKey(messageType))
            {
                Func<T, bool> handlerFunc = m_handlers[messageType] as Func<T, bool>;

                m_stopWatch.Reset();
                m_stopWatch.Start();

                bool result = handlerFunc(message);

                m_stopWatch.Stop();
                Debug.Log(string.Format("[{0} NBMessageQueue] Message Processed. Message: {1} Time {2}"
                    , m_behaviour.gameObject.name, messageType.Name, m_stopWatch.ElapsedMilliseconds));
            }

            Debug.Log(string.Format("[{0} NBMessageQueue] Appropriate handler is not found for Message : {1}"
                        , m_behaviour.gameObject.name, messageType.Name));

            return false;
        }

        public void RegisterMessageHandler<T>(Func<T, bool> handlerFunc) where T : MessageBase
        {
            m_handlers.Add(typeof(T), handlerFunc);
        }
    }
}
