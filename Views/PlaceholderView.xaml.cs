using System.Windows;
using System.Windows.Controls;

namespace Wpc_SutilBox.Views
{
    public partial class PlaceholderView : UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(PlaceholderView),
                new PropertyMetadata(""));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description), typeof(string), typeof(PlaceholderView),
                new PropertyMetadata(""));

        public static readonly DependencyProperty SectionNameProperty =
            DependencyProperty.Register(nameof(SectionName), typeof(string), typeof(PlaceholderView),
                new PropertyMetadata(""));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public string SectionName
        {
            get => (string)GetValue(SectionNameProperty);
            set => SetValue(SectionNameProperty, value);
        }

        public PlaceholderView()
        {
            InitializeComponent();
        }
    }
}
