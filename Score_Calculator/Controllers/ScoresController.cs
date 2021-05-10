#region Using Namespaces

using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

using Score_Calculator.Models;

#endregion

namespace Score_Calculator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScoresController : ControllerBase
    {
        #region Internal Members

        private readonly string[] _gutterBallGameScore  = { "0","0","0","0","0","0","0","0","0","0"};
        private readonly string[] _perfectGameScore     = {"30","60","90","120","150","180","210","240","270","300"};

        #endregion

        #region AllowAnonymous Methods

        // POST: api/<ScoresController>
        [HttpPost]
        public async Task<IActionResult> Calculate([FromBody] GamerScore score)
        {
            // Check if score is valid
            string strResult = IsPinsDownedValid(score.pinsDowned);

            if (!strResult.Equals(string.Empty))
            {
                return await Task.Run(() => BadRequest(new
                {
                    message = $"Error: {strResult}"
                }));
            }

            // Check for gutter-ball game
            if (IsGutterBallGame(score.pinsDowned))
            {
                return await Task.Run(() => Ok(new GameStatus
                {
                    frameProgressScore = _gutterBallGameScore,
                    gameCompleted = true
                }));
            }

            // Check for perfect game
            if (IsPerfectGame(score.pinsDowned))
            {
                return await Task.Run(() => Ok(new GameStatus
                {
                    frameProgressScore = _perfectGameScore,
                    gameCompleted = true
                }));
            }

            return await Task.Run(() => Ok(CalculateScorePlusProgress(score.pinsDowned)));
        }

        #endregion

        #region Internal Methods

        protected bool IsGutterBallGame(int[] pinsDowned)
        {
            return pinsDowned.Where(x => x == 0).Count() == 20;
        }

        protected bool IsPerfectGame(int[] pinsDowned)
        {
            return pinsDowned.Where(x => x == 10).Count() == 12;
        }

        protected string IsPinsDownedValid(int[] pinsDowned)
        {
            // Check if pinsDowned is null
            if (pinsDowned == null)
                return "Input pinsDowned cannot be null.";

            List<BowlingFrame> lstFrames = ConvertToFrames(pinsDowned);

            // No of throws cannot be > 21
            if (pinsDowned.Count() > 21)
                return "No of throws cannot be > 21.";

            // No of throws less than 21 but frames cannot be greater than 10
            if (lstFrames.Count() > 10)
                return "No of frames cannot be > 10.";

            // If no of throws = 21, then no of strikes cannot be greater than 3 i.e. 10th frame is the only one to be allowed 3 strikes
            if (pinsDowned.Count() == 21 && pinsDowned.Where(x => x == 10).Count() > 3)
                return "In 21 throws there cannot be more than 3 strikes that too at the end.";

            // No of pins knocked down cannot be < 0
            if (pinsDowned.Where(x => x < 0).Count() > 0)
                return "pinsDowned cannot be < 0.";

            // No of pins knocked down cannot be > 10
            if (pinsDowned.Where(x => x > 10).Count() > 0)
                return "pinsDowned cannot be > 10.";

            // Check for each frame not adding upto more than 10
            for (int idx = 0; idx < pinsDowned.Count();)
            {
                if (idx != pinsDowned.Count() - 1)
                {
                    // If strike check next value
                    if (pinsDowned[idx] == 10)
                        idx++;
                    // If current frame add upto less than 10 move to next frame
                    else if (pinsDowned[idx] + pinsDowned[idx + 1] <= 10)
                        idx += 2;
                    // Current frame add upto more than 10 i.e. invalid
                    else
                        return "Frame total cannot be > 10.";
                }
                else
                    break;
            }

            // Check if the 10th frame's extra throw is valid only if first two throws are strike or spare
            if (lstFrames.Count() == 10)
            {
                if (lstFrames[9].ExtraThrow != null)
                {
                    if ((lstFrames[9].Throw1 == 10 && lstFrames[9].Throw2 == 10) || (lstFrames[9].Throw1 + lstFrames[9].Throw2.Value == 10))
                    {
                    }
                    else
                    {
                        return "10th frame cannot have an extra throw.";
                    }
                }
            }

            return string.Empty;
        }

        protected GameStatus CalculateScorePlusProgress(int[] pinsDowned)
        {
            int idxFrame = 0;
            int currentScore = 0;
            List<string> lstScores = new List<string>();
            GameStatus gameStatus = new GameStatus { gameCompleted = false };
            List<BowlingFrame> lstFrames = ConvertToFrames(pinsDowned);

            // Calculate scores
            for (; idxFrame < lstFrames.Count(); idxFrame++)
            {
                // Check if strike
                if (lstFrames[idxFrame].Throw1 == 10)
                {
                    // If last frame use the extra throw instead of calculating the next frame
                    if (idxFrame == 9)
                    {
                        // If throw2 is null then frame cannot be determined 
                        if (lstFrames[idxFrame].Throw2 == null)
                        {
                            lstScores.Add("*");
                        }
                        else
                        {
                            if (lstFrames[idxFrame].Throw1 + lstFrames[idxFrame].Throw2.Value < 10)
                            {
                                currentScore += lstFrames[idxFrame].Throw1 + lstFrames[idxFrame].Throw2.Value;
                            }
                            else
                            {
                                // If extra throw is null then frame cannot be determined
                                if (lstFrames[idxFrame].ExtraThrow == null)
                                {
                                    lstScores.Add("*");
                                }
                                else
                                {
                                    currentScore += lstFrames[idxFrame].Throw1 + lstFrames[idxFrame].Throw2.Value + lstFrames[idxFrame].ExtraThrow.Value;
                                    lstScores.Add($"{currentScore}");
                                }
                            }
                        }
                    }
                    else
                    {
                        // Check if current frame + 1 is available
                        if (idxFrame + 1 <= lstFrames.Count() - 1)
                        {
                            // Check if throw2 is null
                            if (lstFrames[idxFrame + 1].Throw2 == null)
                            {
                                // Check if current frame + 1 is also strike
                                if (lstFrames[idxFrame + 1].Throw1 == 10)
                                {
                                    // Check if current frame + 2 is available
                                    if (idxFrame + 2 <= lstFrames.Count() - 1)
                                    {
                                        currentScore += 20 + lstFrames[idxFrame + 2].Throw1;
                                        lstScores.Add($"{currentScore}");
                                    }
                                    else
                                    {
                                        lstScores.Add("*");
                                    }
                                }
                                else
                                {
                                    lstScores.Add("*");
                                }
                            }
                            else
                            {
                                currentScore += 10 + lstFrames[idxFrame + 1].Throw1 + lstFrames[idxFrame + 1].Throw2.Value;
                                lstScores.Add($"{currentScore}");
                            }
                        }
                        else
                        {
                            lstScores.Add("*");
                        }
                    }
                }
                // Cannot ascertain the score of current frame
                else if (lstFrames[idxFrame].Throw2 == null)
                {
                    lstScores.Add("*");
                }
                else
                {
                    // Check if spare
                    if (lstFrames[idxFrame].Throw1 + lstFrames[idxFrame].Throw2.Value == 10)
                    {
                        // If last frame use the extra throw instead of calculating the next frame
                        if (idxFrame == 9)
                        {
                            if (lstFrames[idxFrame].ExtraThrow == null)
                            {
                                lstScores.Add("*");
                            }
                            else
                            {
                                currentScore += 10 + lstFrames[idxFrame].ExtraThrow.Value;
                                lstScores.Add($"{currentScore}");
                            }
                        }
                        else
                        {
                            // Check if current frame + 1 is available
                            if (idxFrame + 1 <= lstFrames.Count() - 1)
                            {
                                currentScore += 10 + lstFrames[idxFrame + 1].Throw1;
                                lstScores.Add($"{currentScore}");
                            }
                            else
                            {
                                lstScores.Add("*");
                            }
                        }
                    }
                    else
                    {
                        currentScore += lstFrames[idxFrame].Throw1 + lstFrames[idxFrame].Throw2.Value;
                        lstScores.Add($"{currentScore}");
                    }
                }
            }

            gameStatus.frameProgressScore = lstScores.ToArray();

            if (gameStatus.frameProgressScore.Contains("*"))
            {
                gameStatus.gameCompleted = false;
            }
            else
            {
                if (idxFrame == 10)
                {
                    gameStatus.gameCompleted = true;
                }
            }

            return gameStatus;
        }

        protected List<BowlingFrame> ConvertToFrames(int[] pinsDowned)
        {
            List<BowlingFrame> lstFrames = new List<BowlingFrame>();

            for (int rolls = 0; rolls < pinsDowned.Count();)
            {
                BowlingFrame bowlingFrame = new BowlingFrame { Throw1 = pinsDowned[rolls] };

                // Check if this is the 10th Frame
                if (lstFrames.Count >= 9)
                {
                    if (rolls + 2 == pinsDowned.Count() - 1)
                    {
                        bowlingFrame.Throw2 = pinsDowned[rolls + 1];
                        bowlingFrame.ExtraThrow = pinsDowned[rolls + 2];
                        lstFrames.Add(bowlingFrame);
                        break;
                    }
                    // If last 1
                    else if (rolls + 1 == pinsDowned.Count() - 1)
                    {
                        bowlingFrame.Throw2 = pinsDowned[rolls + 1];
                        lstFrames.Add(bowlingFrame);
                        break;
                    }
                    else
                    {
                        if (pinsDowned[rolls] == 10)
                        {
                            rolls++;
                        }
                        else
                        {
                            if (rolls < pinsDowned.Count() - 1)
                                bowlingFrame.Throw2 = pinsDowned[rolls + 1];
                            rolls += 2;
                        }

                        lstFrames.Add(bowlingFrame);
                    }
                }
                else
                {
                    if (pinsDowned[rolls] == 10)
                    {
                        rolls++;
                    }
                    else
                    {
                        if (rolls < pinsDowned.Count() - 1)
                            bowlingFrame.Throw2 = pinsDowned[rolls + 1];
                        rolls += 2;
                    }

                    lstFrames.Add(bowlingFrame);
                }
            }

            return lstFrames;
        }

        #endregion
    }
}