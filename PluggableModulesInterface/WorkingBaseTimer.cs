using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PluggableModulesInterface
{
    public abstract class WorkingBaseTimer : WorkingBase
    {
        #region Properties and fields
        private int _dueTime = 100;
        protected virtual int DUETIME
        {
            get { return _dueTime; }
            private set { _dueTime = value; }
        }
        private int _period = Timeout.Infinite;
        protected virtual int PERIOD
        {
            get { return Timeout.Infinite; }
            private set { _period = value; }
        }
        private object _lock = new object();
        public bool Started { get; private set; }
        private Timer timer;
        private int syncPoint = 0;
        private bool sleep = true;
        public bool Sleeping
        {
            get { return sleep; }
            set
            {
                lock (_lock)
                {
                    sleep = value;
                }
            }
        }
        private bool _required_wakeup = false;
        private TimerCallback callback;
        private string TimerType { get; set; }
        #endregion Properties and fields

        #region constructor
        /// <summary>
        /// Hàm khởi tạo mặc định.
        /// Timer thực hiện công việc lặp đi lặp lại không ngừng
        /// </summary>
        public WorkingBaseTimer()
            : base()
        {
            this._required_wakeup = false;
            this.callback = new TimerCallback(TimerCallbackHandle);
        }

        /// <summary>
        /// Hàm khởi tạo
        /// </summary>
        /// <param name="required_wakeup">
        /// Có yêu cầu đánh thức hay không.
        /// Nếu không yêu cầu, timer thực hiện công việc lặp đi lặp lại không ngừng.
        /// Ngược lại, timer chỉ thực hiện công việc 1 lần, và chờ đợi được đánh thức mới thực hiện tiếp công việc
        /// </param>
        public WorkingBaseTimer(bool required_wakeup) :
            base()
        {
            this._required_wakeup = required_wakeup;
            this.callback = new TimerCallback(TimerCallbackHandle);
        }

        /// <summary>
        /// Hàm khởi tạo truyền callback từ ngoài vào.
        /// </summary>
        /// <param name="callback">Phương thức được xử lý bởi timer</param>
        public WorkingBaseTimer(TimerCallback callback)
        {
            this._required_wakeup = true;
            this.callback = callback;
        }
        #endregion constructor

        #region Loop
        public virtual void Start()
        {
            lock (_lock)
            {
                if (!Started)
                {
                    Started = true;
                    Sleeping = false;
                    timer = new Timer(callback, this, 0, PERIOD);
                }
            }
        }

        public virtual void Stop()
        {
            lock (_lock)
            {
                Sleeping = true;
                if (Started)
                {
                    Started = false;
                    if (timer != null)
                    {
                        timer.Dispose();
                        timer = null;
                    }
                }
            }
        }

        public virtual void WakeUp()
        {
            if (timer != null)
            {
                if (Sleeping)
                {
                    //Sleeping = false;
                    timer.Change(DUETIME, PERIOD);
                }
            }
        }

        public void Wait()
        {
            while (!Sleeping)
            {
                Thread.Sleep(100);
            }
        }

        public void SetDueTime(int dueTime)
        {
            DUETIME = dueTime;
        }

        public void SetPeriod(int period)
        {
            PERIOD = period;
        }

        private void TimerCallbackHandle(object state)
        {
            Sleeping = false;
            int sync = Interlocked.CompareExchange(ref syncPoint, 1, 0);
            if (sync == 0)
            {
                try
                {
                    DoWork();
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
                finally
                {
                    Sleeping = true;
                    if (!_required_wakeup)
                        WakeUp();
                    syncPoint = 0;
                }
            }
        }

        protected virtual void DoWork()
        {

        }
        #endregion Loop

        #region Method
        public string GetTimerType()
        {
            return this.TimerType;
        }
        public void SetTimerType(string value)
        {
            this.TimerType = value;
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (timer != null)
                {
                    timer.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        ~WorkingBaseTimer()
        {
            Dispose(false);
        }
        #endregion
    }
}
