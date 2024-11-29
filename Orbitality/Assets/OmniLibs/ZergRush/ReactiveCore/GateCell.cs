using System;

namespace ZergRush.ReactiveCore
{
    [CLIENT]
    public class GateCell : ICell<bool>
    {
        int closingsCount = 0;
        Cell<bool> gateIn = new Cell<bool>();
        ICell<bool> gateOut;
        public bool value => gateOut.value;
        public IDisposable ListenUpdates(Action<bool> reaction) => gateOut.ListenUpdates(reaction);
        public GateCell() : this(0, null) { }
        public GateCell(float openingLag, Action<IDisposable> connectionSink)
        {
            gateIn.value = true;
            if (openingLag == 0)
                gateOut = gateIn;
            else
                gateOut = gateIn.Lag(openingLag, val => val, connectionSink);
        }
        //List<string> debugNames = new List<string>();
        public IDisposable Close(string debugName)
        {
            //debugName += UnityEngine.Random.GetRange(1000, 10000);
            //debugNames.Add(debugName);
            //Debug.Log($"closing start with {debugName}, closings = {debugNames.PrintCollection()}");
            closingsCount++;
            gateIn.value = false;
            return new AnonymousDisposable(() =>
            {
                //debugNames.Remove(debugName);
                //Debug.Log($"closing end with {debugName}, closings = {debugNames.PrintCollection()}");
                closingsCount--;
                if (closingsCount == 0)
                    gateIn.value = true;
            });
        }
        public void Trigger(string debugName) => Close(debugName).Dispose();
        //public void Trigger() => Close().Dispose(); // Trigger timeout = Close and open.
    }
    
}