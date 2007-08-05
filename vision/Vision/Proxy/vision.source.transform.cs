using System;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Core.Transforms;

[assembly: ServiceDeclaration(DssServiceDeclaration.Transform, SourceAssemblyKey = @"Vision, Version=1.0.0.0, Culture=neutral, PublicKeyToken=6167838637cd631e")]
[assembly: System.Security.SecurityTransparent]
[assembly: System.Security.AllowPartiallyTrustedCallers]

namespace Dss.Transforms.TransformVision
{

    public class Transforms: TransformBase
    {

        public static object Transform_Vision_Proxy_VisionServiceState_Vision_VisionServiceState(object transformObj)
        {
            Vision.VisionServiceState target = new Vision.VisionServiceState();
            return target;
        }


        private static Vision.Proxy.VisionServiceState _instance_Vision_Proxy_VisionServiceState = new Vision.Proxy.VisionServiceState();
        public static object Transform_Vision_VisionServiceState_Vision_Proxy_VisionServiceState(object transformObj)
        {
            return _instance_Vision_Proxy_VisionServiceState;
        }


        public static object Transform_Vision_Proxy_DeleteSubscriptionType_Vision_DeleteSubscriptionType(object transformObj)
        {
            Vision.DeleteSubscriptionType target = new Vision.DeleteSubscriptionType();
            Vision.Proxy.DeleteSubscriptionType from = transformObj as Vision.Proxy.DeleteSubscriptionType;
            target.Subscriber = from.Subscriber;
            return target;
        }


        public static object Transform_Vision_DeleteSubscriptionType_Vision_Proxy_DeleteSubscriptionType(object transformObj)
        {
            Vision.Proxy.DeleteSubscriptionType target = new Vision.Proxy.DeleteSubscriptionType();
            Vision.DeleteSubscriptionType from = transformObj as Vision.DeleteSubscriptionType;
            target.Subscriber = from.Subscriber;
            return target;
        }


        public static object Transform_Vision_Proxy_GameObjects_Vision_GameObjects(object transformObj)
        {
            Vision.GameObjects target = new Vision.GameObjects();
            Vision.Proxy.GameObjects from = transformObj as Vision.Proxy.GameObjects;

            // copy IEnumerable OurRobots
            if (from.OurRobots != null)
            {
                target.OurRobots = new System.Collections.Generic.List<Vision.Robot>();
                foreach(Vision.Proxy.Robot elem in from.OurRobots)
                {
                    target.OurRobots.Add((elem == null) ? null : (Vision.Robot)Transform_Vision_Proxy_Robot_Vision_Robot(elem));
                }
            }

            // copy IEnumerable TheirRobots
            if (from.TheirRobots != null)
            {
                target.TheirRobots = new System.Collections.Generic.List<Vision.Robot>();
                foreach(Vision.Proxy.Robot elem in from.TheirRobots)
                {
                    target.TheirRobots.Add((elem == null) ? null : (Vision.Robot)Transform_Vision_Proxy_Robot_Vision_Robot(elem));
                }
            }
            target.Ball = (from.Ball == null) ? null : (Vision.Ball)Transform_Vision_Proxy_Ball_Vision_Ball(from.Ball);
            target.Source = from.Source;
            return target;
        }


        public static object Transform_Vision_GameObjects_Vision_Proxy_GameObjects(object transformObj)
        {
            Vision.Proxy.GameObjects target = new Vision.Proxy.GameObjects();
            Vision.GameObjects from = transformObj as Vision.GameObjects;

            // copy IEnumerable OurRobots
            if (from.OurRobots != null)
            {
                target.OurRobots = new System.Collections.Generic.List<Vision.Proxy.Robot>();
                foreach(Vision.Robot elem in from.OurRobots)
                {
                    target.OurRobots.Add((elem == null) ? null : (Vision.Proxy.Robot)Transform_Vision_Robot_Vision_Proxy_Robot(elem));
                }
            }

            // copy IEnumerable TheirRobots
            if (from.TheirRobots != null)
            {
                target.TheirRobots = new System.Collections.Generic.List<Vision.Proxy.Robot>();
                foreach(Vision.Robot elem in from.TheirRobots)
                {
                    target.TheirRobots.Add((elem == null) ? null : (Vision.Proxy.Robot)Transform_Vision_Robot_Vision_Proxy_Robot(elem));
                }
            }
            target.Ball = (from.Ball == null) ? null : (Vision.Proxy.Ball)Transform_Vision_Ball_Vision_Proxy_Ball(from.Ball);
            target.Source = from.Source;
            return target;
        }


        public static object Transform_Vision_Proxy_Robot_Vision_Robot(object transformObj)
        {
            Vision.Robot target = new Vision.Robot();
            Vision.Proxy.Robot from = transformObj as Vision.Proxy.Robot;
            target.Team = from.Team;
            target.Id = from.Id;
            target.X = from.X;
            target.Y = from.Y;
            target.Orientation = from.Orientation;
            return target;
        }


        public static object Transform_Vision_Robot_Vision_Proxy_Robot(object transformObj)
        {
            Vision.Proxy.Robot target = new Vision.Proxy.Robot();
            Vision.Robot from = transformObj as Vision.Robot;
            target.Team = from.Team;
            target.Id = from.Id;
            target.X = from.X;
            target.Y = from.Y;
            target.Orientation = from.Orientation;
            return target;
        }


        public static object Transform_Vision_Proxy_Ball_Vision_Ball(object transformObj)
        {
            Vision.Ball target = new Vision.Ball();
            Vision.Proxy.Ball from = transformObj as Vision.Proxy.Ball;
            target.X = from.X;
            target.Y = from.Y;
            target.ImageX = from.ImageX;
            target.ImageY = from.ImageY;
            return target;
        }


        public static object Transform_Vision_Ball_Vision_Proxy_Ball(object transformObj)
        {
            Vision.Proxy.Ball target = new Vision.Proxy.Ball();
            Vision.Ball from = transformObj as Vision.Ball;
            target.X = from.X;
            target.Y = from.Y;
            target.ImageX = from.ImageX;
            target.ImageY = from.ImageY;
            return target;
        }

        static Transforms()
        {
            AddProxyTransform(typeof(Vision.Proxy.VisionServiceState), Transform_Vision_Proxy_VisionServiceState_Vision_VisionServiceState);
            AddSourceTransform(typeof(Vision.VisionServiceState), Transform_Vision_VisionServiceState_Vision_Proxy_VisionServiceState);
            AddProxyTransform(typeof(Vision.Proxy.DeleteSubscriptionType), Transform_Vision_Proxy_DeleteSubscriptionType_Vision_DeleteSubscriptionType);
            AddSourceTransform(typeof(Vision.DeleteSubscriptionType), Transform_Vision_DeleteSubscriptionType_Vision_Proxy_DeleteSubscriptionType);
            AddProxyTransform(typeof(Vision.Proxy.GameObjects), Transform_Vision_Proxy_GameObjects_Vision_GameObjects);
            AddSourceTransform(typeof(Vision.GameObjects), Transform_Vision_GameObjects_Vision_Proxy_GameObjects);
            AddProxyTransform(typeof(Vision.Proxy.Robot), Transform_Vision_Proxy_Robot_Vision_Robot);
            AddSourceTransform(typeof(Vision.Robot), Transform_Vision_Robot_Vision_Proxy_Robot);
            AddProxyTransform(typeof(Vision.Proxy.Ball), Transform_Vision_Proxy_Ball_Vision_Ball);
            AddSourceTransform(typeof(Vision.Ball), Transform_Vision_Ball_Vision_Proxy_Ball);
        }
    }
}

