using System;
using System.Collections.Generic;
using System.Text;

namespace Robocup.Core
{
    /// <summary>
    /// The main debugging console, accessible from everywhere. Contains "the" instance
    /// of DebugForm that is to be used by any GUI
    /// </summary>
    public static class DebugConsole
    {
        static DebugForm _form;

        /// <summary>
        /// initialize the debug form in a static constructor
        /// </summary>
        static DebugConsole()
        {
            _form = new DebugForm();
        }
        
        /// <summary>
        /// Get the debug form so it can be displayed
        /// </summary>
        /// <returns></returns>
        public static DebugForm getForm()
        {
            return _form;
        }

        /// <summary>
        /// Write a debug statement to the debugging console with no id or keyword
        /// </summary>
        /// <param name="statement">Statement to be written</param>
        /// <param name="domain">Problem domain in which the debug statement lies</param>
        public static void Write(String statement, ProjectDomains domain)
        {
            Write(statement, domain, -1);
        }

        /// <summary>
        /// Write a debug statement to the debugging console with no id
        /// </summary>
        /// <param name="statement">Statement to be written</param>
        /// <param name="domain">Problem domain in which the debug statement lies</param>
        /// <param name="keyword">A keyword describing the problem and debug statement's nature</param>
        public static void Write(String statement, ProjectDomains domain, String keyword)
        {
            Write(statement, domain, -1, keyword);
        }

        /// <summary>
        /// Write a debug statement to the debugging console with no keyword
        /// </summary>
        /// <param name="statement">Statement to be written</param>
        /// <param name="id">Robot ID</param>
        /// <param name="domain">Problem domain in which the debug statement lies</param>
        public static void Write(String statement, ProjectDomains domain, int id)
        {
            Write(statement, domain, id, "No Keyword");
        }

        /// <summary>
        /// Write a debug statement to the debugging console
        /// </summary>
        /// <param name="statement">Statement to be written</param>
        /// <param name="id">Robot ID</param>
        /// <param name="domain">Problem domain in which the debug statement lies</param>
        /// <param name="keyword">A keyword describing the problem and debug statement's nature</param>
        public static void Write(String statement, ProjectDomains domain, int id, String keyword)
        {
            _form.Write(statement, domain, id, keyword);
        }
    }
}
