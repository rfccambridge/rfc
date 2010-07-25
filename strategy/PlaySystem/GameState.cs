using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Geometry;
using Robocup.Core;
using Robocup.Utilities;

namespace Robocup.PlaySystem
{
    /// <summary>
    /// All information and ability that needs to be passed to a play. Container for the predictor, 
    /// the play type, team, etc., along with the ability to assign actions.
    /// Also doubles as a predictor "freezer"- keeps predictions the same until
    /// the reset method is called, which keeps them consistent while plays are being
    /// interpreted.
    /// </summary>
    public class GameState
    {
        // these are public because the whole point of the GameState is access to information
        public PlayType Playtype;
        public ActionAssigner Assigner;
        public Team OurTeam;
        public PlayFunctions Functions;
        public FrozenPredictor Predictor;

        public List<int> AssignedIDs
        {
            get { return Assigner.AssignedIDs; }
        }

        /// <summary>
        /// Initialized with a predictor, an action interpreter, and our team
        /// </summary>
        /// <param name="p"></param>
        /// <param name="ourteam"></param>
        /// <param name="actioninterpreter"></param>
        public GameState(IPredictor p, IActionInterpreter actioninterpreter, Team ourteam)
        {
            Assigner = new ActionAssigner(actioninterpreter);
            Functions = new PlayFunctions(this);

            // set up as a frozen predictor
            Predictor = new FrozenPredictor(p);
            OurTeam = ourteam;

            // start with a play type of halt (will be set before there are any plays
            Playtype = PlayType.Halt;
        }

        /// <summary>
        /// IDs that are currently unassigned (and therefore available for assignment)
        /// </summary>
        public List<int> UnassignedIDs
        {
            get
            {
                // all robot IDs, minus those that are assigned
                List<int> ret = new List<int>();
                foreach (RobotInfo info in Predictor.GetRobots(OurTeam))
                {
                    if (!AssignedIDs.Contains(info.ID))
                    {
                        ret.Add(info.ID);
                    }
                }
                return ret;
            }
        }

        /// <summary>
        /// reset to a new game state, including removing all assignments and refreshing information
        /// from the predictor.
        /// </summary>
        public void reset()
        {
            // reset to a new game state
            Assigner.reset();

            // freeze the FrozenPredictor
            Predictor.freezeState();
        }

        #region Setters
        /// <summary>
        /// set the state's predictor
        /// </summary>
        public void setPredictor(IPredictor predictor)
        {
            this.Predictor = new FrozenPredictor(predictor);
        }

        /// <summary>
        /// set the state's action interpreter
        /// </summary>
        public void setActionInterpreter(IActionInterpreter actioninterpreter)
        {
            this.Assigner.setActionInterpreter(actioninterpreter);
        }
        #endregion
    }

    /// <summary>
    /// Static class providing fixed points on a soccer field, to be used in the new play system.
    /// All constants are built up from the FIELD_WIDTH and FIELD_HEIGHT constants in plays.txt
    /// </summary>
    static class Field
    {
        /// <summary>
        /// static constructor, reloads constants first time
        /// </summary>
        static Field()
        {
            ReloadConstants();
        }

        public static double FIELD_WIDTH;
        public static double FIELD_HEIGHT;

        public static Vector2 topLeftCorner;
        public static Vector2 bottomLeftCorner;
        public static Vector2 bottomRightCorner;
        public static Vector2 topRightCorner;
        public static Vector2 ourgoal;
        public static Vector2 theirgoal;
        public static Vector2 topCenter;
        public static Vector2 bottomCenter;
        public static Vector2 center;

        /// <summary>
        /// reload constants relating to the field
        /// </summary>
        public static void ReloadConstants()
        {
            // reload main constants
            FIELD_WIDTH = Constants.get<double>("plays", "FIELD_WIDTH");
            FIELD_HEIGHT = Constants.get<double>("plays", "FIELD_HEIGHT");

            // divide by two to get other constants
            double half_width = FIELD_WIDTH / 2;
            double half_height = FIELD_HEIGHT / 2;

            // set all points in terms of the width and height
            topLeftCorner = new Vector2(-half_width, half_height);
            bottomLeftCorner = new Vector2(-half_width, -half_height);
            bottomRightCorner = new Vector2(half_width, -half_height);
            topRightCorner = new Vector2(half_width, half_height);
            ourgoal = new Vector2(-half_width, 0);
            theirgoal = new Vector2(half_width, 0);
            topCenter = new Vector2(0, half_height);
            bottomCenter = new Vector2(0, -half_height);
            center = new Vector2(0, 0);
        }
    }
}
