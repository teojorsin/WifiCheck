// This file under the MIT license.
// See LICENSE for details.

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using NativeWifi;

namespace WifiCheck
{
    public partial class MainForm : Form
    {
        private WlanClient wlanClient           = null;
        private Hashtable securityFromSSID;
        private int scanCounter                 = 0;
        private Color inactiveAPColor           = Color.FromArgb(96, 96, 96);
        private MacDatabase MacDatabase;

        public MainForm()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();

        }

        void MainFormLoad(object sender, EventArgs e)
        {
            Initialize();
        }

        void Initialize()
        {
            securityFromSSID    = new System.Collections.Hashtable();
            MacDatabase         = new MacDatabase();
            MacDatabase.LoadFromResource("MacDatabase");
            startWlanClient();
            startCheckForNotification(); // scan for networks
            FillWifiList();
        }

        // initialize NativeWifi API
        void startWlanClient()
        {
            if (wlanClient == null)
            {
                try
                {
                    wlanClient = new WlanClient();
                    wlanClient.Interfaces.Initialize();
                    foreach (WlanClient.WlanInterface wlanif in wlanClient.Interfaces)
                    {
                        wlanif.WlanNotification += new WlanClient.WlanInterface.WlanNotificationEventHandler(wifiInterface_WlanNotification);
                        wlanif.WlanConnectionNotification += new WlanClient.WlanInterface.WlanConnectionNotificationEventHandler(wifiInterface_WlanConnectionNotification);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                //Debug.WriteLine("wlanClient already started!");
            }
        }

        void startCheckForNotification()
        {
            startWlanClient();
            if (wlanClient.Interfaces.Length != 0)
            {
                foreach (WlanClient.WlanInterface wlanif in wlanClient.Interfaces)
                {
                    wlanif.Scan();
                }
            }
        }

        void wifiInterface_WlanConnectionNotification(NativeWifi.Wlan.WlanNotificationData notifyData, NativeWifi.Wlan.WlanConnectionNotificationData connNotifyData)
        {
            switch (notifyData.notificationCode)
            {
                case (int)Wlan.WlanNotificationSource.ACM:
                    switch ((Wlan.WlanNotificationCodeAcm)notifyData.notificationCode)
                    {
                        case Wlan.WlanNotificationCodeAcm.ConnectionComplete:
                            Debug.WriteLine("Wlan connected!");
                            break;
                        case Wlan.WlanNotificationCodeAcm.Disconnected:
                            Debug.WriteLine("Wlan disconnected!");
                            break;
                    }
                    break;
            }
        }

        void wifiInterface_WlanNotification(NativeWifi.Wlan.WlanNotificationData notifyData)
        {
            //Debug.WriteLine(string.Format("WlanNotification! {0}", notifyData.notificationCode));
            int signalQuality = 0;

            switch (notifyData.notificationCode)
            {
                case (int)Wlan.WlanNotificationCodeMsm.SignalQualityChange:
                    Debug.WriteLine("Wlan SignalQualityChange!");
                    Debug.WriteLine("Datasize: " + notifyData.dataSize.ToString());
                    signalQuality = Marshal.ReadInt32(notifyData.dataPtr);
                    Debug.WriteLine("Signal quality: " + signalQuality.ToString());
                    break;
                case (int)Wlan.WlanNotificationCodeMsm.Connected:
                    Debug.WriteLine("Wlan Connected!");
                    break;
                case (int)Wlan.WlanNotificationCodeMsm.Disconnected:
                    Debug.WriteLine("Wlan Disconnected!");
                    break;
                case (int)Wlan.WlanNotificationCodeMsm.RadioStateChange:
                    Debug.WriteLine("Wlan RadioStateChange!");
                    break;
                default:
                    Debug.WriteLine("Wlan notification!");
                    break;
            }
            uiChannelGraph.Invalidate();
        }

        string SSIDToString(Wlan.Dot11Ssid ssid)
        {
            return Encoding.ASCII.GetString(ssid.SSID, 0, (int)ssid.SSIDLength);
        }

        string BSSIDToString(byte[] bssid)
        {
            StringBuilder result = new StringBuilder(32);
            foreach (Byte b in bssid) result.Append(b.ToString("X2") + ":");
            result.Length -= 1;
            return result.ToString();
        }

        void updateSSIDSecurity()
        {
            //Debug.WriteLine("updateSSIDSecurity()");
            startWlanClient();
            if (wlanClient == null) return;

            foreach (WlanClient.WlanInterface wli in wlanClient.Interfaces)
            {
                // store security per ssid
                foreach (Wlan.WlanAvailableNetwork an in wli.GetAvailableNetworkList(0))
                {
                    string ssid = SSIDToString(an.dot11Ssid);
                    if (!securityFromSSID.Contains(ssid))
                    {
                        securityFromSSID.Add(ssid, an.dot11DefaultAuthAlgorithm.ToString());
                        //Debug.WriteLine(string.Format("Found {0}: {1}", ssid, an.dot11DefaultAuthAlgorithm.ToString()));
                    }
                }
            }
        }

        void FillWifiList()
        {
            string ssid;
            string bssid;
            string sec;
            int channel;
            int quality;
            int frequency;
            APNode apn;

            Wlan.WlanBssEntry[] bssList;

            // initialize NativeWifi API
            startWlanClient();

            // scan for networks
            //startCheckForNotification();

            // make list of security type per SSID
            updateSSIDSecurity();


            // deactivate all APs and activate the ones we can see
            foreach (ListViewItem lvi in uiListWifi.Items)
            {
                lvi.UseItemStyleForSubItems = false;
                for (int i = 1; i < lvi.SubItems.Count; i++)
                {
                    lvi.SubItems[i].ForeColor = inactiveAPColor;
                }
            }
            uiListWifi.Invalidate();

            // deactivate all first
            foreach (APNode ap in uiChannelGraph.NetworkList.Values)
            {
                ap.active = false;
            }


            // go through each interface and update/activate each AP we can still see
            foreach (WlanClient.WlanInterface wli in wlanClient.Interfaces)
            {
                // list APs
                bssList = wli.GetNetworkBssList();

                foreach (Wlan.WlanBssEntry bssEntry in bssList)
                {
                    ssid = SSIDToString(bssEntry.dot11Ssid);
                    bssid = BSSIDToString(bssEntry.dot11Bssid);
                    quality = (int)bssEntry.linkQuality;
                    frequency = (int)bssEntry.chCenterFrequency / 1000;
                    channel = APNode.GetChannelNumber(frequency);

                    ssid = ssid.Replace("\0", "");
                    if (ssid == "") ssid = "(none)";

                    if (securityFromSSID.Contains(ssid)) sec = securityFromSSID[ssid].ToString();
                    else sec = "-";

                    // if AP was already found, update data
                    if (uiChannelGraph.NetworkList.ContainsKey(bssid))
                    {
                        apn                 = uiChannelGraph.NetworkList[bssid];
                        apn.active          = true;
                        apn.frequency       = frequency;
                        apn.channelWidth    = 20;
                        apn.ssid            = ssid;
                        apn.quality         = quality;
                        apn.security        = sec;

                        // find and update item in wifi ap list
                        foreach (ListViewItem lvi in uiListWifi.Items)
                        {
                            if (bssid == lvi.SubItems[4].Text)
                            {
                                lvi.ForeColor = apn.apColor;
                                lvi.Text = apn.ssid;
                                // string[]{channel.ToString(), quality.ToString(), security, bssid, vendor};
                                lvi.SubItems[1].Text = channel.ToString();
                                lvi.SubItems[2].Text = apn.quality.ToString();
                                lvi.SubItems[3].Text = sec;

                                foreach (ListViewItem.ListViewSubItem si in lvi.SubItems)
                                {
                                    si.ForeColor = apn.apColor; // reset default
                                }
                            }
                        }

                    }
                    else
                    {
                        // new AP, add to lists and UI
                        string vendor = MacDatabase.FindVendor(bssid);
                        APNode ap = new APNode(frequency, quality, ssid, bssid, sec, "", vendor);
                        //uiChannelGraph.networks.Add(bssid, ap);
                        uiChannelGraph.AddAP(ap);

                        // add to UI
                        ListViewItem lvi = new ListViewItem(ssid);
                        lvi.ForeColor = ap.apColor; // color is set by uiChannelGraph.AddAP(ap)
                        lvi.SubItems.AddRange(ap.ToStringArray());
                        uiListWifi.Items.Add(lvi);
                    }
                }
            }

            uiListWifi.Invalidate();
            uiChannelGraph.Invalidate();
        }
        
        void UiButtonUpdateClick(object sender, EventArgs e)
        {
            FillWifiList();
        }

        void TimerSignalQualityTick(object sender, EventArgs e)
        {
            FillWifiList();
            Debug.WriteLine("TimerSignalQualityTick()");
            scanCounter++;
            if (scanCounter == 6)
            {
                scanCounter = 0;
                startCheckForNotification();
                Debug.WriteLine("startCheckForNotification()");
            }
        }

        void UiButtonScanClick(object sender, EventArgs e)
        {
            startCheckForNotification();
        }

        void UiRadioBand24CheckedChanged(object sender, EventArgs e)
        {
            uiChannelGraph.ShowGraph(WifiBand.Band2400MHz);
            animationTimer.Enabled = true;
            uiRadioBand24.Enabled = false;
            uiRadioBand5k.Enabled = false;
        }

        void UiRadioBand5kCheckedChanged(object sender, EventArgs e)
        {
            uiChannelGraph.ShowGraph(WifiBand.Band5000MHz);
            animationTimer.Enabled = true;
            uiRadioBand24.Enabled = false;
            uiRadioBand5k.Enabled = false;
        }

        void AnimationTimerTick(object sender, EventArgs e)
        {
            bool animating = uiChannelGraph.UpdateAnimation();
            if (!animating)
            {
                animationTimer.Enabled = false;
                uiRadioBand24.Enabled = true;
                uiRadioBand5k.Enabled = true;
                // can't do this, causes endless animation loop
                // if(uiChannelGraph.showBand == WifiBand.Band2400MHz) uiRadioBand24.Checked = true;
                // else uiRadioBand5k.Checked = true;
            }
        }
    }
}