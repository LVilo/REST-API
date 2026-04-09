using Microsoft.AspNetCore.Mvc;
using MongoAPI.Models;
using MongoAPI.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MongoAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly MongoDeviceService _service;

        public DevicesController(MongoDeviceService service)
        {
            _service = service;
        }

        [HttpPost("Configurations/")]
        public async Task<IActionResult> AddDevice([FromBody] DeviceConfig config)
        {
            if (config.Id is not null) return ValidationProblem("ID должен быть null!!!");
            await _service.AddDeviceConfigAsync(config);
            return Ok(config.Id);
        }

        [HttpGet("Configurations/{serialNumber}")]
        public async Task<IActionResult> GetBySerial(ulong serialNumber)
        {
            var device = await _service.GetBySerialNumberAsync(serialNumber);
            if (device == null) return NotFound();
            return Ok(device);
        }

        [HttpGet("Configurations/max-serial/{deviceFamily}")]
        public async Task<IActionResult> GetRecordForSerial(string deviceFamily)
        {
            var device = await _service.GetRecordMaxSerialNumberByFamilyAsync(deviceFamily);
            if (device == null) return NotFound();
            return Ok(device);
        }
        [HttpGet("Configurations/last-serial/{deviceFamily}")]
        public async Task<IActionResult> GetLastSerial(string deviceFamily)
        {
            var device = await _service.GetRecordMaxSerialNumberByFamilyAsync(deviceFamily);
            if (device == null) return NotFound();
            return Ok(device.SerialNumber);
        }
        [HttpGet("configurations/")]
        public async Task<List<DeviceConfig>> Search([FromQuery] uint? serialNumber = null,
            [FromQuery] string orderNumber = null,
            [FromQuery] string deviceType = null,
            [FromQuery] string deviceFamily = null,
            [FromQuery] string arm = null,
            [FromQuery] bool? isActual = null)
        {
            return await _service.SearchAsync(serialNumber, orderNumber, deviceType, deviceFamily, arm, isActual);
        }
    }
}
