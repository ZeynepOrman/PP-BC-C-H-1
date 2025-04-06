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
        public IActionResult List(string name = null, string sortBy = null, string sortOrder = "asc")
        {
            var products = Products.AsQueryable();

            // Filter by name
            if (!string.IsNullOrEmpty(name))
            {
                products = products.Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            }

            // Sort by specified field
            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase))
                {
                    products = sortBy switch
                    {
                        "name" => products.OrderByDescending(p => p.Name),
                        "price" => products.OrderByDescending(p => p.Price),
                        _ => products
                    };
                }
                else
                {
                    products = sortBy switch
                    {
                        "name" => products.OrderBy(p => p.Name),
                        "price" => products.OrderBy(p => p.Price),
                        _ => products
                    };
                }
            }

            return Ok(new { status = 200, data = products.ToList() });
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
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

            Products.Add(product);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, new { status = 201, data = product });
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
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .Unless(x => IsPatchRequest());

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.")
                .Unless(x => IsPatchRequest());
        }

        private bool IsPatchRequest()
        {
            var httpContext = new HttpContextAccessor().HttpContext;
            return httpContext != null && httpContext.Request.Method.Equals("PATCH", StringComparison.OrdinalIgnoreCase);
        }
    }
}
