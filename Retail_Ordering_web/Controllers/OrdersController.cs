using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Retail_Ordering_web.Data;
using Retail_Ordering_web.Models;

namespace Retail_Ordering_web.Controllers
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

        // GET: api/orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders.ToListAsync();
        }

        // GET: api/orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // POST: api/orders
        [HttpPost]
        public async Task<ActionResult<Order>> PlaceOrder(Order order)
        {
            // Product find karo
            var product = await _context.Products.FindAsync(order.ProductId);

            // Product nahi mila
            if (product == null)
            {
                return NotFound("Product not found");
            }

            // Stock check
            if (product.Quantity < order.Quantity)
            {
                order.Status = "OutOfStock";
                order.TotalPrice = 0;
            }
            else
            {
                // Quantity reduce
                product.Quantity -= order.Quantity;

                // Total calculate
                order.TotalPrice = product.Price * order.Quantity;

                // Status
                order.Status = "Accepted";

                // Product available check
                if (product.Quantity == 0)
                {
                    product.IsAvailable = false;
                }
            }

            // Order save
            _context.Orders.Add(order);

            // Save DB
            await _context.SaveChangesAsync();

            return Ok(order);
        }

        // DELETE: api/orders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}