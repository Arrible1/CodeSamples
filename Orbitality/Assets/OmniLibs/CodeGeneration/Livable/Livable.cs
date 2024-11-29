using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ZergRush.Alive
{
    /*
     *     Livable object makes event/cells connection and other influences on data model in EnliveSelf method
     *     All those connections would be automatically disposed when object is mortified
     *     Enlive and Mortify will be automatically called when added or removed to special containers like LivableSlot/LivableList
     *     So you never call Enlive methods manually
     */
    [GenTask(GenTaskFlags.LivableNodePack)]
    public abstract partial class Livable : DataNode, IConnectionSink
    {
        [GenIgnore] public bool alive { get; private set; }
        [GenIgnore] List<Connection> connections;
        [GenIgnore] List<Action> influence;

        public void DisconnectAll()
        {
            if (connections != null)
            {
                for (var i = 0; i < connections.Count; i++)
                {
                    var connection = connections[i];
                    connection.Disconnect();
                }
                connections.Clear();
            }
            if (influence != null)
            {
                for (var i = 0; i < influence.Count; i++)
                {
                    var inf = influence[i];
                    inf();
                }
                influence.Clear();
            }
        }

        public void AddConnection(Connection conn)
        {
            if (connections == null) connections = new List<Connection>();
            connections.Add(conn);
        }

        public void AddInfluence(Action effect)
        {
            if (influence == null) influence = new List<Action>();
            influence.Add(effect);
        }
        public void AddInfluence(IDisposable effect)
        {
            if (influence == null) influence = new List<Action>();
            influence.Add(effect.Dispose);
        }
        public IDisposable addConnection
        {
            set => AddInfluence(value);
        }

        public bool HasConnections()
        {
            return (influence != null && influence.Count > 0) || (connections != null && connections.Count > 0);
        }

        public void DisconnectConcreteInfluence(Action effect)
        {
            if (influence == null) return;

            if (!influence.Contains(effect))
            {
                throw new ZergRushException("You can not disconnect influence because it does not exist");
            }

            effect();

            influence.Remove(effect);
        }

        protected virtual void EnliveSelf()
        {
            if (alive)
            {
                throw new ZergRushException("You can not enlive living");
            }

            alive = true;
        }

        protected virtual void MortifySelf()
        {
            if (!alive)
            {
                throw new ZergRushException("What Is Dead May Never Die (c)");
            }

            DisconnectAll();
            alive = false;
        }

        public void AddConnection(IDisposable connection)
        {
            AddInfluence(connection.Dispose);
        }
    }
    
    public struct Connection
    {
        public IList reader;
        public object obj;

        public void Disconnect()
        {
            if (reader != null)
            {
                reader.Remove(obj);
                reader = null;
                obj = null;
            }
        }
    }

    public class ZergRushException : Exception
    {
        public ZergRushException(string message) : base(message)
        {
        }
        public ZergRushException() 
        {
        }
    }
}
