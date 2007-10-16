using System;
using System.Collections.Generic;
using System.Text;


using Microsoft.Ccr.Adapters.WinForms;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using Microsoft.Ccr.Core;
using submgr = Microsoft.Dss.Services.SubscriptionManager;
using System.ComponentModel;
using System.Security.Permissions;
using Microsoft.Dss.ServiceModel.Dssp;
using System.Windows.Forms;
using Microsoft.Dss.Services.SubscriptionManager;
using VisionStatic;
using Robocup.Utilities;



//namespace VisionServiceNamespace {
namespace Vision
{

   
    [Contract(Contract.Identifier)]
    public class VisionService : DsspServiceBase {
 

        [ServicePort("/VisionService", AllowMultipleInstances = false)]
        private VisionServiceOperations _mainPort = new VisionServiceOperations();

        private VisionServiceState _state = new VisionServiceState();

        [Partner("SubMgr", Contract = submgr.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.CreateAlways)]
        private submgr.SubscriptionManagerPort _submgrPort = new submgr.SubscriptionManagerPort();  



          /// <summary>
        /// DemoAgent Service Default DSS Constuctor
        /// </summary>
        /// <param name="pCreate"></param>
        public VisionService(DsspServiceCreationPort pCreate)
            : base(pCreate)
        {

        }

        /// <summary>
        /// Entry Point for the DemoAgent Service
        /// </summary>
        protected override void Start()
        {
            base.Start();

            WinFormsServicePort.Post(new RunForm(formConstructor));
        }

        private Form formConstructor() {
            return new Vision.ImageForm(_mainPort);
        }


        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> SubscribeHandler(Subscribe subscribe) {
            LogInfo("Subscribe request from: " + subscribe.Body.Subscriber.ToString());
            
            SubscribeHelper(_submgrPort, subscribe.Body, subscribe.ResponsePort);


           // Microsoft.Dss.Services.SubscriptionManager.DeleteSubscription ds = new Microsoft.Dss.Services.SubscriptionManager.DeleteSubscription();
    
            yield break;
        }

        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> DeleteSubscriptionHandler(DeleteSubscription delSubscription) {
            LogInfo("UnSubscribe request from: " + delSubscription.Body.Subscriber);

            _submgrPort.Post(new Microsoft.Dss.Services.SubscriptionManager.DeleteSubscription(
                new DeleteSubscriptionMessage(delSubscription.Body.Subscriber)));
    
            yield break;
        }
        

        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> ReliableSubscribeHandler(ReliableSubscribe subscribe) {
            LogInfo(LogGroups.Console, "ReliableSubscribe Handler: ");
            yield return Arbiter.Choice(
                SubscribeHelper(_submgrPort, subscribe.Body, subscribe.ResponsePort),
                delegate(SuccessResult success) {
                    //_submgrPort.Post(new submgr.Submit(
                      //  subscribe.Body.Subscriber, DsspActions.UpdateRequest, (object)_state, null));
                    LogInfo("Reliable subscription to vision successful.");
                },
                delegate(Exception ex) { LogError(ex); }
            );

            yield break;
        }

        /// <summary>
        /// Get Handler returns SimpleDashboard State.
        /// </summary>
        /// <remarks>
        /// We declare this handler as an iterator so we can easily do
        /// sequential, logically blocking receives, without the need
        /// of nested Activate calls
        /// </remarks>
        /// <param name="get"></param>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> GetHandler(Get get) {
            get.ResponsePort.Post(_state);
            yield break;
        }

        /// <summary>
        /// Replace Handler sets SimpleDashboard State
        /// </summary>
        /// <param name="replace"></param>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> ReplaceHandler(Replace replace) {
            _state = replace.Body;
            replace.ResponsePort.Post(DefaultReplaceResponseType.Instance);
            yield break;
        }

        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> GameObjInfoReadyHandler(GameObjInfoReady gameObjInfoReady) {

 #if false                       
            GameObjects finalGameObjects = new GameObjects();

            int lostFor;
            int i;
            //is the new gameObjs missing an object? compare new gameObj with prev[length -1]
            //was the object lost in the last N frames?
            //make decision of what to put in current official gameObj

            //all these tasks can work only afeter a minimum of FORGET_AFTER_FRAMES frames have
            //been captured (i.e. PrevGameObjs is full)
            //NOTE: prevGameobj array is filled from the head


            if (_state.PrevGameObjects[_state.PrevGameObjects.Length - 1] != null) {

                //above tasks for the ball
              
                if (gameObjInfoReady.Body.Ball == null || 
                    (gameObjInfoReady.Body.Ball.X == 0 && gameObjInfoReady.Body.Ball.Y == 0)) {

                    lostFor = 0;
                    i = 0;
                    while ((_state.PrevGameObjects[i].Ball == null ||
                        (_state.PrevGameObjects[i].Ball.X == 0 && _state.PrevGameObjects[i].Ball.Y == 0)) && 
                        i < Constants.get<int>("FORGET_AFTER_FRAMES")) {
                        lostFor++;
                        i++;
                    }
                    if (lostFor >= Constants.get<int>("FORGET_AFTER_FRAMES")) {
                        //forget the object
                        finalGameObjects.Ball = null;
                    } else {
                        //remember where the object was last seen, and say it's still there
                        finalGameObjects.Ball = _state.PrevGameObjects[lostFor].Ball;

                    }
                } else {
                    finalGameObjects.Ball = gameObjInfoReady.Body.Ball;
                }

                //above tasks for the robots
                int robotID;
                for (robotID = 0; robotID < gameObjInfoReady.Body.OurRobots.Count; robotID++) {
                    if (gameObjInfoReady.Body.OurRobots[robotID] == null || gameObjInfoReady.Body.OurRobots[robotID].Id < 0) {
                        lostFor = 0;
                        i = 0;
                        while ((_state.PrevGameObjects[i].OurRobots[robotID] == null || 
                                _state.PrevGameObjects[i].OurRobots[robotID].Id < 0) && i < Constants.get<int>("FORGET_AFTER_FRAMES")) {
                            lostFor++;
                            i++;
                        }
                        if (lostFor >= Constants.get<int>("FORGET_AFTER_FRAMES")) {
                            //forget the object
                            finalGameObjects.OurRobots[robotID] = null;
                        } else {
                            //remember where the object was last frame, and say it's still there
                            finalGameObjects.OurRobots[robotID] = _state.PrevGameObjects[0].OurRobots[robotID];

                        }
                    } else {
                        finalGameObjects.OurRobots.Add(gameObjInfoReady.Body.OurRobots[robotID]);
                    }
                }

                // doing the same for the enemy robots is a problem, since robotID does not mean anything

                finalGameObjects.TheirRobots.AddRange(gameObjInfoReady.Body.TheirRobots);

              /*  for (robotID = 0; robotID < gameObjInfoReady.Body.TheirRobots.Count; robotID++) {
                    if (gameObjInfoReady.Body.TheirRobots[robotID] == null || gameObjInfoReady.Body.TheirRobots[robotID].Id < 0) {
                        lostFor = 0;
                        i = 0;
                        while ((_state.PrevGameObjects[i].TheirRobots[robotID] == null ||
                                _state.PrevGameObjects[i].TheirRobots[robotID].Id < 0) && 
                                i < Constants.get<int>("FORGET_AFTER_FRAMES")) {
                            lostFor++;
                            i++;
                        }
                        if (lostFor >= Constants.get<int>("FORGET_AFTER_FRAMES")) {
                            //forget the object
                            finalGameObjects.TheirRobots[robotID] = null;
                        } else {
                            //remember where the object was last frame, and say it's still there
                            finalGameObjects.TheirRobots[robotID] = _state.PrevGameObjects[0].TheirRobots[robotID];
                        }
                    } else {
                        finalGameObjects.TheirRobots.Add(gameObjInfoReady.Body.TheirRobots[robotID]);
                    }
                } */

            } else {
                //prevGameObj is not full, so blindly update
                finalGameObjects.Ball = gameObjInfoReady.Body.Ball;
                finalGameObjects.OurRobots = gameObjInfoReady.Body.OurRobots;
                finalGameObjects.TheirRobots = gameObjInfoReady.Body.TheirRobots;
            }
            

            //add the new gameobject info into memory
            //shift memory right:
            //int i;
            for (i = _state.PrevGameObjects.Length - 1; i > 0; i--) {
                _state.PrevGameObjects[i] = _state.PrevGameObjects[i - 1];
            }
            _state.PrevGameObjects[0] = gameObjInfoReady.Body;


            //make the finalGameObjects the current official info
            _state.GameObjects = finalGameObjects;
                       
            //send out the the official gameobj
            base.SendNotification(_submgrPort, new GameObjInfoReady(_state.GameObjects));

#endif
            base.SendNotification(_submgrPort, gameObjInfoReady);
            gameObjInfoReady.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            yield break;
        }

    }
}
