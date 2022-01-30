namespace TMP.WORK.AramisChetchiki.Model
{
    using System;
    using System.Windows.Threading;

    public class WorkTask : Shared.PropertyChangedBase
    {
        private bool isCompleted = false;
        private string header;
        private string status;
        private bool isIndeterminate = false;
        private bool isProgressVisible = true;
        private double progress = 0;
        private double childProgress = 0;
        private string elapsedTime;
        private string remainingTime;
        private string childRemainingTime;
        private System.Windows.Threading.DispatcherTimer timer;
        private DateTime startRunning;
        private DateTime startProcessing;
        private DateTime startChildProcessing;
        private bool processingStarted;
        private bool childProcessingStarted;

        private Dispatcher callingDispatcher;

        // private readonly System.Windows.Threading.DispatcherTimer updateUITimer;

        public WorkTask(string header)
        {
            // Сохранение диспетчера потока
            // this.callingDispatcher = Dispatcher.CurrentDispatcher;

            this.header = header;

            this.timer = new System.Windows.Threading.DispatcherTimer(TimeSpan.FromSeconds(1), System.Windows.Threading.DispatcherPriority.Render,
                this.Timer_Tick, System.Windows.Application.Current.Dispatcher)
            {
                IsEnabled = true,
            };
            this.startRunning = DateTime.Now;

            // this.updateUITimer = new System.Windows.Threading.DispatcherTimer(TimeSpan.FromMilliseconds(100), System.Windows.Threading.DispatcherPriority.Render,
            //    this.UpdateUITimer_Tick, System.Windows.Application.Current.Dispatcher);
        }

        public bool IsCompleted
        {
            get => this.isCompleted;
            set
            {
                this.isCompleted = value;
                if (this.isCompleted)
                {
                    this.Progress = 100;
                    this.ChildProgress = 0;
                    this.Status = "Завершено!";
                    this.IsProgressVisible = false;

                    // this.updateUITimer.IsEnabled = false;

                    this.timer.IsEnabled = false;
                    this.RemainingTime = string.Empty;
                    this.ChildRemainingTime = string.Empty;
                }

                this.RaisePropertyChanged();
            }
        }

        public string Header { get => this.header; set => this.SetProperty(ref this.header, value); }

        public string Status { get => this.status; set => this.SetProperty(ref this.status, value); }

        public bool IsIndeterminate { get => this.isIndeterminate; set => this.SetProperty(ref this.isIndeterminate, value); }

        public bool IsProgressVisible { get => this.isProgressVisible; set => this.SetProperty(ref this.isProgressVisible, value); }

        public double Progress
        {
            get => this.progress;
            set
            {
                if (this.SetProperty(ref this.progress, value))
                {
                    this.IsIndeterminate = this.progress == -1;
                }
            }
        }

        public bool IsChildProgressVisible => this.childProgress > 0;

        public double ChildProgress
        {
            get => this.childProgress;
            set
            {
                if (this.SetProperty(ref this.childProgress, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsChildProgressVisible));
                }
            }
        }

        public string ElapsedTime { get => this.elapsedTime; set => this.SetProperty(ref this.elapsedTime, value); }

        public string RemainingTime { get => this.remainingTime; set => this.SetProperty(ref this.remainingTime, value); }

        public string ChildRemainingTime { get => this.childRemainingTime; set => this.SetProperty(ref this.childRemainingTime, value); }

        public Action UpdateUIAction { get; set; }

        public void StartProcessing()
        {
            // this.updateUITimer.IsEnabled = true;

            this.startProcessing = DateTime.Now;
            this.processingStarted = true;
        }

        public void StartChildProcessing()
        {
            this.startChildProcessing = DateTime.Now;
            this.childProcessingStarted = true;
        }

        public void SetProgress(int total, int processed)
        {
            if (total <= 0)
            {
                return;
            }

            this.Progress = 100d * processed / total;

            // RemaingTime
            if (this.isIndeterminate == false && this.processingStarted)
            {
                if (this.Progress < 5)
                {
                    this.RemainingTime = "осталось: вычисление ...";
                }
                else
                {
                    TimeSpan timeRemaining = TimeSpan.FromTicks(DateTime.Now.Subtract(this.startProcessing).Ticks * (total - (processed + 1)) / (processed + 1));
                    if (timeRemaining.Seconds < 5)
                    {
                        this.RemainingTime = timeRemaining.Seconds == 0 ? "завершение ..." : "осталось совсем немного";
                    }
                    else
                    {
                        this.RemainingTime = string.Format("осталось: {0:mm\\:ss} с", timeRemaining);
                    }
                }
            }

            this.RaisePropertyChanged(nameof(this.ElapsedTime));
        }

        public void SetChildProgress(int total, int processed)
        {
            if (total <= 0)
            {
                return;
            }

            this.ChildProgress = 100d * processed / total;

            // ChildRemainigTime
            if (this.childProgress > 0 && this.childProcessingStarted)
            {
                if (this.childProgress < 5)
                {
                    this.ChildRemainingTime = "осталось: вычисление ...";
                }
                else
                {
                    TimeSpan timeRemaining = TimeSpan.FromTicks(DateTime.Now.Subtract(this.startChildProcessing).Ticks * (total - (processed + 1)) / (processed + 1));
                    if (timeRemaining.Seconds < 5)
                    {
                        this.ChildRemainingTime = timeRemaining.Seconds == 0 ? "завершение ..." : "осталось совсем немного";
                    }
                    else
                    {
                        this.ChildRemainingTime = string.Format("осталось: {0:mm\\:ss} с", timeRemaining);
                    }
                }
            }
        }

        public void UpdateStatus(string newStatus)
        {
            this.Status = newStatus;
            this.Progress = -1;
        }

        public void UpdateUI(int value, int totalRows, string message = "", string stepNameString = "обработка записей")
        {
            if (value == 1 || value % 5 == 0 || (Math.Abs(totalRows - value) < 5))
            {
                this.Update(value, totalRows, message, stepNameString);
            }
        }

        public void UpdateUI(int value, int totalRows, int childValue, int childTotal, string message = "", string stepNameString = "обработка записей")
        {
            if (value == 1 || value % 5 == 0 || (Math.Abs(totalRows - value) < 5))
            {
                if (totalRows <= 0)
                {
                    return;
                }

                this.Update(value, totalRows, message, stepNameString);
                this.SetChildProgress(childTotal, childValue);
            }
        }

        private void Update(int value, int total, string message, string stepNameString)
        {
            double progress = 100d * value / total;

            string status = string.Format(AppSettings.CurrentCulture, "{0}{1}: {2:N0} из {3:N0}, {4:N1}%",
                string.IsNullOrWhiteSpace(message) ? string.Empty : message + "\n",
                stepNameString,
                value, total, progress);

            this.Status = status;
            this.SetProgress(total, value);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.ElapsedTime = string.Format("{0:mm\\:ss}", DateTime.Now - this.startRunning);
        }

        private void UpdateUITimer_Tick(object sender, EventArgs e)
        {
            if (this.callingDispatcher.CheckAccess())
            {
                this.UpdateUIAction?.Invoke();
            }
            else
            {
                this.callingDispatcher.BeginInvoke(this.UpdateUIAction, DispatcherPriority.Input);
            }
        }
    }
}
