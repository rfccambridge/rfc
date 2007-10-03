//-----------------------------------------------------------------------
//  This file is part of the Microsoft Robotics Studio Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimpleDashboardState.cs $ $Revision: 2 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Dss.Core.Attributes;
using drive = Microsoft.Robotics.Services.Drive.Proxy;
using sicklrf = Microsoft.Robotics.Services.Sensors.SickLRF.Proxy;
using Microsoft.Robotics.Simulation.Engine.Proxy;
using Microsoft.Robotics.Simulation.Proxy;
using Microsoft.Robotics.PhysicalModel.Proxy;
using Vision.Proxy;

namespace Robotics.ControlPanel
{
    /// <summary>
    /// ControlPanel StateType
    /// </summary>
    [DataContract]
    public class ControlPanelState
    {
        public const int NUM_BOTS = 10;
        [DataMember]
        public bool Log;
        [DataMember]
        public string LogFile;


        public void update() {
        }

        float anglechop(float input) {
            while (input > Math.PI * 2) {
                input -= (float)Math.PI * 2;
            }
            while (input < 0) {
                input += (float)Math.PI * 2;
            }
            return input;
        }
    }
}
