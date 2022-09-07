using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using Condition = System.Windows.Automation.Condition;

namespace ProcessInspector
{
    /// <summary>
    /// Interaction logic for ElementChildrenWindow.xaml
    /// </summary>
    public partial class ElementChildrenWindow : Window
    {
        private const string NaStr = "N/A";

        public static DependencyProperty ElementChildrenProperty =
                DependencyProperty.Register(nameof(ElementChildren), typeof(ObservableCollection<AutomationElement>), typeof(ElementChildrenWindow));
        public ObservableCollection<AutomationElement> ElementChildren
        {
            get => (ObservableCollection<AutomationElement>)GetValue(ElementChildrenProperty);
            set => SetValue(ElementChildrenProperty, value);
        }

        public static DependencyProperty CurElementDescProperty =
            DependencyProperty.Register(nameof(CurElementDesc), typeof(string), typeof(ElementChildrenWindow));
        public string CurElementDesc
        {
            get => (string)GetValue(CurElementDescProperty);
            set => SetValue(CurElementDescProperty, value);
        }

        public static DependencyProperty SelectedChildElementProperty =
            DependencyProperty.Register(nameof(SelectedChildElement), typeof(AutomationElement), typeof(ElementChildrenWindow));
        public AutomationElement SelectedChildElement
        {
            get => (AutomationElement)GetValue(SelectedChildElementProperty);
            set => SetValue(SelectedChildElementProperty, value);
        }

        public static DependencyProperty SelectedChildElementPatternsProperty =
            DependencyProperty.Register(nameof(SelectedChildElementPatterns), typeof(string), typeof(ElementChildrenWindow), new PropertyMetadata("Element Patterns: None!"));
        public string SelectedChildElementPatterns
        {
            get => (string)GetValue(SelectedChildElementPatternsProperty);
            set => SetValue(SelectedChildElementPatternsProperty, value);
        }

        private readonly AutomationElement _element;

        public ElementChildrenWindow(AutomationElement element)
        {
            InitializeComponent();

            _element = element;
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            CurElementDesc = $"{_element.Current.AutomationId ?? NaStr} -" +
                $" {_element.Current.Name ?? NaStr} -" +
                $" {_element.Current.ClassName ?? NaStr} -" +
                $" {_element.Current.ControlType.ProgrammaticName ?? NaStr}";

            var elements = _element.FindAll(TreeScope.Descendants, Condition.TrueCondition);
            if (elements is null || elements.Count == 0)
            {
                MessageBox.Show("There's no children for this element!", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
                return;
            }

            ElementChildren = new ObservableCollection<AutomationElement>();
            foreach (var item in elements)
                ElementChildren.Add((AutomationElement)item);
        }

        private void ElementChildrenListView_DoubleClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (SelectedChildElement is null)
                return;

            var ecw = new ElementChildrenWindow(SelectedChildElement);
            ecw.Show();
        }

        private void ElementChildrenListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (SelectedChildElement is null)
                return;

            SelectedChildElementPatterns = "Element Patterns: ";

            var patterns = SelectedChildElement.GetSupportedPatterns();

            if (patterns is null || !patterns.Any())
            {
                SelectedChildElementPatterns += "None!";
                return;
            }

            foreach (var pattern in patterns)
                SelectedChildElementPatterns += pattern.ProgrammaticName + " | ";
        }
    }
}
