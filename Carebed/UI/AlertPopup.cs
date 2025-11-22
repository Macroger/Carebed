using Carebed.Infrastructure.EventBus;
using Carebed.Infrastructure.Message.AlertMessages;
using Carebed.Infrastructure.MessageEnvelope;
using Carebed.Infrastructure.Message;
using System.Windows.Forms;
using System.Drawing;

namespace Carebed.UI
{
    /// <summary>
    /// Popup window that aggregates alerts and displays them modally. When closed, publishes AlertClearMessage for each alert.
    /// </summary>
    public class AlertPopup : Form
    {
        private readonly IEventBus _eventBus;

        private readonly List<AlertEntry> _alerts = new();

        private ListBox alertsListBox;
        private Button closeButton;

        public AlertPopup(IEventBus eventBus, IEnumerable<AlertEntry> alerts)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));

            _alerts.AddRange(alerts);

            InitializeComponent();

            this.FormClosed += AlertPopup_FormClosed;
        }

        private void InitializeComponent()
        {
            this.Text = "Active Alerts";
            this.StartPosition = FormStartPosition.CenterParent;
            this.TopMost = true;
            this.Size = new System.Drawing.Size(500, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            alertsListBox = new ListBox();
            alertsListBox.Location = new System.Drawing.Point(12, 12);
            alertsListBox.Size = new System.Drawing.Size(460, 200);
            alertsListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Use owner-draw so we can color critical alerts red
            alertsListBox.DrawMode = DrawMode.OwnerDrawFixed;
            alertsListBox.DrawItem += AlertsListBox_DrawItem;

            foreach (var a in _alerts)
            {
                alertsListBox.Items.Add(a);
            }

            closeButton = new Button();
            closeButton.Text = "Acknowledge All";
            closeButton.Size = new System.Drawing.Size(120, 30);
            closeButton.Location = new System.Drawing.Point((this.ClientSize.Width - closeButton.Width) / 2, 220);
            closeButton.Anchor = AnchorStyles.Bottom;
            closeButton.Click += (s, e) => this.Close();

            this.Controls.Add(alertsListBox);
            this.Controls.Add(closeButton);
        }

        private void AlertsListBox_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            var item = alertsListBox.Items[e.Index] as AlertEntry;
            if (item == null)
            {
                e.DrawFocusRectangle();
                return;
            }

            var prefix = item.IsCritical ? "[CRITICAL] " : string.Empty;
            var text = $"{prefix}[{item.Source}] {item.AlertText}";
            var color = item.IsCritical ? Color.Red : SystemColors.WindowText;

            using (var brush = new SolidBrush(color))
            {
                e.Graphics.DrawString(text, e.Font, brush, e.Bounds.X, e.Bounds.Y);
            }

            e.DrawFocusRectangle();
        }

        private void AlertPopup_FormClosed(object? sender, FormClosedEventArgs e)
        {
            try
            {
                // Publish an AlertClearMessage for each alert so AlertManager can match/acknowledge them
                foreach (var a in _alerts)
                {
                    var clear = new AlertClearMessage<IEventMessage>
                    {
                        Source = a.Source,
                        Payload = a.Payload
                    };

                    var envelope = new MessageEnvelope<AlertClearMessage<IEventMessage>>(clear, Infrastructure.Enums.MessageOrigins.DisplayManager, Infrastructure.Enums.MessageTypes.Alert);

                    // Fire-and-forget publish
                    _ = _eventBus.PublishAsync(envelope);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to publish alert clear: {ex}");
            }
        }
    }
}
