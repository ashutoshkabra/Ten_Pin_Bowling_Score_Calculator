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

        private const string _gutterBallGameScore = "[0,0,0,0,0,0,0,0,0,0]";
        private const string _perfectGameScore = "[30,60,90,120,150,180,210,240,270,300]";

        #endregion

        #region AllowAnonymous Methods

        // POST: api/<ScoresController>
        [HttpPost]
        public async Task<IActionResult> Calculate([FromBody] GamerScore score)
        {
            // Check if score is valid
            if (!IsPinsDownedValid(score))
            {
                return await Task.Run(() => BadRequest(new
                {
                    message = "Invalid score"
                }));
            }

            // Check for gutter-ball game
            if (IsGutterBallGame(score))
            {
                return await Task.Run(() => Ok(new GameStatus
                {
                    frameProgressScore = _gutterBallGameScore,
                    gameCompleted = true
                }));
            }

            // Check for perfect game
            if (IsPerfectGame(score))
            {
                return await Task.Run(() => Ok(new GameStatus
                {
                    frameProgressScore = _perfectGameScore,
                    gameCompleted = true
                }));
            }

            return await Task.Run(() => Ok(CalculateScorePlusProgress(score)));
        }

        #endregion

        #region Internal Methods

        protected bool IsGutterBallGame(GamerScore score)
        {
            return score.pinsDowned.Where(x => x == 0).Count() == 20;
        }

        protected bool IsPerfectGame(GamerScore score)
        {
            return score.pinsDowned.Where(x => x == 10).Count() == 12;
        }

        protected bool IsPinsDownedValid(GamerScore score)
        {
            // Check if pinsDowned is null
            if (score.pinsDowned == null)
                return false;

            // No of throws cannot be > 21
            if (score.pinsDowned.Count() > 21)
                return false;

            // If no of throws = 21, then no of strikes cannot be greater than 3 i.e. 10th frame is the only one to be allowed 3 strikes
            if (score.pinsDowned.Count() == 21 && score.pinsDowned.Where(x => x == 10).Count() > 3)
                return false;

            // No of pins knocked down cannot be < 0
            if (score.pinsDowned.Where(x => x < 0).Count() > 0)
                return false;

            // No of pins knocked down cannot be > 10
            if (score.pinsDowned.Where(x => x > 10).Count() > 0)
                return false;

            // If no of throws = 21 then check for validity of the 10th frame to have 3 throws
            if (score.pinsDowned.Count() == 21)
            {
                if ((score.pinsDowned[18] != 10 && score.pinsDowned[19] != 10) || (score.pinsDowned[18] + score.pinsDowned[19] == 10))
                    return false;
            }

            // Check for each frame not adding upto more than 10
            for (int idx = 0; idx < score.pinsDowned.Count();)
            {
                if (idx != score.pinsDowned.Count() - 1)
                {
                    // If strike check next value
                    if (score.pinsDowned[idx] == 10)
                        idx++;
                    // If current frame add upto less than 10 move to next frame
                    else if (score.pinsDowned[idx] + score.pinsDowned[idx + 1] <= 10)
                        idx += 2;
                    // Current frame add upto more than 10 i.e. invalid
                    else
                        return false;
                }
                else
                    break;
            }

            return true;
        }

        protected GameStatus CalculateScorePlusProgress(GamerScore score)
        {
            int idxFrame = 0;
            int currentScore = 0;
            StringBuilder sb = new StringBuilder();
            GameStatus gameStatus = new GameStatus { gameCompleted = false };
            List<BowlingFrame> lstFrames = ConvertToFrames(score);

            // Calculate scores
            for (; idxFrame < lstFrames.Count(); idxFrame++)
            {
                // Check if strike
                if (lstFrames[idxFrame].Throw1 == 10)
                {
                    // If last frame use the extra throw instead of calculating the next frame
                    if (idxFrame == 9)
                    {
                        if (lstFrames[idxFrame].Throw2 == null)
                        {
                            sb.Append("*,");
                        }
                        else
                        {
                            if (lstFrames[idxFrame].Throw2.Value == 10)
                            {
                                if (lstFrames[idxFrame].ExtraThrow == null)
                                {
                                    sb.Append("*,");
                                }
                                else
                                {
                                    currentScore += 20 + lstFrames[idxFrame].ExtraThrow.Value;
                                    sb.Append($"{currentScore},");
                                }
                            }
                            else
                            {
                                currentScore += 10 + lstFrames[idxFrame].Throw2.Value;
                                sb.Append($"{currentScore},");
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
                                        sb.Append($"{currentScore},");
                                    }
                                    else
                                    {
                                        sb.Append("*,");
                                    }
                                }
                                else
                                {
                                    sb.Append("*,");
                                }
                            }
                            else
                            {
                                currentScore += 10 + lstFrames[idxFrame + 1].Throw1 + lstFrames[idxFrame + 1].Throw2.Value;
                                sb.Append($"{currentScore},");
                            }
                        }
                        else
                        {
                            sb.Append("*,");
                        }
                    }
                }
                // Cannot ascertain the score of current frame
                else if (lstFrames[idxFrame].Throw2 == null)
                {
                    sb.Append("*,");
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
                                sb.Append("*,");
                            }
                            else
                            {
                                currentScore += 10 + lstFrames[idxFrame].ExtraThrow.Value;
                                sb.Append($"{currentScore},");
                            }
                        }
                        else
                        {
                            // Check if current frame + 1 is available
                            if (idxFrame + 1 <= lstFrames.Count() - 1)
                            {
                                currentScore += 10 + lstFrames[idxFrame + 1].Throw1;
                                sb.Append($"{currentScore},");
                            }
                            else
                            {
                                sb.Append("*,");
                            }
                        }
                    }
                    else
                    {
                        currentScore += lstFrames[idxFrame].Throw1 + lstFrames[idxFrame].Throw2.Value;
                        sb.Append($"{currentScore},");
                    }
                }
            }

            gameStatus.frameProgressScore = $"[{sb.ToString().TrimEnd(',')}]";

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

        protected List<BowlingFrame> ConvertToFrames(GamerScore score)
        {
            List<BowlingFrame> lstFrames = new List<BowlingFrame>();

            for (int rolls = 0; rolls < score.pinsDowned.Count();)
            {
                BowlingFrame bowlingFrame = new BowlingFrame { Throw1 = score.pinsDowned[rolls] };

                // Check if this is the 10th Frame
                if (lstFrames.Count == 9)
                {
                    if (rolls + 2 == score.pinsDowned.Count() - 1)
                    {
                        bowlingFrame.Throw2     = score.pinsDowned[rolls + 1];
                        bowlingFrame.ExtraThrow = score.pinsDowned[rolls + 2];
                    }

                    if (rolls + 1 == score.pinsDowned.Count() - 1)
                    {
                        bowlingFrame.Throw2 = score.pinsDowned[rolls + 1];
                    }

                    lstFrames.Add(bowlingFrame);
                    break;
                }

                if (score.pinsDowned[rolls] == 10)
                {
                    rolls++;
                }
                else
                {
                    if(rolls < score.pinsDowned.Count() - 1)
                        bowlingFrame.Throw2 = score.pinsDowned[rolls + 1];
                    rolls += 2;
                }

                lstFrames.Add(bowlingFrame);
            }

            return lstFrames;
        }

        #endregion
    }
}