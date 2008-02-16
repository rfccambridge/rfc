using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.Core
{
    public static class TagSystem
    {
        private static Dictionary<int, List<string>> tags = new Dictionary<int, List<string>>();
        static public List<string> GetTags(int ID)
        {
            if (!tags.ContainsKey(ID))
                tags.Add(ID, new List<string>());
            return tags[ID];
        }
        static public void AddTag(int ID, string tag)
        {
            if (!tags.ContainsKey(ID))
                tags.Add(ID, new List<string>());
            tags[ID].Add(tag);
        }
    }
}
