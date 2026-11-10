using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OP2StreamingTools
{
    /// <summary>
    /// Simple scrolling debug log window. Displays timestamped messages from the
    /// rest of the app and caps its own size to avoid unbounded memory growth.
    /// </summary>
    public partial class fLog : Form
    {
        public fLog()
        {
            InitializeComponent();
        }

        private void fLog_Load(object sender, EventArgs e)
        {
            this.Text = "OP2 Debug Log";
            this.Size = new Size(800, 500);
            this.BackColor = Color.FromArgb(0, 16, 0); // Dark green background

            // Configure the text box if it exists
            if (txtLog != null)
            {
                txtLog.BackColor = Color.FromArgb(0, 32, 0);
                txtLog.ForeColor = Color.Lime;
                txtLog.Font = new Font("Consolas", 9, FontStyle.Regular);
                txtLog.Multiline = true;
                txtLog.ScrollBars = ScrollBars.Vertical;
                txtLog.WordWrap = true;
                txtLog.ReadOnly = true;
                txtLog.Dock = DockStyle.Fill;
            }

            // Add initial message
            AppendLog("OP2StreamingTools Debug Log Started");
            AppendLog("================================");
        }

        /// <summary>
        /// Appends a timestamped message to the log, auto-scrolls to the bottom,
        /// and trims old lines once the log grows past ~1000 lines.
        /// Thread-safe: marshals back to the UI thread if called from another one.
        /// </summary>
        public void AppendLog(string message)
        {
            if (txtLog.InvokeRequired)
            {
                // Thread-safe call
                txtLog.Invoke(new Action<string>(AppendLog), message);
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string logEntry = $"[{timestamp}] {message}";

            txtLog.AppendText(logEntry + Environment.NewLine);

            // Auto-scroll to bottom
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();

            // Optional: Limit log size to prevent memory issues
            if (txtLog.Lines.Length > 1000)
            {
                // Keep only the last 800 lines
                var lines = txtLog.Lines.Skip(200).ToArray();
                txtLog.Clear();
                txtLog.Lines = lines;
            }
        }

        /// <summary>
        /// Overload that appends a formatted message (string.Format style).
        /// </summary>
        public void AppendLog(string format, params object[] args)
        {
            AppendLog(string.Format(format, args));
        }

        /// <summary>
        /// Clears all text from the log.
        /// </summary>
        public void ClearLog()
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(ClearLog));
                return;
            }

            txtLog.Clear();
            AppendLog("Log cleared");
        }

        /// <summary>
        /// Appends a horizontal separator line to the log.
        /// </summary>
        public void AddSeparator()
        {
            AppendLog("--------------------------------");
        }

        // Convenience wrappers that prefix the message with a severity tag.
        public void LogInfo(string message)
        {
            AppendLog($"[INFO] {message}");
        }

        public void LogWarning(string message)
        {
            AppendLog($"[WARN] {message}");
        }

        public void LogError(string message)
        {
            AppendLog($"[ERROR] {message}");
        }

        public void LogDebug(string message)
        {
            AppendLog($"[DEBUG] {message}");
        }
    }
}