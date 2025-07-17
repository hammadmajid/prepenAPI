using Microsoft.EntityFrameworkCore;
using PrepenAPI.Data;
using PrepenAPI.DTOs;
using PrepenAPI.Models;

namespace PrepenAPI.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto?> GetOrderByIdAsync(int id);
        Task<bool> UpdateOrderStatusAsync(int id, OrderStatus newStatus);
    }

    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    UserEmail = o.User.Email,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt,
                    Items = o.Items.Select(i => new OrderItemDto
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        ProductName = i.Product.Name,
                        Quantity = i.Quantity,
                        Price = i.Product.Price
                    }).ToList()
                })
                .ToListAsync();

            return orders;
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Where(o => o.Id == id)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    UserEmail = o.User.Email,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt,
                    Items = o.Items.Select(i => new OrderItemDto
                    {
                        Id = i.Id,
                        ProductId = i.ProductId,
                        ProductName = i.Product.Name,
                        Quantity = i.Quantity,
                        Price = i.Product.Price
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return order;
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, OrderStatus newStatus)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return false;

            order.Status = newStatus;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
