﻿#region Using Namespaces

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

        private GamerScore _gamerScore;
        private ScoresController _scoreController;

        #endregion

        #region Setup Method

        [SetUp]
        public void Setup()
        {
            _gamerScore = new GamerScore();
            _scoreController = new ScoresController();
        }

        #endregion

        #region Test Methods

        [Test]
        // Null
        [TestCase(null)]
        // No of throws cannot be > 21
        [TestCase(new int[] { 1, 8, 3, 6, 5, 4, 7, 2, 9, 0, 0, 9, 2, 7, 4, 5, 6, 3, 8, 1, 1, 8, 3, 6, 5, 4, 7, 2 })]
        // If no of throws = 21, then no of strikes cannot be greater than 3 i.e. 10th frame is the only one to be allowed 3 strikes
        [TestCase(new int[] { 1, 8, 3, 6, 5, 4, 7, 2, 9, 10, 0, 9, 2, 7, 4, 5, 6, 3, 10, 10, 10 })]
        // No of pins knocked down cannot be < 0
        [TestCase(new int[] { -5, -10, 9, 5, 10, 6, -55, -6, -10, 9, 8, 1, 3, 5, -5, -10, 0, 0, 0, 0 })]
        // No of pins knocked down cannot be > 10
        [TestCase(new int[] { 5, 20, 9, 5, 10, 6, 55, 16, 100, 9, 8, 1, 3, 5, 4, 3, 0, 0, 0, 0 })]
        // Each frame adds upto more than 10 i.e. 9 + 3 = 12 i.e. Invalid
        [TestCase(new int[] { 4, 5, 10, 10, 9, 3 })]
        [TestCase(new int[] { 6, 7, 8, 9, 5, 8 })]
        // If no of throws = 21 then check for validity of the 10th frame to have 3 throws
        [TestCase(new int[] { 1, 2, 3, 6, 4, 5, 5, 1, 8, 0, 7, 0, 7, 0, 7, 0, 7, 0, 7, 0, 1 })]
        // Test if the 10th frame's extra throw is valid only if first two throws are strike or spare
        [TestCase(new int[] { 1, 3, 10, 10, 3, 4, 5, 4, 4, 5, 3, 6, 1, 3, 5, 3, 0, 9, 1 })]
        public async Task Calculate_WhenPinsDownedIsInvalid_ReturnsBadRequest(int[] pinsDowned)
        {
            // Arrange
            _gamerScore.pinsDowned = pinsDowned;

            // Act
            IActionResult actionResult = await _scoreController.Calculate(_gamerScore);

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

        [Test]
        [TestCase(new int[] { 1, 1, 1, 1, 9, 1, 2, 8, 9, 1, 10, 10 }, "[2,4,16,35,55,*,*]")]
        [TestCase(new int[] { 1, 3, 10, 10, 3, 4, 5, 4, 4, 5, 3, 6, 1, 3, 5, 3, 0, 10 }, "[4,27,44,51,60,69,78,82,90,*]")]
        [TestCase(new int[] { 6, 4, 2, 5, 1, 6, 10, 10, 10, 1, 0, 3, 7, 3 }, "[12,19,26,56,77,88,89,102,*]")]
        [TestCase(new int[] { 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 1 }, "[30,60,90,120,150,180,210,240,261,*]")]
        [TestCase(new int[] { 1, 2, 3, 4, 5 }, "[3,10,*]")]
        public async Task Calculate_WhenProgressScoreCannotBeDetermined_ReturnsframeProgressScoreContainsAsterisk(int[] pinsDowned, string expectedResult)
        {
            // Arrange
            _gamerScore.pinsDowned = pinsDowned;

            // Act
            IActionResult actionResult = await _scoreController.Calculate(_gamerScore);

            // Assert
            Assert.That(((GameStatus)((ObjectResult)actionResult).Value).frameProgressScore, Is.EqualTo(expectedResult));
            Assert.That(((GameStatus)((ObjectResult)actionResult).Value).gameCompleted, Is.False);
        }

        [Test]
        [TestCase(new int[] { 1, 1, 1, 1, 9, 1, 2, 8, 9, 1, 5, 4, 6, 1 }, "[2,4,16,35,50,59,66]")]
        [TestCase(new int[] { 1, 3, 10, 10, 3, 4, 5, 4, 4, 5, 3, 6, 1, 3, 5, 3, 0, 10 }, "[4,27,44,51,60,69,78,82,90,*]")]
        [TestCase(new int[] { 6, 4, 2, 5, 1, 6, 10, 10, 10, 1, 0, 3, 7, 3 }, "[12,19,26,56,77,88,89,102,*]")]
        [TestCase(new int[] { 1, 2, 3, 4, 5 }, "[3,10,*]")]
        public async Task Calculate_WhenProgressScoreCanBeDeterminedButGameIncomplete_ReturnsframeProgressScoreDoesNotContainAsterisk(int[] pinsDowned, string expectedResult)
        {
            // Arrange
            _gamerScore.pinsDowned = pinsDowned;

            // Act
            IActionResult actionResult = await _scoreController.Calculate(_gamerScore);

            // Assert
            Assert.That(((GameStatus)((ObjectResult)actionResult).Value).frameProgressScore, Is.EqualTo(expectedResult));
            Assert.That(((GameStatus)((ObjectResult)actionResult).Value).gameCompleted, Is.False);
        }

        [Test]
        [TestCase(new int[] { 1, 3, 10, 10, 3, 4, 5, 4, 4, 5, 3, 6, 1, 3, 5, 3, 0, 9 }, "[4,27,44,51,60,69,78,82,90,99]")]
        [TestCase(new int[] { 3, 7, 2, 8, 1, 9, 5, 5, 9, 1, 10, 10, 7, 3, 8, 2, 10, 10, 10 }, "[12,23,38,57,77,104,124,142,162,192]")]
        public async Task Calculate_WhenProgressScoreCanBeDeterminedAndGameIsComplete_ReturnsframeProgressScoreWithGameCompletedTrue(int[] pinsDowned, string expectedResult)
        {
            // Arrange
            _gamerScore.pinsDowned = pinsDowned;

            // Act
            IActionResult actionResult = await _scoreController.Calculate(_gamerScore);

            // Assert
            Assert.That(((GameStatus)((ObjectResult)actionResult).Value).frameProgressScore, Is.EqualTo(expectedResult));
            Assert.That(((GameStatus)((ObjectResult)actionResult).Value).gameCompleted, Is.True);
        }

        #endregion
    }
}