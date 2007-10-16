using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;

namespace Robotics.ControlPanel
{
    class ControlPanelRobots : IRobots
    {
        ControlPanelService _control;

        public ControlPanelRobots(ControlPanelService control)
        {
            _control = control;
        }



        #region IRobots Members

        public void setMotorSpeeds(int robotID, WheelSpeeds wheelSpeeds)
        {
            _control.sendMove(robotID, wheelSpeeds.lf, wheelSpeeds.rf, wheelSpeeds.lb, wheelSpeeds.rb);
        }

        public void kick(int robotID)
        {
            _control.sendKick(robotID);
        }

        #endregion
    }
}
