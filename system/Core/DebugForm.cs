using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Robocup.Core
{
    /// <summary>
    /// enumerates domains of the various projects that make up the robocup system- for
    /// debug statements purposes only
    /// </summary>
    public enum ProjectDomains
    {
        /// <summary>Planning a path, obstacle avoidance</summary>
        PathPlanning,
        /// <summary>High and low level PID loops, wheel speed calculation and output</summary>
        MotionControl,
        /// <summary>Kicking-specific issues- state machine for kicking and other AI- that does not
        /// specifically fall into motion planning or control</summary>
        Kicking,
        /// <summary>Vision and prediction of game state</summary>
        Vision,
        /// <summary>Play selection, per-robot role assignment</summary>
        Plays,
        /// <summary>RefBox commands and interpretation</summary>
        Refbox,
        /// <summary>Simulator-specific problems</summary>
        Simulator,
        /// <summary>Debugging that does not fit into any other category</summary>
        Other
    }

    public partial class DebugForm : Form
    {
        // Class constants
        int NUM_ROBOTS = 5; // we don't have access to constants...

        public DebugForm()
        {
            InitializeComponent();
            FillBoxes();
        }

        /// <summary>
        /// Fill the selection boxes with options
        /// </summary>
        private void FillBoxes()
        {
            // Add items in enum to domain selector
            Array domainArr = Enum.GetValues(typeof(ProjectDomains));
            object[] domains = new object[domainArr.Length];
            domainArr.CopyTo(domains, 0);

            DomainSelector.Items.AddRange(domains);
            // Select all
            for (int i = 0; i < DomainSelector.Items.Count; i++)
            {
                DomainSelector.SetItemChecked(i, true);
            }


            // Add a select option for each robot
            for (int i = 0; i < NUM_ROBOTS; i++)
            {
                RobotSelector.Items.Add("Robot " + i, true);
            }
            RobotSelector.Items.Add("N/A", true);

            // Keywords are, so far, just "No Keyword"
            KeywordSelector.Items.Add("No Keyword", true);
        }

        /// <summary>
        /// Is a particular problem domain selected
        /// </summary>
        private bool isDomainSelected(ProjectDomains domain)
        {
            return DomainSelector.CheckedItems.Contains(domain);
        }

        /// <summary>
        /// Is a robot ID selected
        /// </summary>
        /// <param name="id"></param>
        private bool isRobotIDSelected(int id)
        {
            return (RobotSelector.CheckedItems.Contains("Robot " + id) || 
                    (id == -1 && RobotSelector.CheckedItems.Contains("N/A")));
        }

        private bool isKeywordSelected(String keyword)
        {
            return KeywordSelector.CheckedItems.Contains(keyword);
        }

        /// <summary>
        /// Write a debug statement to the debugging console
        /// </summary>
        /// <param name="statement">Statement to be written</param>
        /// <param name="id">Robot ID</param>
        /// <param name="domain">Problem domain in which the debug statement lies</param>
        /// <param name="keyword">A keyword describing the problem and debug statement's nature</param>
        public void Write(String statement, ProjectDomains domain, int id, String keyword)
        {
            // If necessary, add to keyword selector
            if (!KeywordSelector.Items.Contains(keyword))
            {
                // Add, checked
                KeywordSelector.Items.Add(keyword, true);
            }

            // Check conditions
            if (isDomainSelected(domain) && isRobotIDSelected(id) && isKeywordSelected(keyword))
            {
                // Satisfactory- write this line to the console
                DebugTextBox.AppendText(statement + "\n");
                DebugTextBox.ScrollToCaret();
            }
        }

        

        // BUTTONS

        /// <summary>
        /// set all items in a checked list box to the given value
        /// </summary>
        /// <param name="box"></param>
        /// <param name="value"></param>
        private void SetAllInBox(CheckedListBox box, bool value)
        {
            for (int i = 0; i < box.Items.Count; i++)
                box.SetItemChecked(i, value);
        }

        private void DomainSelectAll_Click(object sender, EventArgs e)
        {
            SetAllInBox(DomainSelector, true);
        }

        private void DomainSelectNone_Click(object sender, EventArgs e)
        {
            SetAllInBox(DomainSelector, false);
        }

        private void RobotSelectAll_Click(object sender, EventArgs e)
        {
            SetAllInBox(RobotSelector, true);
        }

        private void RobotSelectNone_Click(object sender, EventArgs e)
        {
            SetAllInBox(RobotSelector, false);
        }

        private void KeywordSelectAll_Click(object sender, EventArgs e)
        {
            SetAllInBox(KeywordSelector, true);
        }

        private void KeywordSelectNone_Click(object sender, EventArgs e)
        {
            SetAllInBox(KeywordSelector, false);
        }

        private void testbutton_Click(object sender, EventArgs e)
        {
            DebugConsole.Write("This is a Plays statement with no id or keyword", ProjectDomains.Plays);
            DebugConsole.Write("This is a MotionControl statement with id 2 and no keyword", ProjectDomains.MotionControl, 2);
            DebugConsole.Write("This is a Vision statement with no id and keyword Calibration", ProjectDomains.Vision, "Calibration");
            DebugConsole.Write("This is a PathPlanning statement with id 3 and keyword Bug", ProjectDomains.PathPlanning, 3, "Bug");
        }


        
    }
}