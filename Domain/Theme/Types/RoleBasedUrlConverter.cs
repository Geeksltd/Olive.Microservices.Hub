namespace Olive.Microservices.Hub.Domain.Theme.Types
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    /// <summary>Allows a role based url to be configured as a plain string, which is read as its
    /// Default. Without this the configuration binder cannot turn a scalar setting into the object
    /// and silently discards it.</summary>
    public class RoleBasedUrlConverter<TUrl> : TypeConverter where TUrl : RoleBasedUrl, new()
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string url) return new TUrl { Default = url };

            return base.ConvertFrom(context, culture, value);
        }
    }
}
