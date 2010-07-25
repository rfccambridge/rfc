using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Robocup.Geometry;
using Robocup.Core;
using Robocup.Utilities;

namespace Robocup.PlaySystem
{
    // This is the core of the new play system (Summer 2010, David Robinson)

    // Each RefBoxState has its own corresponding PlayLibrary class, and each play 
    // has a method within that class. This division allows autocomplete to suggest 
    // plays within the same play type, and prevents people from accidentally 
    // calling plays from another play type (since plays, except the mainplay, 
    // are private methods)

    /// <summary>
    /// base class for a library of plays for a particular playtype
    /// all play libraries inherit this
    /// </summary>
    class PlayLibrary
    {
        PlayType Playtype;

        /// <summary>
        /// base constructor takes the library's playtype
        /// </summary>
        /// <param name="type"></param>
        public PlayLibrary(PlayType type)
        {
            Playtype = type;
        }

        /// <summary>
        /// Assign plays for this playtype
        /// </summary>
        public virtual void mainplay(GameState state)
        {
            // virtual method does nothing, only inherited methods
        }

        /// <summary>
        /// return this library's PlayType
        /// </summary>
        /// <returns></returns>
        public PlayType getPlayType()
        {
            return Playtype;
        }

        /// <summary>
        /// Reload constants related to a play
        /// </summary>
        public virtual void ReloadConstants()
        {
            // do nothing in this method, only inherited methods
        }
    }

    /// <summary>
    /// Normal play- offense and defense
    /// </summary>
    class NormalPlayLibrary : PlayLibrary
    {

        // CONSTANTS
        double BALL_KICK_DIST;
        double STANDBY_DISTANCE;
        double GOAL_MINIMUM;
        double BALL_IN_RANGE;
        double SUPPORT_AVOID;
        double CLEAR_DIST;
        double SUPPORT_SETUP_PASS;
        double GOALIE_X_POS_DELTA;
        double FIELD_WIDTH;

        public NormalPlayLibrary()
            : base(PlayType.NormalPlay)
        {
            ReloadConstants();
        }

        public override void ReloadConstants()
        {
            BALL_KICK_DIST = Constants.get<double>("plays", "BALL_KICK_DIST");
            STANDBY_DISTANCE = Constants.get<double>("plays", "STANDBY_DISTANCE");
            GOAL_MINIMUM = Constants.get<double>("plays", "GOAL_MINIMUM");
            BALL_IN_RANGE = Constants.get<double>("plays", "BALL_IN_RANGE");
            SUPPORT_AVOID = Constants.get<double>("plays", "SUPPORT_AVOID");
            CLEAR_DIST = Constants.get<double>("plays", "CLEAR_DIST");
            SUPPORT_SETUP_PASS = Constants.get<double>("plays", "SUPPORT_SETUP_PASS");
            GOALIE_X_POS_DELTA = Constants.get<double>("plays", "GOALIE_X_POS_DELTA");
            FIELD_WIDTH = Constants.get<double>("plays", "FIELD_WIDTH");
        }

        public override void mainplay(GameState state)
        {
            // assign offense and defense plays based on the number of robots
            int numrobots = state.Functions.NumRobots();

            switch (numrobots)
            {
                case 1:
                    OFFENSE_1vs0(state);
                    break;
                case 2:
                    DEFENSE_1vs0(state);
                    OFFENSE_1vs0(state);
                    break;
                case 3:
                    DEFENSE_2vs0(state);
                    OFFENSE_1vs0(state);
                    break;
                case 4:
                    DEFENSE_2vs0(state);
                    OFFENSE_2vs0(state);
                    break;
                case 5:
                    DEFENSE_3vs0(state);
                    OFFENSE_2vs0(state);
                    break;
            }

        }

        // OFFENSE PLAYS

        /// <summary>
        /// Simplest one man offense- use the closest robot to kick towards the goal
        /// </summary>
        private void OFFENSE_1vs0(GameState state)
        {
            SharedPlays.OneKicker_1(state);
        }

        /// <summary>
        /// Send one robot to kick the ball (same as offense 1 vs 0), and another to an appropriate support
        /// position
        /// </summary>
        private void OFFENSE_2vs0(GameState state)
        {
            // just call the two appropriate plays
            OFFENSE_1vs0(state);
            SharedPlays.SupportRobot_1(state);
        }

        // DEFENSE PLAYS

        /// <summary>
        /// Simple goalie play
        /// </summary>
        private void DEFENSE_1vs0(GameState state)
        {
            SharedPlays.Goalie_1(state, GOALIE_X_POS_DELTA);
        }

        /// <summary>
        /// Two man defense, which uses a two man wall at 1 meter
        /// </summary>
        /// <param name="state"></param>
        private void DEFENSE_2vs0(GameState state)
        {
            SharedPlays.TwoManWall_2(state);
        }

        /// <summary>
        /// Three man defense, which combines two man wall and the defense 1vs0 (goalie) play
        /// </summary>
        /// <param name="state"></param>
        private void DEFENSE_3vs0(GameState state)
        {
            SharedPlays.Goalie_1(state, GOALIE_X_POS_DELTA);
            SharedPlays.TwoManWall_2(state);
        }
    }

    #region REFBOXPLAYS
    class StoppedLibrary : PlayLibrary
    {
        public StoppedLibrary() : base(PlayType.Stopped) { }

        // works based on the goalie, the guardBall, and the circularDefender shared play
        public override void mainplay(GameState state)
        {
            // some shared points that are used in all stop plays
            int numRobots = state.Functions.NumRobots();

            // There will always be a goalie
            if (numRobots > 0)
            {
                SharedPlays.Goalie_1(state);
            }

            // If there are only two robots, the second is assigned to guard the ball
            if (numRobots == 2)
            {
                SharedPlays.GuardBall_1_2_3(state, 1);
            }

            // Otherwise, one is a wide defender and the remaining ones guard the ball
            if (numRobots > 2)
            {
                SharedPlays.CircularDefender_1(state, 1.2);
                SharedPlays.GuardBall_1_2_3(state, numRobots - 2);
            }
        }
    }

    class KickOff_OursLibrary : PlayLibrary
    {
        public KickOff_OursLibrary() : base(PlayType.KickOff_Ours) { }

        /// <summary>
        /// Works as a cascade of robots that are added to the field
        /// </summary>
        public override void mainplay(GameState state)
        {
            int numRobots = state.Functions.NumRobots();

            // first robot: the one closest to the center kicks the ball
            if (numRobots > 0)
            {
                state.Assigner.Kick(state.Functions.closestRobot(Field.center).ID, Field.theirgoal);
            }

            // second robot: acts as a receiver
            if (numRobots > 1)
            {
                Vector2 receiverPoint = new Vector2(-.2, -1);
                SharedPlays.MoveClosest_1(state, receiverPoint);
            }

            // third robot, acts as a goalie
            if (numRobots > 2)
            {
                SharedPlays.Goalie_1(state);
            }

            // fourth and fifth robots are not used in the kickoff
            // seriously, look at the old plays! 4 and 5 robots made O_KFF_3x0 be assigned
        }
    }

    class KickOff_Ours_SetupLibrary : PlayLibrary
    {
        public KickOff_Ours_SetupLibrary() : base(PlayType.KickOff_Ours_Setup) { }

        /// <summary>
        /// Usual deal, cascade to place points on the field
        /// </summary>
        public override void mainplay(GameState state)
        {
            int numRobots = state.Functions.NumRobots();

            // first robot: the one closest to a point right behind the center goes there
            if (numRobots > 0)
            {
                Vector2 centerpoint = new Vector2(-.5, 0);
                SharedPlays.MoveClosest_1(state, centerpoint);
            }

            // second robot: acts as a goalie
            if (numRobots > 1)
            {
                SharedPlays.Goalie_1(state);
            }

            // These next three numbers have separate sets of points. The purpose was to be faithful to the
            // original plays, and I believe this is the simplest possible translation. (Part of 
            // the reason the new play system is good is that such differences in plays become apparent)

            // if there are only three robots, third puts itself in the middle
            if (numRobots == 3)
            {
                Vector2 middlepoint = new Vector2(-2, .2);
                SharedPlays.MoveClosest_1(state, middlepoint);
            }

            // but if there are four robots, the two middle robots space themselves out
            if (numRobots == 4)
            {
                SharedPlays.MoveClosest_1(state, new Vector2(-2, .3));
                SharedPlays.MoveClosest_1(state, new Vector2(-2, .3));
            }

            // finally, if there are five robots, follow the play- a top bot, a receiver bot, and a middle bot
            if (numRobots == 5)
            {
                SharedPlays.MoveClosest_1(state, new Vector2(-.2, 1));
                SharedPlays.MoveClosest_1(state, new Vector2(-.2, -1.2));
                SharedPlays.MoveClosest_1(state, new Vector2(-1.5, 0));
            }
        }
    }

    class KickOff_TheirsLibrary : PlayLibrary
    {
        public KickOff_TheirsLibrary() : base(PlayType.KickOff_Theirs) { }

        /// <summary>
        /// Cascade of points
        /// </summary>
        public override void mainplay(GameState state)
        {
            int numRobots = state.Functions.NumRobots();

            // First robot is a goalie
            if (numRobots > 0)
            {
                SharedPlays.Goalie_1(state);
            }

            // Second robot is near the center
            if (numRobots > 1)
            {
                // for some reason, the original play does a line circle intersection
                // to get to a point that is always the same. Fixed here
                SharedPlays.MoveClosest_1(state, new Vector2(-.7, 0));
            }

            // Third robot, if there are exactly 3 robots, is in the middle
            // This robot will also appear if there are 5 robots, but not if there are 4
            // (in which case, as you see below, they will end up in the top and bottom 
            // positions only)
            if (numRobots == 3 || numRobots == 5)
            {
                // 1.2 meters from the goal, towards the center
                SharedPlays.MoveClosest_1(state, new Vector2(-Field.FIELD_WIDTH / 2 + 1.2, 0));
            }

            // If there are four robots, put them at the top and bottom, facing forwards
            if (numRobots > 4)
            {
                Vector2 toppt = new Vector2(-.3, 1.5);
                Vector2 topptfwd = new Vector2(0, 1.5);
                Vector2 botpt = new Vector2(-0.3, -1.5);
                Vector2 botptfwd = new Vector2(0, -1.5);

                SharedPlays.MoveClosest_1(state, toppt, topptfwd);
                SharedPlays.MoveClosest_1(state, botpt, botptfwd);
            }
        }
    }

    /// <summary>
    /// This play type never actually happens. I see it in the enum, but the only reference
    /// to it in the rest of the software is in a switch statement in BasicPredictor (which
    /// watches for it and treats it like the rest of the software.
    /// Furthermore, there are no plays defined for it in the original language.
    /// Therefore, the play throws a NotImplementedException, so that if it does happen 
    /// we can be aware of it (I would not hold my breath).
    /// </summary>
    class KickOff_Theirs_SetupLibrary : PlayLibrary
    {
        public KickOff_Theirs_SetupLibrary() : base(PlayType.Kickoff_Theirs_Setup) { }

        public override void mainplay(GameState state)
        {
            throw new NotImplementedException("As of now, the KickOff_Theirs_Setup play does not "
                + "happen, so no plays are implemented for it. If the RefBoxState class changes so that "
                + "this play occurs, there should be plays written for it.");
        }
    }

    /// <summary>
    /// The only thing this has to do is kick the ball into the goal (we are already set up)
    /// </summary>
    class PenaltyKick_OursLibrary : PlayLibrary
    {
        public PenaltyKick_OursLibrary() : base(PlayType.PenaltyKick_Ours) { }

        public override void mainplay(GameState state)
        {
            // ignore other robots, just kick the ball into the goal with the closest robot
            SharedPlays.OneKicker_1(state);
        }
    }

    class PenaltyKick_Ours_SetupLibrary : PlayLibrary
    {
        public PenaltyKick_Ours_SetupLibrary() : base(PlayType.PenaltyKick_Ours_Setup) { }

        public override void mainplay(GameState state)
        {
            int numRobots = state.Functions.NumRobots();

            // first robot aims for the goal
            if (numRobots > 0)
            {
                Vector2 kickerpt = new Vector2(1.8, 0);
                SharedPlays.MoveClosest_1(state, kickerpt, Field.theirgoal);
            }

            // second robot is a goalie. NOTE: This is different from the traditional penalty goal
            // kick, where the second robots simply goes to the point (-2.3, 0), but it is better
            // since it reduces the use of the magic point and replaces it with a tried and true
            // goalie play
            if (numRobots > 1)
            {
                SharedPlays.Goalie_1(state);
            }

            // third robot is in a support position
            if (numRobots > 2)
            {
                SharedPlays.MoveClosest_1(state, new Vector2(1, 1));
            }

            // fourth is in a support position along the center goal line
            if (numRobots > 3)
            {
                SharedPlays.MoveClosest_1(state, new Vector2(1, 0));
            }

            // in the original play system, the five robot play is the same as the one robot play
            // Looks like a mistake, so I'm putting the fifth robot at a reflected support point
            if (numRobots > 4)
            {
                SharedPlays.MoveClosest_1(state, new Vector2(1, -1));
            }
        }
    }

    /// <summary>
    /// These plays did not exist in the SVN version of the old play system! So I wrote a simple
    /// set of plays where it places it at various points on the field, including a goalie.
    /// Could use some testing for sure!
    /// </summary>
    class PenaltyKick_TheirsLibrary : PlayLibrary
    {
        public PenaltyKick_TheirsLibrary() : base(PlayType.PenaltyKick_Theirs) { }

        public override void mainplay(GameState state)
        {
            int numRobots = state.Functions.NumRobots();

            // first robot is always a goalie, which must be extra close to the goal
            if (numRobots > 0)
            {
                SharedPlays.Goalie_1(state, .1);
            }

            // second robot will be on the other side of the attacker
            if (numRobots > 1)
            {
                SharedPlays.MoveClosest_1(state, new Vector2(-1, 0));
            }

            // third robot will be in a support position to the side
            if (numRobots > 2)
            {
                SharedPlays.MoveClosest_1(state, new Vector2(-1, 1));
            }

            // fourth robot will be in a support position on the other side of the center line
            if (numRobots > 3)
            {
                SharedPlays.MoveClosest_1(state, new Vector2(-1, -1));
            }

            // fifth robot will stay in the center of the field to be a little out of the way
            if (numRobots > 4)
            {
                SharedPlays.MoveClosest_1(state, Field.center);
            }
        }
    }

    class SetPlay_OursLibrary : PlayLibrary
    {
        public SetPlay_OursLibrary() : base(PlayType.SetPlay_Ours) { }

        public override void mainplay(GameState state)
        {
            int numRobots = state.Functions.NumRobots();

            // first robot just kicks the ball
            if (numRobots > 0)
            {
                SharedPlays.OneKicker_1(state);
            }

            // second robot acts as a goalie
            if (numRobots > 1)
            {
                SharedPlays.Goalie_1(state);
            }

            // if there are exactly 3 robots, third is a circular defender .9 meters from the goal
            if (numRobots == 3)
            {
                SharedPlays.CircularDefender_1(state, .9);
            }

            // if there are four or more robots, the third and fourth are a two man wall
            if (numRobots > 3)
            {
                SharedPlays.TwoManWall_2(state);
            }

            // if there are five robots, fifth is in a support position
            if (numRobots > 4)
            {
                SharedPlays.SupportRobot_1(state);
            }
        }
    }

    /// <summary>
    /// This is the only play in which there are substantial differences between the old play system and
    /// the new one. The old plays are almost entirely identical to the stopped plays, with the
    /// exception of times there are multiple enemies on the field. The old plays were designed
    /// by the play designer, and some are rather indecipherable. For these reasons, as a special
    /// case, I am passing this case on to the stopped play, which I believe is entirely appropriate
    /// </summary>
    class SetPlay_TheirsLibrary : PlayLibrary
    {
        public SetPlay_TheirsLibrary() : base(PlayType.SetPlay_Theirs) { }
        PlayLibrary StoppedPlayLibrary = new StoppedLibrary();

        /// <summary>
        ///  just passes it on to the stopped play library
        /// </summary>
        public override void mainplay(GameState state)
        {
            StoppedPlayLibrary.mainplay(state);
        }
    }

    class HaltLibrary : PlayLibrary
    {
        public HaltLibrary() : base(PlayType.Halt) { }

        /// <summary>
        /// This does absolutely nothing, so that the play assigner gives them all stop commands
        /// </summary>
        public override void mainplay(GameState state)
        {
        }
    }
    #endregion REFBOXPLAYS
}
