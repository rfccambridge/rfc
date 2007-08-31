using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace RobocupPlays
{
    public static class PlayDesignerInterface
    {
        static EventWaitHandle ewh = new EventWaitHandle(false, EventResetMode.AutoReset);
        static MainForm mf;
        static string text;
        static private object run_lock = new object();
        static public string CreateAndRunDesigner()
        {
            lock (run_lock)
            {
                text = null;
                new Thread(RunForm).Start();
                ewh.WaitOne();

                if (mf.ReturningPlay)
                    return mf.Play.Save();
                else
                    return null;
            }
        }
        static public string CreateAndRunDesigner(string original)
        {
            lock (run_lock)
            {
                text = original;
                new Thread(RunForm).Start();
                ewh.WaitOne();

                if (mf.ReturningPlay)
                    return mf.Play.Save();
                else
                    return null;
            }
        }
        static void RunForm()
        {
            mf = new MainForm(text);
            mf.FormClosed += delegate(object sender, FormClosedEventArgs e)
            {
                ewh.Set();
            };
            Application.Run(mf);
        }
    }
}
