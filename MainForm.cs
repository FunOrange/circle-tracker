using OsuMemoryDataProvider;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace circle_tracker
{
    public partial class MainForm : Form
    {
        private readonly Tracker tracker;
        public MainForm()
        {
            InitializeComponent();
            tracker = new Tracker();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            tracker.OnClosing();
        }
    }
}
