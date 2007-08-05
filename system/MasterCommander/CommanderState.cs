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

namespace Robotics.Commander
{
    /// <summary>
    /// Commander StateType
    /// </summary>
    [DataContract]
    public class CommanderState
    {
        [DataMember]
        public bool Log;
        [DataMember]
        public string LogFile;
    }
}
