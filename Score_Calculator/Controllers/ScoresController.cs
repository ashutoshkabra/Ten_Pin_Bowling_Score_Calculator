#region Using Namespaces

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

using Score_Calculator.Helper;
using Score_Calculator.Models;

#endregion

namespace Score_Calculator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScoresController : ControllerBase
    {
        #region Internal Members

        private iValidator _iValidator;

        #endregion

        #region Constructors

        public ScoresController(iValidator iValidator)
        {
            _iValidator = iValidator;
        }

        #endregion

        #region AllowAnonymous Methods

        // POST: api/<ScoresController>
        [HttpPost]
        public async Task<IActionResult> Calculate([FromBody] GamerScore score)
        {
            // Check if score is valid
            string strResult = _iValidator.IsPinsDownedValid(score.pinsDowned);

            if (!strResult.Equals(string.Empty))
            {
                return await Task.Run(() => BadRequest(new
                {
                    message = $"Error: {strResult}"
                }));
            }

            return await Task.Run(() => Ok(Utils.CalculateScorePlusProgress(score.pinsDowned)));
        }

        #endregion
    }
}