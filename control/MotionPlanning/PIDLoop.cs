using System;
using System.Collections.Generic;
using System.Text;

using Robocup.Core;
using Robocup.CoreRobotics;
using Robocup.Geometry;

using System.Drawing;

using Navigation.Examples;
using System.IO;
using Robocup.Utilities;

namespace Robocup.MotionControl
{
    /// <summary>
    /// Contains a naive PID loop
    /// Unlike Feedback object, is not built specifically to deal with wheel speeds or
    /// RobotInfo
    /// NOTE: In this formulation, want POSITIVE P, I, and D constants
    /// </summary>
    public class PIDLoop
    {
        // Is initialized with the name of a constants category and a
        // string identifying the constants

        double P;
        double I;
        double D;
        double cap;

        private double error = 0;
        private double olderror = 0;
        private double Ierror = 0;
        private double Derror = 0;

        String category;
        String constype;

        /// <summary>
        /// Initialize from a constants category, with the name of the type of constants
        /// (will look in that constants category for constantsType_P, constantsType_I,
        /// and constantsType_D constants)
        /// </summary>
        /// <param name="constantsCategory"></param>
        /// <param name="constantsType"></param>
        public PIDLoop(String constantsCategory, String constantsType)
        {
            category = constantsCategory;
            constype = constantsType;
            ReloadConstants();
        }

        /// <summary>
        /// Reload PID constants from constants file
        /// </summary>
        public void ReloadConstants()
        {
            // Reload this constants file
            Constants.Load(category);

            // Set P, I, and D, possibly a cap on the error
            P = Constants.get<double>(category, constype + "_P");
            I = Constants.get<double>(category, constype + "_I");
            D = Constants.get<double>(category, constype + "_D");

            cap = 0;
            if (Constants.isDefined(constype + "_CAP")) {
                cap = Constants.get<double>(category, constype + "_CAP");
            }
        }

        /// <summary>
        /// Compute new input based on current and desired states
        /// </summary>
        /// <param name="current">Current world state</param>
        /// <param name="desired">Desired world state</param>
        /// <returns></returns>
        public double compute(double current, double desired)
        {
            // find error
            error = desired - current;

            // accumulate integral error term
            Ierror = Ierror + error;

            // find change in error to get derivative term
            Derror = error - olderror;

            // save old error
            olderror = error;

            // return PID term- use negative terms to ensure P is naturally positive
            double ret = P * error + I * Ierror + D * Derror;

            // if there is a cap, use it as the maximum for both positive and negative
            if (cap != 0) // sentinel value
            {
                ret = Math.Max(Math.Min(ret, cap), -cap);
            }

            Console.WriteLine("PIDLoop Current: " + current.ToString() + " Desired: " + 
                desired.ToString() + " Output: " + ret.ToString());

            return ret;
        }
    }

    /// <summary>
    /// Allows working with one PID per robot
    /// </summary>
    public class MultiRobotPIDLoop
    {
        // constants
        private PIDLoop[] loops;
        private int _numRobots;

        /// <summary>
        /// Initialize from category (constants file) and constant identifier
        /// </summary>
        /// <param name="constantsCategory"></param>
        /// <param name="constantsType"></param>
        public MultiRobotPIDLoop(String constantsCategory, String constantsType, int numRobots)
        {
            _numRobots = numRobots;

            loops = new PIDLoop[_numRobots];

            for (int i = 0; i < numRobots; i++)
                loops[i] = new PIDLoop(constantsCategory, constantsType);
        }

        /// <summary>
        /// Compute input based on appropriate PID loop for that robot
        /// </summary>
        /// <param name="current"></param>
        /// <param name="desired"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public double compute(double current, double desired, int id) {
            return loops[id].compute(current, desired);
        }

        /// <summary>
        /// Reload constants from file for each PID loop
        /// </summary>
        public void ReloadConstants()
        {
            for (int i = 0; i < _numRobots; i++)
                loops[i].ReloadConstants();
        }
    }
}
