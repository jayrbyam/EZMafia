using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EZMafia.Models
{
    public class SessionModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int OwnerId { get; set; }
        public UserModel Owner { get; set; }
        public int TimeLimit { get; set; }
        public GameModel Game { get; set; }
        public List<UserModel> Users { get; set; }
        public bool InProgress { get; set; }

        public SessionModel()
        {
            Users = new List<UserModel>();
        }
    }
}