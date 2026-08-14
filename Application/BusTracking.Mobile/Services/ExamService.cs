namespace BusTracking.Mobile.Services
{
    public class ExamService : IExamService
    {
        private readonly IApiService _api;
        private readonly ICacheService _cache;
        private readonly IAuthService _auth;

        public ExamService(IApiService api, ICacheService cache, IAuthService auth)
        {
            _api = api;
            _cache = cache;
            _auth = auth;
        }

        private string CurrentRole => _auth.CurrentRole;

        private string GetTermsEndpoint() => CurrentRole switch
        {
            Constants.Roles.SuperAdmin => Constants.Admin.ExamTerms,
            Constants.Roles.BusCoordinator => Constants.Coordinator.ExamTerms,
            Constants.Roles.Teacher => Constants.Teacher.ExamTerms,
            Constants.Roles.Student => Constants.Student.ExamTerms,
            Constants.Roles.Parent => Constants.Parent.ExamTerms,
            _ => Constants.Student.ExamTerms
        };

        private string GetTermCreateEndpoint() => CurrentRole switch
        {
            Constants.Roles.SuperAdmin => Constants.Admin.ExamTermCreate,
            Constants.Roles.BusCoordinator => Constants.Coordinator.ExamTermCreate,
            _ => Constants.Admin.ExamTermCreate
        };

        private string GetTermUpdateEndpoint(int id) => CurrentRole switch
        {
            Constants.Roles.SuperAdmin => string.Format(Constants.Admin.ExamTermUpdate, id),
            Constants.Roles.BusCoordinator => string.Format(Constants.Coordinator.ExamTermUpdate, id),
            _ => string.Format(Constants.Admin.ExamTermUpdate, id)
        };

        private string GetTermDeleteEndpoint(int id) => CurrentRole switch
        {
            Constants.Roles.SuperAdmin => string.Format(Constants.Admin.ExamTermDelete, id),
            Constants.Roles.BusCoordinator => string.Format(Constants.Coordinator.ExamTermDelete, id),
            _ => string.Format(Constants.Admin.ExamTermDelete, id)
        };

        private string GetSchedulesEndpoint() => CurrentRole switch
        {
            Constants.Roles.SuperAdmin => Constants.Admin.ExamSchedules,
            Constants.Roles.BusCoordinator => Constants.Coordinator.ExamSchedules,
            Constants.Roles.Teacher => Constants.Teacher.ExamSchedules,
            Constants.Roles.Student => Constants.Student.ExamDatesheet,
            Constants.Roles.Parent => Constants.Parent.ExamDatesheet,
            _ => Constants.Teacher.ExamSchedules
        };

        private string GetScheduleCreateEndpoint() => CurrentRole switch
        {
            Constants.Roles.SuperAdmin => Constants.Admin.ExamScheduleCreate,
            Constants.Roles.BusCoordinator => Constants.Coordinator.ExamScheduleCreate,
            _ => Constants.Admin.ExamScheduleCreate
        };

        private string GetScheduleUpdateEndpoint(int id) => CurrentRole switch
        {
            Constants.Roles.SuperAdmin => string.Format(Constants.Admin.ExamScheduleUpdate, id),
            Constants.Roles.BusCoordinator => string.Format(Constants.Coordinator.ExamScheduleUpdate, id),
            _ => string.Format(Constants.Admin.ExamScheduleUpdate, id)
        };

        private string GetScheduleDeleteEndpoint(int id) => CurrentRole switch
        {
            Constants.Roles.SuperAdmin => string.Format(Constants.Admin.ExamScheduleDelete, id),
            Constants.Roles.BusCoordinator => string.Format(Constants.Coordinator.ExamScheduleDelete, id),
            _ => string.Format(Constants.Admin.ExamScheduleDelete, id)
        };

        private string GetMarksGridEndpoint() => CurrentRole switch
        {
            Constants.Roles.SuperAdmin => Constants.Admin.ExamMarksGrid,
            Constants.Roles.BusCoordinator => Constants.Coordinator.ExamMarksGrid,
            Constants.Roles.Teacher => Constants.Teacher.ExamMarksGrid,
            _ => Constants.Teacher.ExamMarksGrid
        };

        private string GetSaveMarksEndpoint() => CurrentRole switch
        {
            Constants.Roles.SuperAdmin => Constants.Admin.ExamSaveMarks,
            Constants.Roles.BusCoordinator => Constants.Coordinator.ExamSaveMarks,
            Constants.Roles.Teacher => Constants.Teacher.ExamSaveMarks,
            _ => Constants.Teacher.ExamSaveMarks
        };

        private string GetReportCardEndpoint() => CurrentRole switch
        {
            Constants.Roles.SuperAdmin => Constants.Admin.ExamReportCard,
            Constants.Roles.BusCoordinator => Constants.Coordinator.ExamReportCard,
            Constants.Roles.Student => Constants.Student.ExamReportCard,
            Constants.Roles.Parent => Constants.Parent.ExamReportCard,
            _ => Constants.Student.ExamReportCard
        };

        public async Task<List<ExamTermItem>> GetExamTermsAsync(int? academicYearId = null)
        {
            var url = GetTermsEndpoint();
            if (academicYearId.HasValue && academicYearId.Value > 0)
            {
                url += $"?academicYearId={academicYearId.Value}";
            }

            var r = await _api.GetAsync<List<ExamTermItem>>(url);
            return r.Data ?? [];
        }

        public async Task<ApiResponse<object>> CreateExamTermAsync(CreateExamTermRequest req)
        {
            var url = GetTermCreateEndpoint();
            return await _api.PostAsync<object>(url, req);
        }

        public async Task<ApiResponse<object>> UpdateExamTermAsync(int examTermId, UpdateExamTermRequest req)
        {
            var url = GetTermUpdateEndpoint(examTermId);
            return await _api.PutAsync<object>(url, req);
        }

        public async Task<ApiResponse<object>> DeleteExamTermAsync(int examTermId)
        {
            var url = GetTermDeleteEndpoint(examTermId);
            return await _api.DeleteAsync<object>(url);
        }

        public async Task<List<ExamScheduleItem>> GetExamSchedulesAsync(int? examTermId = null, int? standardId = null)
        {
            var baseUrl = GetSchedulesEndpoint();
            var query = new List<string>();
            if (examTermId.HasValue) query.Add($"examTermId={examTermId.Value}");
            if (standardId.HasValue) query.Add($"standardId={standardId.Value}");

            var url = query.Count > 0 ? $"{baseUrl}?{string.Join("&", query)}" : baseUrl;
            var r = await _api.GetAsync<List<ExamScheduleItem>>(url);
            return r.Data ?? [];
        }

        public async Task<ApiResponse<object>> CreateExamScheduleAsync(CreateExamScheduleRequest req)
        {
            var url = GetScheduleCreateEndpoint();
            return await _api.PostAsync<object>(url, req);
        }

        public async Task<ApiResponse<object>> UpdateExamScheduleAsync(int examScheduleId, UpdateExamScheduleRequest req)
        {
            var url = GetScheduleUpdateEndpoint(examScheduleId);
            return await _api.PutAsync<object>(url, req);
        }

        public async Task<ApiResponse<object>> DeleteExamScheduleAsync(int examScheduleId)
        {
            var url = GetScheduleDeleteEndpoint(examScheduleId);
            return await _api.DeleteAsync<object>(url);
        }

        public async Task<List<StudentMarksGridItem>> GetStudentMarksGridAsync(int examScheduleId, int? sectionId)
        {
            var baseUrl = GetMarksGridEndpoint();
            var url = sectionId.HasValue && sectionId.Value > 0
                ? $"{baseUrl}?examScheduleId={examScheduleId}&sectionId={sectionId.Value}"
                : $"{baseUrl}?examScheduleId={examScheduleId}";
            var r = await _api.GetAsync<List<StudentMarksGridItem>>(url);
            return r.Data ?? [];
        }

        public async Task<ApiResponse<object>> SaveStudentMarksGridAsync(SaveStudentMarksGridRequest req)
        {
            var url = GetSaveMarksEndpoint();
            return await _api.PostAsync<object>(url, req);
        }

        public async Task<StudentReportCardItem?> GetStudentReportCardAsync(int examTermId, int? studentId = null)
        {
            var baseUrl = GetReportCardEndpoint();
            var url = $"{baseUrl}?examTermId={examTermId}";
            if (studentId.HasValue && studentId.Value > 0)
            {
                url += $"&studentId={studentId.Value}";
            }

            var r = await _api.GetAsync<StudentReportCardItem>(url);
            return r.Data;
        }
    }
}
