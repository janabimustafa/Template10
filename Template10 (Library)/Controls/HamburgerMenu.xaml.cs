using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Template10.Services.KeyboardService;
using Template10.Services.NavigationService;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace Template10.Controls
{
    // DOCS: https://github.com/Windows-XAML/Template10/wiki/Docs-%7C-HamburgerMenu
    [ContentProperty(Name = nameof(PrimaryButtons))]
    public sealed partial class HamburgerMenu : UserControl, INotifyPropertyChanged
    {
        public HamburgerMenu()
        {
            this.InitializeComponent();
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                // nothing
            }
            else
            {
                PrimaryButtons = new ObservableItemCollection<SplitViewButton>();
                SecondaryButtons = new ObservableItemCollection<SplitViewButton>();
                PrimaryButtons.CollectionChanged += NavButtonsChanged;
                SecondaryButtons.CollectionChanged += NavButtonsChanged;
                new KeyboardService().AfterWindowZGesture = () => { HamburgerCommand.Execute(null); };
                ShellSplitView.RegisterPropertyChangedCallback(SplitView.IsPaneOpenProperty, (d, e) =>
                {
                    if (SecondaryButtonOrientation.Equals(Orientation.Horizontal) && ShellSplitView.IsPaneOpen)
                        _SecondaryButtonStackPanel.Orientation = Orientation.Horizontal;
                    else
                        _SecondaryButtonStackPanel.Orientation = Orientation.Vertical;
                });
            }
        }

        private void NavButtonsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var item in e.NewItems)
            {
                var button = item as SplitViewButton;
                if (button == null && _navButtons.Contains(item))
                    continue;
                button.Loaded += NavButton_Loaded;
            }
        }

        public void HighlightCorrectButton()
        {
            if (NavigationService == null)
                return;
            var pageType = NavigationService.CurrentPageType;
            var pageParam = NavigationService.CurrentPageParam;
            //var values = _navButtons.Select(x => x.Value);
            var button = _navButtons.FirstOrDefault(x => x.PageType == pageType && x.PageParameter == pageParam);
            Selected = button;
        }

        #region commands

        Mvvm.DelegateCommand _hamburgerCommand;
        internal Mvvm.DelegateCommand HamburgerCommand { get { return _hamburgerCommand ?? (_hamburgerCommand = new Mvvm.DelegateCommand(ExecuteHamburger)); } }
        void ExecuteHamburger() { IsOpen = !IsOpen; }

        Mvvm.DelegateCommand<SplitViewButton> _navCommand;
        public Mvvm.DelegateCommand<SplitViewButton> NavCommand { get { return _navCommand ?? (_navCommand = new Mvvm.DelegateCommand<SplitViewButton>(ExecuteNav)); } }
        void ExecuteNav(SplitViewButton commandInfo)
        {
            if (commandInfo == null)
                throw new NullReferenceException("CommandParameter is not set");
            try
            {
                if (commandInfo.PageType != null)
                    Selected = commandInfo;
            }
            finally
            {
                if (commandInfo.ClearHistory)
                    NavigationService.ClearHistory();
            }
        }

        #endregion

        #region VisualStateValues

        public double VisualStateNarrowMinWidth
        {
            get { return VisualStateNarrowTrigger.MinWindowWidth; }
            set { SetValue(VisualStateNarrowMinWidthProperty, VisualStateNarrowTrigger.MinWindowWidth = value); }
        }
        public static readonly DependencyProperty VisualStateNarrowMinWidthProperty =
            DependencyProperty.Register(nameof(VisualStateNarrowMinWidth), typeof(double),
                typeof(HamburgerMenu), new PropertyMetadata(null, (d, e) => { (d as HamburgerMenu).VisualStateNarrowMinWidth = (double)e.NewValue; }));

        public double VisualStateNormalMinWidth
        {
            get { return VisualStateNormalTrigger.MinWindowWidth; }
            set { SetValue(VisualStateNormalMinWidthProperty, VisualStateNormalTrigger.MinWindowWidth = value); }
        }
        public static readonly DependencyProperty VisualStateNormalMinWidthProperty =
            DependencyProperty.Register(nameof(VisualStateNormalMinWidth), typeof(double),
                typeof(HamburgerMenu), new PropertyMetadata(null, (d, e) => { (d as HamburgerMenu).VisualStateNormalMinWidth = (double)e.NewValue; }));

        public double VisualStateWideMinWidth
        {
            get { return VisualStateWideTrigger.MinWindowWidth; }
            set { SetValue(VisualStateWideMinWidthProperty, VisualStateWideTrigger.MinWindowWidth = value); }
        }
        public static readonly DependencyProperty VisualStateWideMinWidthProperty =
            DependencyProperty.Register(nameof(VisualStateWideMinWidth), typeof(double),
                typeof(HamburgerMenu), new PropertyMetadata(null, (d, e) => { (d as HamburgerMenu).VisualStateWideMinWidth = (double)e.NewValue; }));

        #endregion
        public Orientation SecondaryButtonOrientation
        {
            get { return (Orientation)GetValue(SecondaryButtonOrientationProperty); }
            set { SetValue(SecondaryButtonOrientationProperty, value); }
        }
        public static readonly DependencyProperty SecondaryButtonOrientationProperty =
            DependencyProperty.Register(nameof(SecondaryButtonOrientation), typeof(Orientation),
                typeof(HamburgerMenu), new PropertyMetadata(Orientation.Vertical));

        public SplitViewButton Selected
        {
            get { return GetValue(SelectedProperty) as SplitViewButton; }
            set
            {
                IsOpen = false;

                // ensure dp is correct (if diff)
                var previous = Selected;
                if (previous != value)
                {
                    SetValue(SelectedProperty, value);
                    // undo previous
                    if (previous != null)
                    {
                        previous.RaiseUnselected();
                    }
                }

                // reset all
                //var values = _navButtons.FirstOrDefault(x => x.Value);
                foreach (var item in _navButtons)
                {
                    item.IsEnabled = true;
                    item.IsChecked = false;
                }

                // that's it if null
                if (value == null)
                    return;

                // setup new value
                value.IsChecked = true;
                if (previous != value)
                    value.RaiseSelected();

                // navigate only to new pages
                if (value.PageType != null && (NavigationService.CurrentPageType != value.PageType || NavigationService.CurrentPageParam != value.PageParameter))
                {
                    NavigationService.Navigate(value.PageType, value.PageParameter);
                    value.IsEnabled = false;
                    HighlightCorrectButton();
                }
            }
        }
        public static readonly DependencyProperty SelectedProperty =
            DependencyProperty.Register("Selected", typeof(SplitViewButton),
                typeof(HamburgerMenu), new PropertyMetadata(null, (d, e) =>
                {
                    if ((d as HamburgerMenu).Selected != (SplitViewButton)e.NewValue)
                        (d as HamburgerMenu).Selected = (SplitViewButton)e.NewValue;
                }));

        public bool IsOpen
        {
            get
            {
                var open = ShellSplitView.IsPaneOpen;
                if (open != (bool)GetValue(IsOpenProperty))
                    SetValue(IsOpenProperty, open);
                return open;
            }
            set
            {
                var open = ShellSplitView.IsPaneOpen;
                if (open == value)
                    return;
                if (value)
                {
                    ShellSplitView.IsPaneOpen = true;
                }
                else
                {
                    // collapse the window
                    if (ShellSplitView.DisplayMode == SplitViewDisplayMode.Overlay && ShellSplitView.IsPaneOpen)
                        ShellSplitView.IsPaneOpen = false;
                    else if (ShellSplitView.DisplayMode == SplitViewDisplayMode.CompactOverlay && ShellSplitView.IsPaneOpen)
                        ShellSplitView.IsPaneOpen = false;
                }
                SetValue(IsOpenProperty, value);
            }
        }
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(nameof(IsOpen), typeof(bool),
                typeof(HamburgerMenu), new PropertyMetadata(false,
                    (d, e) => { (d as HamburgerMenu).IsOpen = (bool)e.NewValue; }));

        public ObservableItemCollection<SplitViewButton> PrimaryButtons
        {
            get
            {
                var PrimaryButtons = (ObservableItemCollection<SplitViewButton>)base.GetValue(PrimaryButtonsProperty);
                if (PrimaryButtons == null)
                    base.SetValue(PrimaryButtonsProperty, PrimaryButtons = new ObservableItemCollection<SplitViewButton>());
                return PrimaryButtons;
            }
            set { SetValue(PrimaryButtonsProperty, value); }
        }
        public static readonly DependencyProperty PrimaryButtonsProperty =
            DependencyProperty.Register(nameof(PrimaryButtons), typeof(ObservableItemCollection<SplitViewButton>),
                typeof(HamburgerMenu), new PropertyMetadata(null));

        private NavigationService _navigationService;
        public NavigationService NavigationService
        {
            get { return _navigationService; }
            set
            {
                _navigationService = value;
                if (NavigationService.Frame.BackStackDepth > 0)
                {
                    // display content inside the splitview
                    ShellSplitView.Content = NavigationService.Frame;
                }
                else
                {
                    // display content without splitview (splash scenario)
                    NavigationService.AfterRestoreSavedNavigation += (s, e) => IsFullScreen = false;
                    NavigationService.FrameFacade.Navigated += (s, e) => IsFullScreen = false;
                    IsFullScreen = true;
                }
                NavigationService.FrameFacade.Navigated += (s, e) => HighlightCorrectButton();
                NavigationService.AfterRestoreSavedNavigation += (s, e) => HighlightCorrectButton();
                ShellSplitView.RegisterPropertyChangedCallback(SplitView.IsPaneOpenProperty, (s, e) =>
                {
                    // update width
                    PaneWidth = !ShellSplitView.IsPaneOpen ? ShellSplitView.CompactPaneLength : ShellSplitView.OpenPaneLength;
                });
            }
        }

        public bool IsFullScreen
        {
            get { return (bool)GetValue(IsFullScreenProperty); }
            set { SetValue(IsFullScreenProperty, value); }
        }
        public static readonly DependencyProperty IsFullScreenProperty =
            DependencyProperty.Register(nameof(IsFullScreen), typeof(bool),
                typeof(HamburgerMenu), new PropertyMetadata(false, (d, e) =>
                {
                    var menu = d as HamburgerMenu;
                    if ((bool)e.NewValue)
                    {
                        if (menu.RootGrid.Children.Contains(menu.NavigationService.Frame))
                            return;
                        menu.NavigationService.Frame.SetValue(Grid.ColumnProperty, 0);
                        menu.NavigationService.Frame.SetValue(Grid.ColumnSpanProperty, int.MaxValue);
                        menu.NavigationService.Frame.SetValue(Grid.RowProperty, 0);
                        menu.NavigationService.Frame.SetValue(Grid.RowSpanProperty, int.MaxValue);
                        menu.RootGrid.Children.Add(menu.NavigationService.Frame);
                    }
                    else
                    {
                        if (menu.RootGrid.Children.Contains(menu.NavigationService.Frame))
                            menu.RootGrid.Children.Remove(menu.NavigationService.Frame);
                        menu.ShellSplitView.Content = menu.NavigationService.Frame;
                    }
                }));


        public ObservableItemCollection<SplitViewButton> SecondaryButtons
        {
            get
            {
                var SecondaryButtons = (ObservableItemCollection<SplitViewButton>)base.GetValue(SecondaryButtonsProperty);
                if (SecondaryButtons == null)
                    base.SetValue(SecondaryButtonsProperty, SecondaryButtons = new ObservableItemCollection<SplitViewButton>());
                return SecondaryButtons;
            }
            set { SetValue(SecondaryButtonsProperty, value); }
        }
        public static readonly DependencyProperty SecondaryButtonsProperty =
            DependencyProperty.Register(nameof(SecondaryButtons), typeof(ObservableItemCollection<SplitViewButton>),
                typeof(HamburgerMenu), new PropertyMetadata(null));

        public double PaneWidth
        {
            get { return (double)GetValue(PaneWidthProperty); }
            set { SetValue(PaneWidthProperty, value); }
        }
        public static readonly DependencyProperty PaneWidthProperty =
            DependencyProperty.Register("PaneWidth", typeof(double),
                typeof(HamburgerMenu), new PropertyMetadata(220));

        public UIElement HeaderContent
        {
            get { return (UIElement)GetValue(HeaderContentProperty); }
            set { SetValue(HeaderContentProperty, value); }
        }
        public static readonly DependencyProperty HeaderContentProperty =
            DependencyProperty.Register(nameof(HeaderContent), typeof(UIElement),
                typeof(HamburgerMenu), null);

        List<SplitViewButton> _navButtons = new List<SplitViewButton>();
        void NavButton_Loaded(object sender, RoutedEventArgs e)
        {
            // add this radio to the list
            var button = sender as SplitViewButton;
            button.NavCommand = NavCommand;
            //var info = radio.DataContext as SplitViewButton;
            _navButtons.Add(button);

            // udpate UI
            HighlightCorrectButton();
        }

        private void PaneContent_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            HamburgerCommand.Execute(null);
        }



        public event PropertyChangedEventHandler PropertyChanged;

        StackPanel _SecondaryButtonStackPanel;
        private void SecondaryButtonStackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            _SecondaryButtonStackPanel = sender as StackPanel;
        }
    }
}
