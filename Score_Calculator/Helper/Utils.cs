#region Using Namespaces

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Score_Calculator.Models;

#endregion

namespace Score_Calculator.Helper
{
    public static class Utils
    {
        public static GameStatus CalculateScorePlusProgress(int[] pinsDowned)
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

            gameStatus.frameProgressScores = lstScores.ToArray();

            if (gameStatus.frameProgressScores.Contains("*"))
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

        public static List<BowlingFrame> ConvertToFrames(int[] pinsDowned)
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
    }
}