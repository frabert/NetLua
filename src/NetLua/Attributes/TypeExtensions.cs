using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NetLua.Attributes
{
    internal static class TypeExtensions
    {
        public static IEnumerable<T> GetCustomAttributesDeep<T>(this MemberInfo memberInfo)
        {
            var attributeType = typeof(T);
            var attributes = memberInfo.GetCustomAttributes(attributeType, false);

            if (memberInfo.DeclaringType == null)
            {
                return attributes.Cast<T>();
            }

            var interfaceAttributes = memberInfo.DeclaringType.GetInterfaces()
                .Select(t => t.GetProperty(memberInfo.Name))
                .Where(pi => pi != null)
                .SelectMany(pi => pi.GetCustomAttributes(attributeType, false));

            return attributes.Union(interfaceAttributes).Cast<T>();
        }

        public static T GetCustomAttributeDeep<T>(this MemberInfo memberInfo)
        {
            return GetCustomAttributesDeep<T>(memberInfo).FirstOrDefault();
        }
    }
}
