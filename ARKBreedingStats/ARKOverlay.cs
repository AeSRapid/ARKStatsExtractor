﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ARKBreedingStats
{
    public partial class ARKOverlay : Form
    {
        private Control[] labels = new Control[10];
        public Timer inventoryCheckTimer = new Timer();
        public Form1 ExtractorForm;
        public bool OCRing = false;
        private static String extraText;
        public List<TimerListEntry> timers = new List<TimerListEntry>();
        public static ARKOverlay theOverlay;

        public ARKOverlay()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.TopMost = true;

            labels[0] = lblLevel;
            labels[1] = lblHealth;
            labels[2] = lblStamina;
            labels[3] = lblOxygen;
            labels[4] = lblFood;
            labels[5] = lblWeight;
            labels[6] = lblMeleeDamage;
            labels[7] = lblMovementSpeed;
            labels[8] = lblExtraText;
            labels[9] = txtBreedingProgress;

            this.Location = Point.Empty;// this.PointToScreen(ArkOCR.OCR.statPositions["NameAndLevel"]);
            this.Size = new Size(2000, 2000);

            inventoryCheckTimer.Interval = 1000;
            inventoryCheckTimer.Tick += inventoryCheckTimer_Tick;
            theOverlay = this;
        }

        void inventoryCheckTimer_Tick(object sender, EventArgs e)
        {
            if (OCRing == true)
                return;
            lblStatus.Text = "..";
            Application.DoEvents();
            OCRing = true;
            if (!ArkOCR.OCR.isDinoInventoryVisible())
            {
                for (int i = 0; i < labels.Count(); i++)
                    if (labels[i] != null)
                        labels[i].Text = "";
            }
            else
            {
                lblStatus.Text = "Reading Values";
                Application.DoEvents();
                if (ExtractorForm != null)
                    ExtractorForm.doOCR("", false);
            }
            OCRing = false;
            lblStatus.Text = "";
            Application.DoEvents();
            return;
        }

        public void setValues(float[] wildValues, float[] tamedValues, Color[] colors = null)
        {
            foreach (KeyValuePair<String, Point> kv in ArkOCR.OCR.statPositions)
            {
                if (kv.Key == "Torpor")
                    continue;

                int statIndex = -1;
                switch (kv.Key)
                {
                    case "NameAndLevel": statIndex = 0; break;
                    case "Health": statIndex = 1; break;
                    case "Stamina": statIndex = 2; break;
                    case "Oxygen": statIndex = 3; break;
                    case "Food": statIndex = 4; break;
                    case "Weight": statIndex = 5; break;
                    case "Melee Damage": statIndex = 6; break;
                    case "Movement Speed": statIndex = 7; break;
                    default:
                        break;
                }

                if (statIndex == -1)
                    continue;

                labels[statIndex].Text = "[w" + wildValues[statIndex];
                if (tamedValues[statIndex] != 0)
                    labels[statIndex].Text += " + d" + tamedValues[statIndex];
                labels[statIndex].Text += "]";
                labels[statIndex].Location = this.PointToClient(ArkOCR.OCR.lastLetterPositions[kv.Key]);

                if (kv.Key != "NameAndLevel")
                    labels[statIndex].ForeColor = colors[statIndex];

                lblStatus.Location = new Point(labels[0].Location.X - 100, 10);
                lblExtraText.Location = new Point(labels[0].Location.X - 100, 40);
            }
            txtBreedingProgress.Text = "";
        }

        internal void setExtraText(string p)
        {
            if (ArkOCR.OCR.lastLetterPositions.ContainsKey("NameAndLevel"))
            {
                //Point loc = this.PointToClient(ArkOCR.OCR.lastLetterPositions["NameAndLevel"]);
                Point loc = this.PointToClient(ArkOCR.OCR.statPositions["NameAndLevel"]);

                loc.Offset(0, 30);

                extraText = p;
                lblExtraText.Text = p;
                lblExtraText.Location = loc;
            }
        }

        private void ARKOverlay_Load(object sender, EventArgs e)
        {

        }

        internal void setBreedingProgressValues(float percentage, int maxTime)
        {
            if (percentage >= 1)
            {
                txtBreedingProgress.Text = "";
                return;
            }
            string text = "";
            text = string.Format(@"Progress: {0:P2}", percentage);
            TimeSpan ts;
            string tsformat = "";
            if (percentage <= 0.1)
            {
                ts = new TimeSpan(0, 0, (int)(maxTime * (0.1 - percentage)));
                tsformat = "";
                tsformat += ts.Days > 0 ? "d'd'" : "";
                tsformat += ts.Hours > 0 ? "hh'h'" : "";
                tsformat += ts.Minutes > 0 ? "mm'm'" : "";
                tsformat += "ss's'";

                text += "\r\n[juvenile: " + ts.ToString(tsformat) + "]";
            }
            if (percentage <= 0.5)
            {
                ts = new TimeSpan(0, 0, (int)(maxTime * (0.5 - percentage)));
                tsformat = "";
                tsformat += ts.Days > 0 ? "d'd'" : "";
                tsformat += ts.Hours > 0 ? "hh'h'" : "";
                tsformat += ts.Minutes > 0 ? "mm'm'" : "";
                tsformat += "ss's'";

                text += "\r\n[adolescent: " + ts.ToString(tsformat) + "]";
            }

            ts = new TimeSpan(0, 0, (int)(maxTime * (1 - percentage)));
            tsformat = "";
            tsformat += ts.Days > 0 ? "d'd'" : "";
            tsformat += ts.Hours > 0 ? "hh'h'" : "";
            tsformat += ts.Minutes > 0 ? "mm'm'" : "";
            tsformat += "ss's'";

            text += "\r\n[adult: " + ts.ToString(tsformat) + "]";

            txtBreedingProgress.Text = text;
            txtBreedingProgress.Location = this.PointToClient(ArkOCR.OCR.lastLetterPositions["CurrentWeight"]);
        }
    }
}
