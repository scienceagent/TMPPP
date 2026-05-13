using System.Windows;
using System.Windows.Controls;

namespace HotelBookingSystem.Views
{
    public partial class StateNode : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(StateNode), new PropertyMetadata("", OnTitleChanged));

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(StateNode), new PropertyMetadata(false));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public StateNode()
        {
            InitializeComponent();
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var node = (StateNode)d;
            node.TitleText.Text = (string)e.NewValue;
        }
    }
}
