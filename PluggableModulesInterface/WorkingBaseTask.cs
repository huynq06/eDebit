using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PluggableModulesInterface
{
    public abstract class WorkingBaseTask : WorkingBase
    {
        #region properties
        private int infinite_next_time = 100;
        protected virtual int INFINITE_NEXT_TIME
        {
            get { return infinite_next_time; }
            set { infinite_next_time = value; }
        }
        protected object _lock = new object();
        private bool _start = false;
        public bool Started
        {
            get
            {
                return _start;
            }
            protected set
            {
                lock (_lock)
                {
                    _start = value;
                }
            }

        }
        protected List<Task> taskList = new List<Task>();
        protected int NumOfRunningThread
        {
            get
            {
                return taskList.Count;
            }
        }
        protected string ObjectType { get; private set; }
        #endregion propeties

        #region constructor
        public WorkingBaseTask()
            : base()
        {
            ObjectType = this.GetType().FullName;
        }
        #endregion constructor

        #region Loop
        public virtual void Start()
        {
            Stop();
            Started = true;
            if (taskList.Count < 1)
            {
                taskList.Add(Task.Factory.StartNew(CycleLoop, TaskCreationOptions.LongRunning));
            }
        }

        public virtual void Stop()
        {
            Started = false;
            while (taskList.Count > 0)
            {
                taskList[0].Wait();
                taskList[0].Dispose();
                taskList.RemoveAt(0);
            }
        }

        private void CycleLoop()
        {
            try
            {
                while (Started)
                {
                    DoWork();
                    Thread.Sleep(INFINITE_NEXT_TIME);
                }
            }
            catch (AggregateException ae)
            {
                foreach (Exception ex in ae.InnerExceptions)
                    HandleException(ex);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        protected abstract void DoWork();
        #endregion Loop
    }
}
