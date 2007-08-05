using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Infrastructure;

namespace Navigation
{
    namespace Current
    {
        public class State
        {
            public int parent, id;
            public float x, y;

            public State()
            {
                parent = -1;
                id = -1;
                x = -10000.0f;
                y = -10000.0f;
            }

            public State(int parent, int id, float x, float y)
            {
                this.parent = parent;
                this.id = id;
                this.x = x;
                this.y = y;
            }

            public void copy(State mystate)
            {
                parent = mystate.parent;
                id = mystate.id;
                x = mystate.x;
                y = mystate.y;
            }
            public void copy(Vector2 vector)
            {
                x = vector.X;
                y = vector.Y;
            }
            public void set(int parent, int id, float x, float y)
            {
                this.parent = parent;
                this.id = id;
                this.x = x;
                this.y = y;
            }
        }

        public class CurrentNavigator : INavigator
        {
            //for labeling purposes only
            const string NAME = "CurrentNavigator";

            const float RRTHEIGHT = 3.4f;
            const float RRTWIDTH = 4.9f;
            //TODO find good values for this
            const float RANDEXPLORE = 0.07f;//.05
            const float WAYEXPLORE = 0.05f;//.2f
            //const float THRESHOLD = .050f;
            const int WAYPTS = 10;
            const int NODES = 500;
            const float OBSTACLE_STUCK_CONSTANT = 2000;
            const float PENALTYRADIUS = .550f;
            float EXTEND_DISTANCE = .0500f;
            //const float BALL_MULTIPLIER = 1.4f;
            const int NUM_OPP = 5;
            const int NUM_TEAM = 5;
            const float ROBOT_RADIUS = .100f;
            float BOT_AVOID_MULT = 2.5f;
            const float BOT_STUCK_MULT = 1.5f;

            const float FIELDMIN = -(RRTWIDTH / 2 + 1f);
            const float FIELDMAX = (RRTWIDTH / 2 + 1f);

            Vector2 THE_GOAL = new Vector2(2.28f, 0.0f);
            Vector2 HOME_GOAL = new Vector2(-2.28f, 0.0f);

            public State[] rrttree = new State[NODES];
            public State[,] waypoints = new State[NUM_TEAM, WAYPTS];
            public State[] obstacles = new State[NUM_OPP + NUM_TEAM + 1];
            public State target, extend, nearest, result, temp, cur, random, toAdd, ourTarget, goal, start, end;
            //Vector2 lastDest;
            //int nodeIndex;
            public int treecount;
            public int wayptcount;
            public int lasttreecount;

            Random rand;

            public string Name
            {
                get { return NAME; }
            }

            public CurrentNavigator()
            {
                rand = new Random();
                target = new State();
                extend = new State();
                nearest = new State();
                result = new State();
                temp = new State();
                cur = new State();
                random = new State();
                toAdd = new State();
                ourTarget = new State();
                goal = new State();
                start = new State();
                end = new State();
                //lastDest = new Vector2();

                Initialize();
            }

            Vector2 unblockedDestination(Vector2 pos, Vector2 oldDest, Vector2 obs, double radMult)
            {
                //if the destination is too close to ball or enemy, pick a new one that is closer
                //Vector2 ret = new Vector2(0.0f,0.0f);
                //Vector2 inc = new Vector2(0.0f,0.0f); //iterate up or down

                return oldDest + ((float)(radMult * ROBOT_RADIUS)) * (oldDest - obs + .001f * (pos - oldDest)).normalize();

                /*Vector2 ret = new Vector2(oldDest.X, oldDest.Y);
                float deltax = pos.X - oldDest.X;
                float deltay = pos.Y - oldDest.Y;
                float norm = (float)Math.Sqrt(deltax * deltax + deltay * deltay);
                deltax = deltax/norm * ROBOT_RADIUS / 2;
                deltay = deltay/norm * ROBOT_RADIUS / 2;

                while (Math.Sqrt((ret.X - obs.X) * (ret.X - obs.X) +
                    (ret.Y - obs.Y) * (ret.Y - obs.Y)) < radMult * ROBOT_RADIUS)
                {
                    ret.X += deltax;
                    ret.Y += deltay;
                }*/


                //return ret;
            }


            public Vector2 navigate(int robotId, Vector2 pos, Vector2 destination, RobotInfo[] teamBots, RobotInfo[] enemyBots, BallInfo ballpos, double avoid_ball, float avoid_robots, float dist_thresh)
            {
                EXTEND_DISTANCE = (float)Math.Max(.0005, Math.Min(.05, Math.Sqrt(pos.distanceSq(destination)) * .25f));
                BOT_AVOID_MULT = avoid_robots;

                DeleteLastPlan();
                State begin, next;
                //lastDest = new Vector2(destination.X, destination.Y);


                // add enemy robots to obstacle array
                for (int i = 0; i < Math.Min(NUM_OPP, enemyBots.Length); i++)
                {
                    if (Distance(enemyBots[i].Position, destination) < BOT_STUCK_MULT * ROBOT_RADIUS)
                    {
                        destination = unblockedDestination(pos, destination, enemyBots[i].Position, 2);
                    }
                    if (Distance(enemyBots[i].Position, pos) < 2 * ROBOT_RADIUS)
                    {
                        Console.WriteLine("ShortCircuit enemy close");
                        return destination;
                    }
                    obstacles[i].copy(enemyBots[i].Position);
                }

                int id = -1;
                // add our robots to obstacle array
                for (int i = NUM_OPP; i < NUM_OPP + Math.Min(teamBots.Length, NUM_TEAM); i++)
                {
                    // don't want robot to avoid itself
                    if (teamBots[i - NUM_OPP].ID != robotId)
                    {
                        if (Distance(teamBots[i - NUM_OPP].Position, destination) < BOT_STUCK_MULT * ROBOT_RADIUS)
                        {
                            destination = unblockedDestination(pos, destination, teamBots[i - NUM_OPP].Position, 2);
                        }
                        obstacles[i].copy(teamBots[i - NUM_OPP].Position);
                    }
                    else
                    {
                        obstacles[i].x = -10000.5f;
                        obstacles[i].y = -10000.5f;

                        id = i - NUM_OPP;
                    }
                }

                // if we are given an avoid-ball radius, add ball to obstacle vector as well
                if (avoid_ball > 1.0)
                {
                    //		debugstream << "goal to ball: " << (ball.pos-destination).mag() << endl;
                    if (Distance(ballpos.Position, destination) < avoid_ball * ROBOT_RADIUS)
                    {
                        destination = unblockedDestination(pos, destination, ballpos.Position, 1.0);
                    }
                    obstacles[NUM_OPP + NUM_TEAM].copy(ballpos.Position);


                }

                if (Distance(ballpos.Position, pos) < ROBOT_RADIUS)
                {
                    Console.WriteLine("ShortCircuit ball close");
                    return destination;
                }



                // if we are far away enough from the destination...
                if ((Distance(pos, destination) > dist_thresh) &&
                    (pos.X > FIELDMIN) &&
                    (pos.X < FIELDMAX) &&
                    (pos.Y > FIELDMIN) &&
                    (pos.Y < FIELDMAX))
                {

                    // copy beginning and target location
                    begin = new State();
                    begin.copy(pos);
                    end.copy(destination);
                    //		debugstream << "Pos: " << pos.x << " , " << pos.y << endl;

                    // Plan and populate array
                    RRTPlan(id, begin, end, avoid_ball, dist_thresh);
                    // Process plan
                    start.copy(begin);
                    end.copy(rrttree[(treecount - 1)]);
                    lasttreecount = treecount;
                    next = ProcessPlan(id, avoid_ball);

                    Vector2 go = new Vector2(next.x, next.y);

                    if ((next.x == destination.X) && (next.y == destination.Y))
                    {
                        //Console.WriteLine("Strafing to final destination...");
                        return go;
                    }
                    else
                    {
                        //Console.WriteLine("Strafing to nonfinal destination...");
                        return go;
                    }


                }
                //Console.WriteLine("reached target");
                return pos;
            }

            /**
             * Can be reached from cur towards target
             * ball - whether or not we are avoiding the ball
             * NOTE: target and cur have to be set before this is called
             * result placed in extend
             **/
            void Extend(double ball)
            {
                double dy = target.y - cur.y;
                double dx = target.x - cur.x;

                double distance = Distance(cur, target);

                // if we are within threshold, then we are there!
                if (distance < EXTEND_DISTANCE)
                {
                    extend.y = target.y;
                    extend.x = target.x;
                }

                // return normalized vectors with changes for current position
                else
                {
                    extend.y = (float)(EXTEND_DISTANCE * (dy / distance) + cur.y);
                    extend.x = (float)(EXTEND_DISTANCE * (dx / distance) + cur.x);
                }

                // set the parent of the result to the current node
                extend.parent = cur.id;


                //obstacle avoid opponents
                for (int i = 0; i < NUM_OPP; i++)
                {
                    // the point we are given is too close, then set extend to invalid values
                    if (Distance(extend, obstacles[i]) < BOT_AVOID_MULT * ROBOT_RADIUS)
                    {
                        //debugstream << "Obstacle met on opponent team: number: " << i << " dist: "
                        //	<< Distance(extend, obstacles[i]) << endl;

                        // -2 indicates an invalid point on an obstacle
                        // this is returned to the function that called this so a new target can be found
                        extend.y = -10000.2f;
                        extend.x = -10000.2f;
                        return;
                    }
                }

                //obstacle avoid own team
                for (int i = 0; i < NUM_TEAM; i++)
                {
                    // i+NUM_OPP indicates the numbering for our team
                    // the robot should not try to avoid itself, so a null pointer is instead set
                    //if (obstacles[i + NUM_OPP].x < 0) continue;
                    if (Distance(extend, obstacles[i + NUM_OPP]) < BOT_AVOID_MULT * ROBOT_RADIUS)
                    {
                        //debugstream << "Obstacle met on own team: number: " << i << " dist: "
                        //	<< Distance(extend, obstacles[i+NUM_OPP]) << endl;

                        // if our robots are an obstacle return an invalid result
                        extend.y = -10000.3f;
                        extend.x = -10000.3f;
                        return;
                    }
                }

                // check for ball avoidance
                if (ball > 1.0)
                {
                    if (Distance(extend, obstacles[NUM_TEAM + NUM_OPP]) < ball * ROBOT_RADIUS)
                    {
                        //debugstream << "Obstacle met (ball): " << endl;

                        extend.y = -10000.4f;
                        extend.x = -10000.4f;
                        return;
                    }
                }
            }

            double Distance(State a, State b) //distance from cur to target
            {
                double dy = b.y - a.y;
                double dx = b.x - a.x;
                return Math.Sqrt(dy * dy + dx * dx);
            }

            double Distance(Vector2 a, Vector2 b) //distance from cur to target
            {
                double dy = b.Y - a.Y;
                double dx = b.X - a.X;
                return Math.Sqrt(dy * dy + dx * dx);
            }
            //result of this stored in random
            void RandomState() //random state
            { //-300-5100

                random.x = (float)(rand.NextDouble() * (RRTHEIGHT + .600) - RRTHEIGHT / 2 - .300);
                random.y = (float)(rand.NextDouble() * (RRTWIDTH + .600) - RRTWIDTH / 2 - .300);
            }

            // before calling, make sure toAdd has good value
            void addNode()
            {
                //debugstream << "treecount: " << treecount << " parent at: " << node.parent << endl;
                rrttree[treecount].copy(toAdd);
                rrttree[treecount].id = treecount;
                treecount++;
            }

            public int obstacleStuckCount;
            /**
             * main function to generate the tree structure for object avoidance navigation
             **/
            void RRTPlan(int id, State initial, State g, double ball, double distance_threshold) //builds tree from initial to goal
            {
                obstacleStuckCount = 0;
                // new states allocated for target, extended and nearest, with nearest point set
                // the same as the initial
                nearest.x = initial.x;
                nearest.y = initial.y;
                nearest.id = 0;
                nearest.parent = -1;

                // create the beginning point
                treecount = 0;
                toAdd.copy(nearest);
                addNode();
                goal.copy(g);


                //	debugstream << "going into while" << endl;

                // generates the tree from nearest to goal within THRESHOLD
                while (Distance(nearest, goal) > distance_threshold)
                {
                    ChooseTarget(id);		// sets ourTarget: chooses randomly between random state, extend nearest to goal, extend toward waypt.
                    Nearest();			// sets nearest
                    cur.copy(nearest);
                    target.copy(ourTarget);
                    Extend(ball);

                    // if for some reason we are stuck w/in the robot radius
                    if (obstacleStuckCount > OBSTACLE_STUCK_CONSTANT)
                    {
                        // swap target w/ nearest
                        ourTarget.copy(goal);
                        Nearest();
                        /*ourTarget.copy(rrttree[(treecount - 1)]);
                        int targetID = ourTarget.id;
                        ourTarget.id = nearest.id;
                        nearest.id = targetID;*/
                        rrttree[(treecount - 1)].id = nearest.id;
                        nearest.id = treecount - 1;

                        //State temp = new State(nearest.parent, nearest.id, nearest.x, nearest.y);
                        State temp = rrttree[nearest.id];
                        rrttree[nearest.id] = rrttree[treecount - 1];
                        rrttree[treecount - 1] = temp;
                        //rrttree[ourTarget.id].copy(rrttree[(treecount - 1)]);
                        //rrttree[(treecount - 1)].copy(temp);

                        //State temp(goal.parent, goal.id, goal.x, goal.y);
                        //rrttree[treecount-1].copy(temp);
                        break;
                    }
                    /*debugstream << "goal: " << goal.x << " , " << goal.y << endl;
                    debugstream << "extended vector: " << extend.x << " , " << extend.y << endl;
                    debugstream << "target: " << ourTarget.x << " , " << ourTarget.y << endl;
                    debugstream << "initial: " << initial.x << " , " << initial.y << endl;
                    debugstream << "nearest: " << nearest.x << " , " << nearest.y << endl;
                    debugstream << "treecount: " << treecount << endl;
                    debugstream << "rrttree[0]: " << rrttree[0].x << " , " << rrttree[0].y << endl;*/

                    // if valid point (non-negative), add the node to the tree
                    if (extend.x > FIELDMIN && extend.y > FIELDMIN &&
                        extend.x < FIELDMAX && extend.y < FIELDMAX)
                    {
                        //cout << "extended: x: " << extended.x << " y: " << extended.y << endl;
                        toAdd.copy(extend);
                        addNode();
                    }
                    else
                    {
                        obstacleStuckCount++;
                    }


                    // if we exceed the max number of nodes, exit loop
                    if (treecount >= NODES - 1) //only process up to NODES
                    // NODES - 1 because postprocess needs 1 node
                    {
                        break;
                    }
                }
            }

            /**
             * Resets obstacles, nodes, counter variables.
             **/
            void DeleteLastPlan()
            {

                for (int i = 0; i < treecount; i++)
                {
                    rrttree[i].set(-1, -1, -10000.0f, -10000.0f);
                }
                for (int i = 0; i < NUM_OPP + NUM_TEAM + 1; i++)
                {
                    obstacles[i].set(-1, -1, -10000.0f, -10000.0f);
                }

                treecount = 0;
            }

            /**
             * Based on randomly generated probability, choose next point to go to.
             * NOTE: make sure goal is set before calling this
             * Function puts result into ourTarget
             **/
            void ChooseTarget(int id)
            {
                double p = rand.NextDouble();

                // set random point on field as target
                if (p < RANDEXPLORE)
                {
                    RandomState();
                    ourTarget.copy(random);
                    return;
                }

                // otherwise pop random waypoint out of your tree
                else if (p < (RANDEXPLORE + WAYEXPLORE))
                {
                    State way = waypoints[id, rand.Next(WAYPTS)];

                    // if stored waypoint works, return stored waypoint
                    if (way.x > FIELDMIN && way.y > FIELDMIN &&
                        way.x < FIELDMAX && way.x < FIELDMAX)
                    {
                        //cout << "waypointing" << endl;
                        ourTarget.set(-1, -1, way.x, way.y);
                        return;
                    }
                }

                // otherwise go towards the goal
                ourTarget.set(-1, -1, goal.x, goal.y);
            }

            /**
             * Find nearest state in tree to target.
             * NOTE: needs ourTarget to be set before calling this
             * stores result in nearest
             */
            void Nearest()
            {
                // linear search through array of states to find the 
                // nearest node in the array to the target
                int bestnode = 0;
                double tmpDist = Distance(rrttree[0], ourTarget);
                double minDist = tmpDist;
                for (int i = 1; i < treecount; i++)
                {
                    if (rrttree[i].x < FIELDMIN) continue; //ignore bad nodes
                    tmpDist = Distance(rrttree[i], ourTarget);
                    //cout << "node: x: " << rrttree[i].x << " y: " << rrttree[i].y << endl;
                    if (tmpDist < minDist)
                    {
                        minDist = tmpDist;
                        bestnode = i;
                    }
                }
                //cout << "target: x: " << target.x << " y: " << target.y << endl;
                //cout << "size: " << treecount << " nearest: x: " << result.x << " y: " << result.y << endl;
                nearest.set(rrttree[bestnode].parent, bestnode, rrttree[bestnode].x, rrttree[bestnode].y);
            }

            /**
             * need start and end before calling this
             **/
            State ProcessPlan(int id, double ball)
            {
                //int curind;
                ourTarget.copy(goal);
                Nearest();
                State myCur = nearest;
                State last = myCur;

                start.id = 0;

                //    debugstream << "PLAN FOUND: " << endl;
                //    debugstream << "x: " << myCur.x << " y: " << myCur.y << endl;
                AddWaypoint(myCur, id);

                State news = new State();
                // until you get to the start ID--keep looking, bitch
                while (myCur.parent > 0)
                {
                    //if we can get to this node directly from initial state,
                    // short-circuit and link the starting node directly
                    if (PostProcess(myCur, start, ball))
                    {
                        if (myCur.x != start.x || myCur.y != start.y)
                        {
                            //cur.copy(start);
                            //target.copy(*myCur);

                            cur.copy(myCur);
                            target.copy(start);

                            //need intermediate node because don't know where in array we are
                            Extend(ball);
                            news.copy(extend);
                            toAdd.set(0, news.id, news.x, news.y);
                            addNode();

                            //State* news = new State;
                            //news.x = cur.x;
                            //news.y = cur.y;
                            myCur.parent = (treecount - 1);

                            start.parent = -1;

                        }
                    }
                    last = myCur;
                    myCur = rrttree[myCur.parent];
                    //cout << "x: " << cur.x << " y: " << cur.y << endl;
                    AddWaypoint(myCur, id);

                    //edit = waypoints[rand()%WAYPTS];
                    //edit.x = cur.x;
                    //edit.y = cur.y;
                }
                State ret = new State(-1, -1, last.x, last.y);//myb mycur? instead
                return ret;
            }

            void AddWaypoint(State cur, int id)
            {
                //debugstream << "Waypoint: " << cur.x << ", " << cur.y << endl;
                if (cur.x < FIELDMIN || cur.y < FIELDMIN)
                    return;

                int edit;
                if (wayptcount < WAYPTS)
                {//fill waypoints first
                    edit = wayptcount++;
                }
                else
                {//randomly replace
                    edit = rand.Next(WAYPTS);
                }
                waypoints[id, edit].copy(cur);

            }

            double absd(double s)
            {
                return (s > 0) ? s : -s;
            }

            double ret;
            double angleBetween(Vector2 a, Vector2 b)
            {
                ret = (a.X * b.X + a.Y * b.Y) /
                    Math.Sqrt((a.X * a.X + a.Y * a.Y) * (b.X * b.X + b.Y * b.Y));

                if (ret > 0)
                    return Math.Acos(ret);
                else
                    return -Math.Acos(ret);
            }

            double mag(Vector2 a)
            {
                return Math.Sqrt(a.X * a.X + a.Y * a.Y);
            }

            /*double PointToLine(State* pt, State* start, State* end)
            {
                //Ax+y=C
                //Ax1+y1=C, Ax2+y2=C
                //A = -(y2-y1)/(x2-x1)
                //C = Ax1+y1
                double A;
                double C;
                A = -1*(end.y - start.y)/(end.x - start.x);
                C = A*end.x + end.y;
            
                return absd(A*pt.x + pt.y - C)/sqrt(A*A+1);
            
            }*/

            Vector2 starttopt;
            Vector2 endtopt;
            Vector2 starttoend;
            double PointToLineSegment(State pt, State start, State end)
            {
                starttopt = new Vector2(pt.x - start.x, pt.y - start.y);
                endtopt = new Vector2(pt.x - end.x, pt.y - end.y);
                starttoend = new Vector2(end.x - start.x, end.y - start.y);
                /*starttopt.X = pt.x - start.x;
                starttopt.Y = pt.y - start.y;
                endtopt.X = pt.x - end.x;
                endtopt.Y = pt.y - end.y;
                starttoend.X = end.x - start.x;
                starttoend.Y = end.y - start.y;*/

                /*angleFirst = angleBetween(starttopt, starttoend);
                angleSecond = angleBetween(endtopt, starttoend);
                if ((angleFirst < Math.PI / 2 && angleSecond < Math.PI / 2) ||
                    (angleFirst > Math.PI / 2 && angleSecond > Math.PI / 2))*/
                if ((starttopt.X * starttoend.X + starttopt.Y * starttoend.Y) < 0 ||
                    (endtopt.X * starttoend.X + endtopt.Y * starttoend.Y) > 0)
                {
                    //left endpoint
                    //right endpoint
                    return Math.Min(mag(starttopt), mag(endtopt));
                }
                else
                {
                    //if point is in middle of line
                    double A;
                    double B;
                    double C;
                    A = -(end.y - start.y);
                    B = end.x - start.x;
                    C = -1 * (end.y - start.y) * end.x + (end.x - start.x) * end.y;

                    return absd(A * pt.x + B * pt.y - C) / Math.Sqrt(A * A + B * B);
                }
            }

            /**
             * Process tree, replace each node with single straight path unless there is an
             * obstacle in between
             **/
            bool PostProcess(State target, State cur, double ball)
            {
                State obs;
                for (int i = 0; i < NUM_OPP + NUM_TEAM; i++)
                {

                    obs = obstacles[i];
                    if (obs.x < FIELDMIN || obs.y < FIELDMIN)
                    {
                        //don't hit yourself
                        continue;
                    }
                    if (PointToLineSegment(obs, target, cur) < BOT_AVOID_MULT * ROBOT_RADIUS)
                    {
                        //debugstream << "POINTTOLINEERROR: " << i - NUM_OPP << endl;
                        return false;
                    }
                }

                //check for ball
                if (ball > 1.0)
                {
                    if (PointToLineSegment(obstacles[NUM_OPP + NUM_TEAM], target, cur) < ball * ROBOT_RADIUS)
                    {
                        //debugstream << "POINTTOLINEERRORBALL: " << PointToLine(&obstacles[NUM_OPP+NUM_TEAM], target, cur) << endl;
                        return false;
                    }
                }
                return true;
            }

            void Initialize()
            {
                wayptcount = 0;
                treecount = 0;

                for (int j = 0; j < NUM_TEAM; j++)
                {
                    for (int i = 0; i < WAYPTS; i++)
                    {
                        waypoints[j, i] = new State();
                    }
                }

                for (int i = 0; i < NODES; i++)
                {
                    rrttree[i] = new State();
                }

                for (int i = 0; i < NUM_TEAM + NUM_OPP + 1; i++)
                { // 1 is for the ball
                    obstacles[i] = new State();
                }
            }




            public Vector2 navigate(int id, Vector2 position, Vector2 destination, RobotInfo[] teamPositions, RobotInfo[] enemyPositions, BallInfo ballPosition, float avoidBallDist)
            {
                return navigate(id, position, destination, teamPositions, enemyPositions, ballPosition, avoidBallDist, 2.3f, .05f);
            }


            public void drawLast(System.Drawing.Graphics g, ICoordinateConverter c)
            {
                System.Drawing.Brush b = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
                for (int i = 0; i < treecount; i++)
                {
                    //Vector2 pixelPoint = c.fieldtopixelX(rrttree[i].x);
                    g.FillRectangle(b, c.fieldtopixelX(rrttree[i].x) - 1, c.fieldtopixelY(rrttree[i].y) - 1, 2, 2);
                }
                b.Dispose();
            }
        }
    }
}
