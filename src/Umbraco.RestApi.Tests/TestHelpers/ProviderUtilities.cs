using System;
using System.Configuration.Provider;
using System.Reflection;

namespace Umbraco.RestApi.Tests.TestHelpers
{
    /// <summary>
    /// Allows modifying readonly ProviderCollection at runtime (i.e. membership providers) for testing
    /// </summary>
    public static class ProviderUtilities
    {
        static private readonly FieldInfo ProviderCollectionReadOnlyField;

        static ProviderUtilities()
        {
            Type t = typeof(ProviderCollection);
            ProviderCollectionReadOnlyField = t.GetField("_ReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        static public void AddProvider(this ProviderCollection pc, ProviderBase provider)
        {
            bool prevValue = (bool)ProviderCollectionReadOnlyField.GetValue(pc);
            if (prevValue)
                ProviderCollectionReadOnlyField.SetValue(pc, false);

            pc.Add(provider);

            if (prevValue)
                ProviderCollectionReadOnlyField.SetValue(pc, true);
        }

        public static void ClearAll(this ProviderCollection pc)
        {
            bool prevValue = (bool)ProviderCollectionReadOnlyField.GetValue(pc);
            if (prevValue)
                ProviderCollectionReadOnlyField.SetValue(pc, false);

            pc.Clear();

            if (prevValue)
                ProviderCollectionReadOnlyField.SetValue(pc, true);
        }
    }
}