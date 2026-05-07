namespace BusTracking.Common.Helpers
{
    public static class EmailTemplateHelper
    {
        public static string WelcomeEmail(string fullName, string email, string password)
            => $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
              <div style="background:#1a73e8;padding:24px;border-radius:8px 8px 0 0">
                <h2 style="color:#fff;margin:0">🚌 Bus Tracking System</h2>
              </div>
              <div style="padding:24px;border:1px solid #e0e0e0;border-top:none;border-radius:0 0 8px 8px">
                <p>Hi <strong>{fullName}</strong>,</p>
                <p>Your account has been created. Here are your login details:</p>
                <table style="background:#f5f5f5;padding:16px;border-radius:6px;width:100%">
                  <tr><td><strong>Email</strong></td><td>{email}</td></tr>
                  <tr><td><strong>Password</strong></td><td style="font-family:monospace">{password}</td></tr>
                </table>
                <p style="margin-top:16px">Please change your password after your first login.</p>
                <p style="color:#888;font-size:12px">If you did not expect this email, please ignore it.</p>
              </div>
            </div>
            """;

        public static string PasswordResetEmail(string fullName, string resetLink)
            => $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
              <div style="background:#1a73e8;padding:24px;border-radius:8px 8px 0 0">
                <h2 style="color:#fff;margin:0">🚌 Bus Tracking System</h2>
              </div>
              <div style="padding:24px;border:1px solid #e0e0e0;border-top:none;border-radius:0 0 8px 8px">
                <p>Hi <strong>{fullName}</strong>,</p>
                <p>We received a request to reset your password. Click the button below to continue:</p>
                <div style="text-align:center;margin:24px 0">
                  <a href="{resetLink}" style="background:#1a73e8;color:#fff;padding:12px 28px;border-radius:6px;text-decoration:none;font-weight:bold">Reset Password</a>
                </div>
                <p style="color:#888;font-size:12px">This link expires in 2 hours. If you did not request this, ignore this email.</p>
              </div>
            </div>
            """;

        public static string BusAssignedEmail(string studentName, string busNumber, string busName, string routeName)
            => $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto">
              <div style="background:#1a73e8;padding:24px;border-radius:8px 8px 0 0">
                <h2 style="color:#fff;margin:0">🚌 Bus Assigned</h2>
              </div>
              <div style="padding:24px;border:1px solid #e0e0e0;border-top:none;border-radius:0 0 8px 8px">
                <p>Hi <strong>{studentName}</strong>,</p>
                <p>Your bus has been assigned. Here are the details:</p>
                <table style="background:#f5f5f5;padding:16px;border-radius:6px;width:100%">
                  <tr><td><strong>Bus Name</strong></td><td>{busName}</td></tr>
                  <tr><td><strong>Bus Number</strong></td><td>{busNumber}</td></tr>
                  <tr><td><strong>Route</strong></td><td>{routeName}</td></tr>
                </table>
              </div>
            </div>
            """;
    }
}
