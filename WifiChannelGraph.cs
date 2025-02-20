// This file under the MIT license.
// See LICENSE for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace WifiCheck
{
    public enum WifiBand { Band2400MHz, Band5000MHz, Band60000MHz };

    public class APNode
    {
        public bool active;
        public bool infrastructure;
        public int frequency;
        public int channel;
        public int channelWidth;
        public int quality;
        public string ssid;
        public string bssid;
        public string security;
        public string auth;
        public string vendor;
        public Color apColor;

        // channel lookup dictionary, frequency in MHz
        public static readonly Dictionary<int, int> channels = new Dictionary<int, int>{
			// 2,4 GHz band
			{2412, 1},
            {2417, 2},
            {2422, 3},
            {2427, 4},
            {2432, 5},
            {2437, 6},
            {2442, 7},
            {2447, 8},
            {2452, 9},
            {2457, 10},
            {2462, 11},
            {2467, 12},
            {2472, 13},
            {2484, 14},
			
			// 5 GHz band
			{5180, 36}, // Indoors
			{5200, 40},
            {5220, 44},
            {5240, 48},
            {5260, 52}, // Indoors/DFS/TPC
			{5280, 56},
            {5300, 60},
            {5320, 64},
			// forbidden channels here
			{5500, 100}, // DFS/TPC
			{5520, 104},
            {5540, 108},
            {5560, 112},
            {5580, 116},
            {5600, 120},
            {5620, 124},
            {5640, 128},
            {5660, 132},
            {5680, 136},
            {5700, 140},
            {5745, 149}, // SRD (25mW)
			{5765, 153},
            {5785, 157},
            {5805, 161},
            {5825, 165},
        };
        public static int GetChannelNumber(int frequency)
        {
            if (APNode.channels.ContainsKey(frequency)) return APNode.channels[frequency];
            else return -1;
        }

        public APNode() { }

        public APNode(int frequency, int quality, string ssid, string bssid, string security, string auth, string vendor)
        {
            this.active = true;
            this.infrastructure = true;
            this.frequency = frequency;
            this.channelWidth = 20; // standard
            this.quality = quality;
            this.ssid = ssid;
            this.bssid = bssid;
            this.security = security;
            this.auth = auth;
            this.vendor = vendor;

            //Debug.WriteLine("APNode frequency: " + frequency.ToString());

            if (APNode.channels.ContainsKey(frequency)) this.channel = APNode.channels[frequency];
            else this.channel = -1;

            string macid = bssid.Substring(0, 8).ToUpper();
        }

        public string[] ToStringArray()
        {
            return new string[] { channel.ToString(), quality.ToString(), security, bssid, vendor };
        }
    }

    /// <summary>
    /// Description of APChannelGraph.
    /// </summary>
    public partial class APChannelGraph : UserControl
    {
        private Dictionary<string, APNode> networks = new Dictionary<string, APNode>();
        public WifiBand showBand = WifiBand.Band2400MHz;

        private Bitmap backBuffer24;
        private Bitmap backBuffer5k;
        //private Bitmap backBuffer60k;

        private float margin = 50f;
        private float spectrumSizeMHz = 100f;
        private float spectrumSizePixels = 600f;
        private float pixelsPerMHz = 2f;
        private float channelDrawHeight = 100f;
        private float lowLevel = 15f; // lower level in %
        private Pen axisPen = new Pen(Color.White, 2.0f);
        private Pen lowLimitPen = new Pen(Color.Sienna, 2.0f);
        private Pen channelPen = new Pen(Color.White, 2.0f);
        private Pen channelLinePen = new Pen(Color.DarkGray, 1f);
        private SolidBrush channelTextBrush = new SolidBrush(Color.White);

        public readonly static Color[] networkColors = {Color.Gold, Color.Chartreuse, Color.Chocolate, Color.Turquoise, Color.LightCoral,
            Color.DeepSkyBlue, Color.BlueViolet, Color.Fuchsia, Color.LightPink, Color.DarkOrchid};


        // used for animating between graphs horizontally
        private bool animating = false;
        private float animPosition = 0.0f;
        private float animSpeed = 0.05f;
        private WifiBand animateTo = WifiBand.Band2400MHz;


        public APChannelGraph()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();

            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.Invalidate();
        }

        public void AddAP(APNode ap)
        {
            ap.apColor = networkColors[networks.Count % networkColors.Length];
            networks.Add(ap.bssid, ap);
        }

        public Dictionary<string, APNode> NetworkList
        {
            get { return networks; }
        }

        // easing function for smooth animation		
        private float QuadEaseInOut(float t)
        {
            if (t < 0.5f) return 2.0f * t * t;
            else return -1.0f + (4.0f - 2.0f * t) * t;
        }


        // animate and show wifi band
        public void ShowGraph(WifiBand band)
        {
            if (band == showBand) return;
            animating = true;
            animateTo = band;

            // make sure this is done once before animation starts
            UpdateChannelGraph24();
            UpdateChannelGraph5k();

            UpdateAnimation();
        }

        // returns true when animation is complete
        public bool UpdateAnimation()
        {
            // update animation
            if (animateTo == WifiBand.Band2400MHz)
            {
                if (animPosition > 0.0f) animPosition -= animSpeed;
                else animating = false;
            }
            else
            {
                if (animPosition < 1.0f) animPosition += animSpeed;
                else animating = false;
            }
            //Debug.WriteLine(animPosition.ToString());
            // clamp
            if (animPosition < 0.0f) animPosition = 0.0f;
            if (animPosition > 1.0f) animPosition = 1.0f;
            // determine if animation is complete
            //animating = (animPosition>0.0f) && (animPosition<1.0f);
            if (!animating) showBand = animateTo;
            this.Invalidate();
            Application.DoEvents(); // force event processing here to repaint
                                    //Debug.WriteLine("anim: " + animating.ToString());
            return animating;
        }

        // recalculate various measurements used when drawing
        private void updateDrawingParameters()
        {
            spectrumSizePixels = this.Bounds.Width - (2 * margin);
            pixelsPerMHz = spectrumSizePixels / spectrumSizeMHz;
            channelDrawHeight = this.Bounds.Height - (2 * margin);
        }

        // takes frequency in MHz and returns x position
        private float getXPosFromFrequency(int frequency)
        {
            if (frequency < 2500) return (float)(frequency - 2400) * spectrumSizePixels / spectrumSizeMHz;
            else return (float)(frequency - 5000) * spectrumSizePixels / spectrumSizeMHz;
        }

        private void ResizeBackBuffers()
        {
            if (backBuffer24 != null) backBuffer24.Dispose();
            if (backBuffer5k != null) backBuffer5k.Dispose();
            backBuffer24 = null;
            backBuffer5k = null;

            //if(backBuffer60k != null) backBuffer60k.Dispose();
            InitBackBuffers();

            //Debug.WriteLine("Resizing backbuffers!");
        }

        private void InitBackBuffers()
        {
            if (backBuffer24 == null) backBuffer24 = new Bitmap(this.Bounds.Width, this.Bounds.Height, PixelFormat.Format32bppArgb);
            if (backBuffer5k == null) backBuffer5k = new Bitmap(this.Bounds.Width, this.Bounds.Height, PixelFormat.Format32bppArgb);
            //if(backBuffer60k == null) backBuffer60k  = new Bitmap(this.Bounds.Width, this.Bounds.Height, PixelFormat.Format32bppArgb);
        }

        private void UpdateChannelGraph24()
        {
            if (backBuffer24 == null) return;
            if (backBuffer24.Height == 0) return;

            //Debug.WriteLine("UpdateChannelGraph24()");

            spectrumSizeMHz = 100;
            updateDrawingParameters();

            Graphics g = Graphics.FromImage(backBuffer24);

            int imargin = (int)margin;

            // drawing code here
            // clear everything
            g.FillRectangle(new SolidBrush(this.BackColor), new Rectangle(new Point(0, 0), this.Bounds.Size));

            // draw lower quality limit line
            lowLimitPen.Width = 2.0f;
            lowLimitPen.DashCap = DashCap.Round;
            lowLimitPen.DashStyle = DashStyle.Dash;
            int level = this.Bounds.Height - imargin - (int)((float)(this.Bounds.Height - imargin * 2) / 100f * (float)lowLevel);
            g.DrawLine(lowLimitPen, imargin, level, this.Bounds.Width - imargin, level);

            // draw graph region border
            g.DrawRectangle(Pens.DarkGray, new Rectangle(imargin, imargin, this.Bounds.Width - imargin * 2, this.Bounds.Height - imargin * 2));

            // draw each axis
            axisPen.EndCap = LineCap.ArrowAnchor;
            axisPen.Width = 2.0f; // draw lines
            g.DrawLine(axisPen, imargin, this.Bounds.Height - imargin, imargin, imargin);
            g.DrawLine(axisPen, imargin, this.Bounds.Height - imargin, this.Bounds.Width - imargin, this.Bounds.Height - imargin);
            axisPen.Width = 6.0f; // draw arrow caps bigger
            g.DrawLine(axisPen, imargin, imargin + 5, imargin, imargin);
            g.DrawLine(axisPen, this.Bounds.Width - imargin - 5, this.Bounds.Height - imargin, this.Bounds.Width - imargin, this.Bounds.Height - imargin);

            // draw axis descriptions
            g.DrawString("Signal quality (%)", SystemFonts.IconTitleFont, SystemBrushes.Window, imargin / 2 - 10, imargin - 23);
            g.DrawString("Channel", SystemFonts.IconTitleFont, SystemBrushes.Window, this.Bounds.Width - imargin - 20, this.Bounds.Height - margin + 10);

            // draw channel lines
            channelLinePen.Color = Color.FromArgb(90, 90, 90);
            int pos = 0;
            foreach (KeyValuePair<int, int> channel in APNode.channels)
            {
                if (channel.Key > 2500) break; // only draw channels below 2,5GHz
                pos = (int)(margin + getXPosFromFrequency(channel.Key));
                g.DrawLine(channelLinePen, pos, this.Bounds.Height - margin + 6, pos, margin); // channel line
                g.DrawString(channel.Value.ToString(), SystemFonts.IconTitleFont, SystemBrushes.Window, pos - 7, this.Bounds.Height - margin + 10); // channel number
            }

            // draw active networks
            //int netCounter = 0; // only used for color selection
            int xpos = 0;
            foreach (KeyValuePair<string, APNode> node in networks)
            {
                APNode ap = (APNode)node.Value;

                // only draw channels below 2,5GHz
                if (ap.frequency > 2500) continue;

                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.GammaCorrected;

                xpos = (int)getXPosFromFrequency(ap.frequency);
                //channelPen.Color = networkColors[netCounter % networkColors.Length];
                channelPen.Color = ap.apColor;
                channelTextBrush.Color = ap.apColor;

                // draw inactive networks faintly
                if (ap.active)
                {
                    channelPen.Color = Color.FromArgb(255, channelPen.Color);
                    channelPen.DashStyle = DashStyle.Solid;
                }
                else
                {
                    channelPen.Color = Color.FromArgb(180, channelPen.Color);
                    channelPen.DashStyle = DashStyle.Dot;
                }


                // extra caption offset to avoid overlapping network names
                float extraOffset = 0;
                bool begin = false;
                foreach (KeyValuePair<string, APNode> node2 in networks)
                {
                    APNode n = (APNode)node2.Value;
                    if (begin && n.frequency == ap.frequency)
                    {
                        if (Math.Abs(n.quality - ap.quality) < 10) extraOffset += 15;
                        if (n.quality == ap.quality) n.quality -= 1;
                    }
                    if (n == ap) begin = true;
                }

                float h = channelDrawHeight * ((float)ap.quality / 100.0f);
                float w = pixelsPerMHz * ap.channelWidth; // channelWidth is usually 20MHz
                DrawChannel(g, channelPen, new RectangleF(margin + xpos - w / 2f, this.Bounds.Height - margin - h, w, h));
                g.DrawString(ap.ssid, SystemFonts.IconTitleFont, channelTextBrush, margin + xpos, this.Bounds.Height - margin - h - 20 - extraOffset);

                //netCounter++; // used for selecting color
            }

            // draw graph title
            g.DrawString("2,4 GHz band", SystemFonts.CaptionFont, SystemBrushes.Window, this.Bounds.Width / 2 - 50, 7);
        }

        private void UpdateChannelGraph5k()
        {
            if (backBuffer5k == null) return;
            if (backBuffer5k.Height == 0) return;

            //Debug.WriteLine("UpdateChannelGraph5k()");

            spectrumSizeMHz = 1000;
            updateDrawingParameters();

            Graphics g = Graphics.FromImage(backBuffer5k);

            int imargin = (int)margin;

            // drawing code here
            // clear everything
            g.FillRectangle(new SolidBrush(this.BackColor), new Rectangle(new Point(0, 0), this.Bounds.Size));

            // draw lower quality limit line
            lowLimitPen.Width = 2.0f;
            lowLimitPen.DashCap = DashCap.Round;
            lowLimitPen.DashStyle = DashStyle.Dash;
            int level = this.Bounds.Height - imargin - (int)((float)(this.Bounds.Height - imargin * 2) / 100f * (float)lowLevel);
            g.DrawLine(lowLimitPen, imargin, level, this.Bounds.Width - imargin, level);

            // draw graph region border
            g.DrawRectangle(Pens.DarkGray, new Rectangle(imargin, imargin, this.Bounds.Width - imargin * 2, this.Bounds.Height - imargin * 2));

            // draw each axis
            axisPen.EndCap = LineCap.ArrowAnchor;
            axisPen.Width = 2.0f; // draw lines
            g.DrawLine(axisPen, imargin, this.Bounds.Height - imargin, imargin, imargin);
            g.DrawLine(axisPen, imargin, this.Bounds.Height - imargin, this.Bounds.Width - imargin, this.Bounds.Height - imargin);
            axisPen.Width = 6.0f; // draw arrow caps bigger
            g.DrawLine(axisPen, imargin, imargin + 5, imargin, imargin);
            g.DrawLine(axisPen, this.Bounds.Width - imargin - 5, this.Bounds.Height - imargin, this.Bounds.Width - imargin, this.Bounds.Height - imargin);

            // draw axis descriptions
            g.DrawString("Signal quality (%)", SystemFonts.IconTitleFont, SystemBrushes.Window, imargin / 2 - 10, imargin - 23);
            g.DrawString("Channel", SystemFonts.IconTitleFont, SystemBrushes.Window, this.Bounds.Width - imargin - 20, this.Bounds.Height - margin + 10);

            // draw channel lines
            channelLinePen.Color = Color.FromArgb(90, 90, 90);
            int pos = 0;
            int chanLineNum = 0;
            int chanNumOffset = 0;
            float fontSize = (this.Bounds.Width > 600) ? 8.0f : 7.0f;
            Font chanFont = new Font(SystemFonts.IconTitleFont.Name, fontSize);
            foreach (KeyValuePair<int, int> channel in APNode.channels)
            {
                if (channel.Key < 2500) continue; // only draw channels above 2,5GHz (ignore 60GHz for now)
                pos = (int)(margin + getXPosFromFrequency(channel.Key));
                if (1 == (chanLineNum & 1)) chanNumOffset = 13;
                else chanNumOffset = 0;
                g.DrawLine(channelLinePen, pos, this.Bounds.Height - margin + 6 + chanNumOffset, pos, margin); // channel line
                g.DrawString(channel.Value.ToString(), chanFont, SystemBrushes.Window, pos - 9, this.Bounds.Height - margin + 10 + chanNumOffset); // channel number
                chanLineNum += 1;
            }

            // draw active networks
            //int netCounter = 0; // only used for color selection
            int xpos = 0;
            foreach (KeyValuePair<string, APNode> node in networks)
            {
                APNode ap = (APNode)node.Value;

                // only draw channels above 2,5GHz (ignore 60GHz for now)
                if (ap.frequency < 2500) continue;

                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighQuality;

                xpos = (int)getXPosFromFrequency(ap.frequency);
                //channelPen.Color = networkColors[netCounter % networkColors.Length];
                channelPen.Color = ap.apColor;
                channelTextBrush.Color = ap.apColor;

                // draw inactive networks faintly
                if (ap.active)
                {
                    channelPen.Color = Color.FromArgb(255, channelPen.Color);
                    channelPen.DashStyle = DashStyle.Solid;
                }
                else
                {
                    channelPen.Color = Color.FromArgb(180, channelPen.Color);
                    channelPen.DashStyle = DashStyle.Dot;
                }

                float h = channelDrawHeight * ((float)ap.quality / 100.0f);
                float w = pixelsPerMHz * ap.channelWidth; // channelWidth is usually 20MHz
                DrawChannel(g, channelPen, new RectangleF(margin + xpos - w / 2f, this.Bounds.Height - margin - h, w, h));
                g.DrawString(ap.ssid, SystemFonts.IconTitleFont, channelTextBrush, margin + xpos, this.Bounds.Height - margin - h - 20);

                //netCounter++; // used for selecting color
            }

            // draw graph title
            g.DrawString("5 GHz band", SystemFonts.CaptionFont, SystemBrushes.Window, this.Bounds.Width / 2 - 50, 7);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int imargin = (int)margin;

            // make sure we have something to draw onto
            InitBackBuffers();

            if (animating)
            {
                // fill background first
                e.Graphics.FillRectangle(Brushes.Black, new Rectangle(new Point(0, 0), this.Bounds.Size));

                // don't update graphs during animation
                float controlWidth = (float)this.Bounds.Width;
                float pos = QuadEaseInOut(animPosition);
                e.Graphics.DrawImage(backBuffer24, (int)(-controlWidth * pos), 0);
                e.Graphics.DrawImage(backBuffer5k, (int)(-controlWidth * pos + controlWidth), 0);
            }
            else
            {
                // just update graph and draw it to control
                switch (showBand)
                {
                    case WifiBand.Band2400MHz:
                        UpdateChannelGraph24();
                        e.Graphics.DrawImage(backBuffer24, 0, 0, backBuffer24.Width, backBuffer24.Height);
                        break;
                    case WifiBand.Band5000MHz:
                        UpdateChannelGraph5k();
                        e.Graphics.DrawImage(backBuffer5k, 0, 0, backBuffer5k.Width, backBuffer5k.Height);
                        break;
                }
            }
        }

        private void DrawChannel(Graphics g, Pen p, RectangleF r)
        {
            float last_x = r.X;
            float last_y = r.Y + r.Height;
            float x = 0;
            float y = 0;
            float halfWidth = r.Width / 2.0f;

            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
            g.SmoothingMode = SmoothingMode.HighQuality;

            float steps = 20f;
            for (float i = -steps; i <= steps; i += 1f)
            {
                x = r.X + halfWidth + halfWidth * (i / steps);
                y = r.Y + (float)(Math.Pow((i / steps), 2.0) * r.Height);

                g.DrawLine(p, last_x, last_y, x, y);

                last_x = x;
                last_y = y;
            }

            g.SmoothingMode = SmoothingMode.None;
            //g.DrawRectangle(Pens.LimeGreen, Rectangle.Round(r));
        }

        void APChannelGraphSizeChanged(object sender, EventArgs e)
        {
            ResizeBackBuffers();
            updateDrawingParameters();
        }
    }
}