using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EZMafia.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? SessionId { get; set; }
        public SessionModel Session { get; set; }
    }
}