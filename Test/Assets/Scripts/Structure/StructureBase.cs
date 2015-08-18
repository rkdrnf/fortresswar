using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Architecture
{
    public abstract class StructureBase : IStructure
    {
        protected ushort m_ID;
        public GridCoord m_coord;

        public abstract bool CanCollide();

        
    }
}
