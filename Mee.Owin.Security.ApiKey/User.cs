using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Mee.Owin.Security.ApiKey
{
    public class User
    {
        ClaimsIdentity _identity;

        public User()
        {
            _identity = new ClaimsIdentity("ApiKey");
        }

        public string ApiKey { get; set; } = System.Guid.NewGuid().ToString();

        string _name = "";
        public string Name
        {
            get { return _name; }
            set {

                
                //remove previous name in Claims
                _identity.Claims
                    .Where(c => c.Type == ClaimTypes.Name)
                    .ToList()
                    .ForEach(c => _identity.RemoveClaim(c));

                //Store name
                _name = value;

                //Add Name in Claims
               _identity.AddClaim(new Claim(ClaimTypes.Name, _name));
            }
        }

        List<string> _roles = new List<string>();
        public List<string> Roles
        {
            get { return _roles; }
            set
            {

                //Remove previous roles in claims
                _identity.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .ToList()
                    .ForEach(c => _identity.RemoveClaim(c));

                //Store roles list
                _roles = value;

                //Add roles in Claims
                _roles.ForEach(r => _identity.AddClaim(new Claim(ClaimTypes.Role, r)));
            }
        }

        public ClaimsPrincipal GetClaimsPrincipal()
        {
            ClaimsPrincipal principal = new ClaimsPrincipal(_identity);
            return principal;
        }
    }

}