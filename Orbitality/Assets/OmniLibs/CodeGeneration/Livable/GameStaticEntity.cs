using System;
using System.Collections.Generic;

namespace ZergRush.Alive
{
    [GenLoadableConfigData, Immutable]
    public partial class LoadableConfig 
    {
        public ulong id => UId();
    }
    
    [GenConfigData]
    public partial class StubTypeBasedDataFromConfig : LoadableConfig
    {        
    }
    
    public class ConfigStorageDictionary : Dictionary<ulong, IUniquelyIdentifiable> {}
    
    //[GenHashing]
}
