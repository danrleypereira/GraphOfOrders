using DeliveryService.Application;
using DeliveryService.Domain;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryService.Controllers;

[ApiController]
[Route("[controller]")]
public class DeliveryPersonController : ControllerBase
{
    private readonly IDeliveryPersonService _service;
    private readonly ILogger<DeliveryPersonController> _logger;

    public DeliveryPersonController(IDeliveryPersonService service, ILogger<DeliveryPersonController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeliveryPerson>>> GetAll()
    {
        var persons = await _service.GetAllAsync();
        return Ok(persons);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DeliveryPerson>> GetById(int id)
    {
        var person = await _service.GetByIdAsync(id);
        if (person == null)
        {
            return NotFound();
        }
        return Ok(person);
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<DeliveryPerson>>> GetActive()
    {
        var persons = await _service.GetActiveAsync();
        return Ok(persons);
    }

    [HttpPost]
    public async Task<ActionResult<DeliveryPerson>> Create([FromBody] DeliveryPerson person)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _service.CreateAsync(person);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DeliveryPerson>> Update(int id, [FromBody] DeliveryPerson person)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _service.UpdateAsync(id, person);
        if (updated == null)
        {
            return NotFound();
        }

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}