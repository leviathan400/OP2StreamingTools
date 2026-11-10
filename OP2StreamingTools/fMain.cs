using OP2Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

// OP2StreamingTools
// https://github.com/leviathan400/OP2StreamingTools
// Overlay for recording/streaming Outpost 2
//
//
// Outpost 2: Divided Destiny is a real-time strategy video game released in 1997.

namespace OP2StreamingTools
{
    /// <summary>
    /// Main overlay window. Polls the running Outpost 2 process for live game state
    /// (game tick/mark, morale, colonists), tracks that data over time for graphing,
    /// and renders a compact always-on-top heads-up display for streaming/recording.
    /// </summary>
    public partial class fMain : Form
    {
        // WinAPI constants and function imports.
        // Used to drag the borderless form by forwarding clicks as title-bar hits.
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private Timer gameTimer;                // Drives the elapsed-time display (ticks while a game is running)
        private DateTime gameStartTime;         // Wall-clock time the current game started
        private bool gameRunning = false;       // Is a game currently being tracked?
        private string gameDuration;            // Formatted elapsed time shown in lblTimer

        private int gameTick;                   // Raw in-game tick counter read from memory
        private int gameMark;                   // Game mark, derived as gameTick / 100
        private int lastProcessedTick = 0;      // Tick value at the previous poll (detects pause/reset)
        private int lastProcessedMark = 0;      // Mark value at the previous poll (only record history when it advances)

        // Morale tracking variables
        private List<int> moraleHistory;        // Store morale values
        private List<int> colonistsHistory;     // Store colonist values
        private List<int> gameMarkHistory;      // Store corresponding game marks           
        private int morale = 0;                 // Current morale value
        private int totalColonists = 0;         // Current colonists value

        // Outpost 2 process tracking
        private int outpost2Pid = -1;           // PID of the detected Outpost2 process (-1 if not running)
        private bool outpost2Running = false;   // Is the Outpost2 process currently detected?

        private fLog logForm;                   // Separate debug log window

        /// <summary>
        /// Sets up the borderless draggable overlay, the elapsed-time timer,
        /// and the in-memory history collections used for graphing.
        /// </summary>
        public fMain()
        {
            InitializeComponent();

            lblTimer.DoubleClick += lblTimer_DoubleClick;

            // Optional: remove the title bar if you want a borderless form
            this.FormBorderStyle = FormBorderStyle.None;

            // Attach the MouseDown event to the form (or a panel/control if you prefer)
            this.MouseDown += fMain_MouseDown;

            gameTimer = new Timer();
            //gameTimer.Interval = 1000; // 1 second
            gameTimer.Interval = 100;
            gameTimer.Tick += GameTimer_Tick;

            // Initialize morale tracking collections
            moraleHistory = new List<int>();
            colonistsHistory = new List<int>();
            gameMarkHistory = new List<int>();
        }

        /// <summary>
        /// Applies the overlay styling, starts the polling timer, and opens the
        /// companion graph and log windows.
        /// </summary>
        private void fMain_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("--- OP2StreamingTools Started ---");
            this.Icon = Properties.Resources.documents;

            // Start the timer to update values every second
            updateTimer.Interval = 500; 
            updateTimer.Start();

            // Initial update
            UpdateValues();

            lblStatus.Text = "OP2StreamingTools";
            lblStatus.Visible = false;
            btnStart.Enabled = false;
            btnStart.Visible = false;

            this.TopMost = true;
            this.Width = 135;
            this.Height = 105;

            this.BackColor = System.Drawing.ColorTranslator.FromHtml("#001000");
            lblGameMark.ForeColor = System.Drawing.ColorTranslator.FromHtml("#00FF00");
            lblMorale.ForeColor = System.Drawing.ColorTranslator.FromHtml("#00FF00");
            lblTotalColonists.ForeColor = System.Drawing.ColorTranslator.FromHtml("#00FF00");
            lblTimer.ForeColor = System.Drawing.ColorTranslator.FromHtml("#00FF00");

            chkGameMark.Text = "Game Mark";
            chkMorale.Text = "Morale";
            chkTotalColonists.Text = "Total Colonists";

            ResetUI();
                
            fGraphs graphsForm = new fGraphs(this);
            graphsForm.Show();

            ShowLogForm();

            //SimulateHighMarkData();
        }

        /// <summary>
        /// Opens the debug log window, or brings the existing one to the front.
        /// </summary>
        private void ShowLogForm()
        {
            if (logForm == null || logForm.IsDisposed)
            {
                logForm = new fLog();
                logForm.Show();
            }
            else
            {
                logForm.BringToFront();
            }
        }

        /// <summary>
        /// Writes a message to the debug log window (if open) and to the debug output.
        /// </summary>
        public void WriteLog(string message)
        {
            if (logForm != null && !logForm.IsDisposed)
            {
                logForm.AppendLog(message);
            }

            // Also keep the debug output
            System.Diagnostics.Debug.WriteLine(message);
        }

        /// <summary>
        /// Lets the user drag the borderless window by clicking anywhere on it,
        /// by telling Windows to treat the click as a title-bar drag.
        /// </summary>
        private void fMain_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }


        private void fMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Stop the timer when closing the form
            updateTimer.Stop();
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            UpdateValues();
        }

        /// <summary>
        /// Resets the on-screen labels to their zero state and clears tracked game data.
        /// </summary>
        private void ResetUI()
        {
            lblTimer.Text = "00:00:00.0";
            lblGameMark.Text = "Current Mark: 0";
            lblMorale.Text = "Morale: 0";
            lblTotalColonists.Text = "Total Colonists: 0";

            ResetGameData();
        }

        /// <summary>
        /// Clears the morale/colonists/mark history and resets the current values.
        /// Called when a new game starts so graphs begin fresh.
        /// </summary>
        private void ResetGameData()
        {
            moraleHistory.Clear();
            colonistsHistory.Clear();
            gameMarkHistory.Clear();
            morale = 0;
            totalColonists = 0;
            //System.Diagnostics.Debug.WriteLine("Morale tracking data reset");
        }

        /// <summary>
        /// Detects whether Outpost 2 is running and tracks its PID. Logs launch/close
        /// transitions (and restarts under a new PID), and returns true while the
        /// process is available so the caller can decide whether to read memory.
        /// </summary>
        private bool CheckOutpost2Process()
        {
            Process[] processes = Process.GetProcessesByName("Outpost2");

            if (processes.Length == 0)
            {
                if (outpost2Running)
                {
                    // Was running, now gone
                    WriteLog($"Outpost 2 process closed. (PID was {outpost2Pid})");
                    outpost2Running = false;
                    outpost2Pid = -1;
                    StopGame();
                }
                return false;
            }

            int pid = processes[0].Id;

            // Release the Process objects we don't keep
            foreach (Process p in processes)
                p.Dispose();

            // Newly detected, or the game was restarted with a different PID
            if (!outpost2Running || pid != outpost2Pid)
            {
                outpost2Pid = pid;
                outpost2Running = true;
                WriteLog($"Outpost 2 detected. PID: {outpost2Pid}");
            }

            return true;
        }

        /// <summary>
        /// Core polling routine (runs on the update timer). Reads the latest game
        /// state from Outpost 2's memory, refreshes the overlay labels, detects
        /// game start/reset, and appends to the history when the game mark advances.
        /// </summary>
        private void UpdateValues()
        {
            bool processFound = false;

            // Skip the memory reads entirely when Outpost 2 isn't running.
            // This avoids throwing an exception on every poll while we wait for the game.
            if (!CheckOutpost2Process())
            {
                lblStatus.Text = "OP2StreamingTools - Waiting for Outpost 2...";
                return;
            }

            try
            {
                // Read all values from memory
                IntPtr absoluteAddress = (IntPtr)0x0056EB1C;
                gameTick = MemoryReader.ReadIntFromAbsoluteAddress("Outpost2", absoluteAddress);
                gameMark = gameTick / 100;      // Calculate game mark (tick / 100, no decimal places)

                //int commonOre1 = MemoryReader.ReadIntFromModuleOffset("Outpost2", 0x16EF3C);
                //int commonOre2 = MemoryReader.ReadIntFromModuleOffset("Outpost2", 0x175760);
                //int rareOre1 = MemoryReader.ReadIntFromModuleOffset("Outpost2", 0x16EF40);
                //int rareOre2 = MemoryReader.ReadIntFromModuleOffset("Outpost2", 0x175764);
                totalColonists = MemoryReader.ReadIntFromModuleOffset("Outpost2", 0x16EFFC);
                //int children = MemoryReader.ReadIntFromModuleOffset("Outpost2", 0x16EFB8);
                //int workers = MemoryReader.ReadIntFromModuleOffset("Outpost2", 0x16EFB0);
                //int scientists = MemoryReader.ReadIntFromModuleOffset("Outpost2", 0x16EFB4);
                morale = MemoryReader.ReadIntFromModuleOffset("Outpost2", 0x16EF5C);
                //int powerGen = MemoryReader.ReadIntFromModuleOffset("Outpost2", 0x16EFC8);
                //int powerUse = MemoryReader.ReadIntFromModuleOffset("Outpost2", 0x16EFD0);
                //int foodStorage = MemoryReader.ReadIntFromModuleOffset("Outpost2", 0x16EF2C);
                //int foodProduced = MemoryReader.ReadIntFromModuleOffset("Outpost2", 0x16EFF8);
                //int foodConsumed = MemoryReader.ReadIntFromModuleOffset("Outpost2", 0x16EFFC);

                //string loadedMission1 = MemoryReader.ReadStringFromModuleOffset("Outpost2", 0x166A62, 16);
                //string loadedMission2 = MemoryReader.ReadStringFromModuleOffset("Outpost2", 0x16EB4C, 16);

                //int commonOreCapacity = MemoryReader.ReadIntFromModuleOffset("Outpost2", 0x16EF34);
                //int rareOreCapacity = MemoryReader.ReadIntFromModuleOffset("Outpost2", 0x16EF38);

                // Update UI labels
                //lblCommonOre1.Text = $"Common Ore 1: {commonOre1:N0}";
                //lblCommonOre2.Text = $"Common Ore 2: {commonOre2:N0}";
                //lblRareOre1.Text = $"Rare Ore 1: {rareOre1:N0}";
                //lblRareOre2.Text = $"Rare Ore 2: {rareOre2:N0}";
                //lblCommonOreCapacity.Text = $"Common Ore Capacity: {commonOreCapacity:N0}";
                //lblRareOreCapacity.Text = $"Rare Ore Capacity: {rareOreCapacity:N0}";

                lblTotalColonists.Text = $"Total Colonists: {totalColonists:N0}";
                //lblChildren.Text = $"Children: {children:N0}";
                //lblWorkers.Text = $"Workers: {workers:N0}";
                //lblScientists.Text = $"Scientists: {scientists:N0}";
                lblMorale.Text = $"Morale: {morale:N0}";

                //lblPowerGen.Text = $"Power Generated: {powerGen:N0}";
                //lblPowerUse.Text = $"Power Consumed: {powerUse:N0}";

                //lblFoodStorage.Text = $"Food Storage: {foodStorage:N0}";
                //lblFoodProduced.Text = $"Food Produced: {foodProduced:N0}";
                //lblFoodConsumed.Text = $"Food Consumed: {foodConsumed:N0}";

                //lblGameTick.Text = $"Game Tick: {gameTick:N0}";
                lblGameMark.Text = $"Current Mark: {gameMark}";

                //lblLoadedMission1.Text = $"Loaded Mission 1: {loadedMission1}";
                //lblLoadedMission2.Text = $"Loaded Mission 2: {loadedMission2}";

                // Update form title with status
                lblStatus.Text = "OP2StreamingTools";

                if (gameMark < lastProcessedMark)
                {
                    // Game was reset
                    StopGame();
                    WriteLog("New Game. lastProcessedMark was: " + lastProcessedMark + " gameMark is now: " + gameMark);
                    //System.Diagnostics.Debug.WriteLine("New Game. lastProcessedMark was: " + lastProcessedMark + " gameMark is now: " + gameMark);
                    ResetUI();
                    lastProcessedMark = 0;
                    StartGame();
                }

                if (gameTick == lastProcessedTick)
                {
                    if (gameRunning == true)
                    {
                        System.Diagnostics.Debug.WriteLine("gameTick same.. Game is paused?");
                    }
                }
               
                // Only process when the game mark advances
                if (gameMark > lastProcessedMark)
                {
                    if (gameTick > 0)
                    {
                        //System.Diagnostics.Debug.WriteLine("gameTick: " + gameTick);

                        // Game has started.. gameTick is 1
                        if (gameMark == 1)
                        {
                            // We are in the gameMark 1
                            //System.Diagnostics.Debug.WriteLine("gameMark: " + gameMark);

                            if (gameRunning == false)
                            {
                                StartGame();
                            }

                         }
                    }

                    if (gameRunning == true)
                    {
                        // Save morale data every game tick (only when game is running and tick advances)

                        // Add current values to history
                        moraleHistory.Add(morale);
                        colonistsHistory.Add(totalColonists);
                        gameMarkHistory.Add(gameMark);

                        //WriteLog($"Logged: Mark={gameMark}, Morale={morale}");

                        //System.Diagnostics.Debug.WriteLine($"Logged: Mark={gameMark}, Morale={morale}, lastProcessedMark={lastProcessedMark}");
                        System.Diagnostics.Debug.WriteLine($"Logged: Mark={gameMark}, Morale={morale}");

                        // Log every 100 marks to track progress
                        if (gameMark % 100 == 0)
                        {
                            //WriteLog($"Milestone: Mark {gameMark} reached, {moraleHistory.Count} total points collected");
                        }
                    }

                    lastProcessedTick = gameTick;
                    lastProcessedMark = gameMark;
                } 

                processFound = true;

            }
            catch (Exception ex)
            {
                // Handle errors (e.g., when Outpost2 is not running)
                lblStatus.Text = $"OP2StreamingTools - Error: {ex.Message}";
                Debug.WriteLine($"Error reading memory: {ex.Message}");

                // Reset all labels to show error state
                foreach (Control control in this.Controls)
                {
                    if (control is GroupBox groupBox)
                    {
                        foreach (Control subControl in groupBox.Controls)
                        {
                            if (subControl is Label label && label.Name.StartsWith("lbl"))
                            {
                                string baseName = label.Name.Replace("lbl", "").Replace("1", "").Replace("2", "");
                                label.Text = $"{baseName}: N/A";
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Marks a game as running and starts the elapsed-time timer.
        /// </summary>
        private void StartGame()
        {
            gameStartTime = DateTime.Now;
            gameRunning = true;
            gameTimer.Start();
            WriteLog("Game Started!");
            //System.Diagnostics.Debug.WriteLine("Game Started!");
        }

        /// <summary>
        /// Marks the game as no longer running and stops the elapsed-time timer.
        /// </summary>
        private void StopGame()
        {
            gameRunning = false;
            gameTimer.Stop();
        }

        /// <summary>
        /// Updates the elapsed-time label each tick while a game is running.
        /// </summary>
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (gameRunning)
            {
                TimeSpan elapsed = DateTime.Now - gameStartTime;
                gameDuration = elapsed.ToString(@"hh\:mm\:ss") + "." + (elapsed.Milliseconds / 100);
                lblTimer.Text = gameDuration;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            
        }

        private void lblTimer_DoubleClick(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Timer label was double-clicked");
            // Or perform any other action here
            StopGame();
        }

        private void lblTimer_Click(object sender, EventArgs e)
        {

        }


        /// <summary>
        /// Returns a copy of the recorded morale history for graphing.
        /// A copy is returned so callers can't mutate the internal list.
        /// </summary>
        public List<int> GetMoraleHistory()
        {
            return new List<int>(moraleHistory);
        }

        /// <summary>
        /// Returns a copy of the recorded total-colonists history for graphing.
        /// </summary>
        public List<int> GetColonistsHistory()
        {
            return new List<int>(colonistsHistory);
        }

        /// <summary>
        /// Returns a copy of the recorded game-mark history (the X axis for the graphs).
        /// </summary>
        public List<int> GetGameMarkHistory()
        {
            return new List<int>(gameMarkHistory);
        }


        /// <summary>
        /// Test helper that fills the history with random morale data up to mark 1000,
        /// so the graph can be exercised without a running game. Not used in normal operation.
        /// </summary>
        private void SimulateHighMarkData()
        {
            WriteLog("Starting high mark simulation...");

            Random rand = new Random();

            // Simulate data up to mark 1000
            for (int mark = moraleHistory.Count + 1; mark <= 1000; mark++)
            {
                int simulatedMorale = 50 + rand.Next(-20, 30); // Random morale 30-80

                moraleHistory.Add(simulatedMorale);
                 gameMarkHistory.Add(mark);

                if (mark % 100 == 0)
                {
                    WriteLog($"Simulated up to mark {mark}, total points: {moraleHistory.Count}");
                }
            }

            WriteLog($"Simulation complete. Total data points: {moraleHistory.Count}");
        }

    }

}

