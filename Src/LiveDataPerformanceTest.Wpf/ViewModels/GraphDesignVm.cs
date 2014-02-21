using System;
using System.Linq;
using System.Windows.Input;
using OxyPlot;
using OxyPlot.Series;

namespace LiveDataPerformanceTest.Wpf.ViewModels
{
    public class GraphDesignVm : IGraphVm
    {
        public GraphDesignVm()
        {
            SampleRate = 5000;
            var s1 = new LineSeries("Series1");
            var s2 = new LineSeries("Series2");
            var s3 = new LineSeries("Series3");
            var s4 = new LineSeries("Series4");
            Enumerable.Range(0, 1000)
                .ToList()
                .ForEach(i =>
                {
                    const double phaseShift = 2*Math.PI/4;
                    s1.Points.Add(new DataPoint(i/100.0, 5*Math.Sin(i/100.0 + 0*phaseShift)));
                    s2.Points.Add(new DataPoint(i/100.0, 5*Math.Sin(i/100.0 + 1*phaseShift)));
                    s3.Points.Add(new DataPoint(i/100.0, 5*Math.Sin(i/100.0 + 2*phaseShift)));
                    s4.Points.Add(new DataPoint(i/100.0, 5*Math.Sin(i/100.0 + 3*phaseShift)));
                });

            LivePlotModel = new PlotModel("Design Time Graph");
            LivePlotModel.Series.Add(s1);
            LivePlotModel.Series.Add(s2);
            LivePlotModel.Series.Add(s3);
            LivePlotModel.Series.Add(s4);
        }

        public PlotModel LivePlotModel { get; private set; }
        public uint SampleRate { get; set; }
        public ICommand StopCommand { get; private set; }
        public ICommand StartCommand { get; private set; }
    }
}