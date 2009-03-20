using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;

namespace Robocup.CoreRobotics
{
    public class StubRobots : IRobots
    {
        #region IRobots Members

        public void kick(int robotID)
        {
            Console.WriteLine("RFCRobots::kick: " + robotID);
        }

        public void beamKick(int robotID) 
        {
            Console.WriteLine("RFCRobotos::beamKick: " + robotID);
        }

        public void setMotorSpeeds(int robotID, WheelSpeeds wheelSpeeds)
        {
            Console.WriteLine("RFCRobots::setMotorSpeeds: " + wheelSpeeds.lf + " "
                + wheelSpeeds.rf + " " + wheelSpeeds.lb + " " + wheelSpeeds.rb + " ");
            
        }

        #endregion
    }
}
