namespace BusTracking.Mobile.Views.Common;

public partial class CalendarPickerPopup : ContentView, INotifyPropertyChanged
{
    private DateTime? _tempSelectedDate;
    private string _displaySelectedDate = "";

    public string DisplaySelectedDate
    {
        get => _displaySelectedDate;
        private set
        {
            if (_displaySelectedDate != value)
            {
                _displaySelectedDate = value;
                OnPropertyChanged();
            }
        }
    }

    public CalendarPickerPopup()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty IsOpenProperty =
        BindableProperty.Create(
            nameof(IsOpen),
            typeof(bool),
            typeof(CalendarPickerPopup),
            false,
            BindingMode.TwoWay,
            propertyChanged: OnIsOpenChanged);

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static readonly BindableProperty SelectedDateProperty =
        BindableProperty.Create(
            nameof(SelectedDate),
            typeof(DateTime?),
            typeof(CalendarPickerPopup),
            null,
            BindingMode.TwoWay,
            propertyChanged: OnSelectedDateChanged);

    public DateTime? SelectedDate
    {
        get => (DateTime?)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(CalendarPickerPopup), "Select Date");

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty MinimumDateProperty =
        BindableProperty.Create(nameof(MinimumDate), typeof(DateTime?), typeof(CalendarPickerPopup), null);

    public DateTime? MinimumDate
    {
        get => (DateTime?)GetValue(MinimumDateProperty);
        set => SetValue(MinimumDateProperty, value);
    }

    public static readonly BindableProperty CloseCommandProperty =
        BindableProperty.Create(nameof(CloseCommand), typeof(ICommand), typeof(CalendarPickerPopup), null);

    public ICommand? CloseCommand
    {
        get => (ICommand?)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    private static void OnIsOpenChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CalendarPickerPopup popup && (bool)newValue)
        {
            var date = popup.SelectedDate ?? DateTime.Today;
            popup.UpdateTempDate(date);
        }
    }

    private static void OnSelectedDateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CalendarPickerPopup popup && newValue is DateTime dt)
        {
            popup.UpdateTempDate(dt);
        }
    }

    private void UpdateTempDate(DateTime dt)
    {
        _tempSelectedDate = dt;
        CalendarControl.SelectedDate = dt;
        DisplaySelectedDate = dt.ToString("ddd, dd MMM yyyy");
    }

    private void OnCalendarSelectionChanged(object? sender, CalendarSelectionChangedEventArgs e)
    {
        if (e.NewValue is DateTime selected)
        {
            _tempSelectedDate = selected;
            DisplaySelectedDate = selected.ToString("ddd, dd MMM yyyy");
        }
    }

    private void OnOkClicked(object? sender, EventArgs e)
    {
        if (_tempSelectedDate.HasValue)
        {
            SelectedDate = _tempSelectedDate.Value;
        }
        IsOpen = false;
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        IsOpen = false;
        if (CloseCommand?.CanExecute(null) == true)
        {
            CloseCommand.Execute(null);
        }
    }
}
