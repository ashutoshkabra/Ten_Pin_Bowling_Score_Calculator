#region Using Namespaces

using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using NUnit.Framework;

using Score_Calculator.Helper;
using Score_Calculator.Models;

#endregion

namespace Score_Calculator.UnitTests.Helper
{
    [TestFixture]
    public class UtilsTest
    {
        #region Internal Members

        Validator _validator;

        #endregion

        #region Setup Method

        [SetUp]
        public void Setup()
        {
            _validator = new Validator();
        }

        #endregion

        #region Test Methods

        [Test]
        // Null
        [TestCase(null, "Input pinsDowned cannot be null.")]
        // No of throws cannot be > 21
        [TestCase(new int[] { 1, 8, 3, 6, 5, 4, 7, 2, 9, 0, 0, 9, 2, 7, 4, 5, 6, 3, 8, 1, 1, 8, 3, 6, 5, 4, 7, 2 }, "No of throws cannot be > 21.")]
        // No of throws less than 21 but frames cannot be greater than 10
        [TestCase(new int[] { 10, 10, 10, 10, 10, 10, 10, 10, 10, 1, 1, 1, 1, 1, 1, 1 }, "No of frames cannot be > 10.")]
        // If no of throws = 21, then no of strikes cannot be greater than 3 i.e. 10th frame is the only one to be allowed 3 strikes
        [TestCase(new int[] { 1, 8, 3, 6, 5, 4, 7, 2, 9, 10, 0, 9, 2, 7, 4, 5, 6, 3, 10, 10, 10 }, "In 21 throws there cannot be more than 3 strikes.")]
        // No of pins knocked down cannot be < 0
        [TestCase(new int[] { -5, -10, 9, 5, 10, 6, -55, -6, -10, 9, 8, 1, 3, 5, -5, -10, 0, 0, 0, 0 }, "pinsDowned cannot be < 0.")]
        // No of pins knocked down cannot be > 10
        [TestCase(new int[] { 5, 20, 9, 5, 10, 6, 55, 16, 100, 9, 8, 1, 3, 5, 4, 3, 0, 0, 0, 0 }, "pinsDowned cannot be > 10.")]
        // Each frame adds upto more than 10 i.e. 9 + 3 = 12 i.e. Invalid
        [TestCase(new int[] { 4, 5, 10, 10, 9, 3 }, "Frame total cannot be > 10.")]
        [TestCase(new int[] { 6, 7, 8, 9, 5, 8 }, "Frame total cannot be > 10.")]
        // If no of throws = 21 then check for validity of the 10th frame to have 3 throws
        [TestCase(new int[] { 1, 2, 3, 6, 4, 5, 5, 1, 8, 0, 7, 0, 7, 0, 7, 0, 7, 0, 7, 0, 1 }, "10th frame cannot have an extra throw.")]
        // Test if the 10th frame's extra throw is valid only if first two throws are strike or spare
        [TestCase(new int[] { 1, 3, 10, 10, 3, 4, 5, 4, 4, 5, 3, 6, 1, 3, 5, 3, 0, 9, 1 }, "10th frame cannot have an extra throw.")]
        public void Calculate_WhenPinsDownedIsInvalid_ReturnsBadRequest(int[] pinsDowned, string expectedResult)
        {
            // Arrange

            // Act
            string strResult = _validator.IsPinsDownedValid(pinsDowned);

            // Assert
            Assert.That(strResult, Is.EqualTo(expectedResult));
        }

        [Test]
        [TestCase(new int[] { 1, 1, 1, 1, 9, 1, 2, 8, 9, 1, 10, 10 }, new string[] { "2", "4", "16", "35", "55", "*", "*" })]
        [TestCase(new int[] { 1, 3, 10, 10, 3, 4, 5, 4, 4, 5, 3, 6, 1, 3, 5, 3, 0, 10 }, new string[] {"4", "27", "44", "51", "60", "69", "78", "82", "90", "*"})]
        [TestCase(new int[] { 6, 4, 2, 5, 1, 6, 10, 10, 10, 1, 0, 3, 7, 3 }, new string[] { "12", "19", "26", "56", "77", "88", "89", "102", "*" })]
        [TestCase(new int[] { 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 1 }, new string[] { "30", "60", "90", "120", "150", "180", "210", "240", "261", "*" })]
        [TestCase(new int[] { 1, 2, 3, 4, 5 }, new string[] { "3", "10", "*" })]
        public void Calculate_WhenProgressScoreCannotBeDetermined_ReturnsframeProgressScoreContainsAsterisk(int[] pinsDowned, string[] expectedResult)
        {
            // Arrange

            // Act
            GameStatus gameStatus = Utils.CalculateScorePlusProgress(pinsDowned);

            // Assert
            Assert.That(gameStatus.frameProgressScores, Is.EquivalentTo(expectedResult));
            Assert.That(gameStatus.gameCompleted, Is.False);
        }

        [Test]
        [TestCase(new int[] { 1, 1, 1, 1, 9, 1, 2, 8, 9, 1, 5, 4, 6, 1 }, new string[] { "2", "4", "16", "35", "50", "59", "66" })]
        [TestCase(new int[] { 1, 3, 10, 10, 3, 4, 5, 4, 4, 5, 3, 6, 1, 3, 5, 3, 0, 10 }, new string[] { "4", "27", "44", "51", "60", "69", "78", "82", "90", "*" })]
        [TestCase(new int[] { 6, 4, 2, 5, 1, 6, 10, 10, 10, 1, 0, 3, 7, 3 }, new string[] { "12", "19", "26", "56", "77", "88", "89", "102", "*" })]
        [TestCase(new int[] { 1, 2, 3, 4, 5 }, new string[] { "3", "10", "*" })]
        public void Calculate_WhenProgressScoreCanBeDeterminedButGameIncomplete_ReturnsframeProgressScoreDoesNotContainAsterisk(int[] pinsDowned, string[] expectedResult)
        {
            // Arrange

            // Act
            GameStatus gameStatus = Utils.CalculateScorePlusProgress(pinsDowned);

            // Assert
            Assert.That(gameStatus.frameProgressScores, Is.EquivalentTo(expectedResult));
            Assert.That(gameStatus.gameCompleted, Is.False);
        }

        [Test]
        [TestCase(new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, new string[] { "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" })]
        [TestCase(new int[] { 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10 }, new string[] { "30", "60", "90", "120", "150", "180", "210", "240", "270", "300" })]
        [TestCase(new int[] { 1, 3, 10, 10, 3, 4, 5, 4, 4, 5, 3, 6, 1, 3, 5, 3, 0, 9 }, new string[] { "4", "27", "44", "51", "60", "69", "78", "82", "90", "99" })]
        [TestCase(new int[] { 3, 7, 2, 8, 1, 9, 5, 5, 9, 1, 10, 10, 7, 3, 8, 2, 10, 10, 10 }, new string[] { "12", "23", "38", "57", "77", "104", "124", "142", "162", "192" })]
        [TestCase(new int[] { 9, 1, 9, 1, 9, 1, 9, 1, 9, 1, 9, 1, 9, 1, 9, 1, 9, 1, 9, 1, 9 }, new string[] { "19", "38", "57", "76", "95", "114", "133", "152", "171", "190" })]
        public void Calculate_WhenProgressScoreCanBeDeterminedAndGameIsComplete_ReturnsframeProgressScoreWithGameCompletedTrue(int[] pinsDowned, string[] expectedResult)
        {
            // Arrange

            // Act
            GameStatus gameStatus = Utils.CalculateScorePlusProgress(pinsDowned);

            // Assert
            Assert.That(gameStatus.frameProgressScores, Is.EquivalentTo(expectedResult));
            Assert.That(gameStatus.gameCompleted, Is.True);
        }

        #endregion
    }
}