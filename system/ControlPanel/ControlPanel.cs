//-----------------------------------------------------------------------
//  This file is part of the Microsoft Robotics Studio Code Samples.
// 
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: SimpleDashboard.cs $ $Revision: 8 $
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.Dss.Core.Attributes;

using W3C.Soap;
using Microsoft.Ccr.Core;
using Microsoft.Ccr.Adapters.WinForms;
using Microsoft.Dss.Core;

using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using Microsoft.Dss.Services.Serializer;
using Microsoft.Robotics.Simulation.Physics.Proxy;
using Microsoft.Robotics.Simulation.Proxy;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using arm = Microsoft.Robotics.Services.ArticulatedArm.Proxy;
using cs = Microsoft.Dss.Services.Constructor;
//using drive = Microsoft.Robotics.Services.Drive.Proxy;
using drive = Robotics.SimDrive.Proxy;
using ds = Microsoft.Dss.Services.Directory;
using fs = Microsoft.Dss.Services.FileStore;
//using joystick = Microsoft.Robotics.Services.Samples.Drivers.Joystick.Proxy;
using sicklrf = Microsoft.Robotics.Services.Sensors.SickLRF.Proxy;
using submgr = Microsoft.Dss.Services.SubscriptionManager;
using dssp = Microsoft.Dss.ServiceModel.Dssp;
using simengine = Microsoft.Robotics.Simulation.Engine.Proxy;
using simball = Robotics.SimBall.Proxy;
using vision = Vision.Proxy;
using Microsoft.Robotics.PhysicalModel.Proxy;
using Robotics.SimDrive.Proxy;
using Microsoft.Dss.Services.SubscriptionManager;
using Robocup.Core;
using Robocup.CoreRobotics;

using commander = Robotics.Commander.Proxy;

using Robocup.Utilities;

namespace Robotics.ControlPanel
{
    //referee commands
    public enum refereecommand
    { STOP, HALT, READY, START }

    /// <summary>
    /// Simple Dashboard Service
    /// </summary>
    [Contract(Contract.Identifier)]
    class ControlPanelService : DsspServiceBase
    {


        //double REAL_MOTOR_POWER = 1.0f;
        //double SIMCOMMANDSCALING = 1.0 / 160.0;


        int OUR_TEAM = 1;
        int THEIR_TEAM = 2;

        // shared access to state is protected by the interleave pattern
        // when we activate the handlers
        ControlPanelState _state = new ControlPanelState();

        const int NUM_BOTS = 10;
        const int BALLID = 22;
        int curDrive = 2;
        bool ballVision = false;
        bool blueVision = false;
        bool yellowVision = false;

        RFCSystem rfcsystem = new Robocup.CoreRobotics.RFCSystem();

        [ServicePort("/controlpanel", AllowMultipleInstances = true)]
        ControlPanelOperations _mainPort = new ControlPanelOperations();

        [Partner("SubMgr", Contract = submgr.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        submgr.SubscriptionManagerPort _subMgrPort = new submgr.SubscriptionManagerPort();
        string _subMgrUri = string.Empty;

        //[Partner("Vision", Contract = vision.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExistingOrCreate)]
        //vision.VisionServiceOperations _visionPort = new vision.VisionServiceOperations();
        //vision.VisionServiceOperations _visionNotify = new vision.VisionServiceOperations();   

        DriveControl _driveControl;
        DriveControlEvents _eventsPort = new DriveControlEvents();

        #region Startup
        /// <summary>
        /// ControlPanel Service Default DSS Constuctor
        /// </summary>
        /// <param name="pCreate"></param>
        public ControlPanelService(DsspServiceCreationPort pCreate)
            : base(pCreate)
        {



        }

        /// <summary>
        /// Entry Point for the ControlPanel Service
        /// </summary>
        protected override void Start()
        {
            bool hasgoalie = false;
            bool haskicker = false;
            for (int i = 0; i < 9; i++)
            {
                bool hasTag;
                bool worked = Constants.nondestructiveGet<bool>("ROBOT_HAS_KICKER_" + i, out hasTag);
                if (worked && hasTag)
                    haskicker = true;
                worked = Constants.nondestructiveGet<bool>("ROBOT_IS_GOALIE_" + i, out hasTag);
                if (worked && hasTag)
                    hasgoalie = true;
            }
            if (!hasgoalie)
                System.Windows.Forms.MessageBox.Show("warning: no robot has been designated the goalie");
            if (!haskicker)
                System.Windows.Forms.MessageBox.Show("warning: no robots have kickers");

            rfcsystem.registerCommander(new ControlPanelRobots(this));
            //rfcsystem.registerPredictor(new TesterPredictor());
            rfcsystem.setSleepTime(Constants.get<int>("UPDATE_SLEEP_TIME"));



            // Handlers that need write or exclusive access to state go under
            // the exclusive group. Handlers that need read or shared access, and can be
            // concurrent to other readers, go to the concurrent group.
            // Other internal ports can be included in interleave so you can coordinate
            // intermediate computation with top level handlers.

            //_visionPort.Subscribe(_visionNotify);

            // LogInfo("CP Subscribed to Vision!");

            Activate(Arbiter.Interleave(
                new TeardownReceiverGroup
                (
                    Arbiter.Receive<DsspDefaultDrop>(false, _mainPort, DropHandler)
                ),
                new ExclusiveReceiverGroup
                (
                    Arbiter.ReceiveWithIterator<OnConnect>(true, _eventsPort, OnConnectHandler),
                    Arbiter.ReceiveWithIterator<OnConnectMotor>(true, _eventsPort, OnConnectMotorHandler),
                    Arbiter.ReceiveWithIterator<OnConnectBall>(true, _eventsPort, OnConnectBallHandler),
                    Arbiter.ReceiveWithIterator<OnConnectVision>(true, _eventsPort, OnConnectVisionHandler),
                    Arbiter.Receive<OnConnectCommander>(true, _eventsPort, OnConnectCommanderHandler),
                    Arbiter.ReceiveWithIterator<OnUnsubscribeVision>(true, _eventsPort, OnUnsubscribeVisionHandler),


                    Arbiter.ReceiveWithIterator<Replace>(true, _mainPort, ReplaceHandler),
                    Arbiter.Receive<OnLoad>(true, _eventsPort, OnLoadHandler),
                    Arbiter.Receive<OnClosed>(true, _eventsPort, OnClosedHandler),
                //Arbiter.ReceiveWithIterator<OnChangeJoystick>(true, _eventsPort, OnChangeJoystickHandler),
                    Arbiter.Receive<ToggleRFC>(true, _eventsPort, ToggleRFCHandler)
                ),
                new ConcurrentReceiverGroup
                (
                    Arbiter.Receive<DsspDefaultLookup>(true, _mainPort, DefaultLookupHandler),
                    Arbiter.ReceiveWithIterator<Get>(true, _mainPort, GetHandler),
                    Arbiter.ReceiveWithIterator<ReliableSubscribe>(true, _mainPort, ReliableSubscribeHandler),

                    //Arbiter.ReceiveWithIterator<joystick.Replace>(true, _joystickNotify, JoystickReplaceHandler),
                //Arbiter.ReceiveWithIterator<sicklrf.Replace>(true, _laserNotify, OnLaserReplaceHandler),

                    //Arbiter.ReceiveWithIterator<drive.Update>(true, _driveNotify, OnDriveUpdateNotificationHandler),
                    Arbiter.ReceiveWithIterator<simball.Update>(true, _simNotify, OnBallUpdateHandler),


                    Arbiter.ReceiveWithIterator<OnMove>(true, _eventsPort, OnMoveHandler),
                    Arbiter.ReceiveWithIterator<MoveCommand>(true, _mainPort, MoveCommandHandler),
                    Arbiter.ReceiveWithIterator<RobotUpdateCommand>(true, _mainPort, RobotUpdateCommandHandler),
                    Arbiter.ReceiveWithIterator<vision.GameObjInfoReady>(true, _visionNotify, GameObjInfoReadyNotificationHandler),
                    Arbiter.ReceiveWithIterator<OnSetPose>(true, _eventsPort, OnSetPoseHandler),
                    Arbiter.ReceiveWithIterator<OnEStop>(true, _eventsPort, OnEStopHandler),

                    Arbiter.ReceiveWithIterator<OnStartService>(true, _eventsPort, OnStartServiceHandler),
                //Arbiter.ReceiveWithIterator<OnConnectSickLRF>(true, _eventsPort, OnConnectSickLRFHandler),
                //Arbiter.Receive<OnDisconnectSickLRF>(true, _eventsPort, OnDisconnectSickLRFHandler),
                    Arbiter.Receive<OnChangeMotor>(true, _eventsPort, OnChangeMotorHandler),
                    Arbiter.Receive<OnDrag>(true, _eventsPort, OnDragHandler),
                    Arbiter.Receive<RefCommand>(true, _mainPort, RefCommandHandler),
                    Arbiter.ReceiveWithIterator<OnKick>(true, _eventsPort, OnKickHandler),
                    Arbiter.ReceiveWithIterator<KickCommand>(true, _mainPort, KickCommandHandler),
                    Arbiter.Receive<OnChangeInput>(true, _eventsPort, OnChangeInputHandler)


                //Arbiter.ReceiveWithIterator<OnConnectArticulatedArm>(true, _eventsPort, OnConnectArticulatedArmHandler),
                //Arbiter.ReceiveWithIterator<OnApplyJointParameters>(true, _eventsPort, OnApplyJointParametersHandler)
                )
            ));

            DirectoryInsert();

            WinFormsServicePort.Post(new RunForm(CreateForm));

        }



        #endregion

        #region WinForms interaction

        System.Windows.Forms.Form CreateForm()
        {
            return new DriveControl(_eventsPort, rfcsystem);
        }

        #endregion

        #region DSS Handlers

        /// <summary>
        /// Get Handler returns SimpleDashboard State.
        /// </summary>
        /// <remarks>
        /// We declare this handler as an iterator so we can easily do
        /// sequential, logically blocking receives, without the need
        /// of nested Activate calls
        /// </remarks>
        /// <param name="get"></param>
        IEnumerator<ITask> GetHandler(Get get)
        {
            get.ResponsePort.Post(_state);
            yield break;
        }

        /// <summary>
        /// Replace Handler sets SimpleDashboard State
        /// </summary>
        /// <param name="replace"></param>
        IEnumerator<ITask> ReplaceHandler(Replace replace)
        {
            _state = replace.Body;
            replace.ResponsePort.Post(dssp.DefaultReplaceResponseType.Instance);
            yield break;
        }

        /// <summary>
        /// Drop Handler shuts down SimpleDashboard
        /// </summary>
        /// <param name="drop"></param>
        void DropHandler(DsspDefaultDrop drop)
        {
            SpawnIterator(drop, DropIterator);
        }

        IEnumerator<ITask> DropIterator(DsspDefaultDrop drop)
        {
            LogInfo("Starting Drop");

            /*if (_laserShutdown != null)
            {
                yield return PerformShutdown(ref _laserShutdown);
            }*/

            for (int i = 0; i < NUM_BOTS; i++)
            {
                if (_motorShutdowns[i] != null)
                {
                    yield return PerformShutdown(ref _motorShutdowns[i]);
                }
            }

            if (_driveControl != null)
            {
                DriveControl drive = _driveControl;
                _driveControl = null;

                WinFormsServicePort.FormInvoke(
                    delegate()
                    {
                        if (!drive.IsDisposed)
                        {
                            drive.Dispose();
                        }
                    }
                );
            }

            base.DefaultDropHandler(drop);
        }

        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> ReliableSubscribeHandler(ReliableSubscribe subscribe)
        {
            LogInfo(LogGroups.Console, "ControlPanel ReliableSubscribe Handler: ");
            yield return Arbiter.Choice(
                SubscribeHelper(_subMgrPort, subscribe.Body, subscribe.ResponsePort),
                delegate(SuccessResult success)
                {
                    _subMgrPort.Post(new submgr.Submit(
                        subscribe.Body.Subscriber, DsspActions.UpdateRequest, _state, null));
                },
                delegate(Exception ex) { LogError(ex); }
            );
            yield break;
        }

        Choice PerformShutdown(ref Port<Shutdown> port)
        {
            Shutdown shutdown = new Shutdown();
            port.Post(shutdown);
            port = null;

            return Arbiter.Choice(
                shutdown.ResultPort,
                delegate(SuccessResult success) { },
                delegate(Exception e)
                {
                    LogError(e);
                }
            );
        }

        #endregion

        #region Drive Control Event Handlers

        void OnLoadHandler(OnLoad onLoad)
        {
            _driveControl = onLoad.DriveControl;

            LogInfo("Loaded Form");

            //yield return EnumerateJoysticks();

            //yield return SubscribeToJoystick();
        }


        void OnClosedHandler(OnClosed onClosed)
        {
            if (onClosed.DriveControl == _driveControl)
            {
                LogInfo("Form Closed");

                _mainPort.Post(new DsspDefaultDrop(DropRequestType.Instance));
            }
        }


        simball.BallOperations _simPort;
        simball.BallOperations _simNotify = new simball.BallOperations();
        Port<Shutdown> _simShutdown;

        vision.VisionServiceOperations _visionPort;
        vision.VisionServiceOperations _visionNotify = new vision.VisionServiceOperations();
        Port<Shutdown> _visionShutdown;

        commander.CommanderOperations _commanderPort;
        commander.CommanderOperations _commanderNotify = new commander.CommanderOperations();
        Port<Shutdown> _commanderShutdown;

        IEnumerator<ITask> OnConnectHandler(OnConnect onConnect)
        {
            if (onConnect.DriveControl == _driveControl)
            {


                ServiceInfoType[] list = null;

                //ServiceInfoType[] listSim = null;
                //ServiceInfoType[] listVision = null;

                UriBuilder builder;
                ds.DirectoryPort port;
                ds.Get get;

                builder = new UriBuilder(onConnect.Directory);
                builder.Scheme = new Uri(ServiceInfo.Service).Scheme;

                port = ServiceForwarder<ds.DirectoryPort>(builder.Uri);
                get = new ds.Get();     //Get(GetRequestType.Instance);

                port.Post(get);

                yield return Arbiter.Choice(get.ResponsePort,
                    delegate(ds.GetResponseType response)
                    {
                        list = response.RecordList;

                    },
                    delegate(Fault fault)
                    {
                        list = new ServiceInfoType[0];
                        LogError(fault);
                    }
                );



                /*WinFormsServicePort.FormInvoke(
                    delegate()
                    {
                        _driveControl.ReplaceServiceList(list);
                    }
                );*/


                if (list.Length > 0)
                {
                    int i = 0;
                    foreach (ServiceInfoType info in list)
                    {

                        /* if (info.Contract == drive.Contract.Identifier) {
                             serviceList[i++] = info.Service;
                             _eventsPort.Post(new OnConnectMotor(_driveControl, info.Service));
                         } else if (info.Contract == simball.Contract.Identifier) {
                             _eventsPort.Post(new OnConnectBall(_driveControl, info.Service));

                         }*/

                        switch (info.Contract)
                        {
                            case drive.Contract.Identifier:
                                serviceList[i++] = info.Service;
                                _eventsPort.Post(new OnConnectMotor(_driveControl, info.Service));

                                break;
                            case simball.Contract.Identifier:
                                _eventsPort.Post(new OnConnectBall(_driveControl, info.Service));

                                break;
                            case vision.Contract.Identifier:
                                _eventsPort.Post(new OnConnectVision(_driveControl, info.Service));
                                break;
                            case commander.Contract.Identifier:
                                _eventsPort.Post(new OnConnectCommander(_driveControl, info.Service));
                                break;
                        }
                    }

                }

            }
        }


        IEnumerator<ITask> OnUnsubscribeVisionHandler(OnUnsubscribeVision onUnsubscribeVision)
        {

            //_visionPort . Delete DeleteSubscription(new DeleteSubscriptionMessage(_visionService));

            LogInfo("About to unsubscribe from vision.");
            Console.WriteLine("About to unsubscribe from vision.");

            if (_visionPort == null)
            {
                yield break;
            }
            else
            {

                Vision.Proxy.DeleteSubscriptionType dst = new Vision.Proxy.DeleteSubscriptionType();

                dst.Subscriber = _visionService;

                _visionPort.DeleteSubscription(dst);

                yield return Arbiter.Choice(onUnsubscribeVision.ResponsePort,
                        delegate(DefaultDeleteResponseType response)
                        {
                            LogInfo("unsubscribed from vision");
                            Console.WriteLine("unsubscribed from vision");
                        },
                        delegate(Fault fault)
                        {
                            LogError(fault);
                        }
                    );

            }
        }

        void ToggleRFCHandler(ToggleRFC msg)
        {
            if (msg.TurnOn)
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

        void OnChangeInputHandler(OnChangeInput msg)
        {

            if (msg.Objects.Equals("inputYellow"))
            {
                yellowVision = (msg.Result.Equals("Vision")) ? true : false;
            }
            if (msg.Objects.Equals("inputBlue"))
            {
                blueVision = (msg.Result.Equals("Vision")) ? true : false;
            }
            if (msg.Objects.Equals("inputBall"))
            {
                ballVision = (msg.Result.Equals("Vision")) ? true : false;
            }
        }

        IEnumerator<ITask> OnStartServiceHandler(OnStartService onStartService)
        {
            if (onStartService.DriveControl == _driveControl &&
                onStartService.Constructor != null)
            {
                cs.ConstructorPort port = ServiceForwarder<cs.ConstructorPort>(onStartService.Constructor);

                ServiceInfoType request = new ServiceInfoType(onStartService.Contract);
                cs.Create create = new cs.Create(request);

                port.Post(create);

                string service = null;

                yield return Arbiter.Choice(
                    create.ResponsePort,
                    delegate(CreateResponse response)
                    {
                        service = response.Service;
                    },
                    delegate(Fault fault)
                    {
                        LogError(fault);
                    }
                );


                if (service == null)
                {
                    yield break;
                }
            }
        }

        #endregion

        #region Motor operations

        string[] serviceList = new string[NUM_BOTS];

        private int lookupDrive(string name)
        {
            for (int i = 0; i < NUM_BOTS; i++)
            {
                if (name == serviceList[i])
                {
                    return i;
                }
            }
            return -1;
        }

        IEnumerator<ITask> OnKickHandler(OnKick onKick)
        {
            if (onKick.DriveControl == _driveControl &&
                    curDrive >= 0 &&
                    _drivePorts[curDrive] != null)
            {
                drive.KickRequest request = new drive.KickRequest();

                yield return Arbiter.Choice(
                    _drivePorts[curDrive].Kick(request),
                    delegate(DefaultUpdateResponseType response) { },
                    delegate(Fault f)
                    {
                        LogError(f);
                    }
                );
            }
        }

        IEnumerator<ITask> KickCommandHandler(KickCommand onKick)
        {
            if (onKick.Body.ID >= 0 &&
                    _drivePorts[onKick.Body.ID] != null)
            {
                drive.KickRequest request = new drive.KickRequest();

                yield return Arbiter.Choice(
                    _drivePorts[onKick.Body.ID].Kick(request),
                    delegate(DefaultUpdateResponseType response) { },
                    delegate(Fault f)
                    {
                        LogError(f);
                    }
                );
            }
        }

        void RefCommandHandler(RefCommand cmd)
        {
            //lastCmd = cmd.Command;
            //lastCmdPoint = new Vector2(cmd.BallX, cmd.BallY);

            LogInfo("Received Command: " + cmd.Body.Command +
                " at ball (" + cmd.Body.BallX + ", " + cmd.Body.BallY + ")");
            Console.WriteLine("Received Command: " + cmd.Body.Command +
                " at ball (" + cmd.Body.BallX + ", " + cmd.Body.BallY + ")");
        }

        drive.DriveOperations[] _drivePorts = new drive.DriveOperations[NUM_BOTS];
        drive.DriveOperations _driveNotify = new drive.DriveOperations();
        Port<Shutdown>[] _motorShutdowns = new Port<Shutdown>[NUM_BOTS];

        string _visionService;

        bool ballSelected = false;

        void OnChangeMotorHandler(OnChangeMotor onChangeMotor)
        {

            if (onChangeMotor.Service != null)
            {
                Console.WriteLine("OnChangeMotor String");
                curDrive = lookupDrive(onChangeMotor.Service);

            }
            else
            {
                Console.WriteLine("OnChangeMotor X: " + onChangeMotor.X + " Y: " + onChangeMotor.Y);
                if (rfcsystem.Predictor != null)
                    curDrive = closestRobotTo(onChangeMotor.X, onChangeMotor.Y,
                        onChangeMotor.Height, onChangeMotor.Width);


            }
            Console.WriteLine("new curdrive is: " + curDrive + " service: " + lookupDrive(curDrive) + " ballSelected: " + ballSelected);
        }

        private int closestRobotTo(float x, float y, float height, float width)
        {
            float realx = 5.5f * (x / width) - 2.75f;
            float realy = 4.0f * (1 - y / height) - 2.0f;
            Vector2 position = new Robocup.Infrastructure.Vector2(realx, realy);

            /*double realx = 5.5 * (1-x / width) - 2.75;            
            double realy = 4.0 * (1-y/ height) - 2.0;*/
            Console.WriteLine("real x: " + realx + " y: " + realy + " h: " + height + " w: " + width);

            int closestDrive = 0;
            float minDist = 1000000.0f;
            float tmpDist = 1000000.0f;
            List<RobotInfo> ourInfo = rfcsystem.Predictor.getOurTeamInfo();
            for (int i = 0; i < ourInfo.Count; i++)
            {
                tmpDist = position.distanceSq(ourInfo[i].Position);
                if (tmpDist < minDist)
                {
                    closestDrive = i;
                    minDist = tmpDist;
                }
            }
            List<RobotInfo> theirInfo = rfcsystem.Predictor.getTheirTeamInfo();
            for (int i = 0; i < theirInfo.Count; i++)
            {
                tmpDist = position.distanceSq(theirInfo[i].Position);
                if (tmpDist < minDist)
                {
                    closestDrive = i + 5;
                    minDist = tmpDist;
                }
            }
            if (position.distanceSq(rfcsystem.Predictor.getBallInfo().Position) < minDist)
            {
                ballSelected = true;
            }
            else
            {
                ballSelected = false;
            }
            if (minDist < 0.20)
                return closestDrive;
            else
                return curDrive;
        }

        private string lookupDrive(int thisDrive)
        {
            return serviceList[thisDrive];
        }

        void OnConnectCommanderHandler(OnConnectCommander onConnectCommander)
        {

            if (onConnectCommander.DriveControl == _driveControl)
            {
                _commanderPort = ServiceForwarder<commander.CommanderOperations>(onConnectCommander.Service);

                //onConnectVision.Service;

                Console.WriteLine("Commander connected");
                _commanderShutdown = new Port<Shutdown>();

            }

        }

        IEnumerator<ITask> OnConnectVisionHandler(OnConnectVision onConnectVision)
        {

            if (onConnectVision.DriveControl == _driveControl)
            {
                _visionPort = ServiceForwarder<vision.VisionServiceOperations>(onConnectVision.Service);


                _visionShutdown = new Port<Shutdown>();

                vision.ReliableSubscribe request = new vision.ReliableSubscribe(
                    new ReliableSubscribeRequestType(10)
                );

                request.NotificationPort = _visionNotify;
                request.NotificationShutdownPort = _visionShutdown;

                _visionPort.Post(request);

                yield return Arbiter.Choice(
                    request.ResponsePort,
                    delegate(SubscribeResponseType response)
                    {

                        _visionService = response.Subscriber;

                        LogInfo("subscribed to vision: " + _visionService);

                    },
                    delegate(Fault fault)
                    {
                        _visionShutdown = null;
                        LogError(fault);
                    }
                );
            }



        }

        IEnumerator<ITask> OnConnectBallHandler(OnConnectBall onConnectBall)
        {
            if (onConnectBall.DriveControl == _driveControl)
            {

                _simPort = ServiceForwarder<simball.BallOperations>(onConnectBall.Service);
                _simShutdown = new Port<Shutdown>();

                simball.ReliableSubscribe request = new simball.ReliableSubscribe(
                    new ReliableSubscribeRequestType(10)
                );

                request.NotificationPort = _simNotify;
                request.NotificationShutdownPort = _simShutdown;

                _simPort.Post(request);

                yield return Arbiter.Choice(
                    request.ResponsePort,
                    delegate(SubscribeResponseType response)
                    {

                        LogInfo("subscribed to ball: " + onConnectBall.Service);
                    },
                    delegate(Fault fault)
                    {
                        _simShutdown = null;
                        LogError(fault);
                    }
                );
            }
        }

        IEnumerator<ITask> OnConnectMotorHandler(OnConnectMotor onConnectMotor)
        {
            if (onConnectMotor.DriveControl == _driveControl)
            {
                drive.EnableDriveRequest request = new drive.EnableDriveRequest();
                int thisDrive = lookupDrive(onConnectMotor.Service);

                _drivePorts[thisDrive] = ServiceForwarder<drive.DriveOperations>(onConnectMotor.Service);
                _motorShutdowns[thisDrive] = new Port<Shutdown>();

                request.Enable = true;

                yield return Arbiter.Choice(
                    _drivePorts[thisDrive].EnableDrive(request),
                    delegate(DefaultUpdateResponseType response) { },
                    delegate(Fault f)
                    {
                        LogError(f);
                    }
                );
                drive.ReliableSubscribe subscribe = new drive.ReliableSubscribe(
                    new ReliableSubscribeRequestType(10)
                );
                subscribe.NotificationPort = _driveNotify;
                subscribe.NotificationShutdownPort = _motorShutdowns[thisDrive];

                _drivePorts[thisDrive].Post(subscribe);

                yield return Arbiter.Choice(
                    subscribe.ResponsePort,
                    delegate(SubscribeResponseType response)
                    {
                        LogInfo("Subscribed to " + onConnectMotor.Service + " curDrive: " + thisDrive);
                        curDrive = thisDrive;
                    },
                    delegate(Fault fault)
                    {
                        _motorShutdowns[thisDrive] = null;
                        LogError(fault);
                    }
                );
            }
        }

        IEnumerator<ITask> OnBallUpdateHandler(simball.Update notification)
        {
            if (!ballVision)
            {
                //Console.WriteLine("GOT BALL UPDATE");
                if (_driveControl != null)
                {
                    //flip here to get simulator x
                    rfcsystem.Acceptor.updateBallInfo(new BallInfo(new Vector2(
                        -notification.Body.Position.X,
                        notification.Body.Position.Z), 0, 0));

                    base.SendNotification(_subMgrPort, new Update(_state));

                    WinFormsServicePort.FormInvoke(
                        delegate()
                        {
                            _driveControl.UpdateBallData(notification.Body);
                            //internally used xcoords, do not flip, do not change orients
                            _driveControl.DrawField(rfcsystem);
                        }
                    );
                }

                LogObject(notification.Body);
                yield break;
            }
        }


        IEnumerator<ITask> OnMoveHandler(OnMove onMove)
        {
            yield break;
        }

        public void sendMove(int ID, int leftFront, int rightFront, int leftBack, int rightBack)
        {
            bool usBlue = Constants.get<string>("OUR_TEAM_COLOR") == "BLUE";
            if (_commanderPort != null &&
                ((yellowVision && !usBlue) || (blueVision && usBlue)))
            {
                //_commanderPort.MoveCommand(new commander.MoveRequest(ID, leftFront, rightFront, leftBack, rightBack));
                //Console.WriteLine("SendMove called in id: " + ID + " lf: " + leftFront + " rf: " + rightFront + " lb: " + leftFront + " rb: " + rightBack);
                Activate(Arbiter.Receive(false, TimeoutPort(1),
                        delegate
                        {
                            _commanderPort.MoveCommand(new commander.MoveRequest(ID, leftFront, rightFront, leftBack, rightBack));
                        }
                    )
                );
            }
        }

        public void sendKick(int ID)
        {
            _commanderPort.KickCommand(new commander.KickID(ID));
        }

        IEnumerator<ITask> MoveCommandHandler(MoveCommand onMove)
        {
            yield break;
        }

        IEnumerator<ITask> RobotUpdateCommandHandler(RobotUpdateCommand robUpdate)
        {
            //send new information to simulation service
            //_drivePorts[robUpdate.Body.ID]

            LogInfo("Message recieved!!! : " + robUpdate.Body.X);

            robUpdate.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            yield break;
        }


        private void addTags(RobotInfo info)
        {
            bool hasTag;
            bool worked = Constants.nondestructiveGet<bool>("ROBOT_HAS_KICKER_" + info.ID, out hasTag);
            if (worked && hasTag)
                info.Tags.Add("kicker");
            worked = Constants.nondestructiveGet<bool>("ROBOT_IS_GOALIE_" + info.ID, out hasTag);
            if (worked && hasTag)
                info.Tags.Add("goalie");
        }
        IEnumerator<ITask> GameObjInfoReadyNotificationHandler(vision.GameObjInfoReady gameObjInfoReady)
        {

            //send new information to simulation service
            //_drivePorts[robUpdate.Body.ID]
            if (!yellowVision && !blueVision && !ballVision) yield break;

            bool usBlue = Constants.get<string>("OUR_TEAM_COLOR") == "BLUE";

            bool ourvision, theirvision;
            if (!usBlue)
            {
                ourvision = yellowVision;
                theirvision = blueVision;
            }
            else
            {
                ourvision = blueVision;
                theirvision = yellowVision;
            }

            vision.GameObjects gameObjs = gameObjInfoReady.Body;

            ISplitInfoAcceptor acceptor = rfcsystem.Acceptor;
            /*acceptor.clearTheirRobotInfo(
                Constants.get<int>("THEIR_ID_OFFSET_" + System.Windows.Forms.SystemInformation.ComputerName)
                );*/
            //if (gameObjInfoReady.Body.TheirRobots.Count>0)
            //    acceptor.clearTheirRobotInfo(gameObjInfoReady.Body.TheirRobots[0].Id);

            if (ballVision && gameObjs.Ball != null)
            {
                if (!(gameObjs.Ball.X == 0 && gameObjs.Ball.Y == 0))
                {
                    float x, z;
                    x = (float)gameObjs.Ball.Y / 1000f - 2.45f; //y?
                    z = (float)gameObjs.Ball.X / 1000f - 1.7f;  //X?
                    //NEED TO SWITCH X Z

                    acceptor.updateBallInfo(new BallInfo(new Vector2(x, z), 0, 0));
                }
            }

            if (ourvision)
            {
                List<RobotInfo> infos = new List<RobotInfo>();
                foreach (vision.Robot robot in gameObjs.OurRobots)
                {
                    if (robot != null && robot.Id >= 0)
                    {
                        if (robot.Team != OUR_TEAM)
                            throw new ApplicationException("an enemy robot in OurRobots list");
                        //NEED TO SWITCH X Z
                        float x, z;
                        x = (float)robot.Y / 1000f - 2.45f;
                        z = (float)robot.X / 1000f - 1.7f;
                        int id = robot.Id;

                        RobotInfo info = new RobotInfo(new Vector2(x, z), (float)(robot.Orientation - Math.PI / 2), id);
                        addTags(info);
                        infos.Add(info);

                    }
                }
                acceptor.updateHalfOurRobotInfo(infos, gameObjInfoReady.Body.Source);
            }
            if (theirvision)
            {
                List<RobotInfo> infos = new List<RobotInfo>();
                foreach (vision.Robot robot in gameObjs.TheirRobots)
                {
                    if (robot != null && robot.Id >= 0)
                    {
                        if (robot.Team != THEIR_TEAM)
                            throw new ApplicationException("a friendly robot in TheirRobots list");
                        //NEED TO SWITCH X Z
                        float x, z;
                        x = (float)robot.Y / 1000f - 2.45f;
                        z = (float)robot.X / 1000f - 1.7f;
                        int id = robot.Id;

                        infos.Add(new RobotInfo(new Vector2(x, z), (float)(robot.Orientation - Math.PI / 2), id));



                    }
                }
                acceptor.updateHalfTheirRobotInfo(infos, gameObjInfoReady.Body.Source);
                //acceptor.cleanTheirRobotInfo();
            }


            //LogInfo("Message recieved - dll ok!!! : " + gameObjInfoReady.Body.GameObjectsProp.Ball.Location[0]);


            //gameObjInfoReady.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            //send update
            //base.SendNotification(_subMgrPort, new Update(_state));

            WinFormsServicePort.FormInvoke(
                delegate()
                {
                    //internal use, do not flip, same for orients
                    _driveControl.DrawField(rfcsystem);
                }
            );


            yield break;
        }

        ITask SetPose(int id, float X, float Y, float Z, float orient)
        {
            return null;
            /*
            //ball cmd
            if (id == BALLID &&
                _simPort != null)
            {
                simball.SetPoseRequest request = new simball.SetPoseRequest();

                if (Y == -1.0) //flip ball x back here
                    request.Position = new Vector3(-_state.ballx, 0.5f, _state.bally);
                else
                {
                    request.Position = new Vector3(-X, Y, Z);
                }
                Console.WriteLine("NEW BALL X: " + request.Position.X + " Z: " + request.Position.Z);
                return Arbiter.Choice(
                    _simPort.SetPose(request),
                    delegate(DefaultUpdateResponseType response) { },
                    delegate(Fault f)
                    {
                        LogError(f);
                    }
                );
            }
            //drive cmd
            if (id != BALLID &&
                _drivePorts[id] != null)
            {
                drive.SetPoseRequest request = new drive.SetPoseRequest();

                //unused i think
                if (orient == -1.0f)
                {

                    //convert _state.orients back
                    tmpAngle = (float)(_state.orients[curDrive] - Math.PI / 2);
                    request.Orientation = new Quaternion(0.0f, (float)Math.Sin(tmpAngle / 2), 0.0f, (float)Math.Cos(tmpAngle / 2));
                }
                else
                {
                    tmpAngle = (float)(orient - Math.PI / 2);
                    request.Orientation = new Quaternion(0.0f, (float)Math.Sin(tmpAngle / 2), 0.0f, (float)Math.Cos(tmpAngle / 2));
                }

                if (Y == -1.0)
                    //flip xcoords here, about to send to simulator
                    request.Position = new Vector3(-_state.xcoords[curDrive], 0.5f, _state.ycoords[curDrive]);
                else
                {
                    request.Position = new Vector3(-X, Y, Z);
                }

                //Console.WriteLine("X: " + X + " Z: " + Z);
                return Arbiter.Choice(
                    _drivePorts[id].SetPose(request),
                    delegate(DefaultUpdateResponseType response) { },
                    delegate(Fault f)
                    {
                        LogError(f);
                    }
                );
            }
            return null;
            */
        }

        IEnumerator<ITask> OnSetPoseHandler(OnSetPose onSetPose)
        {
            yield break;
            /*
            //ball cmd
            if (onSetPose.DriveControl == _driveControl &&
                ballSelected &&
                _simPort != null)
            {
                yield return SetPose(BALLID,
                        onSetPose.Position.X,
                        onSetPose.Position.Y,
                        onSetPose.Position.Z,
                        0);

                /*simball.SetPoseRequest request = new simball.SetPoseRequest();

                if (onSetPose.Position.Y == -1.0) //flip ball x back here
                    request.Position = new Vector3(-_state.ballx, 0.5f, _state.bally);
                else
                {
                    request.Position = new Vector3(
                        -onSetPose.Position.X, 
                        onSetPose.Position.Y,
                        onSetPose.Position.Z);
                }
                Console.WriteLine("NEW BALL X: " + request.Position.X + " Z: " + request.Position.Z);
                yield return Arbiter.Choice(
                    _simPort.SetPose(request),
                    delegate(DefaultUpdateResponseType response) { },
                    delegate(Fault f)
                    {
                        LogError(f);
                    }
                );*
            }
            //drive cmd
            if (onSetPose.DriveControl == _driveControl &&
                !(ballSelected) &&
                curDrive >= 0 &&
                _drivePorts[curDrive] != null)
            {

                //drive.SetPoseRequest request = new drive.SetPoseRequest();
                yield return SetPose(curDrive,
                        onSetPose.Position.X,
                        onSetPose.Position.Y,
                        onSetPose.Position.Z,
                        onSetPose.Orientation);

                //unused i think
                /*if (onSetPose.Orientation == -1.0f)
                {

                    //convert _state.orients back
                    tmpAngle = (float)(_state.orients[curDrive] - Math.PI / 2);                    
                    request.Orientation = new Quaternion(0.0f, (float)Math.Sin(tmpAngle / 2), 0.0f, (float)Math.Cos(tmpAngle / 2));
                }
                else
                {
                    tmpAngle = (float)(onSetPose.Orientation - Math.PI / 2);
                    request.Orientation = new Quaternion(0.0f, (float)Math.Sin(tmpAngle / 2), 0.0f, (float)Math.Cos(tmpAngle / 2));
                }

                


                if (onSetPose.Position.Y == -1.0)
                    //flip xcoords here, about to send to simulator
                    request.Position = new Vector3(-_state.xcoords[curDrive], 0.5f, _state.ycoords[curDrive]);
                else
                {
                    request.Position = new Vector3(
                        -onSetPose.Position.X,
                        onSetPose.Position.Y,
                        onSetPose.Position.Z);
                }

                Console.WriteLine("X: " + request.Position.X + " Z: " + request.Position.Z);
                yield return Arbiter.Choice(
                    _drivePorts[curDrive].SetPose(request),
                    delegate(DefaultUpdateResponseType response) { },
                    delegate(Fault f)
                    {
                        LogError(f);
                    }
                );*
            }
            yield break;
            */
        }

        void OnDragHandler(OnDrag onDrag)
        {
            //ball cmd
            if (onDrag.DriveControl == _driveControl &&
                ballSelected && onDrag.Position.Y != -1.0)
            {
                rfcsystem.Acceptor.updateBallInfo(new BallInfo(
                    new Vector2(onDrag.Position.X, onDrag.Position.Z), 0, 0));
            }
            //drive cmd
            /*if (onDrag.DriveControl == _driveControl &&
                !(ballSelected) &&
                curDrive >= 0)
            {

                //if (onDrag.Orientation.X != -1.0)
                //{
                //internal use of orientation
                _state.orients[curDrive] = onDrag.Orientation;
                //}

                if (onDrag.Position.Y != -1.0)
                {
                    //internal controlpanel->drivecontrol interaction
                    _state.xcoords[curDrive] = onDrag.Position.X;
                    _state.ycoords[curDrive] = onDrag.Position.Z;
                    _state.update();
                    //base.SendNotification(_subMgrPort, new Update(_state));
                }
            }*/
            try
            {
                _driveControl.DrawField(rfcsystem);
            }
            catch (Exception e)
            {
                Console.WriteLine("Drawing error occured, but we're ignoring it." + e);
                return;
            }
        }

        IEnumerator<ITask> OnEStopHandler(OnEStop onEStop)
        {
            if (onEStop.DriveControl == _driveControl &&
                curDrive >= 0 &&
                _drivePorts[curDrive] != null)
            {
                LogInfo("Requesting EStop");
                drive.AllStopRequest request = new drive.AllStopRequest();

                yield return Arbiter.Choice(
                    _drivePorts[curDrive].AllStop(request),
                    delegate(DefaultUpdateResponseType response) { },
                    delegate(Fault f)
                    {
                        LogError(f);
                    }
                );
            }
        }

        #endregion


        #region Logging operations

        fs.FileStorePort _fileStorePort = null;
        object _fspLock = new object();

        void OnLogSettingHandler(OnLogSetting onLogSetting)
        {
            _state.Log = onLogSetting.Log;
            _state.LogFile = onLogSetting.File;

            if (_state.Log)
            {

            }
            else if (_fileStorePort != null)
            {
                LogInfo("Stop Logging");
                lock (_fspLock)
                {
                    fs.FileStorePort fsp = _fileStorePort;

                    LogInfo("Flush Log");
                    fsp.Post(new fs.Flush());

                    Activate(
                        Arbiter.Receive(false, TimeoutPort(1000),
                            delegate(DateTime signal)
                            {
                                LogInfo("Stop Log");
                                fsp.Post(new Shutdown());
                            }
                        )
                    );

                    _fileStorePort = null;
                }
            }
        }

        void LogObject(object data)
        {
            lock (_fspLock)
            {
                if (_state.Log &&
                    _fileStorePort != null)
                {
                    _fileStorePort.Post(new fs.WriteObject(data));
                }
            }
        }



        #endregion
    }

}
