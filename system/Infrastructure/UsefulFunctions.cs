using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.Infrastructure {
    public static class UsefulFunctions {

        static public float distance(Vector2 p1, Vector2 p2) {
            return (float)Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }
        static public float distancesq(Vector2 p1, Vector2 p2) {
            return (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
        }
        /// <summary>
        /// Returns the z-component of the crossproduct P2P1 x P2P3
        /// </summary>
        static public float crossproduct(Vector2 p1, Vector2 p2, Vector2 p3) {
            return (p1.X - p2.X) * (p3.Y - p2.Y) - (p3.X - p2.X) * (p1.Y - p2.Y);
        }
        /// <summary>
        /// Returns the dotproduct of P2P1 and P2P3
        /// </summary>
        static public float dotproduct(Vector2 p1, Vector2 p2, Vector2 p3) {
            return (p1.X - p2.X) * (p3.X - p2.X) + (p1.Y - p2.Y) * (p3.Y - p2.Y);
        }
        /// <summary>
        /// Takes a string s, assumed to start and end with parenthesis, and splits it up into subexpressions.
        /// Ex: (line (robot robot1) point2) gives {"line","(robot robot1)","point2"}
        /// </summary>
        static public string[] parse(string s) {
            //s = s.Trim('(', ')');
            //TODO is this really what I want?
            //TODO change TODO to NOTE?
            if (s[0] == '(')
                s = s.Substring(1, s.Length - 2);
            StringBuilder token = new StringBuilder();
            List<string> strings = new List<string>();

            int depth = 0;
            for (int i = 0; i < s.Length; i++) {
                char c = s[i];
                if (c == ' ' && depth == 0) {
                    if (token.Length != 0) {
                        strings.Add(token.ToString());
                        token = new StringBuilder();
                    }
                    continue;
                }

                if (c == '(') {
                    depth++;
                } else if (c == ')') {
                    depth--;
                    if (depth == 0) {
                        token.Append(c);
                        if (token.Length != 0) {
                            strings.Add(token.ToString());
                            token = new StringBuilder();
                        }
                        continue;
                    }
                }
                token.Append(c);
            }
            if (token.Length != 0)
                strings.Add(token.ToString());

            return strings.ToArray();
        }

        /// <summary>
        /// Returns how many radians counter-clockwise the ray defined by angle1
        /// needs to be rotated to point in the direction angle2.
        /// Returns a value in the range [-Pi,Pi)
        /// </summary>
        static public double angleDifference(double angle1, double angle2) {
            angle1 = (angle1 + Math.PI * 8) % (Math.PI * 2);
            angle2 = (angle2 + Math.PI * 8) % (Math.PI * 2);
            double anglediff = angle2 - angle1;
            anglediff = (anglediff + Math.PI * 2) % (Math.PI * 2);
            //anglediff is now in the range [0,Pi*2)

            //this shifts the range to [Pi,Pi*3), then takes it mod 2Pi,
            //and then subtracts Pi again.  which has the effect of moving the range
            //[Pi, Pi*2) to [-Pi, 0)
            anglediff = ((anglediff + Math.PI) % (Math.PI * 2)) - Math.PI;
            return anglediff;
        }
    }
}
