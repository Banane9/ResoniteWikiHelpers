using ProtoFlux.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResoniteWikiHelpers.ProtofluxNodeExport
{
    internal class InputListEndMetadata : IElementMetadata
    {
        public int Index { get; }
        public string Name { get; }

        public string Type { get; }

        public InputListEndMetadata(int index, string name, string type)
        {
            Index = index;
            Name = name;
            Type = type;
        }
    }
}