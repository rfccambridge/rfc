using System;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.Transforms;

[assembly: ServiceDeclaration(DssServiceDeclaration.Transform, SourceAssemblyKey = @"ControlPanel.Y2006.M08, Version=0.0.0.0, Culture=neutral, PublicKeyToken=66359fb00c7dbe93")]
[assembly: System.Security.SecurityTransparent]
[assembly: System.Security.AllowPartiallyTrustedCallers]

namespace Dss.Transforms.TransformControlPanel
{

    public class Transforms: TransformBase
    {

        public static object Transform_Robotics_ControlPanel_Proxy_ControlPanelState_Robotics_ControlPanel_ControlPanelState(object transformObj)
        {
            Robotics.ControlPanel.ControlPanelState target = new Robotics.ControlPanel.ControlPanelState();
            Robotics.ControlPanel.Proxy.ControlPanelState from = transformObj as Robotics.ControlPanel.Proxy.ControlPanelState;
            target.Log = from.Log;
            target.LogFile = from.LogFile;
            return target;
        }


        public static object Transform_Robotics_ControlPanel_ControlPanelState_Robotics_ControlPanel_Proxy_ControlPanelState(object transformObj)
        {
            Robotics.ControlPanel.Proxy.ControlPanelState target = new Robotics.ControlPanel.Proxy.ControlPanelState();
            Robotics.ControlPanel.ControlPanelState from = transformObj as Robotics.ControlPanel.ControlPanelState;
            target.Log = from.Log;
            target.LogFile = from.LogFile;
            return target;
        }


        public static object Transform_Robotics_ControlPanel_Proxy_RobotUpdateRequest_Robotics_ControlPanel_RobotUpdateRequest(object transformObj)
        {
            Robotics.ControlPanel.RobotUpdateRequest target = new Robotics.ControlPanel.RobotUpdateRequest();
            return target;
        }


        private static Robotics.ControlPanel.Proxy.RobotUpdateRequest _instance_Robotics_ControlPanel_Proxy_RobotUpdateRequest = new Robotics.ControlPanel.Proxy.RobotUpdateRequest();
        public static object Transform_Robotics_ControlPanel_RobotUpdateRequest_Robotics_ControlPanel_Proxy_RobotUpdateRequest(object transformObj)
        {
            return _instance_Robotics_ControlPanel_Proxy_RobotUpdateRequest;
        }


        public static object Transform_Robotics_ControlPanel_Proxy_KickRequest_Robotics_ControlPanel_KickRequest(object transformObj)
        {
            Robotics.ControlPanel.KickRequest target = new Robotics.ControlPanel.KickRequest();
            Robotics.ControlPanel.Proxy.KickRequest from = transformObj as Robotics.ControlPanel.Proxy.KickRequest;
            target.ID = from.ID;
            return target;
        }


        public static object Transform_Robotics_ControlPanel_KickRequest_Robotics_ControlPanel_Proxy_KickRequest(object transformObj)
        {
            Robotics.ControlPanel.Proxy.KickRequest target = new Robotics.ControlPanel.Proxy.KickRequest();
            Robotics.ControlPanel.KickRequest from = transformObj as Robotics.ControlPanel.KickRequest;
            target.ID = from.ID;
            return target;
        }


        public static object Transform_Robotics_ControlPanel_Proxy_RefRequest_Robotics_ControlPanel_RefRequest(object transformObj)
        {
            Robotics.ControlPanel.RefRequest target = new Robotics.ControlPanel.RefRequest();
            Robotics.ControlPanel.Proxy.RefRequest from = transformObj as Robotics.ControlPanel.Proxy.RefRequest;
            target.Command = from.Command;
            target.BallX = from.BallX;
            target.BallY = from.BallY;
            return target;
        }


        public static object Transform_Robotics_ControlPanel_RefRequest_Robotics_ControlPanel_Proxy_RefRequest(object transformObj)
        {
            Robotics.ControlPanel.Proxy.RefRequest target = new Robotics.ControlPanel.Proxy.RefRequest();
            Robotics.ControlPanel.RefRequest from = transformObj as Robotics.ControlPanel.RefRequest;
            target.Command = from.Command;
            target.BallX = from.BallX;
            target.BallY = from.BallY;
            return target;
        }


        public static object Transform_Robotics_ControlPanel_Proxy_TickRequest_Robotics_ControlPanel_TickRequest(object transformObj)
        {
            Robotics.ControlPanel.TickRequest target = new Robotics.ControlPanel.TickRequest();
            return target;
        }


        private static Robotics.ControlPanel.Proxy.TickRequest _instance_Robotics_ControlPanel_Proxy_TickRequest = new Robotics.ControlPanel.Proxy.TickRequest();
        public static object Transform_Robotics_ControlPanel_TickRequest_Robotics_ControlPanel_Proxy_TickRequest(object transformObj)
        {
            return _instance_Robotics_ControlPanel_Proxy_TickRequest;
        }

        static Transforms()
        {
            AddProxyTransform(typeof(Robotics.ControlPanel.Proxy.ControlPanelState), Transform_Robotics_ControlPanel_Proxy_ControlPanelState_Robotics_ControlPanel_ControlPanelState);
            AddSourceTransform(typeof(Robotics.ControlPanel.ControlPanelState), Transform_Robotics_ControlPanel_ControlPanelState_Robotics_ControlPanel_Proxy_ControlPanelState);
            AddProxyTransform(typeof(Robotics.ControlPanel.Proxy.RobotUpdateRequest), Transform_Robotics_ControlPanel_Proxy_RobotUpdateRequest_Robotics_ControlPanel_RobotUpdateRequest);
            AddSourceTransform(typeof(Robotics.ControlPanel.RobotUpdateRequest), Transform_Robotics_ControlPanel_RobotUpdateRequest_Robotics_ControlPanel_Proxy_RobotUpdateRequest);
            AddProxyTransform(typeof(Robotics.ControlPanel.Proxy.KickRequest), Transform_Robotics_ControlPanel_Proxy_KickRequest_Robotics_ControlPanel_KickRequest);
            AddSourceTransform(typeof(Robotics.ControlPanel.KickRequest), Transform_Robotics_ControlPanel_KickRequest_Robotics_ControlPanel_Proxy_KickRequest);
            AddProxyTransform(typeof(Robotics.ControlPanel.Proxy.RefRequest), Transform_Robotics_ControlPanel_Proxy_RefRequest_Robotics_ControlPanel_RefRequest);
            AddSourceTransform(typeof(Robotics.ControlPanel.RefRequest), Transform_Robotics_ControlPanel_RefRequest_Robotics_ControlPanel_Proxy_RefRequest);
            AddProxyTransform(typeof(Robotics.ControlPanel.Proxy.TickRequest), Transform_Robotics_ControlPanel_Proxy_TickRequest_Robotics_ControlPanel_TickRequest);
            AddSourceTransform(typeof(Robotics.ControlPanel.TickRequest), Transform_Robotics_ControlPanel_TickRequest_Robotics_ControlPanel_Proxy_TickRequest);
        }
    }
}

