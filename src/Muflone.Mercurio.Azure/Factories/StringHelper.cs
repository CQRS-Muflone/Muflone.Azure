using System;

namespace Muflone.Mercurio.Azure.Factories;

public static class StringHelper
{
    public static Guid ToGuid(this string stringValue)
    {
        Guid.TryParse(stringValue, out var guidValue);

        return guidValue;
    }
}