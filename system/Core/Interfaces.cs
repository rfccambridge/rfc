using System;
using System.Collections.Generic;
using System.Text;


/** System overview: Core
 * 
 *              --------Interpreter (X) ---------------------------------------------------------------------
 *              |  (pull)                   |                                                               |
 *    IPredictor (KalmanPredictor I)  IController (RFCController X)---------------------------          RefBox
 *              |  (push)                   |                       |                        |
 *    IPredictor (MRSReceiver)         IRobots(RFCRobots I)     IMovement(RFCMovement I)       INavigator(RFCNavigator I)
 *              |                           |
 *    - - - - - - - - - - - - - - - -  IRobots(MRSSender)     
 *              |     (Vision)      |       |                    (ControlPanel)
 *    IPredictor (MRSSender)        - - - - - - - - - - - - - - - - - - - - - - - 
 *              |                           |                    (MasterCommander)
 *    IPredictor (CameraPredictor)   IRobots(MRSReceiver)
 *                                          |
 *                                   IRobots(SerialRobots)
 * 
 * 
 * In all these interfaces, where robotID is used it refers to the global ID of the robot
 * 
 */
namespace Robocup.Core {

    public enum PlayTypes
    {
        NormalPlay,
        Halt,
        Stopped,
        SetPlay_Ours,
        SetPlay_Theirs,
        PenaltyKick_Ours,
        PenaltyKick_Ours_Setup,
        PenaltyKick_Theirs,
        KickOff_Ours,
        KickOff_Ours_Setup,
        Kickoff_Theirs_Setup,
        KickOff_Theirs
    }

    public interface IReferee
    {
        PlayTypes GetCurrentPlayType();
    }
    public interface IRefBoxListener
    {
        void start();
        void stop();
        int getCmdCounter();
        char getLastCommand();
    }


    /** Abstraction for the command executer
     *  
     *  Implementations: SerialRobots, MRSRobots
     * 
     */
    public interface IRobots {
        /// <summary>
        /// Sets the speeds for the robot wheels.  Positive values mean that the robot will move forward.
        /// </summary>
        void setMotorSpeeds(int robotID, WheelSpeeds wheelSpeeds);
        /// <summary>
        /// Starts a charge/kick sequence - kicking always occurs after a certain charging delay
        /// </summary>
        /// <param name="robotID"></param>
        void kick(int robotID);
        /// <summary>
        /// Starts a charge/kick sequence - kicking occurs if the ball breaks the break-beam on the actual robot
        /// </summary>
        /// <param name="robotID"></param>
        void beamKick(int robotID);
    }

    /// <summary>
    /// An interface for things that will take and handle info about the current state,
    /// usually to later give it back out.  
    /// 
    /// Implementations: FieldState, ISplitInfoAcceptor
    /// </summary>
    public interface IInfoAcceptor
    {
        void updateRobot(int id, RobotInfo newInfo);
        void updateBallInfo(BallInfo ballInfo);
    }

    /// <summary>
    /// An interface for things that will take and handle info about the current state,
    /// usually to later give it back out.  More specifically, for when there are multiple
    /// things that will be giving information (ie: two cameras).
    /// 
    /// Implementations: BasicPredictor
    /// </summary>
    public interface ISplitInfoAcceptor
    {
        void updatePartOurRobotInfo(List<RobotInfo> newInfos, string splitName);
        void updatePartTheirRobotInfo(List<RobotInfo> newInfos, string splitName);
        void updateBallInfo(BallInfo ballInfo);
        //void clearTheirRobotInfo(int offset);
    }

    /// <summary>
    /// Abstracts away a source of data on robot positions.
    /// The returned lists are copies -- any changes to them will not be seen anywhere else.
    /// Implementations: MRSPredictor, CameraPredictor, KalmanPredictor
    /// 
    /// <remarks>These operations could be expensive, and may change during the course of a single function.
    /// So it's best to create a copy of the results at the beginning and use the copy.</remarks>
    /// </summary>
    public interface IPredictor {
        //returns information about the robots (position, velocity, orientation)
        //we don't care where it got its information from
        RobotInfo getCurrentInformation(int robotID);
        List<RobotInfo> getOurTeamInfo();
        List<RobotInfo> getTheirTeamInfo();
        List<RobotInfo> getAllInfos();
        BallInfo getBallInfo();
    }

    /// <summary>
    /// An interface for breaking the commands down from a mid-level tactical decision
    /// (ex: "kick the ball to here") to commands such as "move here" then "turn to face here"
    /// and then "kick".
    /// </summary>
    public interface IActionInterpreter
    {
        void Kick(int robotID, Vector2 target);
        void Move(int robotID, Vector2 target);
        void Move(int robotID, Vector2 target, Vector2 facing);
        void Stop(int robotID);
        void Dribble(int robotID, Vector2 target);
    }

    /// <summary>
    /// Takes any single order for a robot ("move to here", "kick the kicker", "stop"), and
    /// has the robot execute it.
    /// </summary>
    public interface IController {
        void move(int robotID, bool avoidBall, Vector2 dest);
        void move(int robotID, bool avoidBall, Vector2 dest, double orientation);
        void kick(int robotID);
        void beamKick(int robotID);
        void stop(int robotID);
    }

    /** Abstraction for the robot movement controller
     *  
     *  Implementations: RFCMovement, JMovement
     * 
     */
    public interface IMovement {
        WheelSpeeds calculateWheelSpeeds(IPredictor predictor, int robotID, RobotInfo currentInfo, NavigationResults results);
        WheelSpeeds calculateWheelSpeeds(IPredictor predictor, int robotID, RobotInfo currentInfo, NavigationResults results, double desiredOrientation);
    }

    /// <summary>
    /// An interface for things that convert between field coordinates
    /// (ie (0,0) is the center of the field, (1,1) is up and to the right, and units are meters)
    /// and pixel coordinates (ie (0,0) is the top left corner, (1,1) is down and to the right, the unit is pixels)
    /// </summary>
    public interface ICoordinateConverter
    {
        int fieldtopixelX(double x);
        int fieldtopixelY(double y);
        double fieldtopixelDistance(double f);
        double pixeltofieldDistance(double f);
        Vector2 fieldtopixelPoint(Vector2 p);
        double pixeltofieldX(double x);
        double pixeltofieldY(double y);
        Vector2 pixeltofieldPoint(Vector2 p);
    }
}
