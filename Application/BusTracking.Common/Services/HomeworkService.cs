namespace BusTracking.Common.Services
{
    public class HomeworkService : IHomeworkService
    {
        private readonly AppDbContext _db;
        private readonly IFcmPushNotificationService _fcmService;
        private readonly IServiceScopeFactory _scopeFactory;

        public HomeworkService(
            AppDbContext db,
            IFcmPushNotificationService fcmService,
            IServiceScopeFactory scopeFactory)
        {
            _db = db;
            _fcmService = fcmService;
            _scopeFactory = scopeFactory;
        }

        public async Task<ApiResponse<HomeworkDto>> CreateHomeworkAsync(CreateHomeworkDto dto, int teacherUserId)
        {
            if (dto.AcademicYearId <= 0 || dto.StandardId <= 0 || string.IsNullOrWhiteSpace(dto.Title))
            {
                return ApiResponse<HomeworkDto>.Fail("Academic Year, Standard, and Homework Title are required.");
            }

            int? targetSectionId = dto.SectionId > 0 ? dto.SectionId : null;
            if (!targetSectionId.HasValue && dto.StandardId > 0)
            {
                var sec = await _db.Sections.FirstOrDefaultAsync(s => s.StandardId == dto.StandardId && s.IsActive);
                if (sec != null) targetSectionId = sec.SectionId;
            }

            var homework = new Homework
            {
                AcademicYearId = dto.AcademicYearId,
                StandardId = dto.StandardId,
                SectionId = targetSectionId,

                SubjectId = dto.SubjectId > 0 ? dto.SubjectId : null,
                TeacherUserId = teacherUserId,
                Title = dto.Title.Trim(),
                Description = dto.Description.Trim(),
                AttachmentUrl = dto.AttachmentUrl,
                DueDate = dto.DueDate,
                CreatedAt = DateTime.UtcNow,
                IsActive = dto.IsActive
            };

            _db.Homeworks.Add(homework);
            await _db.SaveChangesAsync();

            // Send FCM Push Notifications & DB Notifications safely on a separate DbContext scope
            if (homework.IsActive)
            {
                int homeworkId = homework.HomeworkId;
                string homeworkTitle = homework.Title;
                DateTime dueDate = homework.DueDate;
                int standardId = dto.StandardId;
                int? sectionId = dto.SectionId > 0 ? dto.SectionId : null;

                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                        var fcm = scope.ServiceProvider.GetService<IFcmPushNotificationService>();

                        var studentUserIds = await db.Students
                            .Where(s => s.StandardId == standardId && (!sectionId.HasValue || s.SectionId == sectionId))
                            .Select(s => s.UserId)
                            .ToListAsync();

                        if (studentUserIds.Count > 0)
                        {
                            foreach (var uid in studentUserIds)
                            {
                                db.Notifications.Add(new Notification
                                {
                                    RecipientUserId = uid,
                                    Title = $"New Homework Assigned: {homeworkTitle}",
                                    Body = $"Due on {dueDate:dd MMM yyyy}. Subject: {homeworkTitle}",
                                    NotificationType = NotificationType.Broadcast,
                                    ReferenceId = homeworkId,
                                    ReferenceType = "Homework"
                                });
                            }
                            await db.SaveChangesAsync();

                            if (fcm != null)
                            {
                                await fcm.SendBroadcastPushAsync(
                                    studentUserIds,
                                    $"New Homework Assigned: {homeworkTitle}",
                                    $"Due on {dueDate:dd MMM yyyy}.",
                                    "Homework");
                            }

                        }
                    }
                    catch
                    {
                        // Ignore background notification exceptions safely without locking primary DbContext
                    }
                });
            }

            return await GetHomeworkByIdAsync(homework.HomeworkId);
        }

        public async Task<ApiResponse<HomeworkDto>> UpdateHomeworkAsync(UpdateHomeworkDto dto, int teacherUserId)
        {
            var homework = await _db.Homeworks.FirstOrDefaultAsync(h => h.HomeworkId == dto.HomeworkId && h.TeacherUserId == teacherUserId);
            if (homework == null) return ApiResponse<HomeworkDto>.Fail("Homework assignment not found or permission denied.");

            if (dto.AcademicYearId <= 0 || dto.StandardId <= 0 || string.IsNullOrWhiteSpace(dto.Title))
            {
                return ApiResponse<HomeworkDto>.Fail("Academic Year, Standard, and Homework Title are required.");
            }

            int? targetSectionId = dto.SectionId > 0 ? dto.SectionId : null;
            if (!targetSectionId.HasValue && dto.StandardId > 0)
            {
                var sec = await _db.Sections.FirstOrDefaultAsync(s => s.StandardId == dto.StandardId && s.IsActive);
                if (sec != null) targetSectionId = sec.SectionId;
            }

            homework.AcademicYearId = dto.AcademicYearId;
            homework.StandardId = dto.StandardId;
            homework.SectionId = targetSectionId;

            homework.SubjectId = dto.SubjectId > 0 ? dto.SubjectId : null;
            homework.Title = dto.Title.Trim();
            homework.Description = dto.Description.Trim();
            if (!string.IsNullOrWhiteSpace(dto.AttachmentUrl))
            {
                homework.AttachmentUrl = dto.AttachmentUrl;
            }
            homework.DueDate = dto.DueDate;
            homework.IsActive = dto.IsActive;

            await _db.SaveChangesAsync();
            return await GetHomeworkByIdAsync(homework.HomeworkId);
        }

        public async Task<ApiResponse<List<HomeworkDto>>> GetHomeworksForTeacherAsync(int teacherUserId, int? academicYearId, int? standardId, int? sectionId)
        {
            var query = _db.Homeworks
                .Include(h => h.AcademicYear)
                .Include(h => h.Standard)
                .Include(h => h.Section)
                .Include(h => h.Subject)
                .Include(h => h.TeacherUser)
                .Include(h => h.Submissions)
                .Where(h => h.TeacherUserId == teacherUserId);

            if (academicYearId.HasValue && academicYearId.Value > 0)
                query = query.Where(h => h.AcademicYearId == academicYearId.Value);

            if (standardId.HasValue && standardId.Value > 0)
                query = query.Where(h => h.StandardId == standardId.Value);

            if (sectionId.HasValue && sectionId.Value > 0)
                query = query.Where(h => h.SectionId == sectionId.Value);

            var list = await query.OrderByDescending(h => h.CreatedAt).ToListAsync();

            var dtos = list.Select(h => new HomeworkDto
            {
                HomeworkId = h.HomeworkId,
                AcademicYearId = h.AcademicYearId,
                YearName = h.AcademicYear?.YearName ?? "",
                StandardId = h.StandardId,
                StandardName = h.Standard?.StandardName ?? "",
                SectionId = h.SectionId,
                SectionName = h.Section?.SectionName,
                SubjectId = h.SubjectId,
                SubjectName = h.Subject?.SubjectName,
                TeacherUserId = h.TeacherUserId,
                TeacherName = h.TeacherUser?.FullName ?? "",
                Title = h.Title,
                Description = h.Description,
                AttachmentUrl = h.AttachmentUrl,
                DueDate = h.DueDate,
                CreatedAt = h.CreatedAt,
                IsActive = h.IsActive,
                SubmissionsCount = h.Submissions.Count
            }).ToList();

            return ApiResponse<List<HomeworkDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<List<HomeworkDto>>> GetHomeworksForStudentAsync(int studentUserId, int? academicYearId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == studentUserId);
            if (student == null) return ApiResponse<List<HomeworkDto>>.Ok(new List<HomeworkDto>());

            var query = _db.Homeworks
                .Include(h => h.AcademicYear)
                .Include(h => h.Standard)
                .Include(h => h.Section)
                .Include(h => h.Subject)
                .Include(h => h.TeacherUser)
                .Include(h => h.Submissions)
                .Where(h => h.IsActive && h.StandardId == student.StandardId);

            if (student.SectionId.HasValue && student.SectionId.Value > 0)
            {
                query = query.Where(h => !h.SectionId.HasValue || h.SectionId == student.SectionId.Value);
            }

            if (academicYearId.HasValue && academicYearId.Value > 0)
            {
                query = query.Where(h => h.AcademicYearId == academicYearId.Value);
            }
            else
            {
                var activeSession = await _db.AcademicYears.FirstOrDefaultAsync(y => y.SchoolId == student.SchoolId && y.IsCurrent && y.IsActive);
                if (activeSession != null)
                {
                    query = query.Where(h => h.AcademicYearId == activeSession.AcademicYearId);
                }
            }

            if (fromDate.HasValue)
            {
                var from = fromDate.Value.Date;
                query = query.Where(h => h.CreatedAt.Date >= from || h.DueDate.Date >= from);
            }

            if (toDate.HasValue)
            {
                var to = toDate.Value.Date;
                query = query.Where(h => h.CreatedAt.Date <= to || h.DueDate.Date <= to);
            }

            var list = await query.OrderByDescending(h => h.CreatedAt).ToListAsync();


            var dtos = list.Select(h =>
            {
                var sub = h.Submissions.FirstOrDefault(s => s.StudentId == student.StudentId);
                return new HomeworkDto
                {
                    HomeworkId = h.HomeworkId,
                    AcademicYearId = h.AcademicYearId,
                    YearName = h.AcademicYear?.YearName ?? "",
                    StandardId = h.StandardId,
                    StandardName = h.Standard?.StandardName ?? "",
                    SectionId = h.SectionId,
                    SectionName = h.Section?.SectionName,
                    SubjectId = h.SubjectId,
                    SubjectName = h.Subject?.SubjectName,
                    TeacherUserId = h.TeacherUserId,
                    TeacherName = h.TeacherUser?.FullName ?? "",
                    Title = h.Title,
                    Description = h.Description,
                    AttachmentUrl = h.AttachmentUrl,
                    DueDate = h.DueDate,
                    CreatedAt = h.CreatedAt,
                    IsActive = h.IsActive,
                    SubmissionsCount = sub != null ? 1 : 0,
                    IsSubmitted = sub != null,
                    SubmissionStatus = sub?.Status ?? (sub != null ? "Submitted" : "Pending"),
                    TeacherRemarks = sub?.TeacherRemarks,
                    MarksObtained = sub?.MarksObtained
                };
            }).ToList();

            return ApiResponse<List<HomeworkDto>>.Ok(dtos);
        }


        public async Task<ApiResponse<HomeworkDto>> GetHomeworkByIdAsync(int homeworkId)
        {
            var h = await _db.Homeworks
                .Include(x => x.AcademicYear)
                .Include(x => x.Standard)
                .Include(x => x.Section)
                .Include(x => x.Subject)
                .Include(x => x.TeacherUser)
                .Include(x => x.Submissions)
                .FirstOrDefaultAsync(x => x.HomeworkId == homeworkId);

            if (h == null) return ApiResponse<HomeworkDto>.Fail("Homework assignment not found.");

            var dto = new HomeworkDto
            {
                HomeworkId = h.HomeworkId,
                AcademicYearId = h.AcademicYearId,
                YearName = h.AcademicYear?.YearName ?? "",
                StandardId = h.StandardId,
                StandardName = h.Standard?.StandardName ?? "",
                SectionId = h.SectionId,
                SectionName = h.Section?.SectionName,
                SubjectId = h.SubjectId,
                SubjectName = h.Subject?.SubjectName,
                TeacherUserId = h.TeacherUserId,
                TeacherName = h.TeacherUser?.FullName ?? "",
                Title = h.Title,
                Description = h.Description,
                AttachmentUrl = h.AttachmentUrl,
                DueDate = h.DueDate,
                CreatedAt = h.CreatedAt,
                IsActive = h.IsActive,
                SubmissionsCount = h.Submissions.Count
            };

            return ApiResponse<HomeworkDto>.Ok(dto);
        }

        public async Task<ApiResponse<bool>> SubmitHomeworkAsync(SubmitHomeworkDto dto, int studentUserId)
        {
            var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == studentUserId);
            if (student == null) return ApiResponse<bool>.Fail("Student profile not found.");

            var homework = await _db.Homeworks.FirstOrDefaultAsync(h => h.HomeworkId == dto.HomeworkId && h.IsActive);
            if (homework == null) return ApiResponse<bool>.Fail("Homework assignment not found or inactive.");

            var existing = await _db.HomeworkSubmissions.FirstOrDefaultAsync(s => s.HomeworkId == dto.HomeworkId && s.StudentId == student.StudentId);
            if (existing != null)
            {
                existing.SubmissionText = dto.SubmissionText;
                if (!string.IsNullOrWhiteSpace(dto.AttachmentUrl)) existing.AttachmentUrl = dto.AttachmentUrl;
                existing.SubmittedAt = DateTime.UtcNow;
                existing.Status = "Resubmitted";
            }

            else
            {
                var sub = new HomeworkSubmission
                {
                    HomeworkId = dto.HomeworkId,
                    StudentId = student.StudentId,
                    SubmissionText = dto.SubmissionText,
                    AttachmentUrl = dto.AttachmentUrl,
                    SubmittedAt = DateTime.UtcNow,
                    Status = "Submitted"
                };
                _db.HomeworkSubmissions.Add(sub);
            }

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Homework submitted successfully.");
        }

        public async Task<ApiResponse<List<HomeworkSubmissionDto>>> GetSubmissionsForHomeworkAsync(int homeworkId)
        {
            var homework = await _db.Homeworks.FirstOrDefaultAsync(h => h.HomeworkId == homeworkId);
            if (homework == null) return ApiResponse<List<HomeworkSubmissionDto>>.Fail("Homework assignment not found.");

            var enrolledStudents = await _db.Students
                .Include(s => s.User)
                .Where(s => s.StandardId == homework.StandardId && (!homework.SectionId.HasValue || !s.SectionId.HasValue || s.SectionId == homework.SectionId))
                .ToListAsync();

            var submissions = await _db.HomeworkSubmissions
                .Include(sub => sub.Student)
                    .ThenInclude(st => st.User)
                .Where(sub => sub.HomeworkId == homeworkId)
                .ToListAsync();

            var studentMap = new Dictionary<int, StudentDetail>();
            foreach (var st in enrolledStudents)
            {
                studentMap[st.StudentId] = st;
            }
            foreach (var sub in submissions)
            {
                if (sub.Student != null && !studentMap.ContainsKey(sub.StudentId))
                {
                    studentMap[sub.StudentId] = sub.Student;
                }
            }

            var list = new List<HomeworkSubmissionDto>();
            foreach (var st in studentMap.Values)
            {
                var sub = submissions.FirstOrDefault(s => s.StudentId == st.StudentId);
                list.Add(new HomeworkSubmissionDto
                {
                    SubmissionId = sub?.SubmissionId ?? 0,
                    HomeworkId = homeworkId,
                    HomeworkTitle = homework.Title,
                    StudentId = st.StudentId,
                    StudentCode = st.StudentCode ?? $"STD-{st.StudentId}",
                    StudentName = st.User?.FullName ?? $"Student {st.StudentId}",
                    ProfileImageUrl = st.User?.ProfileImageUrl,
                    SubmissionText = sub?.SubmissionText,
                    AttachmentUrl = sub?.AttachmentUrl,
                    SubmittedAt = sub?.SubmittedAt ?? DateTime.MinValue,
                    Status = sub?.Status ?? "Pending",
                    TeacherRemarks = sub?.TeacherRemarks,
                    MarksObtained = sub?.MarksObtained,
                    EvaluatedAt = sub?.EvaluatedAt
                });
            }

            return ApiResponse<List<HomeworkSubmissionDto>>.Ok(list.OrderBy(x => x.StudentName).ToList());
        }

        public async Task<ApiResponse<bool>> EvaluateSubmissionAsync(EvaluateHomeworkSubmissionDto dto, int teacherUserId)
        {
            var sub = await _db.HomeworkSubmissions.FirstOrDefaultAsync(s => s.SubmissionId == dto.SubmissionId);
            if (sub == null) return ApiResponse<bool>.Fail("Submission not found.");

            sub.TeacherRemarks = dto.TeacherRemarks;
            sub.MarksObtained = dto.MarksObtained;
            sub.Status = string.IsNullOrWhiteSpace(dto.Status) ? "Reviewed" : dto.Status;
            sub.EvaluatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true);
        }

        public async Task<ApiResponse<bool>> DeleteHomeworkAsync(int homeworkId, int teacherUserId)
        {
            var h = await _db.Homeworks.FirstOrDefaultAsync(x => x.HomeworkId == homeworkId && x.TeacherUserId == teacherUserId);
            if (h == null) return ApiResponse<bool>.Fail("Homework assignment not found or permission denied.");

            var submissionsCount = await _db.HomeworkSubmissions.CountAsync(s => s.HomeworkId == homeworkId);
            if (submissionsCount > 0)
            {
                return ApiResponse<bool>.Fail($"Cannot delete homework '{h.Title}' because {submissionsCount} student submission(s) have already been submitted.");
            }

            _db.Homeworks.Remove(h);
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Homework assignment permanently deleted.");
        }
    }
}
