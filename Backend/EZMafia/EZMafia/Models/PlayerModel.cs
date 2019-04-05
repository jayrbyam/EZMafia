using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EZMafia.Models
{
    public class PlayerModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int GameId { get; set; }
        public UserModel User { get; set; }
        public GameModel Game { get; set; }
        public bool Alive { get; set; }
        public bool Mafia { get; set; }
    }
}