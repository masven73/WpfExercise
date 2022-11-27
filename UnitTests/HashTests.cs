using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WpfExercise.Models;
using WpfExercise.Services;
using Xunit;

namespace UnitTests
{
    public class HashTests
    {
        [Fact]
        public void TestFileHash()
        {
            var products = new List<Product> { new()
            {
                Id = 1,
                Title = "iPhone 10",
                Description = "Old model from Apple",
                Price = 549,
                DiscountPercentage = 12.96,
                Rating = 4.69,
                Stock = 94,
                Brand = "Apple",
                Category = "smartphones"
            } };

            var testFileName1 = "HashTestFile1.json";
            File.WriteAllText(testFileName1, JsonConvert.SerializeObject(products));

            var hash1 = HashCalculator.ComputeHash(testFileName1);
            Assert.True(!string.IsNullOrEmpty(hash1));

            var product = products.FirstOrDefault();
            if (product != null) 
                product.Title = "iPhone 11";
            
            var testFileName2 = "HashTestFile2.json";
            File.WriteAllText(testFileName2, JsonConvert.SerializeObject(products));

            var hash2 = HashCalculator.ComputeHash(testFileName2);
            Assert.True(!string.IsNullOrEmpty(hash2));

            Assert.NotEqual(hash1, hash2);
        }
    }
}