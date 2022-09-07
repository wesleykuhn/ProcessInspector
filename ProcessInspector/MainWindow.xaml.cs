using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
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

        public static DependencyProperty SelectedElementPatternsProperty =
            DependencyProperty.Register(nameof(SelectedElementPatterns), typeof(string), typeof(MainWindow), new PropertyMetadata("Element Patterns: None!"));
        public string SelectedElementPatterns
        {
            get => (string)GetValue(SelectedElementPatternsProperty);
            set => SetValue(SelectedElementPatternsProperty, value);
        }

        public static DependencyProperty CurMousePosProperty =
            DependencyProperty.Register(nameof(CurMousePos), typeof(string), typeof(MainWindow), new PropertyMetadata("N/A"));
        public string CurMousePos
        {
            get => (string)GetValue(CurMousePosProperty);
            set => SetValue(CurMousePosProperty, value);
        }

        public static DependencyProperty DetectedElementProperty =
            DependencyProperty.Register(nameof(DetectedElement), typeof(AutomationElement), typeof(MainWindow));
        public AutomationElement DetectedElement
        {
            get => (AutomationElement)GetValue(DetectedElementProperty);
            set => SetValue(DetectedElementProperty, value);
        }

        private readonly DispatcherTimer _dispatcherTimer;

        public MainWindow()
        {
            InitializeComponent();

            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += new EventHandler(DoDetection);
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
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

        private void ElementsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedElement is null)
                return;

            SelectedElementPatterns = "Element Patterns: ";

            var patterns = SelectedElement.GetSupportedPatterns();

            if (patterns is null || !patterns.Any())
            {
                SelectedElementPatterns += "None!";
                return;
            }

            foreach (var pattern in patterns)
                SelectedElementPatterns += pattern.ProgrammaticName + " | ";
        }

        private void StartButton_Clicked(object sender, RoutedEventArgs e)
        {
            _dispatcherTimer.Start();
        }

        private void DoDetection(object sender, EventArgs e)
        {
            var curPos = System.Windows.Forms.Cursor.Position;

            CurMousePos = $"{curPos.X}, {curPos.Y}";
            
            try
            {
                DetectedElement = AutomationElement.FromPoint(new Point(curPos.X, curPos.Y));
            }
            catch (System.ComponentModel.Win32Exception)
            {
                return;
            }

            SelectedElementPatterns = string.Empty;

            if (DetectedElement is null)
                return;

            var patterns = DetectedElement.GetSupportedPatterns();
            if (patterns is null || !patterns.Any())
            {
                SelectedElementPatterns += "None!";
                return;
            }

            foreach (var pattern in patterns)
                SelectedElementPatterns += pattern.ProgrammaticName + " | ";
        }

        private void StopButton_Clicked(object sender, RoutedEventArgs e)
        {
            _dispatcherTimer.Stop();
        }
    }
}
