namespace BusTracking.Common.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _cfg;
        public JwtService(IConfiguration cfg) => _cfg = cfg;

        public string GenerateToken(int userId, string email, string role, int? schoolId = null)
            => GenerateToken(userId, email, role, [], schoolId);

        public string GenerateToken(int userId, string email, string role, IEnumerable<string> permissions, int? schoolId = null)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddHours(double.Parse(_cfg["Jwt:ExpireHours"] ?? "8"));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role)
            };

            if (schoolId.HasValue)
                claims.Add(new Claim("school_id", schoolId.Value.ToString()));

            // Embed each permission key as a separate "permission" claim so that
            // User.HasClaim("permission", "subadmin.view") works in API controllers.
            foreach (var perm in permissions)
                claims.Add(new Claim("permission", perm));

            var token = new JwtSecurityToken(
                issuer: _cfg["Jwt:Issuer"],
                audience: _cfg["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public (int userId, string email, string role, int? schoolId)? ValidateToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!);
                handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _cfg["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _cfg["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out var validated);

                var jwt = (JwtSecurityToken)validated;
                var userId = int.Parse(jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
                var email = jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value;
                var role = jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value;
                
                int? schoolId = null;
                var schoolClaim = jwt.Claims.FirstOrDefault(c => c.Type == "school_id")?.Value;
                if (int.TryParse(schoolClaim, out var sId))
                    schoolId = sId;

                return (userId, email, role, schoolId);
            }
            catch { return null; }
        }
    }
}
