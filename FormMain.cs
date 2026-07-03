using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Runtime.Versioning;
using System.Windows.Forms;
using Barbar.HostsSwitcher.Provider;
using Barbar.HostsSwitcher.Network;

namespace Barbar.HostsSwitcher
{
    [SupportedOSPlatform("windows")]
    public partial class FormMain : Form
    {
        private readonly IHostProvider _hostsProvider;
        private readonly IAutoHostsSwitcher _autoSwitcher;
        private readonly Timer _networkChangeDebounceTimer;

        public FormMain()
        {
            InitializeComponent();

            _hostsProvider = new HostProvider();

            var matchers = new INetworkProfileMatcher[] { new GatewayNetworkProfileMatcher() };
            _autoSwitcher = new AutoHostsSwitcher(
                _hostsProvider,
                new WindowsNetworkContextProvider(),
                new ProfileMetadataParser(),
                matchers);

            _networkChangeDebounceTimer = new System.Windows.Forms.Timer();
            _networkChangeDebounceTimer.Interval = 2000;
            _networkChangeDebounceTimer.Tick += NetworkChangeDebounceTimer_Tick;

            quickSwitchToolStripMenuItem.DropDownItemClicked +=
                new ToolStripItemClickedEventHandler(quickSwitchToolStripMenuItem_DropDownItemClicked);
            foreach (var host in _hostsProvider.GetHostFiles())
            {
                listHosts.Items.Add(host);
                quickSwitchToolStripMenuItem.DropDownItems.Add(host);
            }

            Text = $"Hosts Switcher - v.{typeof(FormMain).Assembly.GetName().Version}";

            NetworkChange.NetworkAddressChanged += OnNetworkAddressChanged;
            NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;

            PerformAutoSwitch();
        }

        private void quickSwitchToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var clicked = e.ClickedItem.Text;
            if (!string.IsNullOrEmpty(clicked))
            {
                UseAsHosts(clicked);
            }
        }

        private void RefreshList()
        {
            var selectedItem = (string)listHosts.SelectedItem;
            bool setSelected = false;

            listHosts.Items.Clear();
            foreach (var host in _hostsProvider.GetHostFiles())
            {
                if (!string.IsNullOrEmpty(selectedItem) &&
                    string.Compare(host, selectedItem, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    setSelected = true;
                }

                listHosts.Items.Add(host);
            }

            if (setSelected)
            {
                listHosts.SelectedItem = selectedItem;
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.Visible = false;
            }
            else
            {
                NetworkChange.NetworkAddressChanged -= OnNetworkAddressChanged;
                NetworkChange.NetworkAvailabilityChanged -= OnNetworkAvailabilityChanged;
                _networkChangeDebounceTimer?.Dispose();
            }
        }

        private void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                WindowState = FormWindowState.Normal;
                this.Visible = true;
                this.Focus();
            }
        }

        private void LogInfo(string format, params object[] args)
        {
            txtLog.Text += string.Format(CultureInfo.InvariantCulture, format, args);
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        private void toolStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == btnExit)
            {
                Application.Exit();
            }

            if (e.ClickedItem == btnUseAsHosts && listHosts.SelectedItem != null)
            {
                UseAsHosts((string)listHosts.SelectedItem);
            }

            if (e.ClickedItem == btnCopy && listHosts.SelectedItem != null)
            {
                var formCopy = new FormCopy(string.Format("Copy {0} to which file ?", listHosts.SelectedItem));
                var result = formCopy.ShowDialog(this);
                if (result == DialogResult.OK && !string.IsNullOrEmpty(formCopy.FileName))
                {
                    _hostsProvider.CopyHosts((string)listHosts.SelectedItem, formCopy.FileName);
                    LogInfo("Copied {0} to {1}\r\n", listHosts.SelectedItem, formCopy.FileName);
                    RefreshList();
                }
            }

            if (e.ClickedItem == btnDelete && listHosts.SelectedItem != null)
            {
                if (MessageBox.Show(string.Format("Really delete {0} ?", listHosts.SelectedItem), string.Empty,
                        MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    _hostsProvider.DeleteHosts((string)listHosts.SelectedItem);
                    LogInfo("Deleted {0}\r\n", listHosts.SelectedItem);
                    RefreshList();
                }
            }

            if (e.ClickedItem == btnViewEdit && listHosts.SelectedItem != null)
            {
                _hostsProvider.LaunchEditor((string)listHosts.SelectedItem);
            }

            if (e.ClickedItem == btnOpenFolder)
            {
                _hostsProvider.OpenFolder();
            }
        }

        private void UseAsHosts(string selectedItem)
        {
            _hostsProvider.ReplaceHosts(selectedItem);
            lblHosts.Text = selectedItem;
            LogInfo("Copied {0} to hosts\r\n", selectedItem);
        }

        private void menuStripExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void menuStripShow_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
            this.Visible = true;
            this.Focus();
        }

        private void listHosts_DoubleClick(object sender, EventArgs e)
        {
            if (listHosts.SelectedItem != null)
            {
                _hostsProvider.ReplaceHosts((string)listHosts.SelectedItem);
                lblHosts.Text = (string)listHosts.SelectedItem;
                LogInfo("Copied {0} to hosts\r\n", listHosts.SelectedItem);
            }
        }

        private void PerformAutoSwitch()
        {
            var result = _autoSwitcher.EvaluateAndSwitch();

            switch (result.Status)
            {
                case AutoSwitchStatus.Switched:
                    lblHosts.Text = result.SelectedProfile;
                    LogInfo("[Auto] {0}\r\n", result.Message);
                    break;
                case AutoSwitchStatus.AlreadyActive:
                    lblHosts.Text = result.SelectedProfile;
                    LogInfo("[Auto] {0}\r\n", result.Message);
                    break;
                case AutoSwitchStatus.NoMatch:
                    LogInfo("[Auto] {0}\r\n", result.Message);
                    notifyIcon.ShowBalloonTip(3000, "Hosts Switcher", result.Message, ToolTipIcon.Warning);
                    break;
                case AutoSwitchStatus.AmbiguousMatch:
                    if (!string.IsNullOrEmpty(result.SelectedProfile))
                    {
                        lblHosts.Text = result.SelectedProfile;
                    }

                    LogInfo("[Auto] {0}\r\n", result.Message);
                    if (result.AmbiguousCandidates != null && result.AmbiguousCandidates.Length > 1)
                    {
                        var candidateList = string.Join(", ", result.AmbiguousCandidates);
                        notifyIcon.ShowBalloonTip(4000, "Hosts Switcher",
                            $"Multiple profiles matched. Using first: {result.SelectedProfile}. Candidates: {candidateList}",
                            ToolTipIcon.Warning);
                    }

                    break;
                case AutoSwitchStatus.Error:
                    LogInfo("[Auto] {0}\r\n", result.Message);
                    notifyIcon.ShowBalloonTip(3000, "Hosts Switcher", result.Message, ToolTipIcon.Error);
                    break;
            }
        }

        private void OnNetworkAddressChanged(object sender, EventArgs e)
        {
            DebounceNetworkChange();
        }

        private void OnNetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            DebounceNetworkChange();
        }

        private void DebounceNetworkChange()
        {
            _networkChangeDebounceTimer.Stop();
            _networkChangeDebounceTimer.Start();
        }

        private void NetworkChangeDebounceTimer_Tick(object sender, EventArgs e)
        {
            _networkChangeDebounceTimer.Stop();
            if (InvokeRequired)
            {
                Invoke(new Action(() => PerformAutoSwitch()));
            }
            else
            {
                PerformAutoSwitch();
            }
        }
    }
}