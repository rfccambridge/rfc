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

}
