using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Ccr.Core;
using Microsoft.Dss.ServiceModel.Dssp;
using W3C.Soap;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.Services.SubscriptionManager;
using Robocup.Constants;

//namespace VisionServiceNamespace {
namespace Vision {

    public static class Contract {
        /// <summary>
        /// The DSS Namespace for VisionService
        /// </summary>
        public const string Identifier = "http://schemas.tempuri.org/2007/02/VisionService.html";
    }

    /// <summary>
    /// DemoAgent StateType
    /// </summary>
    [DataContract]
    public class VisionServiceState {
        /*GameObjects gameObjects;
        GameObjects[] prevGameObjects;

        [DataMember]
        public GameObjects GameObjects {
            get { return gameObjects; }
            set { gameObjects = value; }
        }
        [DataMember]
        public GameObjects[] PrevGameObjects {
            get { return prevGameObjects; }
            set { prevGameObjects = value; }
        }//*/
        
        public VisionServiceState() {
           // prevGameObjects = new GameObjects[Constants.get<int>("FRAMES_TO_REMEMBER")];
        }
        
    }

    public class VisionServiceOperations : PortSet<DsspDefaultLookup, DsspDefaultDrop, Get, Replace, 
                                                   Subscribe, ReliableSubscribe, DeleteSubscription, GameObjInfoReady> {
    }

    
    public class DeleteSubscription: Update<DeleteSubscriptionType, PortSet<DefaultReplaceResponseType, Fault>> {
        public DeleteSubscription() { }

        public DeleteSubscription(DeleteSubscriptionType body)
            : base(body) {
        }

    }

    [DataContract]
    public class DeleteSubscriptionType {
        string _subscriber;
        
        [DataMember]
        public string Subscriber {
            get { return _subscriber; }
            set { _subscriber = value; }
        }

        public DeleteSubscriptionType() {
            
        }

        public DeleteSubscriptionType(string subscriber) {
            _subscriber = subscriber;
        }
    }

    public class Get : Get<GetRequestType, PortSet<VisionServiceState, Fault>> {
        /// <summary>
        /// Default DSS Get Constructor
        /// </summary>
        public Get() {
        }

        /// <summary>
        /// DSS GetRequestType Constructor
        /// </summary>
        /// <param name="body"></param>
        public Get(GetRequestType body)
            : base(body) {
        }
    }

    /// <summary>
    /// DSS Replace Definition for DemoAgent 
    /// </summary>
    public class Replace : Replace<VisionServiceState, PortSet<DefaultReplaceResponseType, Fault>> {
        /// <summary>
        /// Default DSS Get Constructor
        /// </summary>
        public Replace() {
        }

        /// <summary>
        /// DSS DemoAgent StateType Constructor
        /// </summary>
        /// <param name="body"></param>
        public Replace(VisionServiceState body)
            : base(body) {
        }
    }


    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>> {
    }
    /// <summary>
    /// Operation Subscribe to bumper
    /// </summary>
    public class ReliableSubscribe : Subscribe<ReliableSubscribeRequestType, PortSet<SubscribeResponseType, Fault>> {
    }
    //public class DeleteSubscription : DeleteSubscription {
    //}
    public class GameObjInfoReady : Update<GameObjects, PortSet<DefaultUpdateResponseType, Fault>> {
        public GameObjInfoReady() {
        }
        public GameObjInfoReady(GameObjects _gameObjects) :  base(_gameObjects) {
        }
    }

  
}
