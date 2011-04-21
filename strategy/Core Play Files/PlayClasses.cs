using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;
using Robocup.Core;
using Robocup.Geometry;

/* The way that the designer/interpreter is currently set up is so that they can share a lot of the same code logic.
 * Here are the basic classes that each is going to override; they're all abstract and begin with the word "Play"
 * Classes that begin with the word "Designer" are used in the play designer, and most of their added fuctionality
 * is in functions for drawing/handling user interactions, and being able to output its definition.
 * Classes that begin with the word "Interpreter" are used in the interpreter, and most of their extra functions are
 * for handling being evaluated.
 * For example, in this file there's a class PlayRobot, that declares only the things that all robots in this system will have:
 * a definition of some sort, a way to tell which team it's on, and a way to get it's location.
 * Then there's a DesignerRobot, which has added code for drawing itself on the screen, being clicked on and dragged,
 * and naming new robots.  It gets its point from wherever the user happens to place it in the window.
 * There's also an InterpreterRobot, which has the ability to query its definition for its position.  (Like almost all
 * other InterpreterObjects, it has the ability to save its information and determine if it might have changed, so it
 * can eliminate unnecessary calls by just returning its last state.)
 * 
 * One important note: in all classes starting with the word Designer, coordinates are stored in terms of the designer
 * (ie the screen).  In all Interpreter classes, coordinates are stored in terms of the field.
 * 
 */

namespace Robocup.Plays
{

    public class RobotAssignmentType
    {
        private readonly bool okIfAssigned;
        public bool OkIfAssigned
        {
            get { return okIfAssigned; }
        }
        private readonly bool okIfBusy;
        public bool OkIfBusy
        {
            get { return okIfBusy; }
        }
        private readonly bool skipAssigned;
        public bool SkipAssigned
        {
            get { return skipAssigned; }
        }
        private readonly bool skipBusy;
        public bool SkipBusy
        {
            get { return skipBusy; }
        }
        public RobotAssignmentType(bool okassigned, bool okbusy, bool skipassigned, bool skipbusy)
        {
            this.okIfAssigned = okassigned;
            this.okIfBusy = okbusy;
            this.skipAssigned = skipassigned;
            this.skipBusy = skipbusy;
        }
        private int mult(int x, bool b)
        {
            if (b)
                return x;
            return 0;
        }
        public override int GetHashCode()
        {
            return mult(8, this.OkIfAssigned) + mult(4, this.OkIfBusy) + mult(2, this.SkipAssigned) + mult(1, this.SkipBusy);
        }
        public override bool Equals(object obj)
        {
            RobotAssignmentType r = obj as RobotAssignmentType;
            if (r == null)
                return false;
            return (this.OkIfAssigned == r.OkIfAssigned &&
                    this.OkIfBusy == r.OkIfBusy &&
                    this.SkipAssigned == r.SkipAssigned &&
                    this.SkipBusy == r.SkipBusy);
        }
        static public RobotAssignmentType Parse(string s)
        {
            RobotAssignmentType rtn;
            values.TryGetValue(s, out rtn);
            return rtn;
        }
        static private Dictionary<string, RobotAssignmentType> values = new Dictionary<string, RobotAssignmentType>();
        /// <summary>
        /// This represents a robot that will probably get an action, but it's not that critical that
        /// the definition holds.  Guaranteed to produce a robot (if there is at least one available).
        /// </summary>
        static private readonly RobotAssignmentType Loose = new RobotAssignmentType(false, false, true, true);
        /// <summary>
        /// This represents a robot that will not get an action, so it's okay if it gets assigned to the same
        /// field robot as another robot in this play, or a robot that's busy.  Since choosing will be lenient,
        /// you are guaranteed that this robot will fit the definition, and that such a robot exists.
        /// </summary>
        static private readonly RobotAssignmentType Inactive = new RobotAssignmentType(true, true, false, false);
        /// <summary>
        /// The strictest possibility; this represents a robot that will get an action, but it is critical that
        /// it fits its definition.  Will often not return a robot.
        /// </summary>
        static private readonly RobotAssignmentType Strict = new RobotAssignmentType(false, false, false, false);
        /// <summary>
        /// This represents a robot that will get an action, and will skip over busy robots but not assigned ones.
        /// I'm not sure when this would be used.
        /// </summary>
        static private readonly RobotAssignmentType Medium = new RobotAssignmentType(false, false, false, true);

        /// <summary>
        /// Represents a robot that will not get an action, but must be distinct from the other robots in the play.
        /// Example: the receiving robot of a pass.
        /// </summary>
        static private readonly RobotAssignmentType Target = new RobotAssignmentType(false, true, true, false);

        static RobotAssignmentType()
        {
            values.Add("target", Target);
            values.Add("medium", Medium);
            values.Add("strict", Strict);
            values.Add("loose", Loose);
            values.Add("inactive", Inactive);
        }
        public override string ToString()
        {
            foreach (KeyValuePair<string, RobotAssignmentType> pair in values)
            {
                if (pair.Value == this)
                    return pair.Key;
            }
            return base.ToString();
        }

    }
    public interface Robot
    {
        bool Ours { get; }
        Vector2 getPoint();
		double getOrientation();
        int getID();
    }
    /// <summary>
    /// Robot definition used by the play system. Must be default-creatable.
    /// </summary>
    public class PlayRobotDefinition : Robot, GetPointable
    {
        public PlayRobotDefinition()
        {
        }
        public virtual int getID()
        {
            throw new InvalidOperationException("Placeholder only, should never get called");
        }
        public virtual double getOrientation()
        {
            throw new InvalidOperationException("Placeholder only, should never get called");
        }
        public virtual Vector2 getPoint()
        {
            throw new InvalidOperationException("Placeholder only, should never get called");
        }
        public virtual Vector2 getVelocity()
        {
            throw new InvalidOperationException("Placeholder only, should never get called");
        }
        public virtual bool Ours
        {
            get { throw new InvalidOperationException("Placeholder only, should never get called"); }
        }
    }

    abstract public class PlayBall : GetPointable
    {
        public abstract Vector2 getPoint();
        public abstract Vector2 getVelocity();
    }

    public interface GetPointable
    {
        Vector2 getPoint();
        Vector2 getVelocity();
    }
}
