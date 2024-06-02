using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace QuickScreenshot
{

    public partial class Form1 : Form
    {

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        private readonly HotKeyManager _hotKeyManager;

        public string ScreenPath;
        public int counter = 0;
        public Point CurrentTopLeft = new Point(0, 0);
        public Point CurrentBottomRight = new Point(0, 0);

        public bool IsSuccess1 = false, IsSuccess2 = false;
        /*Monitor monitor = new Monitor();*/
        public Form1()
        {
            InitializeComponent();
            _hotKeyManager = new HotKeyManager();
            _hotKeyManager.KeyPressed += HotKeyManagerPressed;
            _hotKeyManager.Register(Key.L, System.Windows.Input.ModifierKeys.Shift);
            _hotKeyManager.Register(Key.Q, System.Windows.Input.ModifierKeys.Shift | System.Windows.Input.ModifierKeys.Alt);
            DataContainer.CaptureCounter = 0;
        }

        public static void LeftMouseClick(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
        }
        public async void HotKeyManagerPressed(object sender, KeyPressedEventArgs e)
        {
            Point curPoint = System.Windows.Forms.Cursor.Position;
            if (e.HotKey.Key == Key.S)
            {
                bool checkresult1 = false;
                checkresult1 = SaveSelection(true, "M", counter);

                if (checkresult1)
                {
                    if(!IsSuccess1)
                    {
                        IsSuccess1 = true;
                    }  
                    //DataContainer.CaptureCounter++;
                }

                LeftMouseClick((DataContainer.SwitchTopLeft.X + DataContainer.SwitchBottomRight.X) / 2, (DataContainer.SwitchTopLeft.Y + DataContainer.SwitchBottomRight.Y) / 2);
                await Task.Delay(int.Parse(DelayUpDown.Value.ToString()) * 1000);

                bool checkresult2 = false;
                checkresult2 = SaveSelection(true, "D", counter);

                if (checkresult2)
                {
                    if (!IsSuccess2)
                    {
                        IsSuccess2 = true;
                    }
                    //DataContainer.CaptureCounter++;
                }
                LeftMouseClick((DataContainer.SwitchTopLeft.X + DataContainer.SwitchBottomRight.X) / 2, (DataContainer.SwitchTopLeft.Y + DataContainer.SwitchBottomRight.Y) / 2);
                SetCursorPos(curPoint.X, curPoint.Y);
            }
            
            if(IsSuccess1 && IsSuccess2)
            {
                counter++;
                IsSuccess2 = false;
                IsSuccess1 = false;
            }
            if (e.HotKey.Key == Key.L)
            {
                this.Show();
                //monitor.Close();
            }
            if (e.HotKey.Key == Key.Q)
            {
                Application.Exit();
            }
            if(e.HotKey.Key == Key.Space)
            {
                LeftMouseClick((DataContainer.SwitchTopLeft.X + DataContainer.SwitchBottomRight.X)/2, (DataContainer.SwitchTopLeft.Y + DataContainer.SwitchBottomRight.Y) / 2);
            }
        }

        public bool SaveSelection(bool showCursor, string option, int index)
        {

            Point curPos = new Point(System.Windows.Forms.Cursor.Position.X - DataContainer.DronTopLeft.X, System.Windows.Forms.Cursor.Position.Y - DataContainer.DronTopLeft.Y);
            Size curSize = new Size();
            curSize.Height = System.Windows.Forms.Cursor.Current.Size.Height;
            curSize.Width = System.Windows.Forms.Cursor.Current.Size.Width;
            bool issuccess = false;

            ScreenPath = "";
            if (!ScreenShot.saveToClipboard)
            {
                if(option == "D")
                {
                    if (!Directory.Exists(filePath.Text + @"\Drone\"))
                    {
                        Directory.CreateDirectory(filePath.Text + @"\Drone\");
                    }
                    ScreenPath = filePath.Text + @"\Drone\" + FileName.Text + index + ".png";
                    CurrentTopLeft = DataContainer.DronTopLeft;
                    CurrentBottomRight = DataContainer.DronBottomRight;
                }
                else if (option == "M")
                {
                    if (!Directory.Exists(filePath.Text + @"\Map\"))
                    {
                        Directory.CreateDirectory(filePath.Text + @"\Map\");
                    }
                    ScreenPath = filePath.Text + @"\Map\" + FileName.Text + index + ".png";
                    CurrentTopLeft = DataContainer.MapTopLeft;
                    CurrentBottomRight= DataContainer.MapBottomRight;
                }
            }


            if (ScreenPath != "" || ScreenShot.saveToClipboard)
            {

                //Allow 250 milliseconds for the screen to repaint itself (we don't want to include this form in the capture)
                System.Threading.Thread.Sleep(250);

                Point StartPoint = new Point(CurrentTopLeft.X, CurrentTopLeft.Y);
                Rectangle bounds = new Rectangle(CurrentTopLeft.X, CurrentTopLeft.Y, CurrentBottomRight.X - CurrentTopLeft.X, CurrentBottomRight.Y - CurrentTopLeft.Y);
                string fi = "";

                if (ScreenPath != "")
                {

                    fi = new FileInfo(ScreenPath).Extension;

                }

                ScreenShot.CaptureImage(showCursor, curSize, curPos, StartPoint, Point.Empty, bounds, ScreenPath, fi);


                if (ScreenShot.saveToClipboard)
                {

                    //MessageBox.Show("Area saved to clipboard", "QuickScreenshot", MessageBoxButtons.OK);

                }
                else
                {
                    issuccess = true;
                    //MessageBox.Show("Area saved to file", "QuickScreenshot", MessageBoxButtons.OK);
                }

            }

            else
            {
                //MessageBox.Show("File save cancelled", "QuickScreenshot", MessageBoxButtons.OK);
            }

            if (issuccess)
            {
                return true;
            }
            return false;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            Screen screen = new Screen("D");
            screen.InstanceRef = this;
            screen.Show();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            DronPoint.Text = "(" + DataContainer.DronTopLeft.X.ToString() + "," + DataContainer.DronTopLeft.Y.ToString() + ") " + "(" + DataContainer.DronBottomRight.X.ToString() + "," + DataContainer.DronBottomRight.Y.ToString() + ")";
            MapPoint.Text = "(" + DataContainer.MapTopLeft.X.ToString() + "," + DataContainer.MapTopLeft.Y.ToString() + ") " + "(" + DataContainer.MapBottomRight.X.ToString() + "," + DataContainer.MapBottomRight.Y.ToString() + ")";
            SwitchPoint.Text = "(" + DataContainer.SwitchTopLeft.X.ToString() + "," + DataContainer.SwitchTopLeft.Y.ToString() + ") " + "(" + DataContainer.SwitchBottomRight.X.ToString() + "," + DataContainer.SwitchBottomRight.Y.ToString() + ")";
            base.OnPaint(e);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog saveFileDialog = new FolderBrowserDialog();
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath.Text = saveFileDialog.SelectedPath;
            }
            if (filePath.Text.Length > 0 )
            {
                ScreenPath = filePath.Text;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(FileIndex.Text) || !string.IsNullOrEmpty(FileName.Text) || !string.IsNullOrEmpty(filePath.Text) || (DataContainer.DronBottomRight.X - DataContainer.DronTopLeft.X) == 0 || (DataContainer.DronBottomRight.Y - DataContainer.DronTopLeft.Y) == 0 || (DataContainer.MapBottomRight.X - DataContainer.MapTopLeft.X) == 0 || (DataContainer.MapBottomRight.Y - DataContainer.MapTopLeft.Y) == 0)
            {
                try
                {
                    counter = int.Parse(FileIndex.Text);
                    _hotKeyManager.Register(Key.S, System.Windows.Input.ModifierKeys.None);
                    _hotKeyManager.Register(Key.Space, System.Windows.Input.ModifierKeys.None);
                    this.Hide();
                   /* monitor.TopMost = true;
                    monitor.Location = new Point(0, 0);
                    monitor.Show();*/
                }
                catch
                {
                    MessageBox.Show("Index in File Name field must be integer");
                }
            }
            else
            {
                MessageBox.Show("Please set all required fields");
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _hotKeyManager.Unregister(Key.S, System.Windows.Input.ModifierKeys.Shift);
            _hotKeyManager.Unregister(Key.L, System.Windows.Input.ModifierKeys.Shift);
            _hotKeyManager.Unregister(Key.Q, System.Windows.Input.ModifierKeys.Shift | System.Windows.Input.ModifierKeys.Alt);
            _hotKeyManager.Dispose();
            base.OnFormClosed(e);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Hide();
            Screen screen = new Screen("M");
            screen.InstanceRef = this;
            screen.Show();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Hide();
            Screen screen = new Screen("S");
            screen.InstanceRef = this;
            screen.Show();
        }
    }
    public static class DataContainer
    {
        public static Point DronTopLeft = new Point();
        public static Point DronBottomRight = new Point();
        public static Point MapTopLeft = new Point();
        public static Point MapBottomRight = new Point();
        public static Point SwitchTopLeft = new Point();
        public static Point SwitchBottomRight = new Point();
        public static int CaptureCounter;
    }

}
