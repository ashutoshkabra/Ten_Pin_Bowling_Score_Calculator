#region Using Namespaces

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Score_Calculator.Helper;
using Score_Calculator.Models;

#endregion

namespace Score_Calculator.Helper
{
    public interface iValidator
    {
        public string IsPinsDownedValid(int[] pinsDowned);
    }

    public class Validator : iValidator
    {
        public string IsPinsDownedValid(int[] pinsDowned)
        {
            // Check if pinsDowned is null
            if (pinsDowned == null)
                return "Input pinsDowned cannot be null.";

            List<BowlingFrame> lstFrames = Utils.ConvertToFrames(pinsDowned);

            // No of throws cannot be > 21
            if (pinsDowned.Count() > 21)
                return "No of throws cannot be > 21.";

            // No of throws less than 21 but frames cannot be greater than 10
            if (lstFrames.Count() > 10)
                return "No of frames cannot be > 10.";

            // If no of throws = 21, then no of strikes cannot be greater than 3 i.e. 10th frame is the only one to be allowed 3 strikes
            if (pinsDowned.Count() == 21 && pinsDowned.Where(x => x == 10).Count() > 3)
                return "In 21 throws there cannot be more than 3 strikes.";

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
    }
}