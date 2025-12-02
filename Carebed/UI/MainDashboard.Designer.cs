using Carebed.Infrastructure.Enums;
using Carebed.Infrastructure.EventBus;
using Carebed.Infrastructure.Message.ActuatorMessages;
using Carebed.Infrastructure.Message.SensorMessages;
using Carebed.Infrastructure.MessageEnvelope;
using Carebed.Managers;
using Carebed.Models.Sensors;
using Carebed.Infrastructure.Message.AlertMessages;
using Carebed.Infrastructure.Message.Actuator;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Carebed.Infrastructure.Message.UI;

namespace Carebed.UI
{
    public partial class MainDashboard : Form 
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

            #region Fields and Properties
        
            // A reference to the event bus for publishing and subscribing to events.        
            private readonly IEventBus _eventBus;

            // A single AlertViewModel instance for databinding
            private AlertViewModel alertViewModel = new AlertViewModel();

            // A databinding source for alert banner
            private BindingSource alertBindingSource = new BindingSource();

            // Flag to indicate if alerts are paused
            private bool alertsPaused = false;

            // Sensor alert handlers
            private Action<MessageEnvelope<AlertActionMessage<SensorTelemetryMessage>>>? _alertHandlerSensorTelemetry;
            private Action<MessageEnvelope<AlertActionMessage<SensorStatusMessage>>>? _alertHandlerSensorStatus;
            private Action<MessageEnvelope<AlertActionMessage<SensorErrorMessage>>>? _alertHandlerSensorError;

            // Actuator alert handlers
            private Action<MessageEnvelope<AlertActionMessage<ActuatorTelemetryMessage>>>? _alertHandlerActuatorTelemetry;
            private Action<MessageEnvelope<AlertActionMessage<ActuatorStatusMessage>>>? _alertHandlerActuatorStatus;
            private Action<MessageEnvelope<AlertActionMessage<ActuatorErrorMessage>>>? _alertHandlerActuatorError;


        #endregion

        #region Windows Forms Elements

        #region Alert Banner

        private TableLayoutPanel alertBannerLayout = new TableLayoutPanel();
        private Panel alertBanner = new Panel();
        private Label alertBannerLabel = new Label();
        private PictureBox alertBannerIcon = new PictureBox();

        private TableLayoutPanel alertBannerTable;
        private Label alertBannerTimeTitle;
        private Label alertBannerSourceTitle;
        private Label alertBannerValueTitle;
        private Label alertBannerTimeValue;
        private Label alertBannerSourceValue;
        private Label alertBannerValueValue;
        private Label alertBannerSeverityTitle;
        private Label alertBannerSeverityValue;

        // Background colour for the alert banner
        private Color NoAlertsActiveColour = Color.Green;
        private Color ActiveAlertsColour = Color.Orange;
        private Color SevereAlertColour = Color.DarkRed;

        // Icons for alert banner
        Image NoActiveAlertsIcon = SystemIcons.Information.ToBitmap();
        Image AlertsActiveIcon = SystemIcons.Warning.ToBitmap();
        Image SevereAlertsIcon = SystemIcons.Error.ToBitmap();
        #endregion
        
        #region Tabs and Viewport

        private Panel tabsPanel;        

        private Button vitalsTabButton;
        private Button actuatorsTabButton;
        private Button logsTabButton;
        private Button settingsTabButton;

        private Panel mainViewportPanel;

        #endregion

        #region Alert Log Panel
        private ListView alertListView = new ListView();
        private Panel alertLogPanel = new Panel();
        private Label alertSourceLabel = new Label();
        private Label alertLabel = new Label();
        private Label alertCountLabel = new Label();

        private Panel pauseStatusIndicator;
        private Label pauseStatusLabel;

        private TableLayoutPanel alertLogContainer;
        private Button clearAlertsButton;
        private Button pauseAlertsButton;

        #endregion

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Constructor for MainDashboard that accepts an IEventBus instance.
        /// </summary>
        internal MainDashboard(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            InitializeComponent();
            InitializeAlertBanner();            
            InitializeTabsPanel();
            InitializeMainViewportPanel();
            InitializeAlertLogPanel();

            // Subscribe to single-click selection
            alertListView.MouseUp += AlertListView_MouseUp;

            // Setup the Alert Banner click event handlers
            AttachAlertBannerClickHandlers(alertBanner);

            // Set z-order: 0 = topmost, higher = further back
            this.Controls.SetChildIndex(alertBanner, this.Controls.Count - 1); // Topmost
            this.Controls.SetChildIndex(tabsPanel, this.Controls.Count - 2);
            this.Controls.SetChildIndex(mainViewportPanel, this.Controls.Count - 3);
            this.Controls.SetChildIndex(alertLogContainer, this.Controls.Count - 4);
        }

        /// <summary>
        /// Setup the alert banner UI components.
        /// </summary>
        private void InitializeAlertBanner()
        {
            // Create the table for the banner
            alertBannerTable = new TableLayoutPanel
            {
                ColumnCount = 4,
                RowCount = 2,
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Padding = new Padding(8, 0, 8, 0)
            };
            alertBannerTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            alertBannerTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            alertBannerTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            alertBannerTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            alertBannerTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
            alertBannerTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Column titles
            alertBannerTimeTitle = new Label
            {
                Text = "Time",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                ForeColor = Color.LightGray
            };
            alertBannerSourceTitle = new Label
            {
                Text = "Source",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                ForeColor = Color.LightGray
            };
            alertBannerValueTitle = new Label
            {
                Text = "Value",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                ForeColor = Color.LightGray
            };
            alertBannerSeverityTitle = new Label
            {
                Text = "Severity",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                ForeColor = Color.LightGray
            };

            // Value labels
            alertBannerTimeValue = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                ForeColor = Color.White
            };
            alertBannerSourceValue = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                ForeColor = Color.White
            };
            alertBannerValueValue = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                ForeColor = Color.White
            };
            alertBannerSeverityValue = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                ForeColor = Color.White
            };

            // Add to table
            alertBannerTable.Controls.Add(alertBannerTimeTitle, 0, 0);
            alertBannerTable.Controls.Add(alertBannerSourceTitle, 1, 0);
            alertBannerTable.Controls.Add(alertBannerValueTitle, 2, 0);
            alertBannerTable.Controls.Add(alertBannerSeverityTitle, 3, 0);
            alertBannerTable.Controls.Add(alertBannerTimeValue, 0, 1);
            alertBannerTable.Controls.Add(alertBannerSourceValue, 1, 1);
            alertBannerTable.Controls.Add(alertBannerValueValue, 2, 1);
            alertBannerTable.Controls.Add(alertBannerSeverityValue, 3, 1);

            // Setup alert banner panel
            alertBanner.Name = "AlertBanner";
            alertBanner.Height = 60;
            alertBanner.Dock = DockStyle.Top;
            alertBanner.BackColor = NoAlertsActiveColour;
            alertBanner.Controls.Clear();
            alertBanner.Controls.Add(alertBannerTable);

            // Add banner to form
            this.Controls.Add(alertBanner);
        }

        /// <summary>
        /// Setup the alert log panel UI components.
        /// </summary>
        private void InitializeAlertLogPanel()
        {
            // Create a label for the alert log title
            var alertLogTitleLabel = new Label
            {
                Text = "Alert Log",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 4),
                Height = 24
            };

            // Create a TableLayoutPanel to stack the title and the ListView
            var alertLogContentLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = Color.WhiteSmoke,
            };
            alertLogContentLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28)); // Title height
            alertLogContentLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // ListView takes the rest

            // Create a bordered panel for the ListView
            var alertListViewBorderPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0),
                BackColor = Color.White
            };

            // Configure alert count label (optional, if you want to show it)
            alertCountLabel.Dock = DockStyle.Top;
            alertCountLabel.TextAlign = ContentAlignment.MiddleLeft;
            alertCountLabel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            alertCountLabel.Text = "0";

            alertListView.Dock = DockStyle.Fill;
            alertListView.View = View.Details;
            alertListView.FullRowSelect = true;
            alertListView.Columns.Clear();
            alertListView.Columns.Add("Count", 80);
            alertListView.Columns.Add("Time", 160);
            alertListView.Columns.Add("Source", 180);
            alertListView.Columns.Add("Alert", 350);
            alertListView.Columns.Add("Severity", 100);

            // Add the ListView to the bordered panel
            alertListViewBorderPanel.Controls.Add(alertListView);

            // Add the title label and bordered ListView panel to the layout
            alertLogContentLayout.Controls.Add(alertLogTitleLabel, 0, 0);
            alertLogContentLayout.Controls.Add(alertListViewBorderPanel, 0, 1);

            // Alert log panel setup
            alertLogPanel.Name = "AlertLogPanel";
            alertLogPanel.Dock = DockStyle.Fill;
            alertLogPanel.Padding = new Padding(0);
            alertLogPanel.BackColor = Color.WhiteSmoke;
            alertLogPanel.Controls.Clear();
            alertLogPanel.Controls.Add(alertLogContentLayout);

            // Pause and Clear buttons
            clearAlertsButton = new Button
            {
                Text = "Clear",
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(72, 48),
                Margin = new Padding(8)
            };
            clearAlertsButton.FlatAppearance.BorderSize = 0;
            clearAlertsButton.Click += ClearAlertsButton_Click;

            pauseAlertsButton = new Button
            {
                Text = "Pause",
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(72, 48),
                Margin = new Padding(8)
            };
            pauseAlertsButton.FlatAppearance.BorderSize = 0;
            pauseAlertsButton.Click += (s, e) =>
            {
                PauseAlertsButton_Click(s, e);
                UpdatePauseStatusIndicator();
            };

            // Status label for clear button
            var clearStatusLabel = new Label
            {
                Text = "Clear Alerts",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Margin = new Padding(0, 0, 8, 0)
            };

            // Status label and indicator for pause button
            pauseStatusLabel = new Label
            {
                Text = "Alerts On",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Margin = new Padding(0, 0, 8, 0)
            };
            pauseStatusIndicator = new Panel
            {
                Size = new Size(16, 16),
                BackColor = Color.Green,
                Margin = new Padding(0, 0, 8, 0)
            };

            // Row for clear button and label
            var clearRowPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(0, 0, 0, 8)
            };
            clearAlertsButton.Anchor = AnchorStyles.Left;
            clearStatusLabel.Anchor = AnchorStyles.Left;
            clearStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            clearRowPanel.Controls.Add(clearAlertsButton);
            clearRowPanel.Controls.Add(clearStatusLabel);

            // Row for pause button, label, and indicator
            var pauseRowPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                WrapContents = false,
                Margin = new Padding(0)
            };
            pauseAlertsButton.Anchor = AnchorStyles.Left;
            pauseStatusLabel.Anchor = AnchorStyles.Left;
            pauseStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            pauseStatusIndicator.Anchor = AnchorStyles.Left;
            pauseRowPanel.Controls.Add(pauseAlertsButton);
            pauseRowPanel.Controls.Add(pauseStatusLabel);
            pauseRowPanel.Controls.Add(pauseStatusIndicator);

            // Main button panel (vertical stack)
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                Padding = new Padding(16, 16, 16, 16),
                WrapContents = false
            };
            buttonPanel.Controls.Add(clearRowPanel);
            buttonPanel.Controls.Add(pauseRowPanel);

            // Alert log panel setup (add buttonPanel to the right)
            alertLogPanel.Controls.Clear();
            alertLogPanel.Controls.Add(alertLogContentLayout);
            alertLogPanel.Controls.Add(buttonPanel);

            // Container for log and buttons
            alertLogContainer = new TableLayoutPanel
            {
                Name = "AlertLogContainer",
                Dock = DockStyle.Bottom,
                Height = 220,
                Padding = new Padding(4),
                BackColor = Color.WhiteSmoke,
                ColumnCount = 1,
                RowCount = 1,
            };
            alertLogContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            alertLogContainer.Controls.Add(alertLogPanel, 0, 0);

            // Add container to form
            this.Controls.Add(alertLogContainer);
        }

        /// <summary>
        /// Setup the tabs panel UI components.
        /// </summary>
        private void InitializeTabsPanel()
        {
            tabsPanel = new Panel
            {
                Name = "TabsPanel",
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.LightGray
            };

            vitalsTabButton = new Button
            {
                Text = "Vitals",
                Width = 120,
                Dock = DockStyle.Left
            };
            actuatorsTabButton = new Button
            {
                Text = "Actuators",
                Width = 120,
                Dock = DockStyle.Left
            };
            logsTabButton = new Button
            {
                Text = "Logs",
                Width = 120,
                Dock = DockStyle.Left
            };
            settingsTabButton = new Button
            {
                Text = "Settings",
                Width = 120,
                Dock = DockStyle.Left
            };

            // Add buttons to panel (reverse order for DockStyle.Left)
            tabsPanel.Controls.Add(settingsTabButton);
            tabsPanel.Controls.Add(logsTabButton);
            tabsPanel.Controls.Add(actuatorsTabButton);
            tabsPanel.Controls.Add(vitalsTabButton);

            this.Controls.Add(tabsPanel);
        }

        /// <summary>
        /// Setup the main viewport panel UI components.
        /// </summary>
        private void InitializeMainViewportPanel()
        {
            mainViewportPanel = new Panel
            {
                Name = "MainViewportPanel",
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            this.Controls.Add(mainViewportPanel);
        }

        /// <summary>
        /// A override for the OnLoad event to perform additional initialization.
        /// </summary>
        /// <remarks> Can be used to subscribe to events or perform other setup tasks. </remarks>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Bind the label text to AlertMessage.Text
            AlertViewModel emptyBinding = new AlertViewModel()
            {
                AlertText = "",
                IsCritical = false
            };
            // Setup data binding for alert banner
            alertBindingSource.DataSource = alertViewModel;
            alertLabel.DataBindings.Add("Text", alertBindingSource, "AlertText");

            // Sensor message handlers
            _alertHandlerSensorTelemetry = HandleAlertActionForSensor<SensorTelemetryMessage>;
            _alertHandlerSensorStatus = HandleAlertActionForSensor<SensorStatusMessage>;
            _alertHandlerSensorError = HandleAlertActionForSensor<SensorErrorMessage>;

            // Actuator message handlers
            _alertHandlerActuatorTelemetry = HandleAlertActionForActuator<ActuatorTelemetryMessage>;
            _alertHandlerActuatorStatus = HandleAlertActionForActuator<ActuatorStatusMessage>;
            _alertHandlerActuatorError = HandleAlertActionForActuator<ActuatorErrorMessage>;

            // Register sensor handlers with event bus
            _eventBus.Subscribe(_alertHandlerSensorTelemetry);
            _eventBus.Subscribe(_alertHandlerSensorStatus);
            _eventBus.Subscribe(_alertHandlerSensorError);

            // Register actuator handlers with event bus
            _eventBus.Subscribe(_alertHandlerActuatorTelemetry);
            _eventBus.Subscribe(_alertHandlerActuatorStatus);
            _eventBus.Subscribe(_alertHandlerActuatorError);

            // Attach tab button click handlers
            vitalsTabButton.Click += VitalsTabButton_Click;
            actuatorsTabButton.Click += ActuatorsTabButton_Click;
            logsTabButton.Click += LogsTabButton_Click;
            settingsTabButton.Click += SettingsTabButton_Click;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for when the user clicks the AlertBanner.
        /// </summary>
        private void AlertBanner_Click(object? sender, EventArgs e)
        {
            // Find the matching alert in the log
            foreach (ListViewItem item in alertListView.Items)
            {
                // item.SubItems[2] = Source, item.SubItems[3] = Alert Text
                if (item.SubItems.Count >= 4 &&
                    item.SubItems[2].Text == alertViewModel.Source &&
                    item.SubItems[3].Text == alertViewModel.AlertText)
                {
                    // Build the details string as in AlertListView_MouseUp
                    string count = item.SubItems[0].Text;
                    string time = item.SubItems[1].Text;
                    string source = item.SubItems[2].Text;
                    string alert = item.SubItems[3].Text;
                    string severity = item.SubItems[4].Text;

                    // Configure details string
                    var details = $"Count: {count}\nTime: {time}\nSource: {source}\nAlert Value: {alert}\nSeverity: {severity}";

                    // Show popup
                    var result = ShowAlertPopup(
                        details + "\n\nClear this alert?",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Information
                    );
                    if (result == DialogResult.OK)
                    {
                        RunOnUiThread(() => RemoveAlertListViewItemAndUpdate(item));
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Handles the click event for the "Clear Alerts" button.
        /// </summary>
        /// <remarks>This method clears all items from the alert list, updates the alert count, and
        /// displays a default message indicating that there are no active alerts.</remarks>
        /// <param name="sender">The source of the event, typically the "Clear Alerts" button.</param>
        /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
        private void ClearAlertsButton_Click(object? sender, EventArgs e)
        {
            alertListView.Items.Clear();
            UpdateAlertCount();
            ShowAlert(new AlertViewModel { AlertText = "No active alerts", IsCritical = false, Source = "" });
        }

        /// <summary>
        /// Handles the click event for the "Pause Alerts" button.
        /// </summary>
        /// <param name="sender">The source of the event, typically the "Pause Alerts" button.</param>
        /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
        private void PauseAlertsButton_Click(object? sender, EventArgs e)
        {
            alertsPaused = !alertsPaused;
            if (alertsPaused)
            {
                // Unsubscribe from alert handlers
                if (_alertHandlerSensorTelemetry != null) _eventBus.Unsubscribe(_alertHandlerSensorTelemetry);
                if (_alertHandlerSensorStatus != null) _eventBus.Unsubscribe(_alertHandlerSensorStatus);
                if (_alertHandlerSensorError != null) _eventBus.Unsubscribe(_alertHandlerSensorError);
                if (_alertHandlerActuatorTelemetry != null) _eventBus.Unsubscribe(_alertHandlerActuatorTelemetry);
                if (_alertHandlerActuatorStatus != null) _eventBus.Unsubscribe(_alertHandlerActuatorStatus);
                if (_alertHandlerActuatorError != null) _eventBus.Unsubscribe(_alertHandlerActuatorError);
                RunOnUiThread(() =>
                {
                    pauseAlertsButton.Text = "Resume Alerts";
                    //pauseAlertsButton.BackColor = Color.Red; // Not-active
                    UpdatePauseStatusIndicator();
                });
                
            }
            else
            {
                // Re-subscribe to alert handlers
                if (_alertHandlerSensorTelemetry != null) _eventBus.Subscribe(_alertHandlerSensorTelemetry);
                if (_alertHandlerSensorStatus != null) _eventBus.Subscribe(_alertHandlerSensorStatus);
                if (_alertHandlerSensorError != null) _eventBus.Subscribe(_alertHandlerSensorError);
                if (_alertHandlerActuatorTelemetry != null) _eventBus.Subscribe(_alertHandlerActuatorTelemetry);
                if (_alertHandlerActuatorStatus != null) _eventBus.Subscribe(_alertHandlerActuatorStatus);
                if (_alertHandlerActuatorError != null) _eventBus.Subscribe(_alertHandlerActuatorError);
                RunOnUiThread(() =>
                {
                    pauseAlertsButton.Text = "Pause Alerts";
                    //pauseAlertsButton.BackColor = Color.Green; // Active
                    UpdatePauseStatusIndicator();
                });
            }
        }

        /// <summary>
        /// Handles the click event for the "Vitals" tab button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VitalsTabButton_Click(object? sender, EventArgs e)
        {
            mainViewportPanel.BackColor = Color.LightSkyBlue; // Example color for Vitals
        }

        /// <summary>
        /// Handles the click event for the "Actuators" tab button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ActuatorsTabButton_Click(object? sender, EventArgs e)
        {
            mainViewportPanel.BackColor = Color.LightGreen; // Example color for Actuators
        }

        /// <summary>
        /// Handles the click event for the "Logs" tab button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogsTabButton_Click(object? sender, EventArgs e)
        {
            mainViewportPanel.BackColor = Color.LightYellow; // Example color for Logs
        }

        /// <summary>
        /// Handles the click event for the "Settings" tab button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsTabButton_Click(object? sender, EventArgs e)
        {
            // 1. Clear the panel
            mainViewportPanel.Controls.Clear();
            mainViewportPanel.BackColor = Color.DarkGray;

            
            Label titleLabel = new Label
            {
                Text = "Settings",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(40, 30)
            };

            
            Label pollingLabel = new Label
            {
                Text = "Adjust Polling Rate (0-60s):",
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                AutoSize = true,
                // Y=100 puts it comfortably below the Title
                Location = new Point(40, 100)
            };

            NumericUpDown pollingInput = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 60,
                Increment = 1,
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                Width = 80,
                
                Location = new Point(350, 96)
            };

            Button saveButton = new Button
            {
                Text = "Apply Settings",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(180, 45),
                BackColor = Color.LightGray, // Disabled initially
                ForeColor = Color.DarkGray,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Default,
                Enabled = false,
                // MOVED UP: Directly below the Polling Rate line
                Location = new Point(40, 150)
            };

            Button powerOffButton = new Button
            {
                Text = "Shut Down System",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(180, 45),
                BackColor = Color.Crimson,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                
                Location = new Point(40, 230)
            };

            powerOffButton.Click += (s, args) =>
            {
                if (MessageBox.Show("Are you sure you want to shut down?", "Power Off",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    Application.Exit();
                }
            };






            mainViewportPanel.Controls.Add(powerOffButton);
            mainViewportPanel.Controls.Add(titleLabel);
            mainViewportPanel.Controls.Add(pollingLabel);
            mainViewportPanel.Controls.Add(pollingInput);
            mainViewportPanel.Controls.Add(saveButton);
        }

        /// <summary>
        /// Handle AlertActionMessage for sensor payloads - show popup.
        /// </summary>
        private void HandleAlertActionForSensor<TPayload>(MessageEnvelope<AlertActionMessage<TPayload>> envelope)
           where TPayload : SensorMessageBase
        {
            var msg = envelope.Payload;
            if (msg == null) return;

            // Use the message's CreatedAt if available, otherwise use DateTime.Now
            string alertTime = (msg.CreatedAt != default)
                ? msg.CreatedAt.ToString("MMM. dd HH:mm:ss")
                : DateTime.Now.ToString("MMM. dd HH:mm:ss");

            AlertViewModel avm = new AlertViewModel
            {
                AlertText = msg.AlertText,
                IsCritical = msg.Payload?.IsCritical ?? false,
                Source = msg.Source,
                Time = alertTime
            };

            RunOnUiThread(() =>
            {
                ShowAlert(avm);

                //// Generate alert entry for log
                string alertCount = (alertListView.Items.Count + 1).ToString();
                string source = msg.Payload?.SensorID ?? "Unknown";
                string alertText = msg.AlertText;
                string severity = msg.Payload?.IsCritical == true ? "Critical" : "Normal";

                // Create ListViewItem
                var item = new ListViewItem(alertCount);

                // Add subitems: Source, Severity, Alert Text
                item.SubItems.Add(alertTime);
                item.SubItems.Add(source);                
                item.SubItems.Add(alertText);
                item.SubItems.Add(severity);

                // Insert at the top
                alertListView.Items.Insert(0, item);
                UpdateAlertCount();
            });
           
        }

        /// <summary>
        /// Handle AlertActionMessage for actuator payloads - show popup.
        /// </summary>
        private void HandleAlertActionForActuator<TPayload>(MessageEnvelope<AlertActionMessage<TPayload>> envelope)
            where TPayload : ActuatorMessageBase
        {
            var msg = envelope.Payload;
            if (msg == null) return;

            var entry = new AlertEntry
            {
                Source = msg.Source,
                AlertText = msg.AlertText,
                Payload = msg.Payload,
                IsCritical = msg.Payload?.IsCritical ?? false
            };
        }

        /// <summary>
        /// Shows an alert in the alert banner.
        /// </summary>
        /// <param name="alert"></param>
        private void ShowAlert(AlertViewModel alert, string? time = null)
        {
            alertViewModel.AlertText = alert.AlertText;
            alertViewModel.IsCritical = alert.IsCritical;
            alertViewModel.Source = alert.Source;
            alertViewModel.Time = alert.Time;

            alertBanner = this.Controls["AlertBanner"] as Panel;
            if (alertBanner != null)
            {
                if (string.IsNullOrWhiteSpace(alert.AlertText) || alert.AlertText == "No active alerts")
                {
                    alertBanner.BackColor = NoAlertsActiveColour;
                    alertBannerSeverityValue.Text = "";
                }
                else if (alert.IsCritical)
                {
                    alertBanner.BackColor = SevereAlertColour;
                    alertBannerSeverityValue.Text = "Critical";
                }
                else
                {
                    alertBanner.BackColor = ActiveAlertsColour;
                    alertBannerSeverityValue.Text = "Normal";
                }
            }

            string displayTime = time ?? alert.Time ?? DateTime.Now.ToString("MMM. dd HH:mm:ss");
            alertBannerTimeValue.Text = displayTime;
            alertBannerSourceValue.Text = alert.Source ?? "";
            alertBannerValueValue.Text = alert.AlertText ?? "";
        }

        #endregion

        /// <summary>
        /// Update the pause status indicator based on the current alert pause state.
        /// </summary>
        private void UpdatePauseStatusIndicator()
        {
            if (alertsPaused)
            {
                pauseStatusLabel.Text = "Alerts Off";
                pauseStatusIndicator.BackColor = Color.Red;
            }
            else
            {
                pauseStatusLabel.Text = "Alerts On";
                pauseStatusIndicator.BackColor = Color.Green;
            }
        }

        /// <summary>
        /// Shows a popup dialog with alert details.
        /// </summary>
        private DialogResult ShowAlertPopup(string details, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            using (var popup = new SingleAlertPopup(details, buttons, icon))
            {
                popup.StartPosition = FormStartPosition.CenterParent;
                return popup.ShowDialog(this);
            }
        }

        /// <summary>
        /// Handler for mouse up event on the alert list view.
        /// </summary>
        private void AlertListView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;

            var info = alertListView.HitTest(e.Location);
            if (info.Item != null)
            {
                var item = info.Item;
                string count = item.SubItems[0].Text;
                string time = item.SubItems[1].Text;
                string source = item.SubItems[2].Text;
                string alert = item.SubItems[3].Text;
                string severity = item.SubItems[4].Text;

                var details = $"Count: {count}\nTime: {time}\nSource: {source}\nAlert Value: {alert}\nSeverity: {severity}";

                var result = ShowAlertPopup(
                    details + "\n\nClear this alert?",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Information
                );

                if (result == DialogResult.OK)
                {
                    RunOnUiThread(() => RemoveAlertListViewItemAndUpdate(item));
                }
            }
        }

        /// <summary>
        /// Removes the specified ListViewItem from the alert list and updates the alert banner accordingly.
        /// </summary>
        /// <param name="item"></param>
        private void RemoveAlertListViewItemAndUpdate(ListViewItem item)
        {
            alertListView.Items.Remove(item);
            UpdateAlertCount();

            if (alertListView.Items.Count == 0)
            {
                ShowAlert(new AlertViewModel { AlertText = "No active alerts", IsCritical = false, Source = "" }, null);
            }
            else
            {
                var nextItem = alertListView.Items[0];
                bool isCritical = nextItem.SubItems.Count > 4 && nextItem.SubItems[4].Text.Equals("Critical", StringComparison.OrdinalIgnoreCase);
                ShowAlert(
                    new AlertViewModel
                    {
                        AlertText = nextItem.SubItems[3].Text, // Value
                        Source = nextItem.SubItems[2].Text,    // Source
                        IsCritical = isCritical
                    },
                    nextItem.SubItems[1].Text // Time
                );
            }
        }

        /// <summary>
        /// Updates the alert count label based on the number of items in the alert list view.
        /// </summary>
        private void UpdateAlertCount()
        {
            alertCountLabel.Text = $"{alertListView.Items.Count}";
        }

        /// <summary>
        /// Attach click handlers to all controls in the alert banner for interactivity.
        /// </summary>
        /// <param name="control"></param>
        private void AttachAlertBannerClickHandlers(Control control)
        {
            control.Click += AlertBanner_Click;
            foreach (Control child in control.Controls)
            {
                AttachAlertBannerClickHandlers(child);
            }
        }

        /// <summary>
        /// Helper to run an action on the UI thread.
        /// </summary>
        /// <param name="action"></param>
        private void RunOnUiThread(Action action)
        {
            if (action == null) return;
            if (InvokeRequired)
                BeginInvoke(action);
            else
                action();
        }

        /// <summary>
        /// Unsubscribe from event bus when the form is closed.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (_alertHandlerSensorTelemetry != null) _eventBus.Unsubscribe(_alertHandlerSensorTelemetry);
            if (_alertHandlerSensorStatus != null) _eventBus.Unsubscribe(_alertHandlerSensorStatus);
            if (_alertHandlerSensorError != null) _eventBus.Unsubscribe(_alertHandlerSensorError);

            if (_alertHandlerActuatorTelemetry != null) _eventBus.Unsubscribe(_alertHandlerActuatorTelemetry);
            if (_alertHandlerActuatorStatus != null) _eventBus.Unsubscribe(_alertHandlerActuatorStatus);
            if (_alertHandlerActuatorError != null) _eventBus.Unsubscribe(_alertHandlerActuatorError);

            base.OnFormClosed(e);
        }

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            // Configure the main form
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.Text = "Carebed Dashboard";
        }

        #endregion

        
    }

    public class CircularButton : Control
    {
        public Color NormalColor { get; set; } = Color.LightGray;
        public Color HoverColor { get; set; } = Color.DodgerBlue;
        public Color PressedColor { get; set; } = Color.DeepSkyBlue;
        public Color ToggledColor { get; set; } = Color.OrangeRed;
        public bool IsToggled { get; set; } = false;
        public string ToolTipText { get; set; } = string.Empty;

        private bool _hovered = false;
        private bool _pressed = false;

        public CircularButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            Size = new Size(48, 48);
            Cursor = Cursors.Hand;
            TabStop = true;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Color fill = NormalColor;
            if (IsToggled)
                fill = ToggledColor;
            else if (_pressed)
                fill = PressedColor;
            else if (_hovered)
                fill = HoverColor;

            using (var g = pevent.Graphics)
            using (var brush = new SolidBrush(fill))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.FillEllipse(brush, 0, 0, Width - 1, Height - 1);
                using (var pen = new Pen(Color.Gray, 2))
                    g.DrawEllipse(pen, 0, 0, Width - 1, Height - 1);
                g.DrawString(Text, Font, Brushes.White, ClientRectangle, sf);
            }
        }

        protected override void OnMouseEnter(EventArgs e) { _hovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hovered = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs mevent) { _pressed = true; Invalidate(); base.OnMouseDown(mevent); }
        protected override void OnMouseUp(MouseEventArgs mevent) { _pressed = false; Invalidate(); base.OnMouseUp(mevent); }
        protected override void OnResize(EventArgs e) { base.OnResize(e); Width = Height; Invalidate(); }

        // Optional: Raise a Click event
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
        }
    }
}
