using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Diagnostics;

namespace ClipStrip
{
	public class Form1 : System.Windows.Forms.Form
	{
		[DllImport("User32.dll")]
		protected static extern int SetClipboardViewer(int hWndNewViewer);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);
       
        private NotifyIcon notifyIcon1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem enableToolStripMenuItem;
        private ToolStripMenuItem disabledToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private IContainer components;
        
        //my defines
        IntPtr nextClipboardViewer;
        private bool isOn = true;        
        Icon off;        
        Icon on;

		public Form1()
		{
			InitializeComponent();
			nextClipboardViewer = (IntPtr)SetClipboardViewer((int) this.Handle);
		}

		protected override void Dispose( bool disposing )
		{
			ChangeClipboardChain(this.Handle, nextClipboardViewer);
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.enableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disabledToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "ClipStrip";
            this.notifyIcon1.Visible = true;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.enableToolStripMenuItem,
            this.disabledToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.ShowCheckMargin = true;
            this.contextMenuStrip1.ShowImageMargin = false;
            this.contextMenuStrip1.Size = new System.Drawing.Size(153, 92);
            // 
            // enableToolStripMenuItem
            // 
            this.enableToolStripMenuItem.Checked = true;
            this.enableToolStripMenuItem.CheckOnClick = true;
            this.enableToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enableToolStripMenuItem.Name = "enableToolStripMenuItem";
            this.enableToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.enableToolStripMenuItem.Text = "Enabled";
            this.enableToolStripMenuItem.Click += new System.EventHandler(this.enableToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // disabledToolStripMenuItem
            // 
            this.disabledToolStripMenuItem.Name = "disabledToolStripMenuItem";
            this.disabledToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.disabledToolStripMenuItem.Text = "Disabled";
            this.disabledToolStripMenuItem.Click += new System.EventHandler(this.disabledToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(205, 167);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Form1";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "ClipStrip";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		[STAThread]
		static void Main() 
		{
            // Get Reference to the current Process
            Process thisProc = Process.GetCurrentProcess();
            // Check how many total processes have the same name as the current one
            if (Process.GetProcessesByName(thisProc.ProcessName).Length > 1)
            {
                // If ther is more than one, than it is already running.                
                MessageBox.Show("Running more than one will cause literally infinite problems, this instance will be terminated.", "Error: Application is already running", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

			Application.Run(new Form1());
            
		}

        protected override void WndProc(ref System.Windows.Forms.Message m)
		{
			// defined in winuser.h
			const int WM_DRAWCLIPBOARD = 0x308;
			const int WM_CHANGECBCHAIN = 0x030D;

			switch(m.Msg)
			{
				case WM_DRAWCLIPBOARD:
                    //convert the data
					ConvertClipboardData();
					SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
					break;

				case WM_CHANGECBCHAIN:
					if (m.WParam == nextClipboardViewer)
						nextClipboardViewer = m.LParam;
					else
						SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
					break;

				default:
					base.WndProc(ref m);
					break;
			}	
		}

		void ConvertClipboardData()		
		{
            if (isOn == true)
            {
                try
                {
                    IDataObject iClpBrd = new DataObject();
                    //make a local instance of the clipboard
                    iClpBrd = Clipboard.GetDataObject();

                    //check for formatted text based datatypes
                    if (iClpBrd.GetDataPresent(DataFormats.Rtf) || iClpBrd.GetDataPresent(DataFormats.UnicodeText))
                    {
                        //if the data in the clipboard is Rich Text or Unicode Text, then convert it to Text type 
                        //and write it back to the clipboard.
                        Clipboard.SetData(DataFormats.Text, ((string)iClpBrd.GetData(DataFormats.Text)));
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show(err.Message);
                }
            }
            else
            {
                //do nothing, off state code here.
            }
		}

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Hide();
            //load the icons from the Assembly           
            off = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("ClipStrip.off.ico"));
            on = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("ClipStrip.on.ico"));            
        }


        //Handle the toolbar clicks
        private void enableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isOn = true;
            notifyIcon1.Icon = on;
            enableToolStripMenuItem.CheckState = CheckState.Checked;
            disabledToolStripMenuItem.CheckState = CheckState.Unchecked;
        }

        private void disabledToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isOn = false;
            notifyIcon1.Icon = off;
            enableToolStripMenuItem.CheckState = CheckState.Unchecked;
            disabledToolStripMenuItem.CheckState = CheckState.Checked;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
	}
}
