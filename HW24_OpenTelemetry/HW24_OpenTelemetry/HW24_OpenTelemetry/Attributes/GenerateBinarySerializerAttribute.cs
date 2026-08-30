using System;
using System.Collections.Generic;
using System.Text;

namespace HW24_OpenTelemetry.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class GenerateBinarySerializerAttribute : Attribute
    {

    }
}
