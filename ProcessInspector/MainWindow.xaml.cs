using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Condition = System.Windows.Automation.Condition;

namespace ProcessInspector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static DependencyProperty ProcessesProperty =
                DependencyProperty.Register(nameof(Processes), typeof(ObservableCollection<Process>), typeof(MainWindow));
        public ObservableCollection<Process> Processes
        {
            get => (ObservableCollection<Process>)GetValue(ProcessesProperty);
            set => SetValue(ProcessesProperty, value);
        }

        public static DependencyProperty SelectedProcessProperty =
            DependencyProperty.Register(nameof(SelectedProcess), typeof(Process), typeof(MainWindow));
        public Process SelectedProcess
        {
            get => (Process)GetValue(SelectedProcessProperty);
            set => SetValue(SelectedProcessProperty, value);
        }

        public static DependencyProperty ProcessHwndChildrenProperty =
            DependencyProperty.Register(nameof(ProcessHwndChildren), typeof(ObservableCollection<AutomationElement>), typeof(MainWindow));
        public ObservableCollection<AutomationElement> ProcessHwndChildren
        {
            get => (ObservableCollection<AutomationElement>)GetValue(ProcessHwndChildrenProperty);
            set => SetValue(ProcessHwndChildrenProperty, value);
        }

        public static DependencyProperty SelectedElementProperty =
            DependencyProperty.Register(nameof(SelectedElement), typeof(AutomationElement), typeof(MainWindow));
        public AutomationElement SelectedElement
        {
            get => (AutomationElement)GetValue(SelectedElementProperty);
            set => SetValue(SelectedElementProperty, value);
        }
        
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            RefreshComboBox();
        }

        private void RefreshComboBox()
        {
            var processes = Process.GetProcesses();

            Processes = new ObservableCollection<Process>(processes);
        }

        private async Task Start()
        {
            //await Task.Delay(1000);

            //var processes = Process.GetProcessesByName("mspaint");
            //var hWnd = processes.FirstOrDefault().MainWindowHandle;

            //var root = AutomationElement.FromHandle(hWnd);
            //var elements = root.FindAll(TreeScope.Descendants, Condition.TrueCondition)
            //            .Cast<AutomationElement>();

            //foreach (var element in elements)
            //{
            //    txbLogs.Dispatcher.Invoke(() =>
            //            txbLogs.Text += $"Name: {element.Current.Name} - " +
            //            $"ClassName: {element.Current.ClassName} - " +
            //            $"CtrlName: {element.Current.ControlType.ProgrammaticName}\n");

            //    if (element.Current.ClassName.Contains("Windows.UI.Input"))
            //    {
            //        //element.TryGetCurrentPattern(InvokePattern.Pattern, out var pattern);
            //        var patterns = element.GetSupportedPatterns();

            //        foreach (var pattern in patterns)
            //        {
            //            txbLogs.Dispatcher.Invoke(() =>
            //                txbLogs.Text += $"Supported patterns: {pattern.ProgrammaticName}\n");
            //        }
            //    }
            //    //var elementHWnd = new IntPtr(element.Current.NativeWindowHandle);

            //    //txbLogs.Dispatcher.Invoke(() =>
            //    //    txbLogs.Text += $"Name: {element.Current.Name} - " +
            //    //    $"ClassName: {element.Current.ClassName} - " +
            //    //    $"CtrlName: {element.Current.ControlType.ProgrammaticName} - " +
            //    //    $"Hanlder: {elementHWnd}\n");
            //}

            //txbLogs.Dispatcher.Invoke(() => txbLogs.Text += "\nFINISHED!");
        }

        private void RefreshButton_Clicked(object sender, RoutedEventArgs e)
        {
            RefreshComboBox();
        }

        private void ProcessesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedProcess is null)
                return;

            if (SelectedProcess.MainWindowHandle == IntPtr.Zero)
            {
                MessageBox.Show("Sadly, there was an error while trying to get the handler of the process!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var root = AutomationElement.FromHandle(SelectedProcess.MainWindowHandle);
            var elements = root.FindAll(TreeScope.Descendants, Condition.TrueCondition).Cast<AutomationElement>().ToArray();
            ProcessHwndChildren = new ObservableCollection<AutomationElement>(elements);
        }

        private void ElementsListView_DoubleClicked(object sender, MouseButtonEventArgs e)
        {
            if (SelectedElement is null)
                return;

            var ecw = new ElementChildrenWindow(SelectedElement);
            ecw.Show();
        }
    }
}
