using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace TomatoTimer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private TaskbarManager _windowsTaskbar = TaskbarManager.Instance;

        private System.Timers.Timer _timer = new System.Timers.Timer(1000);
        private int _timerWorkCount = 0;
        private int _timerRestCount = 0;
        private int _timerCount = 0;
        private bool _bIsWorkTimer = true;
        private SynchronizationContext _syn;

        public MainWindow()
        {
            InitializeComponent();

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

            this._syn = SynchronizationContext.Current;

            this._timer.Elapsed += _timer_Elapsed;
            this._timer.AutoReset = true;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this._syn.Send(o => 
            {
                this.SetTimerText(this._timerCount--);

                this.SetProgressValue();

                this.CheckStatusChanged();
            }, null);
        }

        private void SetTimerText(int count)
        {
            this.tbTimerText.Text = String.Format("{0:00}:{1:00}", count / 60, count % 60);
        }

        private void SetProgressValue()
        {
            this._windowsTaskbar.SetProgressValue(100 - this._timerCount * 100 / (this._bIsWorkTimer ? this._timerWorkCount : this._timerRestCount), 100);
        }

        private void CheckStatusChanged()
        {
            if (0 == this._timerCount)
            {
                this._bIsWorkTimer = !this._bIsWorkTimer;

                string textComplete = string.Empty;

                if (this._bIsWorkTimer)
                {
                    this._timerCount = this._timerWorkCount;

                    this._windowsTaskbar.SetProgressState(TaskbarProgressBarState.Normal);

                    textComplete = "休息结束，继续加油吧！";
                }
                else
                {
                    this._timerCount = this._timerRestCount;

                    this._windowsTaskbar.SetProgressState(TaskbarProgressBarState.Error);

                    textComplete = "辛苦了！休息一下！";
                }

                MessageBox.Show(textComplete);
            }
        }

        private string CheckTextIsInvalid(string text)
        {
            List<char> listText = new List<char>();

            if (!string.IsNullOrEmpty(text))
            {
                for (int i = 0; i < Math.Min(2, text.Length); i++)
                {
                    if (('0' <= text[i]) && (text[i] <= '9'))
                    {
                        listText.Add(text[i]);
                    }
                }

                if (2 == listText.Count)
                {
                    if (('6' <= listText[0]) && (listText[0] <= '9'))
                    {
                        listText[0] = '6';
                        listText[1] = '0';
                    }
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(listText.ToArray());
            
            return sb.ToString();
        }

        private void tbWork_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.tbWork.Text = this.CheckTextIsInvalid(this.tbWork.Text);
        }

        private void tbRest_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.tbRest.Text = this.CheckTextIsInvalid(this.tbRest.Text);
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if ((int.TryParse(this.tbWork.Text.Trim(),out this._timerWorkCount)) && (int.TryParse(this.tbRest.Text.Trim(),out this._timerRestCount)))
            {
                this._timerRestCount *= 60;
                this._timerWorkCount *= 60;

                this._bIsWorkTimer = true;
                this._timerCount = this._timerWorkCount;

                this.SetTimerEnable(true);

                this.WindowState = System.Windows.WindowState.Minimized;
            }
        }
        
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            this.SetTimerEnable(false);

            this._timerCount = 0;

            this.SetTimerText(this._timerCount);
        }

        private void SetTimerEnable(bool bIsStart)
        {
            this._timer.Enabled = bIsStart;

            this._windowsTaskbar.SetProgressState(bIsStart ? TaskbarProgressBarState.Normal : TaskbarProgressBarState.NoProgress);

            this.btnStart.IsEnabled = !bIsStart;
            this.btnStop.IsEnabled = bIsStart;

            this.tbRest.IsEnabled = !bIsStart;
            this.tbWork.IsEnabled = !bIsStart; 
        }

    }
}
