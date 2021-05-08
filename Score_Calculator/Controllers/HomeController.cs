#region Using Namespaces

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

#endregion

namespace Score_Calculator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        #region AllowAnonymous Methods

        // GET: api/<HomeController>
        [HttpGet]
        public async Task<string> Get()
        {
            return await Task.Run(() => "Ten-Pin Bowling: Score Calculator API");
        }

        #endregion
    }
}