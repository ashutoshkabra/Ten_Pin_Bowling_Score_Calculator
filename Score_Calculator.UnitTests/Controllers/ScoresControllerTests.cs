#region Using Namespaces

using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;

using Score_Calculator.Models;
using Score_Calculator.Controllers;

#endregion

namespace Score_Calculator.UnitTests.Controllers
{
    [TestFixture]
    public class ScoresControllerTests
    {
        #region Internal Members

        private ScoresController _scoreController;

        private static IEnumerable<GamerScore> InvalidPinsDowned
        {
            get
            {
                // Null
                yield return new GamerScore { pinsDowned = null };
                
                // No of throws cannot be > 21
                yield return new GamerScore { pinsDowned = new int[] { 1, 8, 3, 6, 5, 4, 7, 2, 9, 0, 0, 9, 2, 7, 4, 5, 6, 3, 8, 1, 1, 8, 3, 6, 5, 4, 7, 2 } };
                
                // If no of throws = 21, then no of strikes cannot be greater than 3 i.e. 10th frame is the only one to be allowed 3 strikes
                yield return new GamerScore { pinsDowned = new int[] { 1, 8, 3, 6, 5, 4, 7, 2, 9, 10, 0, 9, 2, 7, 4, 5, 6, 3, 10, 10, 10 } };
                
                // No of pins knocked down cannot be < 0
                yield return new GamerScore { pinsDowned = new int[] { -5, -10, 9, 5, 10, 6, -55, -6, -10, 9, 8, 1, 3, 5, -5, -10, 0, 0, 0, 0 } };
                
                // No of pins knocked down cannot be > 10
                yield return new GamerScore { pinsDowned = new int[] { 5, 20, 9, 5, 10, 6, 55, 16, 100, 9, 8, 1, 3, 5, 4, 3, 0, 0, 0, 0 } };
                
                // Each frame adds upto more than 10 i.e. 9 + 3 = 12 i.e. Invalid
                yield return new GamerScore { pinsDowned = new int[] { 4, 5, 10, 10, 9, 3 } };
                
                // If no of throws = 21 then check for validity of the 10th frame to have 3 throws
                yield return new GamerScore { pinsDowned = new int[] { 1, 2, 3, 6, 4, 5, 5, 1, 8, 0, 7, 0, 7, 0, 7, 0, 7, 0, 7, 0, 1 } };

            }
        }

        #endregion

        #region Setup Method

        [SetUp]
        public void Setup()
        {
            _scoreController = new ScoresController();
        }

        #endregion

        #region Test Methods

        [Test]
        [TestCaseSource(nameof(InvalidPinsDowned))]
        public async Task Calculate_WhenPinsDownedIsInvalid_ReturnsBadRequest(GamerScore score)
        {
            // Arrange
            
            // Act
            IActionResult actionResult = await _scoreController.Calculate(score);

            // Assert
            Assert.That((int)((ObjectResult)actionResult).StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task Calculate_WhenPinsDownedIsAllGutter_ReturnsGutterBallGameScore()
        {
            // Arrange
            GamerScore gamer = new GamerScore { pinsDowned = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } };

            // Act
            IActionResult actionResult = await _scoreController.Calculate(gamer);

            // Assert
            Assert.That(((GameStatus)((ObjectResult)actionResult).Value).frameProgressScore, Is.EqualTo("[0,0,0,0,0,0,0,0,0,0]"));
            Assert.That(((GameStatus)((ObjectResult)actionResult).Value).gameCompleted, Is.True);
        }

        [Test]
        public async Task Calculate_WhenPinsDownedIsAllPerfect_ReturnsPerfectGameScore()
        {
            // Arrange
            GamerScore gamer = new GamerScore { pinsDowned = new int[] { 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10 } };

            // Act
            IActionResult actionResult = await _scoreController.Calculate(gamer);

            // Assert
            Assert.That(((GameStatus)((ObjectResult)actionResult).Value).frameProgressScore, Is.EqualTo("[30,60,90,120,150,180,210,240,270,300]"));
            Assert.That(((GameStatus)((ObjectResult)actionResult).Value).gameCompleted, Is.True);
        }

        #endregion
    }
}