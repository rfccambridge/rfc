//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.42
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using W3C.Soap;
using compression = System.IO.Compression;
using constructor = Microsoft.Dss.Services.Constructor;
using contractmodel = Microsoft.Dss.Services.ContractModel;
using io = System.IO;
using pxcommander = Robotics.Commander.Proxy;
using pxcontrolpanel = Robotics.ControlPanel.Proxy;
using reflection = System.Reflection;
using subscriptionmanager = Microsoft.Dss.Services.SubscriptionManager.Proxy;


namespace Robotics.ControlPanel.Proxy
{
    
    /// <summary>
    /// ControlPanel Contract
    /// </summary>
    [XmlTypeAttribute(IncludeInSchema=false)]
    public sealed class Contract
    {
        /// The Unique Contract Identifier for the ControlPanel service
        public const String Identifier = "http://schemas.tempuri.org/2006/08/controlpanel.html";
        /// The Dss Service dssModel Contract(s)
        public static List<contractmodel.ServiceSummary> ServiceModel()
        {
            List<contractmodel.ServiceSummary> services = null;
            io.Stream stream = null;
            try
            {
                string Resource = @"Robotics.ControlPanel.Resources.DssModel.dss";
                stream = reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(Resource);
                compression.GZipStream compressionStream = new compression.GZipStream(stream, compression.CompressionMode.Decompress, true);
                XmlSerializer serializer = new XmlSerializer(typeof(List<contractmodel.ServiceSummary>));
                services = (List<contractmodel.ServiceSummary>)serializer.Deserialize(compressionStream);
                compressionStream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving Dss Service Model: ", ex.Message);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream = null;
                }
            }
            return services;

        }
        /// <summary>
        /// Creates an instance of the service associated with this contract
        /// </summary>
        /// <param name="contructorServicePort">Contractor Service that will create the instance</param>
        /// <param name="partners">Optional list of service partners for new service instance</param>
        /// <returns>Result PortSet for retrieving service creation response</returns>
        public static DsspResponsePort<CreateResponse> CreateService(constructor.ConstructorPort contructorServicePort, params PartnerType[] partners)
        {
            DsspResponsePort<CreateResponse> result = new DsspResponsePort<CreateResponse>();
            ServiceInfoType si = new ServiceInfoType(Contract.Identifier, null);
            if (partners != null)
            {
                si.PartnerList = new List<PartnerType>(partners);
            }
            Microsoft.Dss.Services.Constructor.Create create =
                new Microsoft.Dss.Services.Constructor.Create(si, result);
            contructorServicePort.Post(create);
            return result;

        }
        /// <summary>
        /// Creates an instance of the service associated with this contract
        /// </summary>
        /// <param name="contructorServicePort">Contractor Service that will create the instance</param>
        /// <returns>Result PortSet for retrieving service creation response</returns>
        public static DsspResponsePort<CreateResponse> CreateService(constructor.ConstructorPort contructorServicePort)
        {
            return Contract.CreateService(contructorServicePort, null);
        }
    }
    /// <summary>
    /// Control Panel State
    /// </summary>
    [DataContract()]
    [XmlRootAttribute("ControlPanelState", Namespace="http://schemas.tempuri.org/2006/08/controlpanel.html")]
    public class ControlPanelState : System.ICloneable
    {
        private Boolean _log;
        private String _logFile;
        /// <summary>
        /// Log
        /// </summary>
        [DataMember()]
        public Boolean Log
        {
            get
            {
                return this._log;
            }
            set
            {
                this._log = value;
            }
        }
        /// <summary>
        /// Log File
        /// </summary>
        [DataMember()]
        public String LogFile
        {
            get
            {
                return this._logFile;
            }
            set
            {
                this._logFile = value;
            }
        }
        public void CopyTo(ControlPanelState target)
        {
            target.Log = this.Log;
            target.LogFile = this.LogFile;
        }
        public virtual object Clone()
        {
            ControlPanelState target = new ControlPanelState();

            target.Log = this.Log;
            target.LogFile = this.LogFile;
            return target;

        }
    }
    /// <summary>
    /// Robot Update Request
    /// </summary>
    [DataContract()]
    [DataMemberConstructor()]
    [XmlRootAttribute("RobotUpdateRequest", Namespace="http://schemas.tempuri.org/2006/08/controlpanel.html")]
    public class RobotUpdateRequest : System.ICloneable
    {
        public void CopyTo(RobotUpdateRequest target)
        {
        }
        public virtual object Clone()
        {
            // For a class without fields, cloning isn't necessary
            return this;

        }
    }
    /// <summary>
    /// Kick Request
    /// </summary>
    [DataContract()]
    [DataMemberConstructor()]
    [XmlRootAttribute("KickRequest", Namespace="http://schemas.tempuri.org/2006/08/controlpanel.html")]
    public class KickRequest : System.ICloneable
    {
        private Int32 _iD;
        /// <summary>
        /// Default Constructor
        /// </summary>
        public KickRequest()
        {
        }
        /// <summary>
        /// Data Member Initialization Constructor
        /// </summary>
        public KickRequest(int iD)
        {
            this._iD = iD;
        }
        /// <summary>
        /// ID
        /// </summary>
        [DataMember()]
        public Int32 ID
        {
            get
            {
                return this._iD;
            }
            set
            {
                this._iD = value;
            }
        }
        public void CopyTo(KickRequest target)
        {
            target.ID = this.ID;
        }
        public virtual object Clone()
        {
            KickRequest target = new KickRequest();

            target.ID = this.ID;
            return target;

        }
    }
    /// <summary>
    /// Ref Request
    /// </summary>
    [DataContract()]
    [DataMemberConstructor()]
    [XmlRootAttribute("RefRequest", Namespace="http://schemas.tempuri.org/2006/08/controlpanel.html")]
    public class RefRequest : System.ICloneable
    {
        private Int32 _command;
        private Single _ballX;
        private Single _ballY;
        /// <summary>
        /// Default Constructor
        /// </summary>
        public RefRequest()
        {
        }
        /// <summary>
        /// Data Member Initialization Constructor
        /// </summary>
        public RefRequest(int command, float ballX, float ballY)
        {
            this._command = command;
            this._ballX = ballX;
            this._ballY = ballY;
        }
        /// <summary>
        /// Command
        /// </summary>
        [DataMember()]
        public Int32 Command
        {
            get
            {
                return this._command;
            }
            set
            {
                this._command = value;
            }
        }
        /// <summary>
        /// BallX
        /// </summary>
        [DataMember()]
        public Single BallX
        {
            get
            {
                return this._ballX;
            }
            set
            {
                this._ballX = value;
            }
        }
        /// <summary>
        /// BallY
        /// </summary>
        [DataMember()]
        public Single BallY
        {
            get
            {
                return this._ballY;
            }
            set
            {
                this._ballY = value;
            }
        }
        public void CopyTo(RefRequest target)
        {
            target.Command = this.Command;
            target.BallX = this.BallX;
            target.BallY = this.BallY;
        }
        public virtual object Clone()
        {
            RefRequest target = new RefRequest();

            target.Command = this.Command;
            target.BallX = this.BallX;
            target.BallY = this.BallY;
            return target;

        }
    }
    /// <summary>
    /// Tick Request
    /// </summary>
    [DataContract()]
    [XmlRootAttribute("TickRequest", Namespace="http://schemas.tempuri.org/2006/08/controlpanel.html")]
    public class TickRequest : System.ICloneable
    {
        public void CopyTo(TickRequest target)
        {
        }
        public virtual object Clone()
        {
            // For a class without fields, cloning isn't necessary
            return this;

        }
    }
    /// <summary>
    /// Control Panel Operations
    /// </summary>
    [XmlTypeAttribute(IncludeInSchema=false)]
    public class ControlPanelOperations : PortSet<Microsoft.Dss.ServiceModel.Dssp.DsspDefaultLookup, Microsoft.Dss.ServiceModel.Dssp.DsspDefaultDrop, Get, Replace, Update, ReliableSubscribe, RobotUpdateCommand, MoveCommand, KickCommand, RefCommand, Tick>
    {
        // Post DsspDefaultLookup and return the response port.
        public virtual PortSet<Microsoft.Dss.ServiceModel.Dssp.LookupResponse,Fault> DsspDefaultLookup(Microsoft.Dss.ServiceModel.Dssp.LookupRequestType body)
        {
            Microsoft.Dss.ServiceModel.Dssp.DsspDefaultLookup op = new Microsoft.Dss.ServiceModel.Dssp.DsspDefaultLookup();
            op.Body = body ?? new Microsoft.Dss.ServiceModel.Dssp.LookupRequestType();
            this.Post(op);
            return op.ResponsePort;

        }
        // Post DsspDefaultLookup and return the response port.
        public virtual PortSet<Microsoft.Dss.ServiceModel.Dssp.LookupResponse,Fault> DsspDefaultLookup()
        {
            Microsoft.Dss.ServiceModel.Dssp.DsspDefaultLookup op = new Microsoft.Dss.ServiceModel.Dssp.DsspDefaultLookup();
            op.Body = new Microsoft.Dss.ServiceModel.Dssp.LookupRequestType();
            this.Post(op);
            return op.ResponsePort;

        }
        // Post DsspDefaultDrop and return the response port.
        public virtual PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultDropResponseType,Fault> DsspDefaultDrop(Microsoft.Dss.ServiceModel.Dssp.DropRequestType body)
        {
            Microsoft.Dss.ServiceModel.Dssp.DsspDefaultDrop op = new Microsoft.Dss.ServiceModel.Dssp.DsspDefaultDrop();
            op.Body = body ?? new Microsoft.Dss.ServiceModel.Dssp.DropRequestType();
            this.Post(op);
            return op.ResponsePort;

        }
        // Post DsspDefaultDrop and return the response port.
        public virtual PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultDropResponseType,Fault> DsspDefaultDrop()
        {
            Microsoft.Dss.ServiceModel.Dssp.DsspDefaultDrop op = new Microsoft.Dss.ServiceModel.Dssp.DsspDefaultDrop();
            op.Body = new Microsoft.Dss.ServiceModel.Dssp.DropRequestType();
            this.Post(op);
            return op.ResponsePort;

        }
        // Post Get and return the response port.
        public virtual PortSet<ControlPanelState,Fault> Get(Microsoft.Dss.ServiceModel.Dssp.GetRequestType body)
        {
            Get op = new Get();
            op.Body = body ?? new Microsoft.Dss.ServiceModel.Dssp.GetRequestType();
            this.Post(op);
            return op.ResponsePort;

        }
        // Post Get and return the response port.
        public virtual PortSet<ControlPanelState,Fault> Get()
        {
            Get op = new Get();
            op.Body = new Microsoft.Dss.ServiceModel.Dssp.GetRequestType();
            this.Post(op);
            return op.ResponsePort;

        }
        // Post Replace and return the response port.
        public virtual PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultReplaceResponseType,Fault> Replace(ControlPanelState body)
        {
            Replace op = new Replace();
            op.Body = body ?? new ControlPanelState();
            this.Post(op);
            return op.ResponsePort;

        }
        // Post Update and return the response port.
        public virtual PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType,Fault> Update(ControlPanelState body)
        {
            Update op = new Update();
            op.Body = body ?? new ControlPanelState();
            this.Post(op);
            return op.ResponsePort;

        }
        // Post ReliableSubscribe and return the response port.
        public virtual PortSet<Microsoft.Dss.ServiceModel.Dssp.SubscribeResponseType,Fault> ReliableSubscribe(Microsoft.Dss.ServiceModel.Dssp.ReliableSubscribeRequestType body, IPort notificationPort)
        {
            ReliableSubscribe op = new ReliableSubscribe();
            op.Body = body ?? new Microsoft.Dss.ServiceModel.Dssp.ReliableSubscribeRequestType();
            op.NotificationPort = notificationPort;
            this.Post(op);
            return op.ResponsePort;

        }
        // Post ReliableSubscribe and return the response port.
        public virtual PortSet<Microsoft.Dss.ServiceModel.Dssp.SubscribeResponseType,Fault> ReliableSubscribe(IPort notificationPort)
        {
            ReliableSubscribe op = new ReliableSubscribe();
            op.Body = new Microsoft.Dss.ServiceModel.Dssp.ReliableSubscribeRequestType();
            op.NotificationPort = notificationPort;
            this.Post(op);
            return op.ResponsePort;

        }
        // Post RobotUpdateCommand and return the response port.
        public virtual PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType,Fault> RobotUpdateCommand(RobotUpdateRequest body)
        {
            RobotUpdateCommand op = new RobotUpdateCommand();
            op.Body = body ?? new RobotUpdateRequest();
            this.Post(op);
            return op.ResponsePort;

        }
        // Post MoveCommand and return the response port.
        public virtual PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType,Fault> MoveCommand(pxcommander.MoveRequest body)
        {
            MoveCommand op = new MoveCommand();
            op.Body = body ?? new pxcommander.MoveRequest();
            this.Post(op);
            return op.ResponsePort;

        }
        // Post KickCommand and return the response port.
        public virtual PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType,Fault> KickCommand(KickRequest body)
        {
            KickCommand op = new KickCommand();
            op.Body = body ?? new KickRequest();
            this.Post(op);
            return op.ResponsePort;

        }
        // Post RefCommand and return the response port.
        public virtual PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType,Fault> RefCommand(RefRequest body)
        {
            RefCommand op = new RefCommand();
            op.Body = body ?? new RefRequest();
            this.Post(op);
            return op.ResponsePort;

        }
        // Post Tick and return the response port.
        public virtual PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType,Fault> Tick(TickRequest body)
        {
            Tick op = new Tick();
            op.Body = body ?? new TickRequest();
            this.Post(op);
            return op.ResponsePort;

        }
    }
    /// <summary>
    /// Get
    /// </summary>
    [XmlTypeAttribute(IncludeInSchema=false)]
    public class Get : Microsoft.Dss.ServiceModel.Dssp.Get<Microsoft.Dss.ServiceModel.Dssp.GetRequestType, PortSet<ControlPanelState, Fault>>
    {
        public Get()
        {
        }
        public Get(Microsoft.Dss.ServiceModel.Dssp.GetRequestType body) : 
                base(body)
        {
        }
        public Get(Microsoft.Dss.ServiceModel.Dssp.GetRequestType body, Microsoft.Ccr.Core.PortSet<ControlPanelState,W3C.Soap.Fault> responsePort) : 
                base(body, responsePort)
        {
        }
    }
    /// <summary>
    /// Replace
    /// </summary>
    [XmlTypeAttribute(IncludeInSchema=false)]
    public class Replace : Microsoft.Dss.ServiceModel.Dssp.Replace<ControlPanelState, PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultReplaceResponseType, Fault>>
    {
        public Replace()
        {
        }
        public Replace(ControlPanelState body) : 
                base(body)
        {
        }
        public Replace(ControlPanelState body, Microsoft.Ccr.Core.PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultReplaceResponseType,W3C.Soap.Fault> responsePort) : 
                base(body, responsePort)
        {
        }
    }
    /// <summary>
    /// Update
    /// </summary>
    [XmlTypeAttribute(IncludeInSchema=false)]
    public class Update : Microsoft.Dss.ServiceModel.Dssp.Update<ControlPanelState, PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType, Fault>>
    {
        public Update()
        {
        }
        public Update(ControlPanelState body) : 
                base(body)
        {
        }
        public Update(ControlPanelState body, Microsoft.Ccr.Core.PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType,W3C.Soap.Fault> responsePort) : 
                base(body, responsePort)
        {
        }
    }
    /// <summary>
    /// Reliable Subscribe
    /// </summary>
    [XmlTypeAttribute(IncludeInSchema=false)]
    public class ReliableSubscribe : Microsoft.Dss.ServiceModel.Dssp.Subscribe<Microsoft.Dss.ServiceModel.Dssp.ReliableSubscribeRequestType, PortSet<Microsoft.Dss.ServiceModel.Dssp.SubscribeResponseType, Fault>>
    {
        public ReliableSubscribe()
        {
        }
        public ReliableSubscribe(Microsoft.Dss.ServiceModel.Dssp.ReliableSubscribeRequestType body) : 
                base(body)
        {
        }
        public ReliableSubscribe(Microsoft.Dss.ServiceModel.Dssp.ReliableSubscribeRequestType body, Microsoft.Ccr.Core.PortSet<Microsoft.Dss.ServiceModel.Dssp.SubscribeResponseType,W3C.Soap.Fault> responsePort) : 
                base(body, responsePort)
        {
        }
    }
    /// <summary>
    /// Robot Update Command
    /// </summary>
    [XmlTypeAttribute(IncludeInSchema=false)]
    public class RobotUpdateCommand : Microsoft.Dss.ServiceModel.Dssp.Update<RobotUpdateRequest, PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType, Fault>>
    {
        public RobotUpdateCommand()
        {
        }
        public RobotUpdateCommand(RobotUpdateRequest body) : 
                base(body)
        {
        }
        public RobotUpdateCommand(RobotUpdateRequest body, Microsoft.Ccr.Core.PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType,W3C.Soap.Fault> responsePort) : 
                base(body, responsePort)
        {
        }
    }
    /// <summary>
    /// Move Command
    /// </summary>
    [XmlTypeAttribute(IncludeInSchema=false)]
    public class MoveCommand : Microsoft.Dss.ServiceModel.Dssp.Update<pxcommander.MoveRequest, PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType, Fault>>
    {
        public MoveCommand()
        {
        }
        public MoveCommand(Robotics.Commander.Proxy.MoveRequest body) : 
                base(body)
        {
        }
        public MoveCommand(Robotics.Commander.Proxy.MoveRequest body, Microsoft.Ccr.Core.PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType,W3C.Soap.Fault> responsePort) : 
                base(body, responsePort)
        {
        }
    }
    /// <summary>
    /// Kick Command
    /// </summary>
    [XmlTypeAttribute(IncludeInSchema=false)]
    public class KickCommand : Microsoft.Dss.ServiceModel.Dssp.Update<KickRequest, PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType, Fault>>
    {
        public KickCommand()
        {
        }
        public KickCommand(KickRequest body) : 
                base(body)
        {
        }
        public KickCommand(KickRequest body, Microsoft.Ccr.Core.PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType,W3C.Soap.Fault> responsePort) : 
                base(body, responsePort)
        {
        }
    }
    /// <summary>
    /// Ref Command
    /// </summary>
    [XmlTypeAttribute(IncludeInSchema=false)]
    public class RefCommand : Microsoft.Dss.ServiceModel.Dssp.Update<RefRequest, PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType, Fault>>
    {
        public RefCommand()
        {
        }
        public RefCommand(RefRequest body) : 
                base(body)
        {
        }
        public RefCommand(RefRequest body, Microsoft.Ccr.Core.PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType,W3C.Soap.Fault> responsePort) : 
                base(body, responsePort)
        {
        }
    }
    /// <summary>
    /// Tick
    /// </summary>
    [XmlTypeAttribute(IncludeInSchema=false)]
    public class Tick : Microsoft.Dss.ServiceModel.Dssp.Update<TickRequest, PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType, Fault>>
    {
        public Tick()
        {
        }
        public Tick(TickRequest body) : 
                base(body)
        {
        }
        public Tick(TickRequest body, Microsoft.Ccr.Core.PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultUpdateResponseType,W3C.Soap.Fault> responsePort) : 
                base(body, responsePort)
        {
        }
    }
    /// <summary>
    /// On Unsubscribe Vision
    /// </summary>
    [XmlTypeAttribute(IncludeInSchema=false)]
    public class OnUnsubscribeVision : subscriptionmanager.DeleteSubscription
    {
        public OnUnsubscribeVision(Microsoft.Dss.Services.SubscriptionManager.Proxy.DeleteSubscriptionMessage body, Microsoft.Ccr.Core.PortSet<Microsoft.Dss.ServiceModel.Dssp.DefaultDeleteResponseType,W3C.Soap.Fault> responsePort) : 
                base(body, responsePort)
        {
        }
        public OnUnsubscribeVision(Microsoft.Dss.Services.SubscriptionManager.Proxy.DeleteSubscriptionMessage body) : 
                base(body)
        {
        }
        public OnUnsubscribeVision()
        {
        }
    }
}
