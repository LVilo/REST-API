using Microsoft.AspNetCore.Mvc;
using RestAPI.DTOs;
using RestAPI.Models;
using RestAPI.Repositories;
using RestAPI.Services;

namespace RestAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConfigurationsController : ControllerBase
    {
        private readonly IConfigurationRepository _repository;

        public ConfigurationsController(IConfigurationRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int limit = 50,
            [FromQuery] int offset = 0,
            [FromQuery] string sort = "-createdAt",
            [FromQuery] string? name_like = null,
            [FromQuery] string? serial_from = null,
            [FromQuery] string? serial_to = null,
            [FromQuery] DateTime? date_from = null,
            [FromQuery] DateTime? date_to = null)
        {
            var (items, total) = await _repository.GetAllAsync(
                limit, offset, sort, name_like, serial_from, serial_to, date_from, date_to);

            return Ok(new
            {
                items,
                total,
                limit,
                offset
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return NotFound(new { title = "Not Found", status = 404, detail = "Configuration not found" });

            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ConfigurationInputDto dto)
        {
            var extra = dto.ExtraFields ?? new Dictionary<string, object>();
            extra.Remove("serialNumber");
            extra.Remove("deviceName");

            var config = new ConfigurationDocument
            {
                Id = Guid.NewGuid(),
                SerialNumber = dto.SerialNumber,
                DeviceName = dto.DeviceName,
                ExtraData = extra,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _repository.CreateAsync(config);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ConfigurationInputDto dto)
        {
            var extra = dto.ExtraFields ?? new Dictionary<string, object>();
            extra.Remove("serialNumber");
            extra.Remove("deviceName");

            var config = new ConfigurationDocument
            {
                Id = id,
                SerialNumber = dto.SerialNumber,
                DeviceName = dto.DeviceName,
                ExtraData = extra,
                CreatedAt = DateTime.UtcNow
            };

            var updated = await _repository.UpdateAsync(id, config);

            if (updated == null)
                return NotFound(new { title = "Not Found", status = 404, detail = "Configuration not found" });

            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _repository.SoftDeleteAsync(id);

            if (!deleted)
                return NotFound(new { title = "Not Found", status = 404, detail = "Configuration not found" });

            return NoContent();
        }
    }
}
