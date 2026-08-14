namespace BusTracking.Common.Services
{
    public class ExamService : IExamService
    {
        private readonly AppDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public ExamService(AppDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        // ── EXAM TERMS ──────────────────────────────────────────────────────

        public async Task<ApiResponse<List<ExamTermDto>>> GetExamTermsAsync(int? academicYearId)
        {
            var query = _db.ExamTerms
                .Include(t => t.AcademicYear)
                .Where(t => t.SchoolId == _currentUser.SchoolId);

            if (academicYearId.HasValue && academicYearId.Value > 0)
            {
                query = query.Where(t => t.AcademicYearId == academicYearId.Value);
            }

            var terms = await query.OrderByDescending(t => t.StartDate).ToListAsync();

            var dtos = new List<ExamTermDto>();
            foreach (var t in terms)
            {
                var schedCount = await _db.ExamSchedules.CountAsync(s => s.ExamTermId == t.ExamTermId);
                dtos.Add(new ExamTermDto
                {
                    ExamTermId = t.ExamTermId,
                    AcademicYearId = t.AcademicYearId,
                    AcademicYearName = t.AcademicYear?.YearName ?? "N/A",
                    TermName = t.TermName,
                    Description = t.Description,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    IsActive = t.IsActive,
                    ScheduleCount = schedCount
                });
            }

            return ApiResponse<List<ExamTermDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<ExamTermDto>> CreateExamTermAsync(CreateExamTermDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.TermName))
                return ApiResponse<ExamTermDto>.Fail("Term name is required.");

            var term = new ExamTerm
            {
                SchoolId = _currentUser.SchoolId,
                AcademicYearId = dto.AcademicYearId,
                TermName = dto.TermName.Trim(),
                Description = dto.Description?.Trim(),
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _db.ExamTerms.Add(term);
            await _db.SaveChangesAsync();

            var year = await _db.AcademicYears.FindAsync(dto.AcademicYearId);

            return ApiResponse<ExamTermDto>.Ok(new ExamTermDto
            {
                ExamTermId = term.ExamTermId,
                AcademicYearId = term.AcademicYearId,
                AcademicYearName = year?.YearName ?? "N/A",
                TermName = term.TermName,
                Description = term.Description,
                StartDate = term.StartDate,
                EndDate = term.EndDate,
                IsActive = term.IsActive,
                ScheduleCount = 0
            }, "Exam term created successfully.");
        }

        public async Task<ApiResponse<bool>> UpdateExamTermAsync(int examTermId, UpdateExamTermDto dto)
        {
            var term = await _db.ExamTerms.FirstOrDefaultAsync(t => t.ExamTermId == examTermId && t.SchoolId == _currentUser.SchoolId);
            if (term == null) return ApiResponse<bool>.Fail("Exam term not found.");

            term.AcademicYearId = dto.AcademicYearId;
            term.TermName = dto.TermName.Trim();
            term.Description = dto.Description?.Trim();
            term.StartDate = dto.StartDate;
            term.EndDate = dto.EndDate;
            term.IsActive = dto.IsActive;

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Exam term updated successfully.");
        }


        public async Task<ApiResponse<bool>> DeleteExamTermAsync(int examTermId)
        {
            var term = await _db.ExamTerms.FirstOrDefaultAsync(t => t.ExamTermId == examTermId && t.SchoolId == _currentUser.SchoolId);
            if (term == null) return ApiResponse<bool>.Fail("Exam term not found.");

            _db.ExamTerms.Remove(term);
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Exam term deleted.");
        }


        // ── EXAM SCHEDULES (DATESHEET) ──────────────────────────────────────

        public async Task<ApiResponse<List<ExamScheduleDto>>> GetExamSchedulesAsync(int? examTermId, int? standardId)
        {
            var query = _db.ExamSchedules
                .Include(s => s.ExamTerm)
                .Include(s => s.Standard)
                .Include(s => s.Section)
                .Include(s => s.Subject)
                .Where(s => s.SchoolId == _currentUser.SchoolId);

            if (examTermId.HasValue && examTermId.Value > 0)
                query = query.Where(s => s.ExamTermId == examTermId.Value);

            if (standardId.HasValue && standardId.Value > 0)
                query = query.Where(s => s.StandardId == standardId.Value);

            var list = await query.OrderBy(s => s.ExamDate).ThenBy(s => s.StartTime).ToListAsync();

            var dtos = list.Select(s => new ExamScheduleDto
            {
                ExamScheduleId = s.ExamScheduleId,
                ExamTermId = s.ExamTermId,
                TermName = s.ExamTerm?.TermName ?? "N/A",
                StandardId = s.StandardId,
                StandardName = s.Standard?.StandardName ?? "N/A",
                SectionId = s.SectionId,
                SectionName = s.Section != null ? $"Section {s.Section.SectionName}" : "All Sections",
                SubjectId = s.SubjectId,
                SubjectName = s.Subject?.SubjectName ?? "N/A",
                ExamDate = s.ExamDate,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                MaxMarks = s.MaxMarks,
                PassMarks = s.PassMarks,
                RoomNumber = s.RoomNumber
            }).ToList();

            return ApiResponse<List<ExamScheduleDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<ExamScheduleDto>> CreateExamScheduleAsync(CreateExamScheduleDto dto)
        {
            var sched = new ExamSchedule
            {
                SchoolId = _currentUser.SchoolId,
                AcademicYearId = dto.AcademicYearId,
                ExamTermId = dto.ExamTermId,
                StandardId = dto.StandardId,
                SectionId = dto.SectionId > 0 ? dto.SectionId : null,
                SubjectId = dto.SubjectId,
                ExamDate = dto.ExamDate,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                MaxMarks = dto.MaxMarks > 0 ? dto.MaxMarks : 100,
                PassMarks = dto.PassMarks > 0 ? dto.PassMarks : 33,
                RoomNumber = dto.RoomNumber?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _db.ExamSchedules.Add(sched);
            await _db.SaveChangesAsync();

            var created = await _db.ExamSchedules
                .Include(s => s.ExamTerm)
                .Include(s => s.Standard)
                .Include(s => s.Section)
                .Include(s => s.Subject)
                .FirstOrDefaultAsync(s => s.ExamScheduleId == sched.ExamScheduleId);

            return ApiResponse<ExamScheduleDto>.Ok(new ExamScheduleDto
            {
                ExamScheduleId = sched.ExamScheduleId,
                ExamTermId = sched.ExamTermId,
                TermName = created?.ExamTerm?.TermName ?? "N/A",
                StandardId = sched.StandardId,
                StandardName = created?.Standard?.StandardName ?? "N/A",
                SectionId = sched.SectionId,
                SectionName = created?.Section != null ? $"Section {created.Section.SectionName}" : "All Sections",
                SubjectId = sched.SubjectId,
                SubjectName = created?.Subject?.SubjectName ?? "N/A",

                ExamDate = sched.ExamDate,
                StartTime = sched.StartTime,
                EndTime = sched.EndTime,
                MaxMarks = sched.MaxMarks,
                PassMarks = sched.PassMarks,
                RoomNumber = sched.RoomNumber
            }, "Exam schedule added successfully.");
        }

        public async Task<ApiResponse<bool>> UpdateExamScheduleAsync(int examScheduleId, UpdateExamScheduleDto dto)
        {
            var sched = await _db.ExamSchedules.FirstOrDefaultAsync(s => s.ExamScheduleId == examScheduleId && s.SchoolId == _currentUser.SchoolId);
            if (sched == null) return ApiResponse<bool>.Fail("Exam schedule item not found.");

            sched.ExamTermId = dto.ExamTermId;
            sched.StandardId = dto.StandardId;
            sched.SectionId = dto.SectionId > 0 ? dto.SectionId : null;
            sched.SubjectId = dto.SubjectId;
            sched.ExamDate = dto.ExamDate;
            sched.StartTime = dto.StartTime;
            sched.EndTime = dto.EndTime;
            sched.MaxMarks = dto.MaxMarks > 0 ? dto.MaxMarks : 100;
            sched.PassMarks = dto.PassMarks > 0 ? dto.PassMarks : 33;
            sched.RoomNumber = dto.RoomNumber?.Trim();

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Exam schedule item updated successfully.");
        }


        public async Task<ApiResponse<bool>> DeleteExamScheduleAsync(int examScheduleId)
        {
            var sched = await _db.ExamSchedules.FirstOrDefaultAsync(s => s.ExamScheduleId == examScheduleId && s.SchoolId == _currentUser.SchoolId);
            if (sched == null) return ApiResponse<bool>.Fail("Exam schedule item not found.");

            _db.ExamSchedules.Remove(sched);
            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Exam schedule item removed.");
        }

        // ── MARKS ENTRY GRID ────────────────────────────────────────────────

        public async Task<ApiResponse<List<StudentMarksGridItemDto>>> GetStudentMarksGridAsync(int examScheduleId, int? sectionId)
        {
            var sched = await _db.ExamSchedules.FirstOrDefaultAsync(s => s.ExamScheduleId == examScheduleId && s.SchoolId == _currentUser.SchoolId);
            if (sched == null) return ApiResponse<List<StudentMarksGridItemDto>>.Fail("Exam schedule item not found.");

            var studentQuery = _db.Students
                .Include(s => s.User)
                .Where(s => s.SchoolId == _currentUser.SchoolId && s.StandardId == sched.StandardId);

            if (sectionId.HasValue && sectionId.Value > 0)
            {
                var targetSec = await _db.Sections.AsNoTracking().FirstOrDefaultAsync(sec => sec.SectionId == sectionId.Value);
                if (targetSec != null && targetSec.IsDefault)
                {
                    studentQuery = studentQuery.Where(s => s.SectionId == sectionId.Value || s.SectionId == null);
                }
                else
                {
                    studentQuery = studentQuery.Where(s => s.SectionId == sectionId.Value);
                }
            }


            var students = await studentQuery.OrderBy(s => s.User.FullName).ToListAsync();

            var existingMarks = await _db.ExamMarks
                .Where(m => m.ExamScheduleId == examScheduleId)
                .ToDictionaryAsync(m => m.StudentId);

            var result = students.Select(st =>
            {
                existingMarks.TryGetValue(st.StudentId, out var m);
                return new StudentMarksGridItemDto
                {
                    StudentId = st.StudentId,
                    StudentCode = st.StudentCode,
                    StudentName = st.User?.FullName ?? "Student #" + st.StudentId,
                    RollNumber = st.StudentCode,
                    ExamMarkId = m?.ExamMarkId,
                    MarksObtained = m?.MarksObtained,
                    Grade = m?.Grade,
                    IsAbsent = m?.IsAbsent ?? false,
                    Remarks = m?.Remarks
                };
            }).ToList();

            return ApiResponse<List<StudentMarksGridItemDto>>.Ok(result);
        }

        public async Task<ApiResponse<bool>> SaveStudentMarksGridAsync(SaveStudentMarksGridDto dto, int teacherUserId)
        {
            var sched = await _db.ExamSchedules.FirstOrDefaultAsync(s => s.ExamScheduleId == dto.ExamScheduleId && s.SchoolId == _currentUser.SchoolId);
            if (sched == null) return ApiResponse<bool>.Fail("Exam schedule item not found.");

            var existingMarks = await _db.ExamMarks
                .Where(m => m.ExamScheduleId == dto.ExamScheduleId)
                .ToListAsync();

            foreach (var item in dto.Marks)
            {
                var mark = existingMarks.FirstOrDefault(m => m.StudentId == item.StudentId);
                if (mark == null)
                {
                    mark = new ExamMark
                    {
                        SchoolId = _currentUser.SchoolId,
                        ExamScheduleId = dto.ExamScheduleId,
                        StudentId = item.StudentId
                    };
                    _db.ExamMarks.Add(mark);
                }

                mark.IsAbsent = item.IsAbsent;
                mark.Remarks = item.Remarks?.Trim();
                mark.EvaluatedByTeacherUserId = teacherUserId;
                mark.EvaluatedAt = DateTime.UtcNow;

                if (item.IsAbsent)
                {
                    mark.MarksObtained = 0;
                    mark.Grade = "F";
                }
                else if (item.MarksObtained.HasValue)
                {
                    mark.MarksObtained = item.MarksObtained.Value;
                    mark.Grade = CalculateGrade(item.MarksObtained.Value, sched.MaxMarks);
                }
                else
                {
                    mark.MarksObtained = null;
                    mark.Grade = null;
                }
            }

            await _db.SaveChangesAsync();
            return ApiResponse<bool>.Ok(true, "Student marks saved successfully.");
        }

        private static string CalculateGrade(decimal marks, decimal maxMarks)
        {
            if (maxMarks <= 0) return "F";
            var pct = (marks / maxMarks) * 100m;
            if (pct >= 90) return "A+";
            if (pct >= 80) return "A";
            if (pct >= 70) return "B";
            if (pct >= 60) return "C";
            if (pct >= 33) return "D";
            return "F";
        }

        // ── REPORT CARD ─────────────────────────────────────────────────────

        public async Task<ApiResponse<StudentReportCardDto>> GetStudentReportCardAsync(int studentId, int examTermId)
        {
            var student = await _db.Students
                .Include(s => s.User)
                .Include(s => s.Standard)
                .Include(s => s.Section)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null) return ApiResponse<StudentReportCardDto>.Fail("Student record not found.");

            var term = await _db.ExamTerms
                .Include(t => t.AcademicYear)
                .FirstOrDefaultAsync(t => t.ExamTermId == examTermId);

            if (term == null) return ApiResponse<StudentReportCardDto>.Fail("Exam term not found.");

            var schedules = await _db.ExamSchedules
                .Include(s => s.Subject)
                .Where(s => s.ExamTermId == examTermId && s.StandardId == student.StandardId)
                .ToListAsync();

            var scheduleIds = schedules.Select(s => s.ExamScheduleId).ToList();

            var marks = await _db.ExamMarks
                .Where(m => scheduleIds.Contains(m.ExamScheduleId) && m.StudentId == studentId)
                .ToListAsync();

            var subjectMarks = new List<ReportCardSubjectMarkDto>();
            decimal totalMax = 0;
            decimal totalObtained = 0;
            bool hasFailSubject = false;

            foreach (var sched in schedules)
            {
                var m = marks.FirstOrDefault(x => x.ExamScheduleId == sched.ExamScheduleId);
                var obtained = m?.MarksObtained;
                var grade = m?.Grade ?? "N/A";
                var isAbsent = m?.IsAbsent ?? false;

                totalMax += sched.MaxMarks;
                if (obtained.HasValue) totalObtained += obtained.Value;

                if (isAbsent || (obtained.HasValue && obtained.Value < sched.PassMarks))
                {
                    hasFailSubject = true;
                }

                subjectMarks.Add(new ReportCardSubjectMarkDto
                {
                    SubjectName = sched.Subject?.SubjectName ?? "N/A",
                    MaxMarks = sched.MaxMarks,
                    PassMarks = sched.PassMarks,
                    MarksObtained = obtained,
                    Grade = grade,
                    IsAbsent = isAbsent,
                    Remarks = m?.Remarks ?? string.Empty
                });
            }

            var pct = totalMax > 0 ? Math.Round((totalObtained / totalMax) * 100m, 2) : 0;
            var overallGrade = CalculateGrade(totalObtained, totalMax);

            var reportCard = new StudentReportCardDto
            {
                StudentId = student.StudentId,
                StudentCode = student.StudentCode,
                StudentName = student.User?.FullName ?? "N/A",
                StandardName = student.Standard?.StandardName ?? "N/A",
                SectionName = student.Section?.SectionName ?? "N/A",
                RollNumber = student.StudentCode,
                AcademicYearId = term.AcademicYearId,
                AcademicYearName = term.AcademicYear?.YearName ?? "N/A",
                ExamTermId = term.ExamTermId,
                TermName = term.TermName,
                SubjectMarks = subjectMarks,
                TotalMaxMarks = totalMax,
                TotalMarksObtained = totalObtained,
                Percentage = pct,
                OverallGrade = overallGrade,
                ResultStatus = hasFailSubject ? "FAIL" : "PASS"
            };

            return ApiResponse<StudentReportCardDto>.Ok(reportCard);
        }
    }
}
