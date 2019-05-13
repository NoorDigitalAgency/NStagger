using System.Reflection;
using System.Runtime.CompilerServices;

namespace NStagger.ModelMapper
{
    public static class Extensions
    {
        public static void SetFieldValue(this object obj, object value, string fieldName)
        {
            obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.SetValue(obj, value);
        }

        public static object GetFieldValue(this object obj, string fieldName)
        {
            return obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.GetValue(obj);
        }

        public static void SetFieldValue(this object destination, object source, string sourceFieldName, string destinationFieldName)
        {
            object value = source.GetType().GetField(sourceFieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.GetValue(source);

            destination.GetType().GetField(destinationFieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)?.SetValue(destination, value);
        }

        public static void SetPropertyFromField(this object destination, object source, string fieldName, string propertyName)
        {
            PropertyInfo propertyInfo = destination.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            destination.SetFieldValue(source, fieldName, propertyInfo.GetBackingField().Name);
        }

        public static void SetProperty(this object obj, object value, string propertyName)
        {
            PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            obj.SetFieldValue(value, propertyInfo.GetBackingField().Name);
        }

        private static FieldInfo GetBackingField(this PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanRead || !propertyInfo.GetGetMethod(true).IsDefined(typeof(CompilerGeneratedAttribute), true))
            {
                return null;
            }

            FieldInfo backingField = propertyInfo.DeclaringType?.GetField($"<{propertyInfo.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

            if (backingField == null)
            {
                return null;
            }

            return !backingField.IsDefined(typeof(CompilerGeneratedAttribute), true) ? null : backingField;
        }
    }
}