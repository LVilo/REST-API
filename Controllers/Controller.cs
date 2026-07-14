using Asp.Versioning;
using DynamicData.Kernel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoAPI.Models;
using MongoAPI.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MongoAPI.Controllers.V1
{
    [ApiController]
    [ApiVersion("1")]
    [Route("REST/v{version:apiVersion}/Configurations")]
    public class Controller : ControllerBase
    {
        private readonly Database _service;

        public Controller(Database service)
        {
            _service = service;
        }
        [Authorize(Roles = Roles.Admin)]
        [Authorize(Roles = Roles.APM)]
        [HttpPost]// добавить запись
        public async Task<IActionResult> AddDevice([FromBody] Config config)
        {
            if (config.Id is not null) return ValidationProblem("ID записывать не нужно");
            if (string.IsNullOrEmpty(config.DeviceFamily) || config.SerialNumber is 0 || string.IsNullOrEmpty(config.DeviceType) || string.IsNullOrEmpty(config.Revision)) return ValidationProblem("DeviceFamily,SerialNumber,DeviceType и Revision не могут быть null");
            if (config.IsActual is true)
            {
                List<Config> list = await _service.SearchAsync(50, config.SerialNumber, null, null, config.DeviceFamily,null, null, null, null, true);
                if (list.Count > 0)
                {
                    return ValidationProblem("Не может быть одновременно две и более актуальных записей в базе данных с одинаковыми" +
                    " серийными номерами в одном семействе устройств. Необходимо пометить старые записи неактуальными.");
                }
            }
            await _service.AddDeviceConfigAsync(config);
            return Ok(config.Id);
        }
        [Authorize(Roles = Roles.Admin)]
        [Authorize(Roles = Roles.APM)]
        [HttpPost("NewDevice/")]// добавить новое устройство
        public async Task<IActionResult> AddNewDevice([FromBody] Config config)
        {
            if (config.Id is not null) return ValidationProblem("ID записывать не нужно");
            var device = await _service.GetRecordMaxSerialNumberByFamilyAsync(config.DeviceFamily);
            if (device is null) return Problem($"Не существует в базе данных семейства с именем '{config.DeviceFamily}', добавьте запись в базу данных с помощью запроса 'Post REST/vx/Configurations' или добавьте запись вручную");
            else config.SerialNumber = device.SerialNumber + 1;
            await _service.AddDeviceConfigAsync(config);
            return Ok(config);
        }
        [Authorize(Roles = Roles.Admin)]
        [Authorize(Roles = Roles.APM)]
        [HttpPut]
        public async Task<IActionResult> Put([FromBody] Config config)
        {
            if (config.Id is null) return ValidationProblem("Id - null, невозможно изменить запись без этого параметра");
            if(await _service.PutAsync(config) is true)return Ok();
            else return Problem("Запись не была изменена");
        }
        //[HttpGet("Configurations/records{serialNumber}")]//получить записи по серийному номеру
        //public async Task<IActionResult> GetBySerial(ulong serialNumber)
        //{
        //    var devices = await _service.SearchAsync(1,null,serialNumber);
        //    if (devices == null) return NotFound();
        //    return Ok(devices);
        //}

        //[HttpGet("C{Id}")]//получить запись по ID
        //public async Task<IActionResult> GetByID(string Id)
        //{
        //    var device = await _service.SearchAsync(1,Id);
        //    if (device == null) return NotFound();
        //    return Ok(device);
        //}
        //[HttpGet("Configurations/record/{deviceFamily}")] //получить запись с максимальным серийным номером по названию семейства
        //public async Task<IActionResult> GetRecordForSerial(string deviceFamily)
        //{
        //    var device = await _service.GetRecordMaxSerialNumberByFamilyAsync(deviceFamily);
        //    if (device == null) return NotFound();
        //    return Ok(device);
        //}
        [HttpGet("last-serial/{deviceFamily}")] //получить последний серийный номер по названию семейства
        public async Task<IActionResult> GetLastSerial(string deviceFamily)
        {
            var device = await _service.GetRecordMaxSerialNumberByFamilyAsync(deviceFamily);
            if (device == null) return NotFound();
            return Ok(device.SerialNumber);
        }
        [HttpGet("{ID}")] //получить запись по Id
        public async Task<IActionResult> GetRecordById(string ID)
        {
            var device = await _service.GetRecordByIdAsync(ID);
            if (device == null) return NotFound();
            return Ok(device);
        }


        
        [HttpGet]//поиск записей по параметрам
        public async Task<List<Config>> Search(
            [FromQuery] int limit = 50,
            [FromQuery] uint? serialNumber = null,
            [FromQuery] string orderNumber = null,
            [FromQuery] string deviceType = null,
            [FromQuery] string deviceFamily = null,
            [FromQuery] string revision = null,
            [FromQuery] string username = null,
            [FromQuery] string date = null,
            [FromQuery] string arm = null,
            [FromQuery] bool? isActual = true)
        {
            return await _service.SearchAsync(limit, serialNumber, orderNumber, deviceType, deviceFamily, revision, username, date, arm, isActual);
        }

        [Authorize(Roles = Roles.Admin)]
        [Authorize(Roles = Roles.APM)]
        [HttpDelete("{ID}")]
        public async Task<IActionResult> Delete(string ID)
        {
            Config device = await _service.GetRecordByIdAsync(ID);
            if (device is null) return NotFound();
            //device.IsActual = false;
            if (await _service.DeleteAsync(ID) is true) return Ok();
            else return Problem("Запись не была изменена");
        }
        [HttpGet("Status/")]
        public async Task<IActionResult> Status()
        {
            return Ok("OK");
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPost("Update/")]
        public async Task<IActionResult> UpdateDocument([FromBody] UpdateRequest request)
        {
            var result = await _service.Update(request);
            Console.WriteLine("Результат" + result.ToString());
            if(result.ModifiedCount > 0) return Ok();
            else return BadRequest("Не обновленно");
        }
    }
}

