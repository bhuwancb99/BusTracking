namespace BusTracking.Mobile.Services
{
    public class HomeworkMobileService
    {
        private readonly IApiService _apiService;

        public HomeworkMobileService(IApiService apiService)
        {
            _apiService = apiService;
        }


        // ── TEACHER METHODS ────────────────────────────────────────────────────

        public async Task<ApiResponse<List<HomeworkDto>>> GetTeacherHomeworksAsync(int? academicYearId, int? standardId, int? sectionId)
        {
            var url = string.Format(Constants.Teacher.HomeworkList, academicYearId, standardId, sectionId);
            return await _apiService.GetAsync<List<HomeworkDto>>(url)
                   ?? ApiResponse<List<HomeworkDto>>.Fail("Failed to fetch teacher homeworks.");
        }

        public async Task<ApiResponse<HomeworkDto>> CreateHomeworkAsync(CreateHomeworkDto dto, Stream? fileStream, string? fileName)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(dto.AcademicYearId.ToString()), nameof(dto.AcademicYearId));
            content.Add(new StringContent(dto.StandardId.ToString()), nameof(dto.StandardId));
            if (dto.SectionId.HasValue) content.Add(new StringContent(dto.SectionId.Value.ToString()), nameof(dto.SectionId));
            if (dto.SubjectId.HasValue) content.Add(new StringContent(dto.SubjectId.Value.ToString()), nameof(dto.SubjectId));
            content.Add(new StringContent(dto.Title), nameof(dto.Title));
            content.Add(new StringContent(dto.Description), nameof(dto.Description));
            content.Add(new StringContent(dto.DueDate.ToString("yyyy-MM-dd")), nameof(dto.DueDate));
            content.Add(new StringContent(dto.IsActive.ToString().ToLower()), nameof(dto.IsActive));

            if (fileStream != null && !string.IsNullOrEmpty(fileName))
            {
                var fileContent = new StreamContent(fileStream);
                content.Add(fileContent, "attachmentFile", fileName);
            }

            return await _apiService.PostMultipartAsync<HomeworkDto>(Constants.Teacher.HomeworkCreate, content)
                   ?? ApiResponse<HomeworkDto>.Fail("Failed to create homework.");
        }

        public async Task<ApiResponse<HomeworkDto>> UpdateHomeworkAsync(int id, UpdateHomeworkDto dto, Stream? fileStream, string? fileName)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(id.ToString()), nameof(dto.HomeworkId));
            content.Add(new StringContent(dto.AcademicYearId.ToString()), nameof(dto.AcademicYearId));
            content.Add(new StringContent(dto.StandardId.ToString()), nameof(dto.StandardId));
            if (dto.SectionId.HasValue) content.Add(new StringContent(dto.SectionId.Value.ToString()), nameof(dto.SectionId));
            if (dto.SubjectId.HasValue) content.Add(new StringContent(dto.SubjectId.Value.ToString()), nameof(dto.SubjectId));
            content.Add(new StringContent(dto.Title), nameof(dto.Title));
            content.Add(new StringContent(dto.Description), nameof(dto.Description));
            content.Add(new StringContent(dto.DueDate.ToString("yyyy-MM-dd")), nameof(dto.DueDate));
            content.Add(new StringContent(dto.IsActive.ToString().ToLower()), nameof(dto.IsActive));

            if (fileStream != null && !string.IsNullOrEmpty(fileName))
            {
                var fileContent = new StreamContent(fileStream);
                content.Add(fileContent, "attachmentFile", fileName);
            }

            var url = string.Format(Constants.Teacher.HomeworkUpdate, id);
            return await _apiService.PutMultipartAsync<HomeworkDto>(url, content)
                   ?? ApiResponse<HomeworkDto>.Fail("Failed to update homework.");
        }

        public async Task<ApiResponse<bool>> DeleteHomeworkAsync(int id)
        {
            var url = string.Format(Constants.Teacher.HomeworkDelete, id);
            return await _apiService.DeleteAsync<bool>(url)
                   ?? ApiResponse<bool>.Fail("Failed to delete homework.");
        }

        public async Task<ApiResponse<List<HomeworkSubmissionDto>>> GetSubmissionsAsync(int homeworkId)
        {
            var url = string.Format(Constants.Teacher.HomeworkSubmissions, homeworkId);
            return await _apiService.GetAsync<List<HomeworkSubmissionDto>>(url)
                   ?? ApiResponse<List<HomeworkSubmissionDto>>.Fail("Failed to fetch submissions.");
        }

        public async Task<ApiResponse<bool>> EvaluateSubmissionAsync(EvaluateHomeworkSubmissionDto dto)
        {
            return await _apiService.PostAsync<bool>(Constants.Teacher.HomeworkEvaluate, dto)
                   ?? ApiResponse<bool>.Fail("Failed to evaluate submission.");
        }

        // ── STUDENT METHODS ────────────────────────────────────────────────────

        public async Task<ApiResponse<List<HomeworkDto>>> GetStudentHomeworksAsync(int? academicYearId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var url = string.Format(Constants.Student.HomeworkList, academicYearId);
            if (fromDate.HasValue) url += $"&fromDate={fromDate.Value:yyyy-MM-dd}";
            if (toDate.HasValue) url += $"&toDate={toDate.Value:yyyy-MM-dd}";

            return await _apiService.GetAsync<List<HomeworkDto>>(url)
                   ?? ApiResponse<List<HomeworkDto>>.Fail("Failed to fetch student homeworks.");
        }



        public async Task<ApiResponse<bool>> SubmitHomeworkAsync(SubmitHomeworkDto dto, Stream? fileStream, string? fileName)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(dto.HomeworkId.ToString()), nameof(dto.HomeworkId));
            if (!string.IsNullOrWhiteSpace(dto.SubmissionText))
            {
                content.Add(new StringContent(dto.SubmissionText), nameof(dto.SubmissionText));
            }

            if (fileStream != null && !string.IsNullOrEmpty(fileName))
            {
                var fileContent = new StreamContent(fileStream);
                content.Add(fileContent, "solutionFile", fileName);
            }

            return await _apiService.PostMultipartAsync<bool>(Constants.Student.HomeworkSubmit, content)
                   ?? ApiResponse<bool>.Fail("Failed to submit homework.");
        }
    }
}
