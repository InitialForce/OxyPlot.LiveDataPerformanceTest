using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows.Input;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace LiveDataPerformanceTest.Wpf.ViewModels
{
    public interface IGraphVm
    {
        PlotModel LivePlotModel { get; }
        uint SampleRate { get; set; }
        ICommand StartCommand { get; }
        ICommand StopCommand { get; }
    }

    public class GraphVm : ViewModelBase, IGraphVm, IDisposable
    {
        //private readonly Dispatcher _uiDispatcher;
        private readonly LinearAxis _forceAxis;
        private readonly LinearAxis _timeAxis;
        private readonly Timer _timer;

        private TimeSpan? _prevTimestamp;
        private uint _sampleRate;
        private int _seriesCount;
        private DelegateCommand _startCommand;
        private DateTime _startUtc;
        private DelegateCommand _stopCommand;


        public GraphVm()
        {
            //_uiDispatcher = Application.Current.Dispatcher;

            GraphTimelineWindow = TimeSpan.FromSeconds(3);

            LivePlotModel = new PlotModel("Live Data", "Performance Test");
            _forceAxis = new LinearAxis(AxisPosition.Left, "Force")
            {
                Unit = "N",
            };
            _timeAxis = new LinearAxis(AxisPosition.Bottom, "Time")
            {
                StringFormat = "0.#",
                MajorStep = 1,
                MinorStep = 0.5,
            };

            LivePlotModel.Axes.Add(_timeAxis);
            LivePlotModel.Axes.Add(_forceAxis);

            SampleRate = 100;

            // This rebuilds _allSeries, depends on _forceAxis and _timeAxis
            SeriesCount = 8;

            // 20 Hz
            var timer = new Timer(1.0/20*1000);
            timer.Elapsed += TimerOnElapsed;
            _timer = timer;
        }

        public int SeriesCount
        {
            get { return _seriesCount; }
            set
            {
                if (!SetValue(ref _seriesCount, value))
                    return;

                RebuildPlotModel(LivePlotModel, _seriesCount);
            }
        }

        public TimeSpan GraphTimelineWindow { get; set; }

        public void Dispose()
        {
        }

        public PlotModel LivePlotModel { get; private set; }

        public ICommand StopCommand
        {
            get { return _stopCommand ?? (_stopCommand = new DelegateCommand(Stop)); }
        }


        public ICommand StartCommand
        {
            get { return _startCommand ?? (_startCommand = new DelegateCommand(Start)); }
        }

        public uint SampleRate
        {
            get { return _sampleRate; }
            set { SetValue(ref _sampleRate, value); }
        }

        private void RebuildPlotModel(PlotModel plotModel, int seriesCount)
        {
            plotModel.Series.Clear();

            for (int i = 0; i < seriesCount; i++)
            {
                var lineSeries = new LineSeries("Series" + i)
                {
                    YAxisKey = _forceAxis.Key,
                    XAxisKey = _timeAxis.Key,
                    Smooth = false
                };
                plotModel.Series.Add(lineSeries);
            }
        }

        private void Stop()
        {
            Timer timer = _timer;
            if (timer != null)
            {
                timer.Stop();
                _prevTimestamp = null;
            }
        }

        private void Start()
        {
            foreach (Series series1 in LivePlotModel.Series)
            {
                var series = (LineSeries) series1;
                series.Points.Clear();
            }

            _startUtc = DateTime.UtcNow;
            _timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            // Unhook while handling, useful when stepping during debug.
            _timer.Elapsed -= TimerOnElapsed;

            UpdatePlot();

            _timer.Elapsed += TimerOnElapsed;
        }

        private void UpdatePlot()
        {
            // Prevent collection modified exception inside LivePlotModel, 
            // when manipulating it outside the GUI thread.
            lock (LivePlotModel.SyncRoot)
            {
                if (!_prevTimestamp.HasValue)
                {
                    _prevTimestamp = TimeSpan.Zero;
                    return;
                }

                IEnumerable<LineSeries> allSeries = LivePlotModel.Series.Cast<LineSeries>();

                TimeSpan samplingPeriod = TimeSpan.FromSeconds(1d/SampleRate);
                if (samplingPeriod == TimeSpan.Zero)
                    return;

                DateTime utcNow = DateTime.UtcNow;

                // Clear all data.
                foreach (LineSeries series in allSeries)
                {
                    series.Points.Clear();
                }

                TimeSpan nowSampleTime = utcNow - _startUtc;
                TimeSpan timestamp = nowSampleTime - GraphTimelineWindow;
                int seriesCount = allSeries.Count();

                while ((timestamp += samplingPeriod) < nowSampleTime)
                {
                    int seriesIndex = 0;
                    foreach (LineSeries series in allSeries)
                    {
                        // Pseudo data, 1 Hz sinus curves with a little bit of phase shift for each series.
                        double x = timestamp.TotalSeconds;
                        
                        // Shift series evenly over half a period.
                        double phaseShift = ((double) seriesIndex/seriesCount/2)*2*Math.PI;

                        series.Points.Add(new DataPoint(x, 50*Math.Sin(1*x*2*Math.PI + phaseShift)));
                        seriesIndex++;
                    }
                }

                LineSeries firstSeries = allSeries.First();
                Console.WriteLine("Sample count [{0}], last time [{1:0.000}]: ", firstSeries.Points.Count, firstSeries.Points.Last().X);

                _prevTimestamp = TimeSpan.FromSeconds(firstSeries.Points.Last().X);

            }

            LivePlotModel.RefreshPlot(true);
        }
    }
}