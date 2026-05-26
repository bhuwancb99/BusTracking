using BusTracking.Mobile.Interfaces;
using BusTracking.Mobile.Models.Student;
using BusTracking.Mobile.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordStudentListViewModel : BaseViewModel
    {
        private readonly IStudentService _students;

        [ObservableProperty] private ObservableCollection<StudentItem> _items = [];
        [ObservableProperty] private string _searchText = "";

        public bool CanAdd => Can("student.add");
        public bool CanEdit => Can("student.edit");
        public bool CanDelete => Can("student.delete");
        public bool CanView => Can("student.view");

        public CoordStudentListViewModel(IAuthService auth, INavigationService nav, IStudentService students)
            : base(auth, nav) { _students = students; Title = "Students"; }

        public override async Task InitializeAsync() => await LoadAsync();

        [RelayCommand]
        private async Task LoadAsync()
        {
            await RunAsync(async () =>
            {
                var data = await _students.GetAllAsync(SearchText.Trim().Length > 0 ? SearchText : null);
                Items = new ObservableCollection<StudentItem>(data);
                IsEmpty = !Items.Any();
            });
        }

        [RelayCommand] private async Task SearchAsync() => await LoadAsync();
        [RelayCommand] private Task AddAsync() => Nav.GoToAsync("CoordStudentForm");
        [RelayCommand]
        private Task EditAsync(StudentItem s) =>
            Nav.GoToAsync("CoordStudentForm", new Dictionary<string, object> { ["StudentId"] = s.StudentId });
        [RelayCommand]
        private Task ViewAsync(StudentItem s) =>
            Nav.GoToAsync("CoordStudentDetail", new Dictionary<string, object> { ["StudentId"] = s.StudentId });
    }
}
