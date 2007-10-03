//-----------------------------------------------------------------------
//  This file is part of the Microsoft Robotics Studio Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimpleDashboardTypes.cs $ $Revision: 3 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Dss.Core.Attributes;

using Microsoft.Ccr.Core;
using Microsoft.Dss.ServiceModel.Dssp;

using dssp = Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.Services.SubscriptionManager;

using commander = Robotics.Commander.Proxy;

//[assembly: ContractNamespace(Robotics.ControlPanel.Contract.Identifier,
    //ClrNamespace = "Robotics.ControlPanel")]
namespace Robotics.ControlPanel
{

    /// <summary>
    /// DSS Contract for ControlPanel
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// The DSS Namespace for SimpleDashboard
        /// </summary>
        public const string Identifier = "http://schemas.tempuri.org/2006/08/controlpanel.html";
        private Contract()
        {
        }
    }



    /// <summary>
    /// DSS Get Definition for ControlPanel 
    /// </summary>
    class Get : Get<dssp.GetRequestType, PortSet<ControlPanelState, W3C.Soap.Fault>>
    {
        /// <summary>
        /// Default DSS Get Constructor
        /// </summary>
        public Get()
        {
        }

        /// <summary>
        /// DSS GetRequestType Constructor
        /// </summary>
        /// <param name="body"></param>
        public Get(dssp.GetRequestType body)
            : base(body)
        {
        }

    }

    /// <summary>
    /// Operation Update
    /// </summary>
    public class Update : Update<ControlPanelState, PortSet<DefaultUpdateResponseType, W3C.Soap.Fault>>
    {
        /// <summary>
        /// Default DSS Get Constructor
        /// </summary>
        public Update()
        {
        }

        /// <summary>
        /// DSS ControlPanel StateType Constructor
        /// </summary>
        /// <param name="body"></param>
        public Update(ControlPanelState body)
            : base(body)
        {
        }    
    }

    public class KickCommand : Update<KickRequest, PortSet<DefaultUpdateResponseType, W3C.Soap.Fault>>
    {

    }

    public class RefCommand : Update<RefRequest, PortSet<DefaultUpdateResponseType, W3C.Soap.Fault>>
    {
    }

    public class MoveCommand : Update<commander.MoveRequest, PortSet<DefaultUpdateResponseType, W3C.Soap.Fault>>
    {
        
    }

    public class RobotUpdateCommand : Update<RobotUpdateRequest, PortSet<DefaultUpdateResponseType, W3C.Soap.Fault>> {

    }

    /// <summary>
    /// Operation Subscribe to bumper
    /// </summary>
    public class ReliableSubscribe : Subscribe<ReliableSubscribeRequestType, PortSet<SubscribeResponseType, W3C.Soap.Fault>>
    {
    }
    public class OnUnsubscribeVision : DeleteSubscription {
    }

    


    /// <summary>
    /// DSS Replace Definition for ControlPanel 
    /// </summary>
    class Replace : Replace<ControlPanelState, PortSet<dssp.DefaultReplaceResponseType, W3C.Soap.Fault>>
    {
        /// <summary>
        /// Default DSS Get Constructor
        /// </summary>
        public Replace()
        {
        }

        /// <summary>
        /// DSS ControlPanel StateType Constructor
        /// </summary>
        /// <param name="body"></param>
        public Replace(ControlPanelState body)
            : base(body)
        {
        }       
    }

    /// <summary>
    /// The ControlPanel Operations Port
    /// </summary>
    [ServicePort]
    class ControlPanelOperations : PortSet<
        DsspDefaultLookup,
        DsspDefaultDrop,
        Get,
        Replace,
        Update,
        ReliableSubscribe,
        RobotUpdateCommand,
        MoveCommand,
        KickCommand,
        RefCommand,
        Tick>
    {
    }

    
    [DataContract]
    [DataMemberConstructor]
    public class RefRequest
    {
        int _cmd;

        [DataMember]
        public int Command
        {
            get { return _cmd; }
            set { _cmd = value; }
        }

        float _ballx;
        [DataMember]
        public float BallX
        {
            get { return _ballx; }
            set { _ballx = value; }
        }

        float _bally;
        [DataMember]
        public float BallY
        {
            get { return _bally; }
            set { _bally = value; }
        }

        public RefRequest() { }
        public RefRequest(int cmd, int x, int y)
        {
            _cmd = cmd;
            _ballx = x;
            _bally = y;
        }
    }

    [DataContract]
    [DataMemberConstructor]
    public class RobotUpdateRequest {
        /*int _id;
        float _xpos;
        float _ypos;
        float _orient;*/

        // FIX ME

            public int _x;

        public int X {
            get { return _x; }
            set { _x = value; }
        }
    

        public RobotUpdateRequest() { }
        //public RobotUpdateRequest(int id, float x, float y, float orient) {
        public RobotUpdateRequest(int x) {
            /*_id = id;
            _xpos = x;
            _ypos = y;
            _orient = orient;*/
            _x = x;
        }
    }

    [DataContract]
    [DataMemberConstructor]
    public class KickRequest
    {
        int _id;
        [DataMember]
        public int ID
        {
            get{ return _id;}
            set { _id = value; }
        }

        public KickRequest() { }

        public KickRequest(int id)
        {
            _id = id;
        }
    }

    public class Tick : Update<TickRequest, PortSet<DefaultUpdateResponseType, W3C.Soap.Fault>>
    {
    }

    [DataContract]
    public class TickRequest
    {
    }

    
}
