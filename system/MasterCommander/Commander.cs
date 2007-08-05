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
using ds = Microsoft.Dss.Services.Directory;
using fs = Microsoft.Dss.Services.FileStore;

using submgr = Microsoft.Dss.Services.SubscriptionManager;
using dssp = Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Robotics.PhysicalModel.Proxy;

namespace Robotics.Commander
{



    /// <summary>
    /// Simple Dashboard Service
    /// </summary>
    [Contract(Contract.Identifier)]
    class CommanderService : DsspServiceBase
    {
        // shared access to state is protected by the interleave pattern
        // when we activate the handlers
        CommanderState _state = new CommanderState();
        const int NUM_BOTS = 10;
        //int curDrive = -1;

        [ServicePort("/Commander", AllowMultipleInstances = true)]
        CommanderOperations _mainPort = new CommanderOperations();

        RemoteControl _MasterCommander;
        //MasterCommanderEvents _eventsPort = new MasterCommanderEvents();

        #region Startup
        /// <summary>
        /// Commander Service Default DSS Constuctor
        /// </summary>
        /// <param name="pCreate"></param>
        public CommanderService(DsspServiceCreationPort pCreate)
            : base(pCreate)
        {
            //CreateSuccess();
        }

        /// <summary>
        /// Entry Point for the Commander Service
        /// </summary>
        protected override void Start()
        {
            // Handlers that need write or exclusive access to state go under
            // the exclusive group. Handlers that need read or shared access, and can be
            // concurrent to other readers, go to the concurrent group.
            // Other internal ports can be included in interleave so you can coordinate
            // intermediate computation with top level handlers.


            Activate(Arbiter.Interleave(
                new TeardownReceiverGroup
                (
                    Arbiter.Receive<DsspDefaultDrop>(false, _mainPort, DropHandler)
                ),
                new ExclusiveReceiverGroup
                (
                    Arbiter.ReceiveWithIterator<Replace>(true, _mainPort, ReplaceHandler)
                ),
                new ConcurrentReceiverGroup
                (
                    Arbiter.Receive<DsspDefaultLookup>(true, _mainPort, DefaultLookupHandler),
                    Arbiter.ReceiveWithIterator<Get>(true, _mainPort, GetHandler),
                //Arbiter.Receive<OnTest>(true, _eventsPort, OnTestHandler),
                    Arbiter.Receive<MoveCommand>(true, _mainPort, MoveCommandHandler),
                    Arbiter.Receive<KickCommand>(true, _mainPort, KickCommandHandler)
                //Arbiter.Receive<ChargeCommand>(true, _mainPort, ChargeCommandHandler),
                //Arbiter.Receive<StopChargeCommand>(true, _mainPort, StopChargeCommandHandler)
                    )
            ));

            DirectoryInsert();

            WinFormsServicePort.Post(new RunForm(CreateForm));

            //send test movement message here

            /*Activate(Arbiter.Receive(false, TimeoutPort(4000),
                delegate(DateTime time)
                {
                    Console.WriteLine("sent dummy message");
                    ChargeCommand c = new ChargeCommand();
                    c.Body = new KickID(2);
                    _mainPort.Post(c);
                    Activate(Arbiter.Receive(false, TimeoutPort(15000),
                        delegate(DateTime time2)
                        {
                            StopChargeCommand sc = new StopChargeCommand();
                            sc.Body = new KickID(2);
                            _mainPort.Post(sc);
                            Activate(Arbiter.Receive(false, TimeoutPort(200),
                                delegate(DateTime time3)
                                {
                                    KickCommand k = new KickCommand();
                                    k.Body = new KickID(2);
                                    _mainPort.Post(k);

                                }
                            ));
                        }
                    ));
                }
            ));*/


        }

        #endregion

        #region WinForms interaction

        System.Windows.Forms.Form CreateForm()
        {
            _MasterCommander = new RemoteControl();
            return _MasterCommander;
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
            //SpawnIterator(drop, DropIterator);
            DropIterator(drop);
        }

        void DropIterator(DsspDefaultDrop drop)
        {
            LogInfo("Starting Drop");

            if (_MasterCommander != null)
            {
                RemoteControl drive = _MasterCommander;
                _MasterCommander = null;

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


        #region CommanderOperations


        void MoveCommandHandler(MoveCommand onMove)
        {
            int toSend = onMove.Body.ID;

            WinFormsServicePort.FormInvoke(
                delegate()
                {

                    _MasterCommander.sendMove(onMove.Body.ID,
                        onMove.Body.LeftFront,
                        onMove.Body.RightFront,
                        onMove.Body.LeftBack,
                        onMove.Body.RightBack);
                }
            );

        }

        void KickCommandHandler(KickCommand msg)
        {
            WinFormsServicePort.FormInvoke(
                        delegate()
                        {

                            _MasterCommander.kick(msg.Body.ID);
                        }
                    );
        }

        /*void ChargeCommandHandler(ChargeCommand msg)
        {
            WinFormsServicePort.FormInvoke(
                        delegate()
                        {

                            _MasterCommander.charge(msg.Body.ID);
                        }
                    );
        }*/

        /*void StopChargeCommandHandler(StopChargeCommand msg)
        {
            WinFormsServicePort.FormInvoke(
                        delegate()
                        {

                            _MasterCommander.stopcharge(msg.Body.ID);
                        }
                    );
        }*/

        #endregion

    }

}
