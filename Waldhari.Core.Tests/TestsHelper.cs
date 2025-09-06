using System.Diagnostics;
using System.Reflection;

namespace Waldhari.Core.Tests
{
    public static class TestsHelper
    {
        public static T GetPrivateField<T>(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(field != null, nameof(field) + " != null");
            return (T)field.GetValue(obj);
        }
        
        public static MethodInfo GetPrivateMethod(object instance, string name)
        {
            
            var method = instance.GetType().GetMethod(name,
                BindingFlags.NonPublic | BindingFlags.Instance);
            Debug.Assert(method != null, nameof(method) + " != null");
            return method;
        }
    }
}