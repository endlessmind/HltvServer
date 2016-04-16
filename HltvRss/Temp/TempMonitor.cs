using HltvRss.Temp;
using OpenHardwareMonitor.Hardware;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

namespace HltvRss
{
    class TempMonitor
    {
        public delegate void UpdateHandler(object sender, EventArgs e);
        public event UpdateHandler OnUpdateConnected;

        PersistentSettings settings;
        Computer computerHardware;
        UpdateVisitor updateVisitor = new UpdateVisitor();

        DispatcherTimer dispatcherTimer;
        BackgroundWorker worker;

        IHardware cpu;
        IHardware mobo;

        private bool loaded = false;

        public TempMonitor()
        {
            dispatcherTimer = new DispatcherTimer();
            worker = new BackgroundWorker();

            dispatcherTimer.Tick += new EventHandler(TimerTick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 2);

            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;

        }

        public IHardware CPU
        {
            get
            {
                return cpu;
            }
        }

        public IHardware Motherboard
        {
            get
            {
                return mobo;
            }
        }

        public bool isLoaded
        {
            get
            {
                return loaded;
            }
        }

        public void Init() //Should be run in a thread at all times!
        {
            worker.RunWorkerAsync();
        }

        public void InternalInit()
        {
            settings = new PersistentSettings();
            settings.Load(System.IO.Path.ChangeExtension(System.Windows.Forms.Application.ExecutablePath, ".config"));

            computerHardware = new Computer(settings);
            computerHardware.Open();

            computerHardware.MainboardEnabled = true;
            computerHardware.CPUEnabled = true;
            computerHardware.GPUEnabled = true;
            computerHardware.HDDEnabled = false;

            Thread.Sleep(1000);

            //I just want the CPU and Mainboard temp for this project
            foreach (var hardware in computerHardware.Hardware)
            {
                hardware.Accept(updateVisitor);
                if (hardware.HardwareType == HardwareType.CPU)
                {
                    cpu = hardware;
                }
                if (hardware.HardwareType == HardwareType.Mainboard)
                {
                    foreach (var sub in hardware.SubHardware)
                    {
                        if (sub.HardwareType == HardwareType.SuperIO)
                        {
                            mobo = sub;
                        }

                    }
                }


                hardware.Update();
            }

            loaded = true;
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            InternalInit();
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            dispatcherTimer.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (computerHardware != null)
                computerHardware.Accept(updateVisitor);

            if (cpu != null)
                cpu.Update();

            if (mobo != null)
                mobo.Update();

            if (OnUpdateConnected != null)
                OnUpdateConnected(null, new EventArgs());


        }


    }
}
