using System.Windows;
using LiveDataPerformanceTest.Wpf.ViewModels;

namespace LiveDataPerformanceTest.Wpf
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new GraphVm();
        }
    }
}