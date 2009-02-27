using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using VisionStatic;
using System.Windows;
using Robocup.Utilities;
using Vision;
using Robocup.Core;
using System.Collections;

namespace Vision
{

    [Serializable]
    public class GameObjects
    {
        private string source;
        private List<Robot> ourRobots;
        private List<Robot> theirRobots;
        private Ball ball;

        public void ReplaceOurRobot(int id, Robot robot)
        {
            bool found = false;
            for (int i = 0; i < ourRobots.Count; i++)
            {
                if (ourRobots[i].Id == id)
                {
                    ourRobots[i] = robot;
                    found = true;
                    break;
                }
            }
            //if we couldn't find an already existing one, then add it
            if (!found)
            {
                ourRobots.Add(robot);
            }
        }

        public List<Robot> OurRobots
        {
            get
            {
                if (ourRobots == null)
                    Console.WriteLine("ourRobots is null");
                //if (ourRobots[1]==null)
                //    Console.WriteLine("ourRobots[1]==null");
                return ourRobots;
            }
            set { ourRobots = value; }
        }

        public List<Robot> TheirRobots
        {
            get { return theirRobots; }
            set { theirRobots = value; }
        }

        public Ball Ball
        {
            get { return ball; }
            set { ball = value; }
        }

        public int TotalObjects
        {
            get
            {
                int objects = 0;
                if (Ball != null && (Ball.X != 0 && Ball.Y != 0))
                    objects++;
                foreach (Robot robot in OurRobots)
                    if (robot.Id >= 0)
                        objects++;

                foreach (Robot robot in TheirRobots)
                    if (robot.Id >= 0)
                        objects++;

                return objects;
            }
        }

        public string Source
        {
            get { return source; }
            set { source = value; }
        }

        public GameObjects()
            : this(SystemInformation.ComputerName)
        {
        }
        public GameObjects(string src)
        {

            source = src;

            ball = new Ball(0, 0, 0, 0); //???

            ourRobots = new List<Robot>();
            theirRobots = new List<Robot>();
        }



        //Debuging only
        public void print()
        {
            Console.WriteLine("---- ball: (" + ball.X.ToString() + ", " + ball.Y.ToString() + ")");
            Console.WriteLine("---- our robots ----");
            foreach (Robot robot in ourRobots)
            {
                if (robot != null && robot.Id >= 0)
                {
                    Console.Write(robot.Id.ToString() + " ");
                }
                else
                {
                    Console.Write("- ");
                }
            }
            Console.WriteLine();
            Console.WriteLine("---- their robots ----");
            foreach (Robot robot in theirRobots)
            {
                if (robot != null && robot.Id >= 0)
                {
                    Console.Write(robot.Id.ToString() + " ");
                    Console.Write(String.Format("({0:N},{1:N})   ", robot.X, robot.Y));
                }
                else
                {
                    Console.Write("-              ");
                }
            }
            Console.WriteLine();
        }
    }

    [Serializable]
    public class Ball
    {
        private double x, y;
        private int imageX, imageY;

        public double X
        {
            get { return x; }
            set { x = value; }
        }

        public double Y
        {
            get { return y; }
            set { y = value; }
        }

        public int ImageX
        {
            get { return imageX; }
            set { imageX = value; }
        }

        public int ImageY
        {
            get { return imageY; }
            set { imageY = value; }
        }

        public Ball()
        {
        }

        public Ball(double _x, double _y, int _imageX, int _imageY)
        {
            x = _x;
            y = _y;
            imageX = _imageX;
            imageY = _imageY;
        }
    }

    [Serializable]
    public class Robot
    {

        private int team;  //0/1
        private int id;
        private double x, y;
        private double orient;

        private Blob[] dots;
        public int numDots;

        public int Team
        {
            get { return team; }
            set { team = value; }
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public double X
        {
            get { return x; }
            set { x = value; }
        }
        public double Y
        {
            get { return y; }
            set { y = value; }
        }

        public double Orientation
        {
            get { return orient; }
            set { orient = value; }
        }

        public Robot()
        {
            id = -1;
            dots = new Blob[4];
        }
        public Robot(double _x, double _y, int _team, int _id)
        {
            x = _x;
            y = _y;
            team = _team;
            id = _id;

            dots = new Blob[4];

            orient = 0;
            numDots = 0;
        }

        public Blob GetDot(int i)
        {
            return dots[i];
        }

        public int AddDot(Blob b)
        {
            if (numDots >= 4) return 1; //1 is false, right?
            dots[numDots++] = b;
            return 0;
        }
    }
}

namespace VisionStatic
{
    static class RobotFinder
    {

        private class Pattern
        {
            public Blob centerDot;
            public Blob[] dots;
            public double[] ctrDistsSq;
            public Vector[] ctrVectors;

            public Pattern(Blob centerDotValue, IList<Blob> dotsValue)
            {

                // For now we make use only of four dots, but will be smarter later: e.g. try all combinations of dots.
                if (dotsValue.Count != 4)
                {
                    throw new ApplicationException("Pattern can only contain 4 dots.");
                }

                centerDot = centerDotValue;
                dots = new Blob[dotsValue.Count];
                ctrDistsSq = new double[dotsValue.Count];
                ctrVectors = new Vector[dotsValue.Count];

                dotsValue.CopyTo(dots, 0);

                int i = 0;
                foreach (Blob dot in dotsValue)
                {
                    ctrDistsSq[i] = RobotFinder.distanceSq(centerDotValue.CenterWorldX, centerDotValue.CenterWorldY, dot.CenterWorldX, dot.CenterWorldY);
                    //reverse X when dealing with world coordinates
                    ctrVectors[i] = new Vector(-1 * (dot.CenterWorldX - centerDot.CenterWorldX), dot.CenterWorldY - centerDot.CenterWorldY);
                    //uncomment to use image coordinates
                    // reverse Y to get a righthanded coord system
                    //ctrVectors[i] = new Vector(dot.CenterX - centerDot.CenterX, -1 * (dot.CenterY - centerDot.CenterY));
                    i++;
                }

            }
        }

        static int OUR_TEAM = 1;
        static int THEIR_TEAM = 2;

        // This is the only constants needed for the new (after Jan. 2009) robot id method
        public static float DIST_SQ_TO_CENTER_PIX;
        public static float AREA_OUR_CENTER_DOT;
        public static float AREA_THEIR_CENTER_DOT;
        public static float ERROR_OUR_CENTER_DOT;
        public static float ERROR_THEIR_CENTER_DOT;
        public static float AREA_BALL;
        public static float ERROR_BALL;
    	public static float MIN_BALL_DIST_FROM_ROBOT_SQ;

        // These constants are needed when using the old (before Jan. 2009) robot id method
        public static float DIST_SQ_TO_CENTER;
        public static float FRONT_ANGLE;
        public static float ERROR_ANGLE;
        public static float DIST_SHORT_SQ;

        public static float AREA_DOT;
        public static float ERROR_DOT;

        public static float AREA_CYAN_DOT;
        public static float AREA_PINK_DOT;
        public static float AREA_GREEN_DOT;

        public static float ERROR_CYAN_DOT;
        public static float ERROR_PINK_DOT;
        public static float ERROR_GREEN_DOT;

        static int THEIR_ID_OFFSET;
        static float BALL_HEIGHT_TSAI;

        public static bool VERBOSE;

        // Neural Net robot id method not implemented
        //static BrainNet.NeuralFramework.INeuralNetwork _nnIdentifier;

        static RobotFinder()
        {
            LoadParameters();
        }

        static public void LoadParameters()
        {

            VERBOSE = (Constants.get<string>("vision", "VERBOSE") == "false") ? false : true;

            if (Constants.get<string>("configuration", "OUR_TEAM_COLOR").ToUpper() == "BLUE")
            {
                AREA_OUR_CENTER_DOT = Constants.get<float>("vision", "AREA_BLUE_CENTER_DOT");
                AREA_THEIR_CENTER_DOT = Constants.get<float>("vision", "AREA_YELLOW_CENTER_DOT");
                ERROR_OUR_CENTER_DOT = Constants.get<float>("vision", "ERROR_BLUE_CENTER_DOT");
                ERROR_THEIR_CENTER_DOT = Constants.get<float>("vision", "ERROR_YELLOW_CENTER_DOT");
            }
            else
            {
                AREA_THEIR_CENTER_DOT = Constants.get<float>("vision", "AREA_BLUE_CENTER_DOT");
                AREA_OUR_CENTER_DOT = Constants.get<float>("vision", "AREA_YELLOW_CENTER_DOT");
                ERROR_THEIR_CENTER_DOT = Constants.get<float>("vision", "ERROR_BLUE_CENTER_DOT");
                ERROR_OUR_CENTER_DOT = Constants.get<float>("vision", "ERROR_YELLOW_CENTER_DOT");
            }

            THEIR_ID_OFFSET = Constants.get<int>("vision", "THEIR_ID_OFFSET_" + System.Windows.Forms.SystemInformation.ComputerName);

            BALL_HEIGHT_TSAI = Constants.get<float>("vision", "BALL_HEIGHT_TSAI");

            // This is the only constants needed for the new (after Jan. 2009) robot id method
            DIST_SQ_TO_CENTER_PIX = Constants.get<float>("vision", "DIST_SQ_TO_CENTER_PIX");
            AREA_BALL = Constants.get<float>("vision", "AREA_BALL");
            ERROR_BALL = Constants.get<float>("vision", "ERROR_BALL");
        	MIN_BALL_DIST_FROM_ROBOT_SQ = Constants.get<float>("vision", "MIN_BALL_DIST_FROM_ROBOT_SQ");

            // These constants are needed when using the old (before Jan. 2009) robot id method
            DIST_SQ_TO_CENTER = Constants.get<float>("vision", "DIST_SQ_TO_CENTER");

            FRONT_ANGLE = Constants.get<float>("vision", "FRONT_ANGLE");
            ERROR_ANGLE = Constants.get<float>("vision", "ERROR_ANGLE");
            DIST_SHORT_SQ = Constants.get<float>("vision", "DIST_SHORT_SQ");

            AREA_DOT = Constants.get<float>("vision", "AREA_DOT");
            ERROR_DOT = Constants.get<float>("vision", "ERROR_DOT");

            AREA_CYAN_DOT = Constants.get<float>("vision", "AREA_CYAN_DOT");
            AREA_PINK_DOT = Constants.get<float>("vision", "AREA_PINK_DOT");
            AREA_GREEN_DOT = Constants.get<float>("vision", "AREA_GREEN_DOT");

            ERROR_CYAN_DOT = Constants.get<float>("vision", "ERROR_CYAN_DOT");
            ERROR_PINK_DOT = Constants.get<float>("vision", "ERROR_PINK_DOT");
            ERROR_GREEN_DOT = Constants.get<float>("vision", "ERROR_GREEN_DOT");
        }

        static public double distanceSq(double x1, double y1, double x2, double y2)
        {
            return ((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }

        /// <summary>
        /// This is new (after Jan. 2009) robot identification method -- only a couple constants involved. 
        /// TODO: break up into smaller functions, add ability to select "use TsaiCalibrator / use pixels"
        /// </summary>
        /// <param name="blobs"></param>
        /// <param name="totalBlobs"></param>
        /// <param name="tsaiCalibrator"></param>
        /// <returns></returns>
        static public VisionMessage findGameObjects2(Vision.Blob[] blobs, int totalBlobs, TsaiCalibrator tsaiCalibrator)
        {
            VisionMessage visionMessage = new VisionMessage();
            List<Vector2> enemyPositions = new List<Vector2>();

            // Each center dot (blobID) can have multiple candidate patterns
            Dictionary<int, List<Pattern>> patterns = new Dictionary<int, List<Pattern>>();

            List<int> ballBlobs = new List<int>();

            for (int i = 0; i < totalBlobs; i++) {
                if (blobs[i].ColorClass == ColorClasses.OUR_CENTER_DOT) {
                    Blob tempCtrDot = blobs[i];
                    LinkedList<Blob> tempDots = new LinkedList<Blob>();
                    double distSq;

                    for (int j = 0; j < totalBlobs; j++) {
                        byte c = blobs[j].ColorClass;
                        if (c == ColorClasses.COLOR_DOT_CYAN || c == ColorClasses.COLOR_DOT_GREEN ||
                            c == ColorClasses.COLOR_DOT_PINK) {
                            //uncomment to use image coordinates
                            //distSq = RobotFinder.distanceSq(tempCtrDot.CenterX, tempCtrDot.CenterY, blobs[j].CenterX, blobs[j].CenterY);
                            distSq = RobotFinder.distanceSq(tempCtrDot.CenterWorldX, tempCtrDot.CenterWorldY, blobs[j].CenterWorldX, blobs[j].CenterWorldY);
                            if (distSq < RobotFinder.DIST_SQ_TO_CENTER) {
                                //uncomment to use image coordinates
                                //if (distSq < RobotFinder.DIST_SQ_TO_CENTER_PIX){
                                tempDots.AddLast(blobs[j]);
                            }
                        }
                    }


                    // Choose only the largest two dots for each color
                    Dictionary<int, List<Blob>> dotsByColor = new Dictionary<int, List<Blob>>();
                    foreach (Blob dot in tempDots) {
                        if (!dotsByColor.ContainsKey(dot.ColorClass)) {
                            dotsByColor[dot.ColorClass] = new List<Blob>();
                            dotsByColor[dot.ColorClass].Add(dot);
                        }
                        else {
                            dotsByColor[dot.ColorClass].Add(dot);
                        }
                    }
                    foreach (int color in dotsByColor.Keys) {
                        List<Blob> dots = dotsByColor[color];
                        if (dots.Count > 2) {
                            dots.Sort(new BlobAreaScaledComparer());
                            dots.Reverse();
                            dots.RemoveRange(2, dots.Count - 2);
                        }
                    }




                    // TODO: change everything to List<> instead of LinkedList<>
                    IList<Blob> tempDotsIList = new List<Blob>(tempDots);
                    Combinations<Blob> dotCombinations = new Combinations<Blob>(tempDotsIList, 4, GenerateOption.WithoutRepetition);

                    if (tempDots.Count >= 4) {
                        foreach (IList<Blob> fourDots in dotCombinations) {
                            // Only patters with two dots of one color and two dots of another are possible
                            Dictionary<int, int> colorOccurance = new Dictionary<int, int>();
                            foreach (Blob dot in fourDots)
                                if (colorOccurance.ContainsKey(dot.ColorClass))
                                    colorOccurance[dot.ColorClass]++;
                                else
                                    colorOccurance[dot.ColorClass] = 1;

                            bool eachTwice = true;
                            foreach (int value in colorOccurance.Values) {
                                if (value != 2) {
                                    eachTwice = false;
                                    break;
                                }
                            }

                            if (eachTwice) {
                                Pattern pattern = new Pattern(tempCtrDot, fourDots);
                                if (!patterns.ContainsKey(tempCtrDot.BlobID))
                                    patterns[tempCtrDot.BlobID] = new List<Pattern>();
                                patterns[tempCtrDot.BlobID].Add(pattern);
                            }
                        }
                    }
                }
                else if (blobs[i].ColorClass == ColorClasses.COLOR_BALL) {
                    ballBlobs.Add(i);
                }
                else if (blobs[i].ColorClass == ColorClasses.THEIR_CENTER_DOT) {
                    if (Math.Abs(blobs[i].AreaScaled - AREA_THEIR_CENTER_DOT) < ERROR_THEIR_CENTER_DOT) {
                        enemyPositions.Add(new Vector2(blobs[i].CenterWorldX, blobs[i].CenterWorldY));                        
                    }
                }

            }

            foreach (List<Pattern> patternCandidates in patterns.Values)
            {
                double minCandidateScore = double.MaxValue;
                Pattern bestPattern = null;
                int bestFL, bestFR, bestRL, bestRR;
                bestFL = bestFR = bestRL = bestRR = -1;
                int bestID = -1;

                foreach (Pattern pattern in patternCandidates)
                {
                    if (VERBOSE)
                    {
                        Console.WriteLine("Center dot: " + ((pattern.centerDot == null) ? "! not found !" : pattern.centerDot.BlobID.ToString()));
                        Console.Write("Dots found (" + pattern.dots.Length + "): ");

                        foreach (Blob dot in pattern.dots)
                        {
                            Console.Write(dot.BlobID + " " + ColorClasses.GetName(dot.ColorClass) + "; ");
                        }
                        Console.WriteLine();
                    }

                    if (pattern.dots.Length < 4)
                    {
                        Console.WriteLine("Only " + pattern.dots.Length.ToString() + " dots found. Can't handle less than 4 dots yet.");

                        continue;
                    }

                    // to do: maybe insert some out-lier detector here to filter out the false dots
                    // for now just take the closes 4 if there are more dots        
                    // I only know how to sort using Array.Sort()                                             
                    //Array.Sort(pattern.ctrDistsSq, pattern.dots);           

                    if (VERBOSE)
                    {
                        Console.Write("Four dots chosen: ");


                        for (int i = 0; i < 4; i++)
                        {
                            Console.Write(pattern.dots[i].BlobID + " " + ColorClasses.GetName(pattern.dots[i].ColorClass) + "; ");
                        }
                        Console.WriteLine();
                    }
                    // Find the three vectors for each dot, and based on their length determine whether the dot a front one or a rear one
                    System.Windows.Vector[,] vectors = new System.Windows.Vector[4, 4];
                    double[,] lengths = new double[4, 4];
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            //reverse X when dealing with world coordinates
                            vectors[i, j] = new Vector(-1 * (pattern.dots[j].CenterWorldX - pattern.dots[i].CenterWorldX), pattern.dots[j].CenterWorldY - pattern.dots[i].CenterWorldY);
                            //uncomment to use image coordinates
                            // reverse Y to get a righthanded coord system
                            //vectors[i, j] = new System.Windows.Vector(pattern.dots[j].CenterX - pattern.dots[i].CenterX, -1 * (pattern.dots[j].CenterY - pattern.dots[i].CenterY));
                            lengths[i, j] = vectors[i, j].LengthSquared;
                        }
                    }

                    int id = -1;
                    int frontLeft, frontRight, rearLeft, rearRight;
                    frontLeft = frontRight = rearLeft = rearRight = -1;
                    double minScore = double.MaxValue;
                    double maxTerm1 = 0;
                    double maxTerm2 = 0;
                    double maxTerm3 = 0;

                    // for debugging
                    double[] scores = new double[24];
                    Pair<Pair<int, int>, Pair<int, int>>[] guesses = new Pair<Pair<int, int>, Pair<int, int>>[24];
                    int k = 0;

                    const double REAR_TO_FRONT_RATIO = 1.5;
                    #region guesses
                    // To combine terms in the score formula, we need to normalize them. So we get the max values here.
                    for (int fl = 0; fl < 4; fl++)
                    {
                        for (int fr = 0; fr < 4; fr++)
                        {
                            for (int rl = 0; rl < 4; rl++)
                            {
                                for (int rr = 0; rr < 4; rr++)
                                {
                                    // Iterate over permutations only
                                    if (fl == fr || fl == rl || fl == rr || fr == rl || fr == rr || rl == rr) continue;

                                    double term1 = Math.Abs(lengths[fl, rl] - lengths[fr, rr]);
                                    double term2 = Math.Abs(lengths[fl, rr] - lengths[fr, rl]);
                                    double term3 = Math.Abs(lengths[fl, fr] - REAR_TO_FRONT_RATIO * lengths[rl, rr]);

                                    if (term1 > maxTerm1) { maxTerm1 = term1; }
                                    if (term2 > maxTerm2) { maxTerm2 = term2; }
                                    if (term3 > maxTerm3) { maxTerm3 = term3; }
                                }
                            }
                        }
                    }

                    // Make sure we don't divide by zero
                    maxTerm1 = (maxTerm1 == 0) ? 1 : maxTerm1;
                    maxTerm2 = (maxTerm2 == 0) ? 1 : maxTerm2;
                    maxTerm3 = (maxTerm3 == 0) ? 1 : maxTerm3;

                    for (int fl = 0; fl < 4; fl++)
                    {
                        for (int fr = 0; fr < 4; fr++)
                        {
                            for (int rl = 0; rl < 4; rl++)
                            {
                                for (int rr = 0; rr < 4; rr++)
                                {
                                    // Iterate over permutations only
                                    if (fl == fr || fl == rl || fl == rr || fr == rl || fr == rr || rl == rr) continue;

                                    int tempID = ColorClasses.DOT_PATTERNS[pattern.dots[fl].ColorClass, pattern.dots[fr].ColorClass,
                                                     pattern.dots[rl].ColorClass, pattern.dots[rr].ColorClass];

                                    double term1 = Math.Abs(lengths[fl, rl] - lengths[fr, rr]);
                                    double term2 = Math.Abs(lengths[fl, rr] - lengths[fr, rl]);
                                    double term3 = Math.Abs(lengths[fl, fr] - REAR_TO_FRONT_RATIO * lengths[rl, rr]);
                                    double term4 = (tempID < 0) ? 1 : 0;
                                    double term5 = (lengths[fl, fr] < lengths[rl, rr]) ? 1 : 0;
                                    double term6 = (Vector.CrossProduct(pattern.ctrVectors[fr], pattern.ctrVectors[fl]) < 0) ? 1 : 0;
                                    double term7 = (Vector.CrossProduct(pattern.ctrVectors[rl], pattern.ctrVectors[rr]) < 0) ? 1 : 0;
                                    double term8 = (Vector.CrossProduct(pattern.ctrVectors[fl], pattern.ctrVectors[rr]) < 0) ? 1 : 0;
                                    double term9 = (Vector.CrossProduct(pattern.ctrVectors[rl], pattern.ctrVectors[fr]) < 0) ? 1 : 0;

                                    double score = (term1 / maxTerm1 + term2 / maxTerm2 + term3 / maxTerm3 + term4 + term5 + term6 + term7 + term8 + term9) / 9;


                                    scores[k] = score;
                                    guesses[k] = new Pair<Pair<int, int>, Pair<int, int>>(new Pair<int, int>(fl, fr), new Pair<int, int>(rl, rr));
                                    k++;


                                    if (score < minScore)
                                    {
                                        minScore = score;
                                        frontLeft = fl;
                                        frontRight = fr;
                                        rearLeft = rl;
                                        rearRight = rr;
                                        id = tempID;
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                    #region dump
                    if (VERBOSE || id < 0)
                    {
                        if (id < 0)
                        {
                            Console.WriteLine("Robot identification failed. Dumping state...");
                        }

                        Array.Sort<double, Pair<Pair<int, int>, Pair<int, int>>>(scores, guesses);
                        Array.Reverse(scores);
                        Array.Reverse(guesses);

                        for (int i = 0; i < scores.Length; i++)
                        {
                            int fl = guesses[i].First.First;
                            int fr = guesses[i].First.Second;
                            int rl = guesses[i].Second.First;
                            int rr = guesses[i].Second.Second;

                            int tempID = ColorClasses.DOT_PATTERNS[pattern.dots[fl].ColorClass, pattern.dots[fr].ColorClass,
                                                        pattern.dots[rl].ColorClass, pattern.dots[rr].ColorClass];

                            double term1 = Math.Abs(lengths[fl, rl] - lengths[fr, rr]);
                            double term2 = Math.Abs(lengths[fl, rr] - lengths[fr, rl]);
                            double term3 = Math.Abs(lengths[fl, fr] - REAR_TO_FRONT_RATIO * lengths[rl, rr]);
                            double term4 = (tempID < 0) ? 1 : 0;
                            double term5 = (lengths[fl, fr] < lengths[rl, rr]) ? 1 : 0;
                            double term6 = (Vector.CrossProduct(pattern.ctrVectors[fr], pattern.ctrVectors[fl]) < 0) ? 1 : 0;
                            double term7 = (Vector.CrossProduct(pattern.ctrVectors[rl], pattern.ctrVectors[rr]) < 0) ? 1 : 0;
                            double term8 = (Vector.CrossProduct(pattern.ctrVectors[fl], pattern.ctrVectors[rr]) < 0) ? 1 : 0;
                            double term9 = (Vector.CrossProduct(pattern.ctrVectors[rl], pattern.ctrVectors[fr]) < 0) ? 1 : 0;

                            Console.WriteLine("[FL FR RL RR] = [" + ColorClasses.GetName(pattern.dots[fl].ColorClass)[4] + "(" + pattern.dots[fl].BlobID + ") " +
                                                                    ColorClasses.GetName(pattern.dots[fr].ColorClass)[4] + "(" + pattern.dots[fr].BlobID + ") " +
                                                                    ColorClasses.GetName(pattern.dots[rl].ColorClass)[4] + "(" + pattern.dots[rl].BlobID + ") " +
                                                                    ColorClasses.GetName(pattern.dots[rr].ColorClass)[4] + "(" + pattern.dots[rr].BlobID + ")]: score = " + scores[i].ToString());
                            Console.WriteLine("dist(fl, rl) - dist(fr, rr) = " + (term1 / maxTerm1).ToString());
                            Console.WriteLine("dist(fl, rr) - dist(fr, rl) = " + (term2 / maxTerm2).ToString());
                            Console.WriteLine("dist(fl, fr) - " + REAR_TO_FRONT_RATIO.ToString() + " * dist(rl, rr) = " +
                                (term3 / maxTerm3).ToString());
                            Console.WriteLine("(ID == -1) = " + term4.ToString());
                            Console.WriteLine("(dist(fl, fr) < dist(rl, rr)) = " + term5.ToString());
                            Console.WriteLine("(fr x fl < 0) = " + term6.ToString());
                            Console.WriteLine("(rl x rr < 0) = " + term7.ToString());
                            Console.WriteLine("(fl x rr < 0) = " + term8.ToString());
                            Console.WriteLine("(rl x fr < 0) = " + term9.ToString());
                            Console.WriteLine();

                        }

                        if (id < 0)
                        {
                            continue;
                        }
                    }
                    #endregion

                    if (VERBOSE)
                    {
                        Console.WriteLine("Positions identified: ");
                        Console.WriteLine(pattern.dots[frontLeft].BlobID + "(" + ColorClasses.GetName(pattern.dots[frontLeft].ColorClass) + ")        " +
                                          pattern.dots[frontRight].BlobID + "(" + ColorClasses.GetName(pattern.dots[frontRight].ColorClass) + ")");
                        Console.WriteLine("  " + pattern.dots[rearLeft].BlobID + "(" + ColorClasses.GetName(pattern.dots[rearLeft].ColorClass) + ")    " +
                                          pattern.dots[rearRight].BlobID + "(" + ColorClasses.GetName(pattern.dots[rearRight].ColorClass) + ")");
                        Console.WriteLine();
                    }

                    if (minScore < minCandidateScore)
                    {
                        minCandidateScore = minScore;
                        bestPattern = pattern;
                        bestFL = frontLeft;
                        bestFR = frontRight;
                        bestRL = rearLeft;
                        bestRR = rearRight;
                        bestID = id;
                    }
                }

                if (bestPattern == null)
                {
                    Console.WriteLine("No patterns found.");
                    return visionMessage;
                }

                Robot robot = new Robot();

                if (bestFL == -1 || bestFR == -1 || bestRL == -1 || bestRR == -1)
                    return new VisionMessage();
                // ORIENTATION

                Vector orientV;
                orientV = Vector.Add(bestPattern.ctrVectors[bestFL], bestPattern.ctrVectors[bestFR]);
                orientV.Normalize();
                /*  refer to field sketch in TsaiCalibrator
                 *  the coord system of the field is NOT standard, the x-axis increases from right to left:
                 *                y ^|
                 *                   |
                 *       x           |
                 *      <-----------------------
                 *                   |
                 *                   |
                 *                   |
                 * hence, the minus sign here. 
                 */
                robot.Orientation = (float)Math.Atan2(orientV.Y, orientV.X);


                //find exact location
                double[] front = new double[2] {
                    (bestPattern.dots[bestFL].CenterWorldX + bestPattern.dots[bestFR].CenterWorldX) / 2, 
                    (bestPattern.dots[bestFL].CenterWorldY + bestPattern.dots[bestFR].CenterWorldY) / 2
                };

                double[] rear = new double[2] { 
                    (bestPattern.dots[bestRL].CenterWorldX + bestPattern.dots[bestRR].CenterWorldX) / 2, 
                    (bestPattern.dots[bestRL].CenterWorldY + bestPattern.dots[bestRR].CenterWorldY) / 2 
                };


                robot.X = (front[0] + rear[0]) / 2;
                robot.Y = (front[1] + rear[1]) / 2;

                // Not sure what this code was supposed to do...
                //robot.X = ((front[0] * .7933 + rear[0] * .3827) / 1.176) * 0.5 + robot.X * 0.5;
                //robot.Y = ((front[1] * .7933 + rear[1] * .3827) / 1.176) * 0.5 + robot.Y * 0.5;

                robot.Id = bestID;

                if (VERBOSE)
                {
                    Console.WriteLine("WX=" + robot.X.ToString() + "  WY=" + robot.Y.ToString() + "  Orient=" + robot.Orientation.ToString());
                }

                if (robot.Id >= 0)
                {
                    VisionMessage.RobotData vmRobot = new VisionMessage.RobotData(robot.Id, true,
                                                                                  VisionToGeneralCoords(robot.X, robot.Y),
                                                                                  VisionToGeneralOrientation(robot.Orientation));
                    visionMessage.OurRobots.Add(vmRobot);
                }

				//Check if possible ball isn't too near to center of robot 
				//obviously a wrong blob -> should be discarded
				for(int i=0; i<ballBlobs.Count; i++)
				{
					int ind = ballBlobs[i];
					if (ind == -1) continue;
					Vector2 tmpBallPosition = new Vector2(blobs[ind].CenterWorldX, blobs[ind].CenterWorldY);
					if (tmpBallPosition.distanceSq(new Vector2(robot.X, robot.Y)) < MIN_BALL_DIST_FROM_ROBOT_SQ)
						//discard this ball blob
						ballBlobs[i] = -1;
				}

            }

			Ball goBall = null;
			double currBallAreaError;
			double bestBallAreaError = 1000; //initally a ridiculously big number
			
			foreach(int i in ballBlobs)
            {
				if (i == -1) continue;
				currBallAreaError = Math.Abs(blobs[i].AreaScaled - AREA_BALL);
				if (currBallAreaError < ERROR_BALL && currBallAreaError < bestBallAreaError)
				{
					//tsaiCalibrator.ImageCoordToWorldCoord(blobs[i].CenterX, blobs[i].CenterY, BALL_HEIGHT_TSAI, out wx, out wy);
					//tsaiCalibrator.ImageCoordToWorldCoord(blobs[i].CenterX, blobs[i].CenterY, (wx - 2100) / 10, out wx, out wy);
					//gameObjects.Ball = new Vision.Ball(wx, wy, blobs[i].CenterX, blobs[i].CenterY);
					goBall = new Ball(blobs[i].CenterWorldX, blobs[i].CenterWorldY, blobs[i].CenterX, blobs[i].CenterY);
					bestBallAreaError = currBallAreaError;
				}
            }
			
			Vector2 ballPos;
            if (goBall == null)
               ballPos = null;
            else
               ballPos = VisionToGeneralCoords(goBall.X, goBall.Y);

            visionMessage.BallPosition = ballPos;

			// assign distinct ids to enemies
            int enemyID = 0;
            foreach (Vector2 enemyPosition in enemyPositions) {
                visionMessage.TheirRobots.Add(new Robocup.Core.VisionMessage.RobotData(enemyID++, false,
                    VisionToGeneralCoords(enemyPosition.X, enemyPosition.Y), 0));
            }

            return visionMessage;

        }

        /// <summary>
        /// This is old (before Jan. 2009) robot identification method -- many constants involved.
        /// </summary>
        /// <param name="blobs"></param>
        /// <param name="totalBlobs"></param>
        /// <param name="tsaiCalibrator"></param>
        /// <returns></returns>
        static public VisionMessage findGameObjects(Vision.Blob[] blobs, int totalBlobs, TsaiCalibrator tsaiCalibrator)
        {

            List<Vision.Robot> ourRobots = new List<Vision.Robot>();
            List<Vision.Robot> theirRobots = new List<Vision.Robot>();

            Vision.Blob[] ourDots = new Vision.Blob[100];
            int ourDotsIndex = 0;

            Vision.GameObjects gameObjects = new Vision.GameObjects();
            Vision.Ball goBall = null;

            double currBallAreaError;
            double bestBallAreaError;


            double wx, wy;
            int i, j;

            bestBallAreaError = 1000; //ridiculously big number
            i = 0;
            for (i = 0; i < totalBlobs; i++)
            {
                switch (blobs[i].ColorClass)
                {
                    case ColorClasses.COLOR_BALL:
                        currBallAreaError = Math.Abs(blobs[i].AreaScaled - AREA_BALL);
                        if (currBallAreaError < ERROR_BALL && currBallAreaError < bestBallAreaError)
                        {
                            tsaiCalibrator.ImageCoordToWorldCoord(blobs[i].CenterX, blobs[i].CenterY, BALL_HEIGHT_TSAI, out wx, out wy);
                            tsaiCalibrator.ImageCoordToWorldCoord(blobs[i].CenterX, blobs[i].CenterY, (wx - 2100) / 10, out wx, out wy);
                            //gameObjects.Ball = new Vision.Ball(wx, wy, blobs[i].CenterX, blobs[i].CenterY);
                            goBall = new Vision.Ball(wx, wy, blobs[i].CenterX, blobs[i].CenterY);
                            bestBallAreaError = currBallAreaError;
                        }
                        break;
                    case ColorClasses.COLOR_DOT_PINK:
                        if (Math.Abs(blobs[i].AreaScaled - AREA_PINK_DOT) < ERROR_PINK_DOT && ourDotsIndex < ourDots.Length)
                        {
                            ourDots[ourDotsIndex++] = blobs[i];
                        }
                        break;
                    case ColorClasses.COLOR_DOT_GREEN:
                        if (Math.Abs(blobs[i].AreaScaled - AREA_GREEN_DOT) < ERROR_GREEN_DOT && ourDotsIndex < ourDots.Length)
                        {
                            ourDots[ourDotsIndex++] = blobs[i];
                        }
                        break;
                    case ColorClasses.COLOR_DOT_CYAN:
                        if (Math.Abs(blobs[i].AreaScaled - AREA_CYAN_DOT) < ERROR_CYAN_DOT && ourDotsIndex < ourDots.Length)
                        {
                            ourDots[ourDotsIndex++] = blobs[i];
                        }
                        break;
                    default:
                        if (blobs[i].ColorClass == ColorClasses.OUR_CENTER_DOT)
                        {
                            if (Math.Abs(blobs[i].AreaScaled - AREA_OUR_CENTER_DOT) < ERROR_OUR_CENTER_DOT)
                            {
                                Vision.Robot robot = new Vision.Robot(blobs[i].CenterWorldX, blobs[i].CenterWorldY, OUR_TEAM, -1);
                                ourRobots.Add(robot);
                            }
                            break;
                        }
                        else if (blobs[i].ColorClass == ColorClasses.THEIR_CENTER_DOT)
                        {
                            if (Math.Abs(blobs[i].AreaScaled - AREA_THEIR_CENTER_DOT) < ERROR_THEIR_CENTER_DOT)
                            {
                                // the offset (THEIR_ID_OFFSET) helps the receiver distiguish between objects received from the two different cameras
                                Vision.Robot robot = new Vision.Robot(blobs[i].CenterWorldX, blobs[i].CenterWorldY, THEIR_TEAM, THEIR_ID_OFFSET + theirRobots.Count);
                                theirRobots.Add(robot);
                                gameObjects.TheirRobots.Add(robot);
                            }
                            break;
                        }
                        break;

                }
            }


            for (i = 0; i < ourRobots.Count; i++)
            {
                for (j = 0; j < ourDotsIndex; j++)
                {
                    if (ourDots[j] != null &&
                        distanceSq(ourDots[j].CenterWorldX, ourDots[j].CenterWorldY, ourRobots[i].X, ourRobots[i].Y) < DIST_SQ_TO_CENTER)
                    {
                        ourRobots[i].AddDot(ourDots[j]);
                        ourDots[j] = null;
                    }
                }
            }



            for (i = 0; i < ourRobots.Count; i++)
            {
                if (ourRobots[i].numDots == 4)
                {

                    RobotFinder.IdentifyAndOrient(ourRobots[i]);

                    if (ourRobots[i].Id < 0)
                    {
                        Console.WriteLine("Robot ID failed: 4 dots found:");
                        for (j = 0; j < 4; j++)
                            Console.WriteLine("Blob #" + ourRobots[i].GetDot(j).BlobID.ToString() +
                                              " ColorClass: " + ColorClasses.GetName(ourRobots[i].GetDot(j).ColorClass) +
                                              " Area:" + ourRobots[i].GetDot(j).AreaScaled.ToString());

                        continue;
                    }

                    gameObjects.ReplaceOurRobot(ourRobots[i].Id, ourRobots[i]);
                    //gameObjects.OurRobots[ourRobots[i].Id] = ourRobots[i];

                }
                else if (ourRobots[i].numDots >= 2)
                {
                    Console.WriteLine("Robot ID failed: " + ourRobots[i].numDots.ToString() + " dots found:");
                    for (j = 0; j < ourRobots[i].numDots; j++)
                        Console.WriteLine("Blob #" + ourRobots[i].GetDot(j).BlobID.ToString() +
                                          " ColorClass: " + ColorClasses.GetName(ourRobots[i].GetDot(j).ColorClass) +
                                          " Area:" + ourRobots[i].GetDot(j).AreaScaled.ToString());
                }

            }

            // Convert data form GameObjects (go-) to Vision Message (vm-)

            Vector2 ballPos;

            if (goBall == null)
                ballPos = null;
            else
                ballPos = VisionToGeneralCoords(goBall.X, goBall.Y);

            VisionMessage visionMessage = new VisionMessage(ballPos);

            foreach (Vision.Robot goRobot in ourRobots)
            {
                if (goRobot.Id >= 0)
                {
                    VisionMessage.RobotData vmRobot = new VisionMessage.RobotData(goRobot.Id, true,
                                                                                  VisionToGeneralCoords(goRobot.X, goRobot.Y),
                                                                                  (double)goRobot.Orientation - Math.PI / 2);
                    visionMessage.OurRobots.Add(vmRobot);
                }
            }


            foreach (Vision.Robot goRobot in theirRobots)
            {
                VisionMessage.RobotData vmRobot = new VisionMessage.RobotData(goRobot.Id, false,
                                                                              VisionToGeneralCoords(goRobot.X, goRobot.Y),
                                                                              (double)goRobot.Orientation);
                visionMessage.TheirRobots.Add(vmRobot);
            }

            //return gameObjects;
            return visionMessage;
        }

        /// <summary>
        /// This is the old (before Jan. 2009) robot identification method used by the old findGameObjects().
        /// </summary>
        /// <param name="robot"></param>
        public static void IdentifyAndOrient(Vision.Robot robot)
        {

            //find two front Dots
            //determine right/left for front
            //determine right/left for back
            //match against definition

            //find four vectors -> center to dot
            //and normalize
            Vector[] vectors = new Vector[4];
            int i, j, k;

            for (i = 0; i < 4; i++)
            {
                vectors[i] = new Vector(robot.GetDot(i).CenterWorldX - robot.X, robot.GetDot(i).CenterWorldY - robot.Y);
                vectors[i].Normalize();
            }

            //find the front Dots
            int fDotLeft = -1, fDotRight = -1;
            int rDotLeft = -1, rDotRight = -1;

            double[,] errors = new double[4, 4]; // for debug output
            double[,] dists = new double[4, 4]; // for debug output
            double error;
            double dist;
            double minDistSoFar = 100000; // ridiculously large number
            // double minError = 1000; //ridiculously large number
            for (i = 0; i < 4; i++)
            {
                for (j = i + 1; j < 4; j++)
                {
                    error = Math.Abs(Math.Abs(Vector.AngleBetween(vectors[i], vectors[j])) - FRONT_ANGLE);
                    errors[i, j] = error;
                    if (error <= ERROR_ANGLE)
                    {

                        //find rear and
                        //make sure that we didn't get the other wide angle
                        rDotLeft = -1;
                        rDotRight = -1;
                        for (k = 0; k < 4; k++)
                        {
                            if (k != i && k != j)
                            {
                                if (rDotLeft == -1)
                                {
                                    rDotLeft = k;
                                }
                                else
                                {
                                    rDotRight = k;
                                }
                            }
                        }
                        dist = distanceSq(robot.GetDot(rDotLeft).CenterWorldX, robot.GetDot(rDotLeft).CenterWorldY,
                                          robot.GetDot(rDotRight).CenterWorldX, robot.GetDot(rDotRight).CenterWorldY);
                        if (dist < minDistSoFar)
                            minDistSoFar = dist;

                        dists[rDotLeft, rDotRight] = dist;

                        if (dist > DIST_SHORT_SQ || dist > minDistSoFar)
                        {
                            //got wrong angle
                        }
                        else
                        {
                            fDotLeft = i;
                            fDotRight = j;
                        }

                    }
                }
            }



            if (fDotLeft == -1 || fDotRight == -1)
            {
                // info dump
                Console.WriteLine("Robot ID failed: Could not determine front Dots. Angle errors are:");
                for (i = 0; i < 4; i++)
                {
                    for (j = i + 1; j < 4; j++)
                        Console.Write(i.ToString() + "<->" + j.ToString() + ": " + String.Format("{0:0.00}", errors[i, j]) + "\t");
                    Console.WriteLine();
                }
                Console.WriteLine("Rear distances:");
                for (i = 0; i < 4; i++)
                {
                    for (j = i + 1; j < 4; j++)
                        Console.Write(i.ToString() + "<->" + j.ToString() + ": " + String.Format("{0:0.00}", dists[i, j]) + "\t");
                    Console.WriteLine();
                }
                return;
            }

            //get rear
            rDotLeft = -1;
            rDotRight = -1;
            for (i = 0; i < 4; i++)
            {
                if (i != fDotLeft && i != fDotRight)
                {
                    if (rDotLeft == -1)
                    {
                        rDotLeft = i;
                    }
                    else
                    {
                        rDotRight = i;
                    }
                }
            }

            //determine left/right
            int t;
            if (Vector.CrossProduct(vectors[fDotLeft], vectors[fDotRight]) < 0)
            {
                t = fDotLeft;
                fDotLeft = fDotRight;
                fDotRight = t;
            }

            if (Vector.CrossProduct(vectors[rDotLeft], vectors[rDotRight]) > 0)
            {
                t = rDotLeft;
                rDotLeft = rDotRight;
                rDotRight = t;
            }

            robot.Id = ColorClasses.DOT_PATTERNS[robot.GetDot(fDotLeft).ColorClass, robot.GetDot(fDotRight).ColorClass,
                                           robot.GetDot(rDotLeft).ColorClass, robot.GetDot(rDotRight).ColorClass];

            if (robot.Id == -1)
            {
                Console.WriteLine("Error: Undefined dot pattern");
                return;
            }


            //orient
            Vector orientV;
            orientV = Vector.Add(vectors[fDotLeft], vectors[fDotRight]);
            orientV.Normalize();
            /*  refer to field sketch in TsaiCalibrator
             *  the coord system of the field is NOT standard, the x-axis increases from right to left:
             *                y ^|
             *                   |
             *       x           |
             *      <-----------------------
             *                   |
             *                   |
             *                   |
             * hence, the minus sign here. 
             */
            robot.Orientation = (float)Math.Atan2(orientV.Y, -1 * orientV.X);


            //find exact location
            double[] front = new double[2] {
                    (robot.GetDot(fDotLeft).CenterWorldX + robot.GetDot(fDotRight).CenterWorldX) / 2, 
                    (robot.GetDot(fDotLeft).CenterWorldY + robot.GetDot(fDotRight).CenterWorldY) / 2
                };

            double[] back = new double[2] { 
                    (robot.GetDot(rDotLeft).CenterWorldX + robot.GetDot(rDotRight).CenterWorldX) / 2, 
                    (robot.GetDot(rDotLeft).CenterWorldY + robot.GetDot(rDotRight).CenterWorldY) / 2 
                };

            robot.X = ((front[0] * .7933 + back[0] * .3827) / 1.176) * 0.5 + robot.X * 0.5;
            robot.Y = ((front[1] * .7933 + back[1] * .3827) / 1.176) * 0.5 + robot.Y * 0.5;

            //location[0] = (front[0] * .7933 + back[0] * .3827 + x * .3) / 1.476;
            // .7933 = sin(7pi/24), .3827 = sin(pi/8), .3 is arbitrary, 1.5 = .7933 + .3827 + .3
            //location[1] = (front[1] * .7933 + back[1] * .3827 + y * .3) / 1.476;

        }


        // Vision coord system  (units: mm)

        /*  Competition values 
        *
        * --------------------------------
        * |(4200, 6100)        (0, 6100) |
        * |                              |
        * |           TOP                |
        * |           CAM                |
        * |                              |
        * |                              |
        * |------------------------------|
        * |                              |
        * |            BOTTOM            |
        * |            CAM               |
        * |                              |
        * |(4200, 0)                (0,0)|
        * --------------------------------

         * General coord system (units: m)
                * -----------------------------------------
                * |(-3.05, 2.1)        |        (3.05, 2.1)|
                * |                    |                   |
                * |                    |                   |
                * |                    |                   |
                * |                    |                   |
                * |                    |                   |
                * |    Bottom Cam    (0, 0)     Top Cam    |
                * |                    |                   |
                * |                  BOTTOM                |
                * |                  CAM 2                 |
                * |                    |                   |
                * |(-3.05, -2.1)       |       (3.05, -2.1)|
                * ------------------------------------------
        
         * 

        *  Practice field MD3rd values 
        *
        * --------------------------------
        * |(3500, 4500)        (0, 4500) |
        * |                              |
        * |           TOP                |
        * |           CAM                |
        * |                              |
        * |                              |
        * |------------------------------|
        * |                              |
        * |            BOTTOM            |
        * |            CAM               |
        * |                              |
        * |(3500, 0)                (0,0)|
        * --------------------------------
         * 
         *  * General coord system (units: m)
                * -----------------------------------------
                * |(-1.75, 2.25)       |       (1.75, 2.25)|
                * |                    |                   |
                * |                    |                   |
                * |                    |                   |
                * |                    |                   |
                * |                    |                   |
                * |    Bottom Cam    (0, 0)     Top Cam    |
                * |                    |                   |
                * |                  BOTTOM                |
                * |                  CAM 2                 |
                * |                    |                   |
                * |(-1.75, -2.25)      |      (1.75, -2.25)|
                * ------------------------------------------
         */


        static double G_HEIGHT = Constants.get<double>("plays", "FIELD_HEIGHT");
        static double G_WIDTH = Constants.get<double>("plays", "FIELD_WIDTH");
        static double M_TO_MM_FACTOR = 1000;
        static double V_HEIGHT = G_WIDTH * M_TO_MM_FACTOR;
        static double V_WIDTH = G_HEIGHT * M_TO_MM_FACTOR;

        /// <summary>
        /// General coord system is rotated by pi/2 counter-clockwise with respect to the vision system.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static Vector2 VisionToGeneralCoords(double x, double y)
        {
            // Move origin from bottom-right (vision) to center (general), and change the dimensions 
            // from half-field to full-field and rotate the coord system by pi/2 clockwise (by flipping
            // width and height) because vision coord syste is rotated by pi/2 counter-clockwise with
            // respect to vision system.
            return new Vector2((y - V_HEIGHT / 2) / V_HEIGHT * G_WIDTH,
                (x - V_WIDTH / 2) / V_WIDTH * G_HEIGHT);
        }
        /// <summary>
        /// General coord system is rotated by pi/2 counter-clockwise with respect to the vision system.
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private static double VisionToGeneralOrientation(double angle)
        {
            return angle - Math.PI / 2;
        }

        #region NeuralNets_NOTIMPLEMENTED
        /*
        public static void CreateNN()
        {
            BackPropNetworkFactory factory = new BackPropNetworkFactory();

            ArrayList layers = new ArrayList();
            layers.Add(2);
            layers.Add(2);
            layers.Add(1);

            _nnIdentifier = factory.CreateNetwork(layers);
        }

        public static void TrainNN(string filename)
        {
            Blob centerDot = null;
            List<Blob> dots = new List<Blob>();



            // Read training data from file
            TextReader tr = new StreamReader(filename);
            string line;

            do
            {

                line = tr.ReadLine();
                if (line == null) break;
                string[] parts = line.Split(' ');
                int robotId = int.Parse(parts[0]);
                int numDots = int.Parse(parts[1]);
                int center = int.Parse(parts[2]);
                if (center == 1)
                {
                    line = tr.ReadLine();
                    if (line == null) throw new ApplicationException("Training file is corrupt.");

                    parts = line.Split(' ');
                    centerDot = new Blob(byte.Parse(parts[0]));
                    centerDot.CenterWorldX = float.Parse(parts[1]);
                    centerDot.CenterWorldY = float.Parse(parts[2]);
                    centerDot.AreaScaled = float.Parse(parts[3]);
                }

                dots.Clear();
                for (int i = 0; i < numDots; i++)
                {
                    line = tr.ReadLine();
                    if (line == null) throw new ApplicationException("Training file is corrupt.");
                    Blob dot;
                    parts = line.Split(' ');
                    dot = new Blob(byte.Parse(parts[0]));
                    dot.CenterWorldX = float.Parse(parts[1]);
                    dot.CenterWorldY = float.Parse(parts[2]);
                    dot.AreaScaled = float.Parse(parts[3]);

                    dots.Add(dot);
                }

                // Load the data into the NN's native structure
                TrainingData data = new TrainingData();

                data.Inputs.Clear();
                data.Outputs.Clear();
                if (center == 1)
                {
                    data.Inputs.Add(centerDot.ColorClass);
                    data.Inputs.Add(centerDot.CenterWorldX);
                    data.Inputs.Add(centerDot.CenterWorldY);
                    data.Inputs.Add(centerDot.AreaScaled);
                }
                else
                {
                    data.Inputs.Add(-1);
                    data.Inputs.Add(-1);
                    data.Inputs.Add(-1);
                    data.Inputs.Add(-1);
                }

                foreach (Blob dot in dots)
                {
                    data.Inputs.Add(dot.ColorClass);
                    data.Inputs.Add(dot.CenterWorldX);
                    data.Inputs.Add(dot.CenterWorldY);
                    data.Inputs.Add(dot.AreaScaled);
                }

                // We need to fill all inputs (do we??)
                for (int i = 0; i < numDots - NUM_DOTS; i++)
                {
                    data.Inputs.Add(-1);
                    data.Inputs.Add(-1);
                    data.Inputs.Add(-1);
                    data.Inputs.Add(-1);
                }

                data.Outputs.Add(robotId);

                // Train neural net!
                _nnIdentifier.TrainNetwork(data);

            } while (true);

            tr.Close();



            /* TrainingData data = new TrainingData();
             int[] truthVals = {0, 1};
             for (int i = 0; i < 100000; i++)
             {
                 foreach (int a in truthVals)
                 {
                     foreach (int b in truthVals)
                     {
                         data.Inputs.Clear();
                         data.Outputs.Clear();

                         data.Inputs.Add(a);
                         data.Inputs.Add(b);
                         data.Outputs.Add(((a == 1 ? true : false) && (b == 1 ? true : false)) ? 1 : 0);

                         _nnIdentifier.TrainNetwork(data);
                     }
                 }
             }*
        }

        public static double RunNN(ArrayList inputs)
        {
            ArrayList output = _nnIdentifier.RunNetwork(inputs);
            return (float)output[0];
        }
        */
        #endregion
    }
}