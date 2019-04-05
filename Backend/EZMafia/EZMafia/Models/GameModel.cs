using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EZMafia.Models
{
    public class GameModel
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public SessionModel Session { get; set; }
        public DateTime? EndTime { get; set; }
        public bool InProgress { get; set; }
        public bool? MafiaWon { get; set; }
        public List<PlayerModel> Players { get; set; }

        public GameModel()
        {
            Players = new List<PlayerModel>();
        }
    }
}