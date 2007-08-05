using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Vision {
    public partial class frmGameObjects : Form {
        string[] properties = new string[] {
                "team",
                "id",
                "numDots",
                "worldCenter",
                "location",
                "direction"
            };

        string[] captions = new string[] {
                "Team:",
                "Robot ID:",
                "Num of dots:",
                "World Center:", 
                "Location:",
                "Direction:"
            };

        public frmGameObjects() {
            InitializeComponent();

            int i, j;
            int team;

            
            
            Label captionLbl, infoLbl;
            TableLayoutPanel[] gameInfoTables = new TableLayoutPanel[3]; // for 2 teams

            int top = 5;
            
            for (team = 1; team <= 2; team++) {
                gameInfoTables[team] = new TableLayoutPanel();
                gameInfoTables[team].Name = "gameInfoTable_team_" + team.ToString();

                gameInfoTables[team].ColumnCount = 6; // 1 caption + 5 robots
                gameInfoTables[team].RowCount = properties.Length;

                gameInfoTables[team].CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;


                gameInfoTables[team].Size = new Size(120 + 110 * 5 + 7, 15 * properties.Length + 7);
                gameInfoTables[team].Left = 5;
                gameInfoTables[team].Top = top;

                for (i = 0; i < properties.Length; i++) {
                    captionLbl = new Label();
                    //captionLbl.Name = "caption_" + i.ToString();
                    captionLbl.Text = captions[i];
                    gameInfoTables[team].Controls.Add(captionLbl, 0, i);
                }
                gameInfoTables[team].ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));

                

                infoLbl = new Label();

                //initialize the info-tables for each robot

                for (j = 0; j < 5; j++) {
                    captionLbl = new Label();
                    //captionLbl.Name = "caption_team_" + team.ToString() + "_" + j.ToString();
                    captionLbl.Text = team.ToString();
                    gameInfoTables[team].Controls.Add(captionLbl, j, 0);
                    gameInfoTables[team].RowStyles.Add(new RowStyle(SizeType.Absolute, 15));

                    captionLbl = new Label();
                    //captionLbl.Name = "caption_rid_" + team.ToString() + "_" + j.ToString();                    
                    captionLbl.Text = j.ToString();
                    gameInfoTables[team].Controls.Add(captionLbl, j, 1);
                    gameInfoTables[team].RowStyles.Add(new RowStyle(SizeType.Absolute, 15));

                    
                    for (i = 2; i < properties.Length; i++) {
                        infoLbl = new Label();
                        infoLbl.Name = "robot_" + j.ToString() + "_prop_" + properties[i];
                        infoLbl.Font = new Font("Microsoft Sans Serif", 7F);

                        gameInfoTables[team].Controls.Add(infoLbl, j, i);
                        gameInfoTables[team].RowStyles.Add(new RowStyle(SizeType.Absolute, 15));
                    }
                    gameInfoTables[team].ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));

                }

                this.Controls.Add(gameInfoTables[team]);

                top += gameInfoTables[1].Height + 20;
            }

        }

        

        public void displayRobotInfo(int team, int robotID, string[] propertyValues) {
            Control[] tmpSearchResult;
            
            TableLayoutPanel gameInfoTable;
            Label infoLbl;
            int i;

            //DEBUG
            /*if (robotID == 0) {
                robotID = 1;
            }*/

            if (robotID == -1) {
                Console.WriteLine("Unrecognized robot found. NumDots=" + propertyValues[0]);
                return;
            }

            tmpSearchResult = this.Controls.Find("gameInfoTable_team_" + team.ToString(), false);
            if (tmpSearchResult.Length <= 0) {
                Console.WriteLine("Error: could not find GameInfoTable for specified team.");
                return;
            }

            gameInfoTable = (TableLayoutPanel)tmpSearchResult[0];

            for (i = 2; i < properties.Length; i++) {
                tmpSearchResult = gameInfoTable.Controls.Find("robot_" + robotID.ToString() + "_prop_" + properties[i], false);
                infoLbl = (Label)tmpSearchResult[0];
                infoLbl.Text = propertyValues[i - 2];
            }

            this.Refresh();
            

            /*lblID.Text = String.Format("{0:G}", 0);
            lblTeam.Text = String.Format("{0:G}", team);
            lblCenterWorld.Text = String.Format("({0:F}, {1:F})", x, y);
            lblLocation.Text = String.Format("({0:F}, {1:F})", location[0], location[1]);
            lblDirection.Text = String.Format("[{0:F}, {1:F}]", direction[0], direction[1]);
            lblNumDots.Text = String.Format("{0:G}", numDots);*/


            /*for (i = 0; i < numDots; i++) {
                String.Format("   dot #{0:G} color: {1:G}", i, dots[i].ColorClass));
            }*/
        }
    }
}