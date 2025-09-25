using DeliveryService.Application;
using DeliveryService.Domain;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryService.Controllers;

[ApiController]
[Route("[controller]")]
public class DeliveryOrderController : ControllerBase
{
    private readonly IDeliveryOrderService _service;
    private readonly ILogger<DeliveryOrderController> _logger;

    public DeliveryOrderController(IDeliveryOrderService service, ILogger<DeliveryOrderController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<DeliveryOrder>> Create([FromBody] CreateOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var order = new DeliveryOrder
        {
            OrderId = request.OrderId,
            CustomerId = request.CustomerId,
            Category = request.Category,
            Snapshot = request.Snapshot
        };

        var created = await _service.CreateFromMonolithAsync(order, request.IdempotencyToken);
        return Ok(created);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeliveryOrder>>> GetAvailable([FromQuery] string? category = null)
    {
        var orders = await _service.GetAvailableOrdersByCategoryAsync(category);
        return Ok(orders);
    }

    [HttpGet("{orderId}/{customerId}")]
    public async Task<ActionResult<DeliveryOrder>> Get(int orderId, int customerId)
    {
        var order = await _service.GetOrderAsync(orderId, customerId);
        if (order == null)
        {
            return NotFound();
        }
        return Ok(order);
    }

    [HttpPost("{orderId}/{customerId}/assign")]
    public async Task<ActionResult<DeliveryOrder>> AssignDeliveryPerson(
        int orderId,
        int customerId,
        [FromBody] AssignDeliveryPersonRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var order = await _service.AssignDeliveryPersonAsync(
            orderId,
            customerId,
            request.DeliveryPersonId,
            request.IdempotencyToken);

        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    [HttpPut("{orderId}/{customerId}/status")]
    public async Task<ActionResult<DeliveryOrder>> UpdateStatus(
        int orderId,
        int customerId,
        [FromBody] UpdateStatusRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var order = await _service.UpdateOrderStatusAsync(orderId, customerId, request.Status);
        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }

    [HttpGet("deliveryPerson/{deliveryPersonId}")]
    public async Task<ActionResult<IEnumerable<DeliveryOrder>>> GetByDeliveryPerson(int deliveryPersonId)
    {
        var orders = await _service.GetOrdersByDeliveryPersonAsync(deliveryPersonId);
        return Ok(orders);
    }
}

public class CreateOrderRequest
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public string Category { get; set; } = string.Empty;
    public Dictionary<string, object>? Snapshot { get; set; }
    public string IdempotencyToken { get; set; } = string.Empty;
}

public class AssignDeliveryPersonRequest
{
    public int DeliveryPersonId { get; set; }
    public string IdempotencyToken { get; set; } = string.Empty;
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
}