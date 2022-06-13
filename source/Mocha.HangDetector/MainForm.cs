using System.Diagnostics;

namespace Mocha.HangDetector
{
	public partial class MainForm : Form
	{
		static bool show = false;

		public MainForm()
		{
			InitializeComponent();
		}

		private void HangDetectionWorker_DoWork( object sender, System.ComponentModel.DoWorkEventArgs e )
		{
			Process[] prs = Process.GetProcesses();

			bool show = false;

			foreach ( Process pr in prs )
			{
				if ( !pr.Responding )
				{
					show = true;
				}
			}

			if ( show )
				this.Invoke( () => this.Show() );
			else
				this.Invoke( () => this.Hide() );
		}

		private void MainForm_Load( object sender, EventArgs e )
		{
			HangDetectionWorker.RunWorkerAsync();
		}
	}
}
