using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Template10.Mvvm;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace Template10.Controls
{
    [TemplatePart(Name = RadioButtonName, Type = typeof(RadioButton))]    
    public sealed class SplitViewButton : ContentControl, IBindable
    {
        private const string RadioButtonName = "radioButton";
        private RadioButton _radioButton;

        public SplitViewButton()
        {
            this.DefaultStyleKey = typeof(SplitViewButton);
        }


        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _radioButton = GetTemplateChild(RadioButtonName) as RadioButton;
            HookEvents();
        }

        private void HookEvents()
        {
            _radioButton.Tapped += _radioButton_Tapped;
            _radioButton.Loaded += _radioButton_Loaded;
        }

        private void _radioButton_Loaded(object sender, RoutedEventArgs e)
        {
            _radioButton.Checked += (s, args) => RaiseChecked(args);
            _radioButton.Unchecked += (s, args) => RaiseUnchecked(args);
        }

        private void _radioButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.RaiseChecked(e);
            this.RaiseTapped(e);
            e.Handled = true;
        }

        public Type PageType
        {
            get { return (Type)GetValue(PageTypeProperty); }
            set { SetValue(PageTypeProperty, value); }
        }

        public static readonly DependencyProperty PageTypeProperty =
            DependencyProperty.Register("PageType", typeof(Type), typeof(SplitViewButton), new PropertyMetadata(default(Type)));



        public bool ClearHistory
        {
            get { return (bool)GetValue(ClearHistoryProperty); }
            set { SetValue(ClearHistoryProperty, value); }
        }

        public static readonly DependencyProperty ClearHistoryProperty =
            DependencyProperty.Register("ClearHistory", typeof(bool), typeof(SplitViewButton), new PropertyMetadata(default(bool)));


        public ICommand NavCommand
        {
            get { return (ICommand)GetValue(NavCommandProperty); }
            set { SetValue(NavCommandProperty, value); }
        }

        public static readonly DependencyProperty NavCommandProperty =
            DependencyProperty.Register("NavCommand", typeof(ICommand), typeof(SplitViewButton), new PropertyMetadata(default(ICommand)));



        public object PageParameter
        {
            get { return (object)GetValue(PageParameterProperty); }
            set { SetValue(PageParameterProperty, value); }
        }

        public static readonly DependencyProperty PageParameterProperty =
            DependencyProperty.Register("PageParameter", typeof(object), typeof(SplitViewButton), new PropertyMetadata(default(object)));




        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(SplitViewButton), new PropertyMetadata(default(bool)));

        public event RoutedEventHandler Selected;
        internal void RaiseSelected()
        {
            Selected?.Invoke(this, new RoutedEventArgs());
        }

        public event RoutedEventHandler Unselected;
        internal void RaiseUnselected()
        {
            Unselected?.Invoke(this, new RoutedEventArgs());
        }

        public event RoutedEventHandler Checked;
        internal void RaiseChecked(RoutedEventArgs args)
        {
            Checked?.Invoke(this, args);
        }

        public event RoutedEventHandler Unchecked;
        internal void RaiseUnchecked(RoutedEventArgs args)
        {
            Unchecked?.Invoke(this, args);
        }

        public event RoutedEventHandler Tapped;
        internal void RaiseTapped(RoutedEventArgs args)
        {
            Tapped?.Invoke(this, args);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                return;
            try
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            catch
            {
                // nothing
            }
        }
    }
}

