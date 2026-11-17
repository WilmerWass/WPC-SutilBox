using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wpc_SutilBox.ViewModels;

namespace Wpc_SutilBox.Views
{
    public partial class PeripheralDiagnosticsView : UserControl
    {
        private System.Windows.Point? _lastPointerPoint;

        public PeripheralDiagnosticsView()
        {
            InitializeComponent();
        }

        private void KeyboardCaptureTarget_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is IInputElement target)
                Keyboard.Focus(target);
        }

        private void KeyboardCaptureTarget_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (DataContext is PeripheralDiagnosticsViewModel viewModel)
                viewModel.RecordKey(e.Key == Key.System ? e.SystemKey.ToString() : e.Key.ToString());
            e.Handled = true;
        }

        private void MouseCaptureTarget_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is not PeripheralDiagnosticsViewModel viewModel)
                return;

            string button = e.ChangedButton switch
            {
                MouseButton.Left => "Izquierdo",
                MouseButton.Right => "Derecho",
                MouseButton.Middle => "Central",
                MouseButton.XButton1 or MouseButton.XButton2 => "Adicional",
                _ => string.Empty
            };
            viewModel.RecordMouseButton(button);
            e.Handled = true;
        }

        private void MouseCaptureTarget_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (DataContext is not PeripheralDiagnosticsViewModel viewModel || sender is not IInputElement target)
                return;

            System.Windows.Point current = e.GetPosition(target);
            if (_lastPointerPoint is null || Math.Abs(current.X - _lastPointerPoint.Value.X) >= 8
                || Math.Abs(current.Y - _lastPointerPoint.Value.Y) >= 8)
            {
                viewModel.RecordPointerMove();
                _lastPointerPoint = current;
            }
        }

        private void MouseCaptureTarget_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is PeripheralDiagnosticsViewModel viewModel)
                viewModel.RecordMouseWheel();
            e.Handled = true;
        }
    }
}
