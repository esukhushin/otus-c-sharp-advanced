using System;
using System.Collections.Generic;
using System.Text;

namespace HW21_Roslyn_SerializerTest
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class GenerateBinarySerializerAttribute : Attribute
    {

    }
}
