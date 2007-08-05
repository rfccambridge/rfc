using System;
using System.Collections.Generic;
using System.Text;
using Vision;
using Robocup.Constants;
using System.Windows.Forms;
using System.Drawing;

namespace VisionStatic {
    static class VisionTest {
        const int NUM_ROBOTS = 5;
        
        static public bool testingOn = false;
        static public Queue<Ball> lostBalls;
        static public Queue<Robot>[] lostOurRobots;
        static public Queue<Robot>[] lostTheirRobots;

        static VisionTest() {
            lostBalls = new Queue<Ball>(Constants.get<int>("VISION_TEST_QUEUE_SIZE"));
            lostOurRobots = new Queue<Robot>[NUM_ROBOTS];
            lostTheirRobots = new Queue<Robot>[NUM_ROBOTS];
            for (int i = 0; i < NUM_ROBOTS; i++) {
                lostOurRobots[i] = new Queue<Robot>(Constants.get<int>("VISION_TEST_QUEUE_SIZE"));
                lostTheirRobots[i] = new Queue<Robot>(Constants.get<int>("VISION_TEST_QUEUE_SIZE"));
            }
        }

        static public void toggle() {
            if (!testingOn) {
                //clear the queues
                lostBalls.Clear();
                for (int i = 0; i < NUM_ROBOTS; i++) {
                    lostOurRobots[i].Clear();
                    lostTheirRobots[i].Clear();
                }
                testingOn = true;
            } else {
                testingOn = false;
            }
        }

        //DOES NOT DRAW - to be fixed.
        static public void displayLostBalls(Graphics objGraphics) {
            //DEBUGING only
            //lostBalls.Clear();
            //lostBalls.Enqueue(new Ball(10.1, 10.1, 100, 100));
            //end debugging
            MessageBox.Show("Number of lost locations: " + lostBalls.Count);
            foreach (Ball ball in lostBalls) {
                
                //could go out of bounds here..
                objGraphics.FillEllipse(Brushes.Red, ball.ImageX - 3, ball.ImageY - 3, 6, 6);
            }
            
        }


    }
}
