using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        private const string ProcessNamingFile = "process_dict.txt";

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
            //UIAutomation.SendMessageToCalculator();
            RefreshComboBox();
        }

        private void RefreshComboBox()
        {
            var processes = Process.GetProcesses().OrderBy(ob => ob.ProcessName);

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

            IntPtr hwnd;

            try
            {
                hwnd = SelectedProcess.MainWindowHandle != IntPtr.Zero ?
                    SelectedProcess.MainWindowHandle :
                    SelectedProcess.Handle;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                MessageBox.Show("That process cannot be selected because is protected by Windows!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                SelectedProcess = null;
                return;
            }

            if (hwnd == IntPtr.Zero)
            {
                MessageBox.Show("Sadly, there was an error while trying to get the handler of the process!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AutomationElement root = null;
            try
            {
                root = AutomationElement.FromHandle(hwnd);
            }
            catch (ElementNotAvailableException)
            {
                TryGetAutomationElementFromWindowName(out root);
            }

            if (root is null)
            {
                MessageBox.Show("Sadly, There's an error while trying to get the AutomationElement of the process!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var elements = root.FindAll(TreeScope.Descendants, Condition.TrueCondition).Cast<AutomationElement>().ToArray();
            if (elements is null || !elements.Any())
            {
                TryGetAutomationElementFromWindowName(out root);
                elements = root.FindAll(TreeScope.Descendants, Condition.TrueCondition).Cast<AutomationElement>().ToArray();
            }

            ProcessHwndChildren = new ObservableCollection<AutomationElement>(elements);

            //DEBUG
            UIAutomation.SendMessageToCalculator(root);
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

        private bool TryGetAutomationElementFromWindowName(out AutomationElement element)
        {
            var hwnd = IntPtr.Zero;
            if (!string.IsNullOrEmpty(SelectedProcess.MainWindowTitle))
            {
                hwnd = UIAutomation.FindWindow(null, SelectedProcess.MainWindowTitle);
            }
            else if (File.Exists(AppContext.BaseDirectory + ProcessNamingFile))
            {
                var windowName = GetWindowNameFromProcessDictFile(SelectedProcess.ProcessName);
                hwnd = UIAutomation.FindWindow(null, windowName);
            }

            try
            {
                element = AutomationElement.FromHandle(hwnd);

                if (element is null)
                    return false;
                else
                    return true;
            }
            catch (ElementNotAvailableException)
            {
                element = null;
                return false;
            }
        }

        private string GetWindowNameFromProcessDictFile(string processName)
        {
            Dictionary<string, string> readed = new Dictionary<string, string>();
            //using(var sr = new StreamReader(AppContext.BaseDirectory + ProcessNamingFile))
            var lines = File.ReadLines(AppContext.BaseDirectory + ProcessNamingFile);

            foreach (var line in lines)
            {
                var keyValue = line.Split('=');
                readed.Add(keyValue[0], keyValue[1]);
            }

            return readed.Where(w => w.Key == processName).Select(s => s.Value).FirstOrDefault();
        }
    }
}
