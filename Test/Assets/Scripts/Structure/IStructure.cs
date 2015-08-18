using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Architecture
{
    public interface IStructure
    {
    }

    public interface IStructure<T> : IStructure
    {
        void SetChunk(T chunkManager);
    }
}
