using System;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.Transforms;

[assembly: ServiceDeclaration(DssServiceDeclaration.Transform, SourceAssemblyKey = @"Commander.Y2006.M08, Version=0.0.0.0, Culture=neutral, PublicKeyToken=ff207bcee59b1d39")]
[assembly: System.Security.SecurityTransparent]
[assembly: System.Security.AllowPartiallyTrustedCallers]

namespace Dss.Transforms.TransformCommander
{

    public class Transforms: TransformBase
    {

        public static object Transform_Robotics_Commander_Proxy_CommanderState_Robotics_Commander_CommanderState(object transformObj)
        {
            Robotics.Commander.CommanderState target = new Robotics.Commander.CommanderState();
            Robotics.Commander.Proxy.CommanderState from = transformObj as Robotics.Commander.Proxy.CommanderState;
            target.Log = from.Log;
            target.LogFile = from.LogFile;
            return target;
        }


        public static object Transform_Robotics_Commander_CommanderState_Robotics_Commander_Proxy_CommanderState(object transformObj)
        {
            Robotics.Commander.Proxy.CommanderState target = new Robotics.Commander.Proxy.CommanderState();
            Robotics.Commander.CommanderState from = transformObj as Robotics.Commander.CommanderState;
            target.Log = from.Log;
            target.LogFile = from.LogFile;
            return target;
        }


        public static object Transform_Robotics_Commander_Proxy_MoveRequest_Robotics_Commander_MoveRequest(object transformObj)
        {
            Robotics.Commander.MoveRequest target = new Robotics.Commander.MoveRequest();
            Robotics.Commander.Proxy.MoveRequest from = transformObj as Robotics.Commander.Proxy.MoveRequest;
            target.ID = from.ID;
            target.LeftFront = from.LeftFront;
            target.RightFront = from.RightFront;
            target.LeftBack = from.LeftBack;
            target.RightBack = from.RightBack;
            return target;
        }


        public static object Transform_Robotics_Commander_MoveRequest_Robotics_Commander_Proxy_MoveRequest(object transformObj)
        {
            Robotics.Commander.Proxy.MoveRequest target = new Robotics.Commander.Proxy.MoveRequest();
            Robotics.Commander.MoveRequest from = transformObj as Robotics.Commander.MoveRequest;
            target.ID = from.ID;
            target.LeftFront = from.LeftFront;
            target.RightFront = from.RightFront;
            target.LeftBack = from.LeftBack;
            target.RightBack = from.RightBack;
            return target;
        }


        public static object Transform_Robotics_Commander_Proxy_KickID_Robotics_Commander_KickID(object transformObj)
        {
            Robotics.Commander.KickID target = new Robotics.Commander.KickID();
            Robotics.Commander.Proxy.KickID from = transformObj as Robotics.Commander.Proxy.KickID;
            target.ID = from.ID;
            return target;
        }


        public static object Transform_Robotics_Commander_KickID_Robotics_Commander_Proxy_KickID(object transformObj)
        {
            Robotics.Commander.Proxy.KickID target = new Robotics.Commander.Proxy.KickID();
            Robotics.Commander.KickID from = transformObj as Robotics.Commander.KickID;
            target.ID = from.ID;
            return target;
        }

        static Transforms()
        {
            AddProxyTransform(typeof(Robotics.Commander.Proxy.CommanderState), Transform_Robotics_Commander_Proxy_CommanderState_Robotics_Commander_CommanderState);
            AddSourceTransform(typeof(Robotics.Commander.CommanderState), Transform_Robotics_Commander_CommanderState_Robotics_Commander_Proxy_CommanderState);
            AddProxyTransform(typeof(Robotics.Commander.Proxy.MoveRequest), Transform_Robotics_Commander_Proxy_MoveRequest_Robotics_Commander_MoveRequest);
            AddSourceTransform(typeof(Robotics.Commander.MoveRequest), Transform_Robotics_Commander_MoveRequest_Robotics_Commander_Proxy_MoveRequest);
            AddProxyTransform(typeof(Robotics.Commander.Proxy.KickID), Transform_Robotics_Commander_Proxy_KickID_Robotics_Commander_KickID);
            AddSourceTransform(typeof(Robotics.Commander.KickID), Transform_Robotics_Commander_KickID_Robotics_Commander_Proxy_KickID);
        }
    }
}

