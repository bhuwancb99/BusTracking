namespace BusTracking.Mobile.Views.Teacher
{
    public partial class EvaluateSubmissionPopup : Popup
    {
        private readonly HomeworkSubmissionDto _submission;

        public EvaluateSubmissionPopup(HomeworkSubmissionDto submission)
        {
            InitializeComponent();
            _submission = submission;
            LblStudentInfo.Text = $"{submission.StudentName} ({submission.StudentCode})";

            if (submission.MarksObtained.HasValue)
            {
                TxtMarks.Text = submission.MarksObtained.Value.ToString("F1");
            }
            if (!string.IsNullOrWhiteSpace(submission.TeacherRemarks))
            {
                TxtRemarks.Text = submission.TeacherRemarks;
            }
        }

        private async void OnCancelClicked(object? sender, EventArgs e)
        {
            if (Application.Current?.Windows[0].Page is Page p)
            {
                await p.ClosePopupAsync((object?)null);
            }
        }

        private async void OnSubmitClicked(object? sender, EventArgs e)
        {
            decimal? marks = null;
            if (decimal.TryParse(TxtMarks.Text, out var m))
            {
                marks = m;
            }

            var dto = new EvaluateHomeworkSubmissionDto
            {
                SubmissionId = _submission.SubmissionId,
                MarksObtained = marks,
                TeacherRemarks = TxtRemarks.Text?.Trim(),
                Status = "Evaluated"
            };

            if (Application.Current?.Windows[0].Page is Page p)
            {
                await p.ClosePopupAsync(dto);
            }
        }
    }
}
