using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;

namespace Robocup.RRT {
    public static class RRTFactory {
        static public IMotionPlanner createPlanner(Type navigatorType) {
            return (IMotionPlanner)Activator.CreateInstance(navigatorType);
        }
        static private Type[] navigatortypes = getPlannerTypes();
        static public Type[] NavigatorTypes {
            get { return (Type[])navigatortypes.Clone(); }
        }

        static private Type[] getPlannerTypes() {
            Type[] allTypes = System.Reflection.Assembly.GetAssembly(typeof(IMotionPlanner)).GetTypes();
            List<Type> rtn = new List<Type>();
            foreach (Type t in allTypes) {
                if (t.IsAbstract || t.IsInterface || t.IsGenericType || !t.IsPublic)
                    continue;
                if ((typeof(IMotionPlanner)).IsAssignableFrom(t))
                    rtn.Add(t);
            }
            rtn.Sort(new Comparison<Type>(delegate(Type t1, Type t2)
            {
                return t1.Name.CompareTo(t2.Name);
            }));
            return rtn.ToArray();
        }
    }
}
