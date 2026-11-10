using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization;
using System.Windows.Forms.DataVisualization.Charting;

namespace OP2StreamingTools
{
    /// <summary>
    /// Live graph window. Periodically pulls the morale, colonists, and game-mark
    /// history from the main form and plots them — morale on the primary Y axis,
    /// total colonists on the secondary Y axis, against game mark on the X axis.
    /// </summary>
    public partial class fGraphs : Form
    {
        private Chart moraleChart;          // The chart control hosting both data series
        private fMain mainForm;             // Source of the history data
        private Timer updateTimer;          // Periodically refreshes the chart

        /// <summary>
        /// Stores the main form (data source) and builds the chart and refresh timer.
        /// </summary>
        /// <param name="parent">The main overlay form that owns the tracked history.</param>
        public fGraphs(fMain parent)
        {
            InitializeComponent();
            mainForm = parent;
            SetupChart();
            SetupTimer();
        }

        private void fGraphs_Load(object sender, EventArgs e)
        {
            this.Text = "OP2 Colony Graph";
            this.Size = new Size(800, 600);
            this.BackColor = Color.FromArgb(0, 16, 0); // Dark green background like main form

            // Initial data load
            UpdateGraph();
        }

        /// <summary>
        /// Builds the chart: the dark-green chart area, both Y axes, and the
        /// morale and colonists line series with their styling.
        /// </summary>
        private void SetupChart()
        {
            // Create and configure the chart
            moraleChart = new Chart();
            moraleChart.Dock = DockStyle.Fill;
            moraleChart.BackColor = Color.FromArgb(0, 16, 0);

            // Create chart area
            ChartArea chartArea = new ChartArea("MoraleArea");
            chartArea.BackColor = Color.FromArgb(0, 32, 0);
            chartArea.AxisX.LabelStyle.ForeColor = Color.Lime;
            chartArea.AxisY.LabelStyle.ForeColor = Color.Lime;
            chartArea.AxisX.LineColor = Color.Lime;
            chartArea.AxisY.LineColor = Color.Lime;
            chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(0, 64, 0);
            chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(0, 64, 0);
            chartArea.AxisX.Title = "Game Mark";
            chartArea.AxisY.Title = "Morale";
            chartArea.AxisX.TitleForeColor = Color.Lime;
            chartArea.AxisY.TitleForeColor = Color.Lime;

            // Secondary Y axis for colonists (different scale from morale)
            chartArea.AxisY2.Title = "Total Colonists";
            chartArea.AxisY2.TitleForeColor = Color.Cyan;
            chartArea.AxisY2.LabelStyle.ForeColor = Color.Cyan;
            chartArea.AxisY2.LineColor = Color.Cyan;
            chartArea.AxisY2.MajorGrid.Enabled = false;
            chartArea.AxisY2.Enabled = AxisEnabled.True;

            moraleChart.ChartAreas.Add(chartArea);

            // Create series for morale data
            Series moraleSeries = new Series("Morale");
            moraleSeries.ChartType = SeriesChartType.Line;
            moraleSeries.Color = Color.Lime;
            moraleSeries.BorderWidth = 2;
            moraleSeries.MarkerStyle = MarkerStyle.Circle;
            moraleSeries.MarkerSize = 4;
            moraleSeries.MarkerColor = Color.Yellow;

            moraleChart.Series.Add(moraleSeries);

            // Create series for colonists data (plotted against secondary Y axis)
            Series colonistsSeries = new Series("Total Colonists");
            colonistsSeries.ChartType = SeriesChartType.Line;
            colonistsSeries.Color = Color.Cyan;
            colonistsSeries.BorderWidth = 2;
            colonistsSeries.MarkerStyle = MarkerStyle.Circle;
            colonistsSeries.MarkerSize = 4;
            colonistsSeries.MarkerColor = Color.White;
            colonistsSeries.YAxisType = AxisType.Secondary;

            moraleChart.Series.Add(colonistsSeries);

            // Add legend
            Legend legend = new Legend();
            legend.ForeColor = Color.Lime;
            legend.BackColor = Color.Transparent;
            moraleChart.Legends.Add(legend);

            // Add chart to form
            this.Controls.Add(moraleChart);
        }

        /// <summary>
        /// Starts the timer that refreshes the graph from the main form's data.
        /// </summary>
        private void SetupTimer()
        {
            // Timer to update graph every 500ms
            updateTimer = new Timer();
            updateTimer.Interval = 500;
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            UpdateGraph();
        }

        /// <summary>
        /// Pulls the latest history from the main form and redraws both series.
        /// Safe to call when no data exists yet — it simply returns early.
        /// </summary>
        private void UpdateGraph()
        {
            if (mainForm == null)
                return;

            // Get data from main form
            var moraleData = mainForm.GetMoraleHistory();
            var colonistsData = mainForm.GetColonistsHistory();
            var markData = mainForm.GetGameMarkHistory();

            // Log to the debug form
            //mainForm.WriteLog($"Graph Update: Data points = {moraleData?.Count ?? 0}");

            if (moraleData == null || markData == null || moraleData.Count == 0)
                return;

            // Clear existing data
            moraleChart.Series["Morale"].Points.Clear();
            moraleChart.Series["Total Colonists"].Points.Clear();

            // Add data points
            for (int i = 0; i < moraleData.Count && i < markData.Count; i++)
            {
                moraleChart.Series["Morale"].Points.AddXY(markData[i], moraleData[i]);
            }

            // Add colonist data points
            if (colonistsData != null)
            {
                for (int i = 0; i < colonistsData.Count && i < markData.Count; i++)
                {
                    moraleChart.Series["Total Colonists"].Points.AddXY(markData[i], colonistsData[i]);
                }
            }

            // Auto-scale the chart
            if (moraleData.Count > 0)
            {
                moraleChart.ChartAreas["MoraleArea"].RecalculateAxesScale();
            }
        }
        /// <summary>
        /// Stops and disposes the refresh timer when the graph window closes.
        /// </summary>
        private void fGraphs_FormClosing(object sender, FormClosingEventArgs e)
        {
            updateTimer?.Stop();
            updateTimer?.Dispose();
        }
    }
}