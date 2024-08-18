using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.Standard.Reflector;

public interface IReflectorTypeData
{
    public ReflectorTypeMember[] GetMembers();
}
