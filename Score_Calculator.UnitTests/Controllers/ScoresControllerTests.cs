#region Using Namespaces

using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;

using Score_Calculator.Models;
using Score_Calculator.Helper;
using Score_Calculator.Controllers;

using Moq;

#endregion

namespace Score_Calculator.UnitTests.Controllers
{
    [TestFixture]
    public class ScoresControllerTests
    {
        #region Internal Members

        private GamerScore _gamerScore;
        private Mock<iValidator> _mockIValidator;
        private ScoresController _scoreController;

        #endregion

        #region Setup Method

        [SetUp]
        public void Setup()
        {
            _mockIValidator = new Mock<iValidator>();
            _gamerScore     = new GamerScore { pinsDowned = new int[] { 0 } };
        }

        #endregion

        #region Test Methods

        [Test]
        [TestCase("", (int)HttpStatusCode.OK)]
        [TestCase("Error", (int)HttpStatusCode.BadRequest)]
        public async Task Calculate_ValidatorReturnsString_TestRequestStatusCode(string strResult, int expectedStatusCode)
        {
            // Arrange
            _mockIValidator.Setup(iv => iv.IsPinsDownedValid(_gamerScore.pinsDowned)).Returns(strResult);
            _scoreController = new ScoresController(_mockIValidator.Object);

            // Act
            IActionResult actionResult = await _scoreController.Calculate(_gamerScore);

            // Assert
            Assert.That((int)((ObjectResult)actionResult).StatusCode, Is.EqualTo(expectedStatusCode));
        }

        #endregion
    }
}