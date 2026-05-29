namespace BusTracking.Mobile.Viewmodels.Coordinator
{
    public partial class CoordParentDetailViewModel : BaseViewModel, IQueryAttributable
    {
        private readonly IParentService _parents;
        [ObservableProperty] private int _userId;
        [ObservableProperty] private ParentItem? _parent;

        public CoordParentDetailViewModel(IAuthService auth, INavigationService nav, IParentService parents)
            : base(auth, nav) { _parents = parents; Title = "Parent Details"; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("UserId", out var id)) UserId = (int)id;
        }

        public override async Task InitializeAsync()
        {
            await RunAsync(async () => { Parent = await _parents.GetByIdAsync(UserId); });
        }
    }
}