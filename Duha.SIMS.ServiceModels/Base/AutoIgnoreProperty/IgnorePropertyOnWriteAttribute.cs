namespace Duha.SIMS.ServiceModels.Base.AutoIgnoreProperty
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class IgnorePropertyOnWriteAttribute : AutoInjectRootAttribute
    {
        public AutoMapConversionType ConversionType { get; set; }

        public IgnorePropertyOnWriteAttribute(AutoMapConversionType conversionType = AutoMapConversionType.All)
        {
            ConversionType = conversionType;
        }
    }
}
