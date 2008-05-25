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

namespace Vision {

    [Serializable]
    public class GameObjects {
        private string source;
        private List<Robot> ourRobots;
        private List<Robot> theirRobots;
        private Ball ball;

        public void ReplaceOurRobot(int id, Robot robot) {
            bool found = false;
            for (int i = 0; i < ourRobots.Count; i++) {
                if (ourRobots[i].Id == id) {
                    ourRobots[i] = robot;
                    found = true;
                    break;
                }
            }
            //if we couldn't find an already existing one, then add it
            if (!found) {
                ourRobots.Add(robot);
            }
        }

        public List<Robot> OurRobots
        {
            get { 
                if (ourRobots==null)
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

        public int TotalObjects {
            get {
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

        public GameObjects() : this(SystemInformation.ComputerName)
        {
        }
        public GameObjects(string src) {

            source = src;

            ball = new Ball(0, 0, 0, 0); //???
            
            ourRobots = new List<Robot>();
            theirRobots = new List<Robot>();
        }


  
        //Debuging only
        public void print() {
            Console.WriteLine("---- ball: (" + ball.X.ToString() + ", " + ball.Y.ToString() + ")");
            Console.WriteLine("---- our robots ----");
            foreach (Robot robot in ourRobots) {
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
    public class Ball {
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

        public int ImageX {
            get { return imageX; }
            set { imageX = value; }
        }

        public int ImageY {
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
    public class Robot {

        private int team;  //0/1
        private int id;                       
        private double x, y; 
        private float orient;

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

        public float Orientation
        {
            get { return orient; }
            set { orient = value; }
        }

        public Robot()
        {
            id = -1;
            dots = new Blob[4];
        }
        public Robot(double _x, double _y, int _team, int _id) {
            x = _x;
            y = _y;
            team = _team;
            id = _id;

            dots = new Blob[4];

            orient = 0;
            numDots = 0;
        }

        public Blob GetDot(int i) {
            return dots[i];
        }

        public int AddDot(Blob b) {
            if (numDots >= 4) return 1; //1 is false, right?
            dots[numDots++] = b;
            return 0;
        }
    }
}

namespace VisionStatic {
    static class RobotFinder {

        static int OUR_TEAM = 1;
        static int THEIR_TEAM = 2;

        static float DIST_SQ_TO_CENTER;

        static float FRONT_ANGLE;
        static float ERROR_ANGLE;
        static float DIST_SHORT_SQ;

        static float AREA_BALL;
        static float ERROR_BALL;

        static float AREA_OUR_CENTER_DOT;
        static float AREA_THEIR_CENTER_DOT;

        static float ERROR_OUR_CENTER_DOT;
        static float ERROR_THEIR_CENTER_DOT;

        static float AREA_CYAN_DOT;
        static float AREA_PINK_DOT;
        static float AREA_GREEN_DOT;

        static float ERROR_CYAN_DOT;
        static float ERROR_PINK_DOT;
        static float ERROR_GREEN_DOT;

        static int THEIR_ID_OFFSET;

        static float BALL_HEIGHT_TSAI;

        static RobotFinder() {
            LoadParameters();
        }

        static public void LoadParameters() {

            DIST_SQ_TO_CENTER = Constants.get<float>("vision", "DIST_SQ_TO_CENTER");

            FRONT_ANGLE = Constants.get<float>("vision", "FRONT_ANGLE");
            ERROR_ANGLE = Constants.get<float>("vision", "ERROR_ANGLE");
            DIST_SHORT_SQ = Constants.get<float>("vision", "DIST_SHORT_SQ");

            AREA_BALL = Constants.get<float>("vision", "AREA_BALL");
            ERROR_BALL = Constants.get<float>("vision", "ERROR_BALL");

            AREA_CYAN_DOT = Constants.get<float>("vision", "AREA_CYAN_DOT");
            AREA_PINK_DOT = Constants.get<float>("vision", "AREA_PINK_DOT");
            AREA_GREEN_DOT = Constants.get<float>("vision", "AREA_GREEN_DOT");

            ERROR_CYAN_DOT = Constants.get<float>("vision", "ERROR_CYAN_DOT");
            ERROR_PINK_DOT = Constants.get<float>("vision", "ERROR_PINK_DOT");
            ERROR_GREEN_DOT = Constants.get<float>("vision", "ERROR_GREEN_DOT");


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
        }
        
        static private double distanceSq(double x1, double y1, double x2, double y2) {
            return ((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }

        //static public Vision.GameObjects findGameObjects(Vision.Blob[] blobs, int totalBlobs, TsaiCalibrator tsaiCalibrator) {
        static public VisionMessage findGameObjects(Vision.Blob[] blobs, int totalBlobs, 
                                                                 TsaiCalibrator tsaiCalibrator) {

            

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
            for (i = 0; i < totalBlobs; i++) {
                switch (blobs[i].ColorClass) {
                    case ColorClasses.COLOR_BALL:
                        currBallAreaError = Math.Abs(blobs[i].AreaScaled - AREA_BALL);
                        if (currBallAreaError < ERROR_BALL && currBallAreaError < bestBallAreaError) {
                            tsaiCalibrator.ImageCoordToWorldCoord(blobs[i].CenterX, blobs[i].CenterY, BALL_HEIGHT_TSAI, out wx, out wy);
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
                        else if(blobs[i].ColorClass == ColorClasses.THEIR_CENTER_DOT)
                        {
                            if (Math.Abs(blobs[i].AreaScaled - AREA_THEIR_CENTER_DOT) < ERROR_THEIR_CENTER_DOT) 
                            {
                                // the offset (THEIR_ID_OFFSET) helps the receiver distiguish between objects received from the two different cameras
                                Vision.Robot robot =  new Vision.Robot(blobs[i].CenterWorldX, blobs[i].CenterWorldY, THEIR_TEAM, THEIR_ID_OFFSET + theirRobots.Count);
                                theirRobots.Add(robot);
                                gameObjects.TheirRobots.Add(robot);
                            }
                            break;
                        }
                        break;
                    
                }
            }


            for (i = 0; i < ourRobots.Count; i++) {
                for (j = 0; j < ourDotsIndex; j++) {
                    if (ourDots[j] != null &&
                        distanceSq(ourDots[j].CenterWorldX, ourDots[j].CenterWorldY, ourRobots[i].X, ourRobots[i].Y) < DIST_SQ_TO_CENTER) {
                        ourRobots[i].AddDot(ourDots[j]);
                        ourDots[j] = null;
                    }
                }
            }

            

           for (i = 0; i < ourRobots.Count; i++) {
               if (ourRobots[i].numDots == 4) {

                   RobotFinder.IdentifyAndOrient(ourRobots[i]);

                   if (ourRobots[i].Id < 0) {
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
               else if (ourRobots[i].numDots >= 2) {
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

            foreach (Vision.Robot goRobot in ourRobots) {
                if (goRobot.Id >= 0) {
                    VisionMessage.RobotData vmRobot = new VisionMessage.RobotData(goRobot.Id, true,
                                                                                  VisionToGeneralCoords(goRobot.X, goRobot.Y),
                                                                                  (double)goRobot.Orientation);
                    visionMessage.OurRobots.Add(vmRobot);
                }
            }

            
            foreach (Vision.Robot goRobot in theirRobots) {
                VisionMessage.RobotData vmRobot = new VisionMessage.RobotData(goRobot.Id, false,
                                                                              VisionToGeneralCoords(goRobot.X, goRobot.Y),
                                                                              (double)goRobot.Orientation);
                visionMessage.TheirRobots.Add(vmRobot);
            }

            
            

            //return gameObjects;
            return visionMessage;
        }

        public static void IdentifyAndOrient(Vision.Robot robot) {
            
            //find two front Dots
            //determine right/left for front
            //determine right/left for back
            //match against definition

            //find four vectors -> center to dot
            //and normalize
            Vector[] vectors = new Vector[4];
            int i, j, k;

            for (i = 0; i < 4; i++) {
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
            for (i = 0; i < 4; i++) {
                for (j = i + 1; j < 4; j++) {
                    error = Math.Abs(Math.Abs(Vector.AngleBetween(vectors[i], vectors[j])) - FRONT_ANGLE);
                    errors[i, j] = error;
                    if (error <= ERROR_ANGLE) {

                        //find rear and
                        //make sure that we didn't get the other wide angle
                        rDotLeft = -1;
                        rDotRight = -1;
                        for (k = 0; k < 4; k++) {
                            if (k != i && k != j) {
                                if (rDotLeft == -1) {
                                    rDotLeft = k;
                                }
                                else {
                                    rDotRight = k;
                                }
                            }
                        }
                        dist = distanceSq(robot.GetDot(rDotLeft).CenterWorldX, robot.GetDot(rDotLeft).CenterWorldY,
                                          robot.GetDot(rDotRight).CenterWorldX, robot.GetDot(rDotRight).CenterWorldY);
                        if (dist < minDistSoFar)
                            minDistSoFar = dist;
                        
                        dists[rDotLeft, rDotRight] = dist;
                        
                        if (dist > DIST_SHORT_SQ || dist > minDistSoFar ) {
                            //got wrong angle
                        }
                        else {
                            fDotLeft = i;
                            fDotRight = j;
                        }

                    }
                }
            }

            
            
            if (fDotLeft == -1 || fDotRight == -1) {
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
            for (i = 0; i < 4; i++) {
                if (i != fDotLeft && i != fDotRight) {
                    if (rDotLeft == -1) {
                        rDotLeft = i;
                    }
                    else {
                        rDotRight = i;
                    }
                }
            }

            //determine front left/right
            int t;
            if (Vector.CrossProduct(vectors[fDotLeft], vectors[fDotRight]) < 0) {
                t = fDotLeft;
                fDotLeft = fDotRight;
                fDotRight = t;
            }

            if (Vector.CrossProduct(vectors[rDotLeft], vectors[rDotRight]) > 0) {
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
        /* --------------------------------
         * |(5000, 6100)        (0, 6100) |
         * |                              |
         * |           TOP                |
         * |           CAM 1              |
         * |                              |
         * |                              |
         * |------------------------------|
         * |                              |
         * |            BOTTOM            |
         * |            CAM 2             |
         * |                              |
         * |(5000, 0)                (0,0)|
         * --------------------------------
         */

        // General coord system (units: m)
        /* -----------------------------------------
        * |(-3.05, 2.5)        |        (3.05, 2.5)|
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
        * |(-3.05, -2.5)       |       (3.05, -2.5)|
        * ------------------------------------------
        */
        static double G_HEIGHT = Constants.get<double>("plays", "FIELD_HEIGHT");
        static double G_WIDTH = Constants.get<double>("plays", "FIELD_WIDTH");
        static double MM_TO_M_FACTOR = 1000;
        static double V_HEIGHT = G_WIDTH * MM_TO_M_FACTOR;
        static double V_WIDTH = G_HEIGHT * MM_TO_M_FACTOR;
        private static Vector2 VisionToGeneralCoords(double x, double y) {
            //these are for the general coords

            return new Vector2((y - V_HEIGHT / 2) / V_HEIGHT * G_WIDTH,
                (x - V_WIDTH / 2) / V_WIDTH * G_HEIGHT);

            //return new Vector2((-(x - V_WIDTH / 2)) / MM_TO_M_FACTOR, 
            //                   (y - V_HEIGHT / 2) / MM_TO_M_FACTOR);
        }
    }
}
/*   public void findDirection() {



       //now contained_blobs is center, color1, color1, color2, color2

       //simplest algorithm - not using center dot
       direction[0] = (dots[0].CenterWorldX + dots[1].CenterWorldX) - (dots[2].CenterWorldX + dots[3].CenterWorldX);
       direction[1] = (dots[0].CenterWorldY + dots[1].CenterWorldY) - (dots[2].CenterWorldY + dots[3].CenterWorldY);



       //if(contained_blobs[1,2] is the back one) switch direction[]
       if (square(dots[0].CenterWorldX - dots[1].CenterWorldX) +
           square(dots[0].CenterWorldY - dots[1].CenterWorldY) <
           square(dots[2].CenterWorldX - dots[3].CenterWorldX) +
           square(dots[2].CenterWorldY - dots[3].CenterWorldY)) {
           direction[0] *= -1;
           direction[1] *= -1;
       }

       orient = (float)Math.Atan2(direction[1], direction[0]);
            
   } */

/*  public int orderDots() {

      // if (numDots != 4) return 1;

      for (int j = 1; j < 4; j++)
          if (dots[j].ColorClass == dots[0].ColorClass) swch(1, j);

      if (square(dots[0].CenterWorldX - dots[1].CenterWorldX) +
          square(dots[0].CenterWorldY - dots[1].CenterWorldY) <
          square(dots[2].CenterWorldX - dots[3].CenterWorldX) +
          square(dots[2].CenterWorldY - dots[3].CenterWorldY)) {

          swch(0, 2);
          swch(1, 3);
      }

      return 0;
  }*/


/*      public double[] findLocation() {
          //orderDots();


          double[] front = new double[2] {
                  (dots[].CenterWorldX + dots[1].CenterWorldX) / 2, 
                  (dots[0].CenterWorldY + dots[1].CenterWorldY) / 2
              };

          double[] back = new double[2] { 
                  (dots[2].CenterWorldX + dots[3].CenterWorldX) / 2, 
                  (dots[2].CenterWorldY + dots[3].CenterWorldY) / 2 
              };

          location[0] = ((front[0] * .7933 + back[0] * .3827) / 1.176) * 0.5 + x * 0.5;
          location[1] = ((front[1] * .7933 + back[1] * .3827) / 1.176) * 0.5 + y * 0.5;

          //location[0] = (front[0] * .7933 + back[0] * .3827 + x * .3) / 1.476;
          // .7933 = sin(7pi/24), .3827 = sin(pi/8), .3 is arbitrary, 1.5 = .7933 + .3827 + .3
          //location[1] = (front[1] * .7933 + back[1] * .3827 + y * .3) / 1.476;

          return location;
      }

      private void swch(int a, int b) //switch blobs in contained_blobs
      {
          if (a == b) return;
          Blob temp;
          temp = dots[a];
          dots[a] = dots[b];
          dots[b] = temp;
      }*/