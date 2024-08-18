using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Emotion.Standard.Reflector;

public class GenerateReflectionForNamespaceAttribute : Attribute
{
    public GenerateReflectionForNamespaceAttribute()
    {

    }
}


[GenerateReflectionForNamespace]
public class EmotionReflectionMarkerClass
{

}