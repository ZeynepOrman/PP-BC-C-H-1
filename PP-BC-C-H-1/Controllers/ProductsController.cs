using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;

namespace PP_BC_C_H_1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private static List<Product> Products = new List<Product>
        {
            new Product { Id = 1, Name = "Product1", Price = 10 },
            new Product { Id = 2, Name = "Product2", Price = 20 }
        };

        private readonly IValidator<Product> _validator;

        public ProductsController(IValidator<Product> validator)
        {
            _validator = validator;
        }

        [HttpGet("list")]
        public IActionResult List([FromQuery] string name)
        {
            var products = Products.Where(p => string.IsNullOrEmpty(name) || p.Name.Contains(name)).ToList();
            return Ok(new { status = 200, data = products });
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound(new { status = 404, error = "Product not found" });

            return Ok(new { status = 200, data = product });
        }

        [HttpPost]
        public IActionResult Create([FromBody] Product product)
        {
            ValidationResult result = _validator.Validate(product);
            if (!result.IsValid)
                return BadRequest(new { status = 400, errors = result.Errors });

            product.Id = Products.Max(p => p.Id) + 1;
            Products.Add(product);
            return CreatedAtAction(nameof(Get), new { id = product.Id }, new { status = 201, data = product });
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] Product product)
        {
            ValidationResult result = _validator.Validate(product);
            if (!result.IsValid)
                return BadRequest(new { status = 400, errors = result.Errors });

            var existingProduct = Products.FirstOrDefault(p => p.Id == id);
            if (existingProduct == null)
                return NotFound(new { status = 404, error = "Product not found" });

            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            return Ok(new { status = 200, data = existingProduct });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var product = Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
                return NotFound(new { status = 404, error = "Product not found" });

            Products.Remove(product);
            return NoContent();
        }

        [HttpPatch("{id}")]
        public IActionResult Patch(int id, [FromBody] Product product)
        {
            var existingProduct = Products.FirstOrDefault(p => p.Id == id);
            if (existingProduct == null)
                return NotFound(new { status = 404, error = "Product not found" });

            if (!string.IsNullOrEmpty(product.Name))
                existingProduct.Name = product.Name;

            if (product.Price > 0)
                existingProduct.Price = product.Price;

            return Ok(new { status = 200, data = existingProduct });
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class ProductValidator : AbstractValidator<Product>
    {
        public ProductValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than zero.");
        }
    }
}
