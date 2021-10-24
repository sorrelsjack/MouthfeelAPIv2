using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Helpers
{
    public class IdentityHelper
    {
        public static int GetIdFromUser(ClaimsPrincipal user)
            => Int32.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}
