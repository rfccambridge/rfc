using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using Robocup.Simulation;

namespace NavigationRacer
{
    class FieldState
    {
        public FieldState(Vector2 ballpos, Vector2[] destinations, Vector2[][] ourWaypoints, Vector2[][] theirWaypoints, Vector2[] ourVelocities,
            Vector2[] theirVelocities,double totalMS, double MSPerIteration, double iterations)
            :
            this(ballpos, destinations, ourWaypoints, theirWaypoints,ourVelocities,theirVelocities, new TestResults(1, (int)iterations, totalMS, .25))
        {
        }
        public FieldState(Vector2 ballpos, Vector2[] destinations, Vector2[][] ourWaypoints,
            Vector2[][] theirWaypoints, Vector2[] ourVelocities,Vector2[] theirVelocities, TestResults referenceResults)
        {
            this.ballpos = ballpos;
            this.destinations = destinations;
            this.ourWaypoints = ourWaypoints;
            this.theirWaypoints = theirWaypoints;
            this.ourVelocities = ourVelocities;
            this.theirVelocities = theirVelocities;
            ourPositions = new Vector2[ourWaypoints.Length];
            for (int i = 0; i < ourWaypoints.Length; i++)
            {
                ourPositions[i] = ourWaypoints[i][0];
                if (i < destinations.Length)
                {
                    this.ourWaypoints[i] = new Vector2[1];
                    this.ourWaypoints[i][0] = ourPositions[i];
                }
            }
            theirPositions = new Vector2[theirWaypoints.Length];
            for (int i = 0; i < theirWaypoints.Length; i++)
            {
                theirPositions[i] = theirWaypoints[i][0];
            }
            this.referenceResults = referenceResults;
            this.ourCurrentWaypoint = new int[ourWaypoints.Length];
            this.theirCurrentWaypoint = new int[theirWaypoints.Length];
        }

        Vector2 ballpos;
        Vector2[] destinations;
        Vector2[] ourPositions;
        Vector2[] theirPositions;
        Vector2[][] ourWaypoints, theirWaypoints;
        private Vector2[] ourVelocities;
        public Vector2[] OurVelocities
        {
            get { return ourVelocities; }
        }
        private Vector2[] theirVelocities;
        public Vector2[] TheirVelocities
        {
            get { return theirVelocities; }
        }
	
	
        public Vector2 BallPos
        {
            get { return ballpos; }
            set { ballpos = value; }
        }
        public Vector2[] Destinations
        {
            get { return destinations; }
            set { destinations = value; }
        }
        public Vector2[] OurPositions
        {
            get { return ourPositions; }
        }
        public Vector2[] TheirPositions
        {
            get { return theirPositions; }
        }
        public Vector2[][] OurWaypoints
        {
            get { return ourWaypoints; }
        }
        public Vector2[][] TheirWaypoints
        {
            get { return theirWaypoints; }
        }
        private TestResults referenceResults;
        public TestResults ReferenceResults
        {
            get { return referenceResults; }
            set { referenceResults = value; }
        }

        const double defaultspeed = .005;
        static private FieldState basic = new FieldState(new Vector2(1, 0), new Vector2[] { new Vector2(2, 0) }, new Vector2[][]{
                new Vector2[]{new Vector2(-2,0)},
                new Vector2[]{new Vector2(-.5, .3)},
                new Vector2[]{new Vector2(.6,-.4)},
                new Vector2[]{new Vector2(.8,-.8)},
                new Vector2[]{new Vector2(.5,-1.0)}}, new Vector2[][]{
                new Vector2[]{new Vector2(-1,0)},
                new Vector2[]{new Vector2(-1.5,0)},
                new Vector2[]{new Vector2(-.5,0)},
                new Vector2[]{new Vector2(-.5, -.6)},
                new Vector2[]{new Vector2(-.5,-.3)}},
            new Vector2[] { new Vector2(defaultspeed, 0), new Vector2(defaultspeed, 0), new Vector2(defaultspeed, 0), new Vector2(defaultspeed, 0), new Vector2(defaultspeed, 0) },
            new Vector2[] { new Vector2(defaultspeed, 0), new Vector2(defaultspeed, 0), new Vector2(defaultspeed, 0), new Vector2(defaultspeed, 0), new Vector2(defaultspeed, 0) },
                    735, 1.48, 495);
        static public FieldState Default
        {
            get { return basic.Clone(); }
        }

        public FieldState Clone()
        {
            //need to check if any of the robots without any paths were changed,
            //and reflect those changes in the waypoints:
            for (int i = 0; i < OurPositions.Length; i++)
            {
                if (OurWaypoints[i].Length == 1)
                {
                    OurWaypoints[i][0] = OurPositions[i];
                }
            }
            for (int i = 0; i < TheirPositions.Length; i++)
            {
                if (TheirWaypoints[i].Length == 1)
                {
                    TheirWaypoints[i][0] = TheirPositions[i];
                }
            }
            return new FieldState(BallPos, (Vector2[])Destinations.Clone(), (Vector2[][])OurWaypoints.Clone(),
                (Vector2[][])TheirWaypoints.Clone(), (Vector2[])ourVelocities.Clone(),(Vector2[])theirVelocities.Clone(), ReferenceResults);
        }

        private static string getLine(System.IO.StreamReader reader)
        {
            string s = "";
            while (s == "" || s[0] == '#')
            {
                s = reader.ReadLine();
            }
            return s;
        }
        public static FieldState load(System.IO.StreamReader reader)
        {
            double totalMS = double.Parse(getLine(reader));
            double iterations = double.Parse(getLine(reader));
            double MSPerIteration = double.Parse(getLine(reader));
            string[] descriptors = getLine(reader).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
#if DEBUG
            System.Diagnostics.Debug.Assert(descriptors.Length == 3, "wrong number of file descriptors");
#endif
            int numdestinations = int.Parse(descriptors[0]);
            int numOurPositions = int.Parse(descriptors[1]);
            int numTheirPositions = int.Parse(descriptors[2]);
            Vector2[] destinations = new Vector2[numdestinations];
            Vector2[][] ourWaypoints = new Vector2[numOurPositions][];
            Vector2[][] theirWaypoints = new Vector2[numTheirPositions][];
            Vector2[] ourVelocities=new Vector2[numOurPositions],theirVelocities=new Vector2[numTheirPositions];

            string[] ballcoords = getLine(reader).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Vector2 ballpos = new Vector2(double.Parse(ballcoords[0]), double.Parse(ballcoords[1]));

            for (int i = 0; i < numdestinations; i++)
            {
                string[] coords = getLine(reader).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                destinations[i] = new Vector2(double.Parse(coords[0]), double.Parse(coords[1]));
            }
            for (int i = 0; i < numOurPositions; i++)
            {
                string[] coords = getLine(reader).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                int numpoints = int.Parse(coords[0]);
#if DEBUG
                System.Diagnostics.Debug.Assert(coords.Length == numpoints * 2 + 2, "wrong number of coordinates");
#endif
                ourVelocities[i] = new Vector2(double.Parse(coords[1]), 0);
                ourWaypoints[i] = new Vector2[numpoints];
                for (int j = 0; j < numpoints; j++)
                {
                    ourWaypoints[i][j] = new Vector2(double.Parse(coords[2 * j + 2]), double.Parse(coords[2 * j + 3]));
                }
            }
            for (int i = 0; i < numOurPositions; i++)
            {
                string[] coords = getLine(reader).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                int numpoints = int.Parse(coords[0]);
#if DEBUG
                System.Diagnostics.Debug.Assert(coords.Length == numpoints * 2 + 2, "wrong number of coordinates");
#endif
                theirVelocities[i] = new Vector2(double.Parse(coords[1]), 0);
                theirWaypoints[i] = new Vector2[numpoints];
                for (int j = 0; j < numpoints; j++)
                {
                    theirWaypoints[i][j] = new Vector2(double.Parse(coords[2 * j + 2]), double.Parse(coords[2 * j + 3]));
                }
            }

            return new FieldState(ballpos, destinations, ourWaypoints, theirWaypoints,ourVelocities,theirVelocities,
                new TestResults(1, (int)iterations, totalMS, MSPerIteration));

        }
        string formatPoint(Vector2 p)
        {
            return p.X + " " + p.Y;
        }
        public string save()
        {
            //need to check if any of the robots without any paths were changed,
            //and reflect those changes in the waypoints:
            for (int i = 0; i < OurPositions.Length; i++)
            {
                if (OurWaypoints[i].Length == 1)
                {
                    OurWaypoints[i][0] = OurPositions[i];
                }
            }
            for (int i = 0; i < TheirPositions.Length; i++)
            {
                if (TheirWaypoints[i].Length==1){
                    TheirWaypoints[i][0]=TheirPositions[i];
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# any line that starts with a \'#\' is a comment, and any empty line is skipped");
            sb.AppendLine("# otherwise, the format of these files must be exactly like this");
            sb.AppendLine();
            sb.AppendLine("# these 3 numbers are information about how the reference navigator did.  they are:");
            sb.AppendLine("# total time spent calculating; total iterations to get to the destination; average time per iteration");
            sb.AppendLine("" + referenceResults.AverageMillisecondsPerRun);
            sb.AppendLine("" + referenceResults.AverageIterationsPerRun);
            sb.AppendLine("" + referenceResults.AverageMillisecondsPerIteration);
            sb.AppendLine();
            sb.AppendLine("# these 3 numbers give some description of this file.  they are:");
            sb.AppendLine("# number of destination; number of our robots; number of their robots");
            sb.AppendLine(Destinations.Length + " " + OurPositions.Length + " " + TheirPositions.Length);
            sb.AppendLine();
            sb.AppendLine("# this is the ball position:");
            sb.AppendLine(formatPoint(BallPos));
            sb.AppendLine();
            sb.AppendLine("# these are the destinations for the robots");
            sb.AppendLine("# the one on the first line is the destination for the first robot, second is for the second, etc");
            foreach (Vector2 p in Destinations)
            {
                sb.AppendLine(formatPoint(p));
            }
            sb.AppendLine();
            sb.AppendLine("# here come our robot positions");
            sb.AppendLine("# each line starts with an integer, the number of points in that robot's path");
            sb.AppendLine("# the next number is the speed of that robot");
            sb.AppendLine("# the rest of the numbers on that line are the coordinates of the points");
            for (int j=0;j<OurWaypoints.Length;j++){
                Vector2[] points=OurWaypoints[j];
                sb.Append(points.Length);
                sb.AppendFormat(" {0:G4} ", Math.Sqrt(OurVelocities[j].magnitudeSq()));
                for (int i = 0; i < points.Length; i++)
                {
                    sb.Append(" " + formatPoint(points[i]));
                }
                sb.AppendLine();
            }
            sb.AppendLine("# same thing with the enemy positions:");
            for (int j = 0; j < TheirWaypoints.Length; j++)
            {
                Vector2[] points = TheirWaypoints[j];
                sb.Append(points.Length);
                sb.AppendFormat(" {0:G4} ", Math.Sqrt(TheirVelocities[j].magnitudeSq()));
                for (int i = 0; i < points.Length; i++)
                {
                    sb.Append("    " + formatPoint(points[i]));
                }
                sb.AppendLine();
            }
            sb.AppendLine();
            sb.AppendLine("# and we're done");
            return sb.ToString();
        }
        int[] ourCurrentWaypoint, theirCurrentWaypoint;
        public Vector2 currentPathWaypoint(bool ours, int robot)
        {
            if (ours)
                return ourWaypoints[robot][ourCurrentWaypoint[robot]];
            else
                return theirWaypoints[robot][theirCurrentWaypoint[robot]];
        }
        public void nextPathWaypoint(bool ours, int robot)
        {
            if (ours)
                ourCurrentWaypoint[robot] = (ourCurrentWaypoint[robot] + 1) % OurWaypoints[robot].Length;
            else
                theirCurrentWaypoint[robot] = (theirCurrentWaypoint[robot] + 1) % TheirWaypoints[robot].Length;
        }
    }
}
