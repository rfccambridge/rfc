//-----------------------------------------------------------------------
//  This file is part of the Microsoft Robotics Studio Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimpleDashboard.cs $ $Revision: 8 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;

using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Windows.Forms;

using Robocup.Core;
using Robocup.CoreRobotics;
using Robocup.Utilities;

namespace Robotics.ControlPanel
{
    //referee commands
    public enum refereecommand
    { STOP, HALT, READY, START }

    class ControlPanel
    {

        // shared access to state is protected by the interleave pattern
        // when we activate the handlers

        const int NUM_BOTS = 10;
        const int BALLID = 22;
        int curDrive = 2;
        bool ballVision = false;
        bool blueVision = false;
        bool yellowVision = false;

        RFCSystem rfcsystem = new Robocup.CoreRobotics.RFCSystem();

        #region Startup
        /// <summary>
        /// ControlPanel Service Default DSS Constuctor
        /// </summary>
        /// <param name="pCreate"></param>
        public ControlPanel()
        {
            Start();
        }

        /// <summary>
        /// Entry Point for the ControlPanel Service
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ControlPanel thisControl = new ControlPanel();
            DriveControl _driveControl = new DriveControl(thisControl);
            Application.Run(_driveControl);
        }

        void Start()
        {
            bool hasgoalie = false;
            bool haskicker = false;
            for (int i = 0; i < 9; i++)
            {
                bool hasTag;
                bool worked = Constants.nondestructiveGet<bool>("default", "ROBOT_HAS_KICKER_" + i, out hasTag);
                if (worked && hasTag)
                    haskicker = true;
                worked = Constants.nondestructiveGet<bool>("default", "ROBOT_IS_GOALIE_" + i, out hasTag);
                if (worked && hasTag)
                    hasgoalie = true;
            }
            if (!hasgoalie)
                System.Windows.Forms.MessageBox.Show("warning: no robot has been designated the goalie");
            if (!haskicker)
                System.Windows.Forms.MessageBox.Show("warning: no robots have kickers");

            //rfcsystem.registerPredictor(new TesterPredictor());
            rfcsystem.setSleepTime(Constants.get<int>("default", "UPDATE_SLEEP_TIME"));

        }

        #endregion


        #region Drive Control Event Handlers


        public void ToggleRFCHandler(bool turnOn)
        {
            if (turnOn)
            { // turn it on
                rfcsystem.start();
                Console.WriteLine("ON");
            }
            else
            {
                rfcsystem.stop();
                Console.WriteLine("OFF");
            }
        }

        public void OnChangeInputSource(string changeTo, string result)
        {

            if (changeTo.Equals("inputYellow"))
            {
                yellowVision = (result.Equals("Vision")) ? true : false;
            }
            if (changeTo.Equals("inputBlue"))
            {
                blueVision = (result.Equals("Vision")) ? true : false;
            }
            if (changeTo.Equals("inputBall"))
            {
                ballVision = (result.Equals("Vision")) ? true : false;
            }
        }


        #endregion

        #region Motor operations

        private int closestRobotTo(float x, float y, float height, float width)
        {
            float realx = 5.5f * (x / width) - 2.75f;
            float realy = 4.0f * (1 - y / height) - 2.0f;
            Vector2 position = new Robocup.Core.Vector2(realx, realy);

            /*double realx = 5.5 * (1-x / width) - 2.75;            
            double realy = 4.0 * (1-y/ height) - 2.0;*/
            Console.WriteLine("real x: " + realx + " y: " + realy + " h: " + height + " w: " + width);

            int closestDrive = 0;
            float minDist = 1000000.0f;
            float tmpDist = 1000000.0f;
            List<RobotInfo> ourInfo = rfcsystem.Predictor.getOurTeamInfo();
            for (int i = 0; i < ourInfo.Count; i++)
            {
                tmpDist = (float)position.distanceSq(ourInfo[i].Position);
                if (tmpDist < minDist)
                {
                    closestDrive = i;
                    minDist = tmpDist;
                }
            }
            List<RobotInfo> theirInfo = rfcsystem.Predictor.getTheirTeamInfo();
            for (int i = 0; i < theirInfo.Count; i++)
            {
                tmpDist = (float)position.distanceSq(theirInfo[i].Position);
                if (tmpDist < minDist)
                {
                    closestDrive = i + 5;
                    minDist = tmpDist;
                }
            }
            if (position.distanceSq(rfcsystem.Predictor.getBallInfo().Position) < minDist)
            {
                //ballSelected = true;
            }
            else
            {
                //ballSelected = false;
            }
            if (minDist < 0.20)
                return closestDrive;
            else
                return curDrive;
        }

        public void sendMove(int ID, int leftFront, int rightFront, int leftBack, int rightBack)
        {
            bool usBlue = Constants.get<string>("default", "OUR_TEAM_COLOR") == "BLUE";
        }


        private void addTags(RobotInfo info)
        {
            bool hasTag;
            bool worked = Constants.nondestructiveGet<bool>("default", "ROBOT_HAS_KICKER_" + info.ID, out hasTag);
            if (worked && hasTag)
                info.Tags.Add("kicker");
            worked = Constants.nondestructiveGet<bool>("default", "ROBOT_IS_GOALIE_" + info.ID, out hasTag);
            if (worked && hasTag)
                info.Tags.Add("goalie");
        }


        #endregion

    }

}
