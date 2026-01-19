using System;
using System.Collections.Generic;
using System.Linq;

namespace train.Data
{
    // Central place for allowed category names per audience.
    public static class CategoryPresets
    {
        // MEN
        private static readonly (string Group, string[] Items)[] Men = new[]
        {
            ("CLOTHING", new[] { "Tees", "Polos", "Shirts", "Jackets / Coats", "Sweaters", "Sweatshirts / Hoodies" }),
            ("BOTTOMS",  new[] { "Jeans", "Cargo Pants", "Trousers / Chinos", "Joggers", "Shorts" })
        };

        // WOMEN
        private static readonly (string Group, string[] Items)[] Women = new[]
        {
            ("CLOTHING", new[] { "Tops & Tees", "Dresses", "Jackets" }),
            ("BOTTOMS",  new[] { "Jeans", "Skirts", "Trousers" })
        };

        // BOYS
        private static readonly (string Group, string[] Items)[] Boys = new[]
        {
            ("CLOTHING", new[] { "Shirts", "Hoodies" }),
            ("BOTTOMS",  new[] { "Jeans", "Shorts" })
        };

        // GIRLS
        private static readonly (string Group, string[] Items)[] Girls = new[]
        {
            ("CLOTHING", new[] { "Tops", "Dresses" }),
            ("BOTTOMS",  new[] { "Jeans", "Skirts" })
        };

        public static readonly IReadOnlyDictionary<string, (string Group, string Name)[]> Map =
            new Dictionary<string, (string, string)[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["Men"] = Expand(Men),
                ["Women"] = Expand(Women),
                ["Boys"] = Expand(Boys),
                ["Girls"] = Expand(Girls)
            };

        private static (string Group, string Name)[] Expand((string Group, string[] Items)[] groups)
            => groups.SelectMany(g => g.Items.Select(i => (g.Group, i))).ToArray();
    }
}
