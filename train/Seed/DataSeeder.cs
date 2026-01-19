using Microsoft.EntityFrameworkCore;
using train.Areas.Identity.Data;
using train.Models;

namespace train.Seed
{
    public static class DataSeeder
    {
        public static async Task SeedDataAsync(appdbcontext context)
        {
            // Check if data already exists
            if (await context.Categories.AnyAsync() || await context.Products.AnyAsync())
            {
                Console.WriteLine("Database already seeded.");
                return;
            }

            Console.WriteLine("Seeding database...");

            // Seed Categories
            var categories = new List<Category>
            {
                // Men's Categories
                new Category { Name = "Tees", TargetAudience = "Men", Color = "Black" },
                new Category { Name = "Polos", TargetAudience = "Men", Color = "White" },
                new Category { Name = "Shirts", TargetAudience = "Men", Color = "Blue" },
                new Category { Name = "Sweaters", TargetAudience = "Men", Color = "Gray" },
                new Category { Name = "Sweatshirts / Hoodies", TargetAudience = "Men", Color = "Navy" },
                new Category { Name = "Jackets / Coats", TargetAudience = "Men", Color = "Black" },
                new Category { Name = "Jeans", TargetAudience = "Men", Color = "Blue" },
                new Category { Name = "Cargo Pants", TargetAudience = "Men", Color = "Beige" },
                new Category { Name = "Trousers / Chinos", TargetAudience = "Men", Color = "Navy" },
                new Category { Name = "Joggers", TargetAudience = "Men", Color = "Gray" },
                new Category { Name = "Shorts", TargetAudience = "Men", Color = "Navy" },

                // Women's Categories
                new Category { Name = "Tops & Tees", TargetAudience = "Women", Color = "White" },
                new Category { Name = "Dresses", TargetAudience = "Women", Color = "Black" },
                new Category { Name = "Jackets", TargetAudience = "Women", Color = "Beige" },
                new Category { Name = "Jeans", TargetAudience = "Women", Color = "Blue" },
                new Category { Name = "Skirts", TargetAudience = "Women", Color = "Black" },
                new Category { Name = "Trousers", TargetAudience = "Women", Color = "Gray" },

                // Boys' Categories
                new Category { Name = "Shirts", TargetAudience = "Boys", Color = "Blue" },
                new Category { Name = "Hoodies", TargetAudience = "Boys", Color = "Navy" },
                new Category { Name = "Jeans", TargetAudience = "Boys", Color = "Blue" },
                new Category { Name = "Shorts", TargetAudience = "Boys", Color = "Navy" },

                // Girls' Categories
                new Category { Name = "Tops", TargetAudience = "Girls", Color = "Pink" },
                new Category { Name = "Dresses", TargetAudience = "Girls", Color = "Purple" },
                new Category { Name = "Jeans", TargetAudience = "Girls", Color = "Blue" },
                new Category { Name = "Skirts", TargetAudience = "Girls", Color = "Pink" }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();

            Console.WriteLine($"Seeded {categories.Count} categories.");

            // Seed Products
            var products = new List<Product>
            {
                // Men's Products
                new Product
                {
                    Name = "Classic Black Tee",
                    Description = "Premium cotton crew neck t-shirt. Perfect for everyday wear.",
                    Price = 29.99M,
                    Stock = 150,
                    CategoryId = categories.First(c => c.Name == "Tees" && c.TargetAudience == "Men").Id,
                    TargetAudience = "Men",
Sizes = "S,M,L,XL,XXL",
                    Colors = "Black,White,Gray",
                    ImageUrl = "/images/products/black-tee.jpg",
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new Product
                {
                    Name = "White Polo Shirt",
                    Description = "Classic polo with breathable fabric. Great for casual or smart-casual occasions.",
                    Price = 45.99M,
                    Stock = 100,
                    CategoryId = categories.First(c => c.Name == "Polos" && c.TargetAudience == "Men").Id,
                    TargetAudience = "Men",
                    Sizes = "S,M,L,XL",
                    Colors = "White,Navy,Black",
                    ImageUrl = "/images/products/white-polo.jpg",
                    CreatedAt = DateTime.UtcNow.AddDays(-25)
                },
                new Product
                {
                    Name = "Blue Denim Jeans",
                    Description = "Slim fit jeans with stretch denim for comfort. Modern cut with classic style.",
                    Price = 69.99M,
                    Stock = 80,
                    CategoryId = categories.First(c => c.Name == "Jeans" && c.TargetAudience == "Men").Id,
                    TargetAudience = "Men",
                    Sizes = "30,32,34,36,38",
                    Colors = "Blue,Black",
                    ImageUrl = "/images/products/blue-jeans.jpg",
                    CreatedAt = DateTime.UtcNow.AddDays(-20)
                },
                new Product
                {
                    Name = "Gray Hoodie",
                    Description = "Comfortable pullover hoodie with kangaroo pocket. Perfect for layering.",
                    Price = 54.99M,
                    Stock = 120,
                    CategoryId = categories.First(c => c.Name == "Sweatshirts / Hoodies" && c.TargetAudience == "Men").Id,
                    TargetAudience = "Men",
                    Sizes = "S,M,L,XL,XXL",
                    Colors = "Gray,Black,Navy",
                    ImageUrl = "/images/products/gray-hoodie.jpg",
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new Product
                {
                    Name = "Navy Jacket",
                    Description = "Lightweight windbreaker jacket. Water-resistant and packable.",
                    Price = 89.99M,
                    Stock = 60,
                    CategoryId = categories.First(c => c.Name == "Jackets / Coats" && c.TargetAudience == "Men").Id,
                    TargetAudience = "Men",
                    Sizes = "M,L,XL",
                    Colors = "Navy,Black,Beige",
                    ImageUrl = "/images/products/navy-jacket.jpg",
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new Product
                {
                    Name = "Charcoal Sweater",
                    Description = "Warm knit sweater with ribbed cuffs. Timeless design for cold weather.",
                    Price = 59.99M,
                    Stock = 90,
                    CategoryId = categories.First(c => c.Name == "Sweaters" && c.TargetAudience == "Men").Id,
                    TargetAudience = "Men",
                    Sizes = "S,M,L,XL",
                    Colors = "Gray,Navy,Black",
                    ImageUrl = "https://placehold.co/600x800/4A4A4A/FFFFFF?text=Sweater",
                    CreatedAt = DateTime.UtcNow.AddDays(-8)
                },
                new Product
                {
                    Name = "Black Cargo Pants",
                    Description = "Utility cargo pants with multiple pockets. Durable and functional.",
                    Price = 64.99M,
                    Stock = 70,
                    CategoryId = categories.First(c => c.Name == "Cargo Pants" && c.TargetAudience == "Men").Id,
                    TargetAudience = "Men",
                    Sizes = "30,32,34,36",
                    Colors = "Black,Beige,Olive",
                    ImageUrl = "https://placehold.co/600x800/000000/FFFFFF?text=Cargo+Pants",
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new Product
                {
                    Name = "Navy Chinos",
                    Description = "Smart casual chino trousers. Perfect for office or weekend wear.",
                    Price = 55.99M,
                    Stock = 95,
                    CategoryId = categories.First(c => c.Name == "Trousers / Chinos" && c.TargetAudience == "Men").Id,
                    TargetAudience = "Men",
                    Sizes = "30,32,34,36,38",
                    Colors = "Navy,Beige,Gray",
                    ImageUrl = "https://placehold.co/600x800/1A1A5E/FFFFFF?text=Chinos",
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },

                // Women's Products
                new Product
                {
                    Name = "Elegant Black Dress",
                    Description = "Classic little black dress. Versatile and timeless for any occasion.",
                    Price = 79.99M,
                    Stock = 65,
                    CategoryId = categories.First(c => c.Name == "Dresses" && c.TargetAudience == "Women").Id,
                    TargetAudience = "Women",
                    Sizes = "XS,S,M,L,XL",
                    Colors = "Black,Navy",
                    ImageUrl = "/images/products/women-dress.jpg",
                    CreatedAt = DateTime.UtcNow.AddDays(-12)
                },
                new Product
                {
                    Name = "White Casual Top",
                    Description = "Lightweight and breathable casual top. Perfect for summer days.",
                    Price = 35.99M,
                    Stock = 110,
                    CategoryId = categories.First(c => c.Name == "Tops & Tees" && c.TargetAudience == "Women").Id,
                    TargetAudience = "Women",
                    Sizes = "XS,S,M,L",
                    Colors = "White,Beige,Pink",
                    ImageUrl = "https://placehold.co/600x800/FFFFFF/000000?text=Women+Top",
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                },
                new Product
                {
                    Name = "Skinny Blue Jeans",
                    Description = "Modern skinny fit jeans with stretch. Comfortable and flattering.",
                    Price = 64.99M,
                    Stock = 85,
                    CategoryId = categories.First(c => c.Name == "Jeans" && c.TargetAudience == "Women").Id,
                    TargetAudience = "Women",
                    Sizes = "24,26,28,30,32",
                    Colors = "Blue,Black, White",
                    ImageUrl = "https://placehold.co/600x800/4169E1/FFFFFF?text=Women+Jeans",
                    CreatedAt = DateTime.UtcNow.AddDays(-14)
                },
                new Product
                {
                    Name = "Beige Trench Jacket",
                    Description = "Classic trench coat. Elegant and sophisticated for any season.",
                    Price = 129.99M,
                    Stock = 45,
                    CategoryId = categories.First(c => c.Name == "Jackets" && c.TargetAudience == "Women").Id,
                    TargetAudience = "Women",
                    Sizes = "S,M,L",
                    Colors = "Beige,Black,Navy",
                    ImageUrl = "https://placehold.co/600x800/F5F5DC/000000?text=Trench+Coat",
                    CreatedAt = DateTime.UtcNow.AddDays(-6)
                },
                new Product
                {
                    Name = "Pleated Black Skirt",
                    Description = "Elegant pleated skirt. Perfect for formal or casual wear.",
                    Price = 49.99M,
                    Stock = 75,
                    CategoryId = categories.First(c => c.Name == "Skirts" && c.TargetAudience == "Women").Id,
                    TargetAudience = "Women",
                    Sizes = "XS,S,M,L",
                    Colors = "Black,Navy,Gray",
                    ImageUrl = "https://placehold.co/600x800/000000/FFFFFF?text=Pleated+Skirt",
                    CreatedAt = DateTime.UtcNow.AddDays(-4)
                },

                // Boys' Products
                new Product
                {
                    Name = "Boys Graphic Tee",
                    Description = "Cool graphic t-shirt for kids. Comfortable cotton fabric.",
                    Price = 19.99M,
                    Stock = 130,
                    CategoryId = categories.First(c => c.Name == "Shirts" && c.TargetAudience == "Boys").Id,
                    TargetAudience = "Boys",
                    Sizes = "4,6,8,10,12",
                    Colors = "Blue,Red,Green",
                    ImageUrl = "https://placehold.co/600x800/1E90FF/FFFFFF?text=Boys+Shirt",
                    CreatedAt = DateTime.UtcNow.AddDays(-11)
                },
                new Product
                {
                    Name = "Boys Navy Hoodie",
                    Description = "Warm and cozy hoodie for active kids. Durable construction.",
                    Price = 34.99M,
                    Stock = 100,
                    CategoryId = categories.First(c => c.Name == "Hoodies" && c.TargetAudience == "Boys").Id,
                    TargetAudience = "Boys",
                    Sizes = "4,6,8,10,12",
                    Colors = "Navy,Gray,Black",
                    ImageUrl = "https://placehold.co/600x800/000080/FFFFFF?text=Boys+Hoodie",
                    CreatedAt = DateTime.UtcNow.AddDays(-9)
                },
                new Product
                {
                    Name = "Boys Denim Jeans",
                    Description = "Classic denim jeans for boys. Reinforced knees for extra durability.",
                    Price = 39.99M,
                    Stock = 90,
                    CategoryId = categories.First(c => c.Name == "Jeans" && c.TargetAudience == "Boys").Id,
                    TargetAudience = "Boys",
                    Sizes = "4,6,8,10,12",
                    Colors = "Blue,Black",
                    ImageUrl = "https://placehold.co/600x800/4169E1/FFFFFF?text=Boys+Jeans",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },

                // Girls' Products
                new Product
                {
                    Name = "Girls Pink Dress",
                    Description = "Adorable dress with floral pattern. Perfect for special occasions.",
                    Price = 44.99M,
                    Stock = 80,
                    CategoryId = categories.First(c => c.Name == "Dresses" && c.TargetAudience == "Girls").Id,
                    TargetAudience = "Girls",
                    Sizes = "4,6,8,10,12",
                    Colors = "Pink,Purple,White",
                    ImageUrl = "https://placehold.co/600x800/FFC0CB/000000?text=Girls+Dress",
                    CreatedAt = DateTime.UtcNow.AddDays(-13)
                },
                new Product
                {
                    Name = "Girls Casual Top",
                    Description = "Comfortable and stylish casual top. Great for everyday wear.",
                    Price = 24.99M,
                    Stock = 120,
                    CategoryId = categories.First(c => c.Name == "Tops" && c.TargetAudience == "Girls").Id,
                    TargetAudience = "Girls",
                    Sizes = "4,6,8,10,12",
                    Colors = "Pink,Purple,White",
                    ImageUrl = "https://placehold.co/600x800/FFB6C1/000000?text=Girls+Top",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new Product
                {
                    Name = "Girls Denim Skirt",
                    Description = "Cute denim skirt with adjustable waist. Fun and practical.",
                    Price = 29.99M,
                    Stock = 95,
                    CategoryId = categories.First(c => c.Name == "Skirts" && c.TargetAudience == "Girls").Id,
                    TargetAudience = "Girls",
                    Sizes = "4,6,8,10,12",
                    Colors = "Blue,Pink",
                    ImageUrl = "https://placehold.co/600x800/87CEEB/000000?text=Girls+Skirt",
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            Console.WriteLine($"Seeded {products.Count} products.");
            Console.WriteLine("Database seeding completed successfully!");
        }
    }
}
