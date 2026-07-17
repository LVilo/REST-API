using Microsoft.AspNetCore.Mvc;
using MongoAPI.Models;

namespace MongoAPI.Controllers
{
    public class Controller : ControllerBase
    {
        public async Task<IActionResult> Try(Func<Task<Response>> func)
        {
            try
            {
                Response response = await func();
                if (response.IsOk) return Ok(response.Message);
                else return BadRequest(response.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
                return Problem(ex.Message);
            }
        }
    }
}
