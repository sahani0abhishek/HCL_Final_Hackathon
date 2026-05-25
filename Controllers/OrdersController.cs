using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Retail_Ordering_Web.Data;
using Retail_Ordering_Web.Models;

namespace Retail_Ordering_Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("place")]
        public async Task<IActionResult> PlaceOrder(string customerName)
        {
            int userId = 1;

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
                return BadRequest("Cart is empty");

            foreach (var item in cart.CartItems)
            {
                if (item.Product.Quantity < item.Quantity)
                {
                    return BadRequest($"Not enough stock for {item.Product.Name}");
                }
            }

            decimal totalAmount = cart.CartItems.Sum(
                item => item.Product.Price * item.Quantity
            );

            var order = new Order
            {
                CustomerName = customerName,
                TotalAmount = totalAmount,
                OrderDate = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in cart.CartItems)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                };

                _context.OrderItems.Add(orderItem);

                item.Product.Quantity -= item.Quantity;
            }

            _context.CartItems.RemoveRange(cart.CartItems);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Order placed successfully",
                OrderId = order.Id,
                Customer = customerName,
                TotalAmount = totalAmount,
                OrderDate = order.OrderDate
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToListAsync();

            var result = orders.Select(order => new
            {
                order.Id,
                order.CustomerName,
                order.TotalAmount,
                order.OrderDate,
                Items = order.OrderItems.Select(item => new
                {
                    ProductName = item.Product.Name,
                    item.Quantity,
                    item.Price
                })
            });

            return Ok(result);
        }
    }
}