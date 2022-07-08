using API.Data;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class ErrorController : BaseApiController
    {
        private readonly DataContext dataContext;

        public ErrorController(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        [Authorize]
        [HttpGet("auth")]
        public ActionResult<string> GetSecret()
        {
            return "Secret Text";
        }

        [HttpGet("not-found")]
        public ActionResult<User> GetNotFound()
        {
            var result = dataContext.Users.Find(-1);

            if (result == null)
                return NotFound();
            else
                return Ok(result);
        }

        [HttpGet("server-error")]
        public ActionResult<string> GetServerError()
        {
            var result = dataContext.Users.Find(-1);

            var resultToReturn = result.ToString();

            return resultToReturn;
        }

        [HttpGet("bad-request")]
        public ActionResult<string> GetBadRequest()
        {
            return BadRequest("Bad Request");
        }
    }
}
