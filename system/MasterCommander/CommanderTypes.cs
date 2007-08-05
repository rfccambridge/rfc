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

//[assembly: ContractNamespace(Robotics.Commander.Contract.Identifier,
//ClrNamespace = "Robotics.Commander")]
namespace Robotics.Commander
{

    /// <summary>
    /// DSS Contract for Commander
    /// </summary>
    public static class Contract
    {
        /// <summary>
        /// The DSS Namespace for Commander
        /// </summary>
        public const string Identifier = "http://schemas.tempuri.org/2006/08/Commander.html";
    }

    /// <summary>
    /// The Commander Operations Port
    /// </summary>
    //[ServicePort]
    public class CommanderOperations : PortSet<
        DsspDefaultLookup,
        DsspDefaultDrop,
        Get,
        Replace,
        MoveCommand,
        KickCommand>
    {
    }

    /// <summary>
    /// DSS Get Definition for Commander 
    /// </summary>
    public class Get : Get<dssp.GetRequestType, PortSet<CommanderState, W3C.Soap.Fault>>
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
    /// DSS Replace Definition for Commander 
    /// </summary>
    public class Replace : Replace<CommanderState, PortSet<dssp.DefaultReplaceResponseType, W3C.Soap.Fault>>
    {
        /// <summary>
        /// Default DSS Get Constructor
        /// </summary>
        public Replace()
        {
        }

        /// <summary>
        /// DSS Commander StateType Constructor
        /// </summary>
        /// <param name="body"></param>
        public Replace(CommanderState body)
            : base(body)
        {
        }
    }

    public class MoveCommand : Update<MoveRequest, PortSet<DefaultUpdateResponseType, W3C.Soap.Fault>>
    {

    }

    public class KickCommand : Update<KickID, PortSet<DefaultUpdateResponseType, W3C.Soap.Fault>>
    {

    }

    /*public class StopChargeCommand : Update<KickID, PortSet<DefaultUpdateResponseType, W3C.Soap.Fault>>
    {

    }*/

    [DataContract]
    [DataMemberConstructor]
    public class MoveRequest
    {
        int _robotID;
        int _leftFront;
        int _rightFront;
        int _leftBack;
        int _rightBack;

        [DataMember]
        public int ID
        {
            get { return _robotID; }
            set { _robotID = value; }
        }



        [DataMember]
        public int LeftFront
        {
            get { return _leftFront; }
            set { _leftFront = value; }
        }



        [DataMember]
        public int RightFront
        {
            get { return _rightFront; }
            set { _rightFront = value; }
        }



        [DataMember]
        public int LeftBack
        {
            get { return _leftBack; }
            set { _leftBack = value; }
        }



        [DataMember]
        public int RightBack
        {
            get { return _rightBack; }
            set { _rightBack = value; }
        }


        public MoveRequest() { }

        public MoveRequest(int id, int leftf, int rightf, int leftb, int rightb)
        {
            _leftFront = leftf * 750 / 1250;
            _rightFront = rightf * 750 / 1250;
            _leftBack = leftb * 750 / 1250;
            _rightBack = rightb * 750 / 1250;

            _robotID = id;
        }


    }



    /*public class ChargeCommand : Update<KickID, PortSet<DefaultUpdateResponseType, W3C.Soap.Fault>>
    {

    }*/


    [DataContract]
    [DataMemberConstructor]
    public class KickID
    {

        private int _id;

        [DataMember]
        public int ID
        {
            get { return _id; }
            set { _id = value; }

        }

        public KickID()
        {

        }
        public KickID(int id)
        {
            _id = id;
        }
    }


}
