using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Retail_Ordering_Web.Data;
using Retail_Ordering_Web.Models;

namespace Retail_Ordering_Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            int userId = 1;

            var product = await _context.Products.FindAsync(productId);

            if (product == null)
                return NotFound("Product not found");

            if (product.Quantity < quantity)
                return BadRequest("Not enough stock");

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = cart.CartItems
                .FirstOrDefault(x => x.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.CartItems.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity
                });
            }

            await _context.SaveChangesAsync();

            return Ok("Added to cart");
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            int userId = 1;

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound("Cart empty");

            var result = new
            {
                cart.Id,
                cart.UserId,
                Items = cart.CartItems.Select(item => new
                {
                    item.Id,
                    ProductName = item.Product.Name,
                    item.Product.Price,
                    item.Quantity,
                    Total = item.Product.Price * item.Quantity
                }),
                GrandTotal = cart.CartItems.Sum(item => item.Product.Price * item.Quantity)
            };

            return Ok(result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateCart(int cartItemId, int quantity)
        {
            var item = await _context.CartItems
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

            if (item == null)
                return NotFound("Item not found");

            if (item.Product.Quantity < quantity)
                return BadRequest("Not enough stock");

            item.Quantity = quantity;

            await _context.SaveChangesAsync();

            return Ok("Cart updated");
        }

        [HttpDelete("remove/{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var item = await _context.CartItems.FindAsync(id);

            if (item == null)
                return NotFound("Item not found");

            _context.CartItems.Remove(item);

            await _context.SaveChangesAsync();

            return Ok("Item removed");
        }
    }
}