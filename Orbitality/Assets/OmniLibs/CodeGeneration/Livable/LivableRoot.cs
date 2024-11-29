namespace ZergRush.Alive
{
    [GenTask(GenTaskFlags.LivableNodePack)]
    public abstract partial class LivableRoot : DataRoot, ILivable
    {
        [GenIgnore] bool alive;
        public virtual void EnliveWorld()
        {
            if (!alive)
                Enlive();
        }

        public virtual void MortifyWorld()
        {
            // Can be uncommented to test performance gain in multiplayer tests
            //Mortify();
        }

        public virtual void EnliveSelf()
        {
            alive = true;
        }

        public virtual void MortifySelf()
        {
            alive = false;
        }
    }
    
    public interface IDataRootWithStep
    {
        int step { get; }
    }
}

