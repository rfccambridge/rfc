using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Geometry;
using Robocup.Core;
using Robocup.Utilities;

namespace Robocup.PlaySystem
{
    // Note: This file and PlayLibrary.cs are the two files in which all actual play
    // logic resides

    /// <summary>
    /// Shared plays provide functionality, such as goalie or two man wall, that
    /// is used in plays of multiple other types.
    /// </summary>
    static class SharedPlays
    {
        // static constants
        // offense
        static double OFFENSE_SUPPORT_AVOID; // offense
        static double OFFENSE_STANDBY_DISTANCE;
        static double OFFENSE_BALL_IN_RANGE;
        static double GOALIE_DISTANCE; // goalie
        static double GOALIE_MAX_MIN_Y;
        static double TWO_MAN_WALL_PERIMETER; // two man wall
        static double TWO_MAN_WALL_KICK_DISTANCE;
        static double TWO_MAN_WALL_DISTANCE_APART;
        static double GUARD_BALL_DISTANCE; // guard ball
        static double GUARD_BALL_ROTATE_ANGLE;
        static double CIRCULAR_DEFENDER_PERIMETER;

        /// <summary>
        /// static constructor, to set up constants the first time a play is called
        /// </summary>
        static SharedPlays()
        {
            // reload constants for the first time
            ReloadConstants();
        }

        /// <summary>
        /// Reload all static constants used in shared plays.
        /// </summary>
        public static void ReloadConstants()
        {
            OFFENSE_SUPPORT_AVOID = ConstantsRaw.get<double>("plays", "OFFENSE_SUPPORT_AVOID");
            OFFENSE_STANDBY_DISTANCE = ConstantsRaw.get<double>("plays", "OFFENSE_STANDBY_DISTANCE");
            OFFENSE_BALL_IN_RANGE = ConstantsRaw.get<double>("plays", "OFFENSE_BALL_IN_RANGE");
            GOALIE_DISTANCE = ConstantsRaw.get<double>("plays", "GOALIE_DISTANCE");
            GOALIE_MAX_MIN_Y = ConstantsRaw.get<double>("plays", "GOALIE_MAX_MIN_Y");
            TWO_MAN_WALL_PERIMETER = ConstantsRaw.get<double>("plays", "TWO_MAN_WALL_PERIMITER");
            TWO_MAN_WALL_KICK_DISTANCE = ConstantsRaw.get<double>("plays", "TWO_MAN_WALL_KICK_DISTANCE");
            TWO_MAN_WALL_DISTANCE_APART = ConstantsRaw.get<double>("plays", "TWO_MAN_WALL_DISTANCE_APART");
            GUARD_BALL_DISTANCE = ConstantsRaw.get<double>("plays", "GUARD_BALL_DISTANCE");
            GUARD_BALL_ROTATE_ANGLE = ConstantsRaw.get<double>("plays", "GUARD_BALL_ROTATE_ANGLE");
            CIRCULAR_DEFENDER_PERIMETER = ConstantsRaw.get<double>("plays", "CIRCULAR_DEFENDER_PERIMETER");
        }

        // DEFENSE HELPERS

        /// <summary>
        /// Place the closest robot to the goal at an appropriate goalie point, 
        /// using the default distance from the goal
        /// </summary>
        /// <param name="state"></param>
        public static void Goalie_1(GameState state)
        {
            Goalie_1(state, GOALIE_DISTANCE);
        }

        /// <summary>
        /// Given the distance that the goalie should be from the boundary, place the closest
        /// robot to the goal at an appropriate goalie point
        /// </summary>
        public static void Goalie_1(GameState state, double goaliedist)
        {
            Vector2 ballpoint = state.Predictor.GetBall().Position;
            Line ballgoalline = new Line(ballpoint, Constants.FieldPts.OUR_GOAL);

            // Find a good blocking position for the goalie
            double goalieXpos = goaliedist - (Constants.Field.WIDTH / 2);
            Line vertgoalline = new Line(new Vector2(goalieXpos, Constants.Field.HEIGHT),
                                         new Vector2(goalieXpos, -Constants.Field.HEIGHT));
            Vector2 goaliept = Intersections.intersect(ballgoalline, vertgoalline);

            // prevent the Y coordinate of the goalie point from being too high or too low
            Vector2 newgoaliept = new Vector2(goaliept.X, Math.Max(Math.Min(goaliept.Y, GOALIE_MAX_MIN_Y), -GOALIE_MAX_MIN_Y));

            // move the closest robot to that point
            MoveClosest_1(state, newgoaliept, ballpoint);
        }

        /// <summary>
        /// Two man wall- position two robots as a wall the default perimeter from the goal
        /// </summary>
        public static void TwoManWall_2(GameState state)
        {
            TwoManWall_2(state, TWO_MAN_WALL_PERIMETER);
        }


        /// <summary>
        /// Two man wall- position two robots as a wall the given perimeter from the goal
        /// </summary>
        public static void TwoManWall_2(GameState state, double perimeter)
        {
            Vector2 ballpoint = state.Predictor.GetBall().Position;
            Line ballgoalline = new Line(ballpoint, Constants.FieldPts.OUR_GOAL);
            Circle goalperim = new Circle(Constants.FieldPts.OUR_GOAL, perimeter);

            Vector2 point1 = Intersections.intersect(ballgoalline, goalperim, -1);
            Circle circle1 = new Circle(point1, TWO_MAN_WALL_DISTANCE_APART);

            Vector2 twomanpt1 = Intersections.intersect(circle1, goalperim, -1);
            Vector2 twomanpt2 = Intersections.intersect(circle1, goalperim, 1);

            // split the functionality of checking for kicking
            twomanwallOneRobot(state, twomanpt1);
            twomanwallOneRobot(state, twomanpt2);
        }

        /// <summary>
        /// Move one robot assigned to a two man wall to a given point, but kick
        /// the ball if it is within an appropriate distance
        /// </summary>
        private static void twomanwallOneRobot(GameState state, Vector2 point)
        {
            RobotInfo robot = state.Functions.closestRobot(point);

            if (robot.Position.distanceSq(state.Predictor.GetBall().Position)
                 < TWO_MAN_WALL_KICK_DISTANCE * TWO_MAN_WALL_KICK_DISTANCE)
            {
                state.Assigner.Kick(robot.ID, Constants.FieldPts.THEIR_GOAL);
            }
            else
            {
                state.Assigner.Move(robot.ID, point);
            }
        }

        /// <summary>
        /// given a distance, have a robot defend by staying the default distance from the goal, always in
        /// between the goal and the ball, facing the ball. Similar to two man wall, for one robot
        /// </summary>
        public static void CircularDefender_1(GameState state)
        {
            CircularDefender_1(state, CIRCULAR_DEFENDER_PERIMETER);
        }

        /// <summary>
        /// given a distance, have a robot defend by staying that distance from the goal, always in
        /// between the goal and the ball, facing the ball
        /// </summary>
        public static void CircularDefender_1(GameState state, double distance)
        {
            Vector2 ballpoint = state.Predictor.GetBall().Position;
            Line ballgoalline = new Line(ballpoint, Constants.FieldPts.OUR_GOAL);
            Circle goalperim = new Circle(Constants.FieldPts.OUR_GOAL, distance);
            Vector2 defenderpoint = Intersections.intersect(ballgoalline, goalperim, -1);

            MoveClosest_1(state, defenderpoint, ballpoint);
        }

        /// <summary>
        /// Move the closest robot to a point to that point
        /// </summary>
        public static void MoveClosest_1(GameState state, Vector2 point)
        {
            RobotInfo robot = state.Functions.closestRobot(point);
            state.Assigner.Move(robot.ID, point);
        }

        /// <summary>
        /// Move the closest robot to a point to that point, facing another point
        /// </summary>
        public static void MoveClosest_1(GameState state, Vector2 point, Vector2 facing)
        {
            RobotInfo robot = state.Functions.closestRobot(point);
            state.Assigner.Move(robot.ID, point, facing);
        }

        /// <summary>
        /// Use the robot closest to the ball to kick the ball
        /// </summary>
        public static void OneKicker_1(GameState state)
        {
            int closestRobot = state.Functions.closestRobot(state.Predictor.GetBall().Position).ID;
            state.Assigner.Kick(closestRobot, Constants.FieldPts.THEIR_GOAL);
        }

        /// <summary>
        /// Place a robot at a support position, where it is in front of the ball
        /// and out of the way
        /// </summary>
        /// <param name="state"></param>
        public static void SupportRobot_1(GameState state)
        {
            // see if the ball is in range of the goal
            Vector2 ballpoint = state.Predictor.GetBall().Position;
            bool ballinrange = (ballpoint.distanceSq(Constants.FieldPts.THEIR_GOAL) < OFFENSE_BALL_IN_RANGE * OFFENSE_BALL_IN_RANGE);

            // find the appropriate robot
            RobotInfo supportRobot = state.Functions.closestRobot(ballpoint);

            // figure out where to place it
            Vector2 support_point;
            if (ballinrange)
            {
                // the ball is close to the goal, find a good standby point
                Circle theirgoalcircle = new Circle(Constants.FieldPts.THEIR_GOAL, OFFENSE_STANDBY_DISTANCE);
                Vector2 theirgoalpositivequad = Intersections.intersect(new Line(Constants.FieldPts.THEIR_GOAL, Constants.FieldPts.TOP), theirgoalcircle, 1);
                Vector2 theirgoalnegativequad = Intersections.intersect(new Line(Constants.FieldPts.THEIR_GOAL, Constants.FieldPts.BOTTOM), theirgoalcircle, 1);
                support_point = (ballpoint.Y > 0) ? theirgoalnegativequad : theirgoalpositivequad;
            }
            else
            {
                // the ball is far from the goal, find a support point to retrieve the ball after it is kicked
                Vector2 balldestination = Intersections.intersect(new Line(ballpoint, Constants.FieldPts.THEIR_GOAL), new Circle(ballpoint, 1), 1);

                // support point is either above or below the point
                if (state.Functions.pointAboveLine(supportRobot.Position, new Line(ballpoint, Constants.FieldPts.THEIR_GOAL)))
                {
                    support_point = balldestination + new Vector2(0, OFFENSE_SUPPORT_AVOID);
                }
                else
                {
                    support_point = balldestination - new Vector2(0, OFFENSE_SUPPORT_AVOID);
                }
            }

            // assign it to this point
            state.Assigner.Move(supportRobot.ID, support_point);
        }

        /// <summary>
        /// Guard the ball from a given distance with up to three robots. Good for plays where
        /// the robot cannot go within 50 cm of the ball
        /// </summary>
        /// <param name="numRobots">Must be 1, 2 or 3 robots</param>
        public static void GuardBall_1_2_3(GameState state, int numRobots)
        {
            // the number of robots given to this play must be between 1 and 3
            if (numRobots < 1 || numRobots > 3)
            {
                throw new PlayLanguageException("guardBall_1_2_3_vs_0 must be given 1, 2, or 3 as numRobots");
            }

            // set up geometry for the play
            Vector2 ballpoint = state.Predictor.GetBall().Position;
            Circle ballcircle = new Circle(ballpoint, .63);
            Line ballgoalline = new Line(ballpoint, Constants.FieldPts.OUR_GOAL);
            Vector2 wall1pt = Intersections.intersect(ballgoalline, ballcircle, 1);

            // First robot- guards the ball from GUARD_BALL_DISTANCE away
            if (numRobots > 0)
            {
                // if it is outside the field, find a better point
                Vector2 robot1destination = state.Functions.inField(wall1pt) ? wall1pt : new Vector2(1, 0);
                SharedPlays.MoveClosest_1(state, robot1destination);
            }

            // Second robot: like the first robot, but rotated over
            if (numRobots > 1)
            {
                Vector2 wall2pt = state.Functions.rotatePointAroundAnotherPoint(ballpoint, wall1pt, .35);
                Vector2 robot2destination = state.Functions.inField(wall2pt) ? wall2pt : new Vector2(1.5, -1);
                SharedPlays.MoveClosest_1(state, robot2destination);
            }

            // Third robot, rotated to the opposite side of the first robot
            if (numRobots > 2)
            {
                Vector2 wall3pt = state.Functions.rotatePointAroundAnotherPoint(ballpoint, wall1pt, -0.35);
                Vector2 robot3destination = state.Functions.inField(wall3pt) ? wall3pt : new Vector2(1.5, 1);
                SharedPlays.MoveClosest_1(state, robot3destination);
            }
        }
    }

    /// <summary>
    /// An exception thrown in cases where the play language is used incorrectly, such as when a
    /// function requiring three free robots is used with too few robots
    /// </summary>
    class PlayLanguageException : Exception
    {
        public PlayLanguageException(string message)
            : base(message)
        {
        }
    }
}
