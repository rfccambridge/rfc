using System;
using System.Collections.Generic;
using System.Text;
using Robocup.Core;
using Navigation;

namespace NavigationRacer {
    public static class NavigatorFactory {
        static public INavigator createNavigator(Type navigatorType) {
            return (INavigator)Activator.CreateInstance(navigatorType);
        }
        /// <summary>
        /// Returns a reference navigator, against which we score the others (for reference).
        /// </summary>
        static public INavigator createReferenceNavigator() {
            return new Navigation.Current.CurrentNavigator();
        }
        static private Type[] navigatortypes = getNavigatorTypes();
        static public Type[] NavigatorTypes {
            get { return (Type[])navigatortypes.Clone(); }
        }

        static private Type[] getNavigatorTypes() {
            Type[] allTypes = System.Reflection.Assembly.GetAssembly(typeof(Obstacle)).GetTypes();
            List<Type> rtn = new List<Type>();
            foreach (Type t in allTypes) {
                if (t.IsAbstract || t.IsInterface || t.IsGenericType)
                    continue;
                if ((typeof(INavigator)).IsAssignableFrom(t))
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
