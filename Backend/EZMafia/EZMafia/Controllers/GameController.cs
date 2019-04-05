using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net;
using System.Net.Http;
using EZMafia.Models;
using System.Data.Entity;

namespace EZMafia.Controllers
{
    public class GameController : ApiController
    {
        [HttpGet]
        [ActionName("User")]
        public HttpResponseMessage GetUser(string name)
        {
            using (EZMafiaEntities db = new EZMafiaEntities())
            {
                User user = db.Users.Where(u => u.Username == name).FirstOrDefault();
                if (user == null) return Request.CreateResponse(HttpStatusCode.OK); // No such user
                return Request.CreateResponse(HttpStatusCode.OK, new UserModel
                {
                    Id = user.Id,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    SessionId = user.SessionId
                });
            }
        }

        [HttpPost]
        [ActionName("User")]
        public HttpResponseMessage NewUser(UserModel user)
        {
            using (EZMafiaEntities db = new EZMafiaEntities())
            {
                if (db.Users.Count(u => u.Username == user.Username) > 0)
                {
                    if (user.SessionId != null)
                    {
                        User dbUser = db.Users.Where(u => u.Username == user.Username).FirstOrDefault();
                        if (dbUser != null)
                        {
                            dbUser.SessionId = user.SessionId == -1 ? null : user.SessionId;
                            db.SaveChanges();
                        }
                    }
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else
                {
                    User newUser = db.Users.Add(new User
                    {
                        Username = user.Username,
                        FirstName = user.FirstName,
                        LastName = user.LastName
                    });
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, newUser.Id);
                }
            }
        }

        [HttpGet]
        [ActionName("Sessions")]
        public HttpResponseMessage Sessions(string search)
        {
            using (EZMafiaEntities db = new EZMafiaEntities())
            {
                var results = new List<SessionModel>();
                db.Sessions.Include(s => s.Owner).Where(s => s.InProgress && s.Name.ToLower().Contains(search.ToLower())).ToList().ForEach(s =>
                {
                    results.Add(new SessionModel
                    {
                        Id = s.Id,
                        Name = s.Name,
                        OwnerId = s.OwnerId,
                        Owner = new UserModel
                        {
                            Username = s.Owner.Username,
                            FirstName = s.Owner.FirstName,
                            LastName = s.Owner.LastName
                        }
                    });
                });
                return Request.CreateResponse(HttpStatusCode.OK, results);
            }
        }

        [HttpGet]
        [ActionName("Session")]
        public HttpResponseMessage Session(int Id)
        {
            using (EZMafiaEntities db = new EZMafiaEntities())
            {
                Session session = db.Sessions.Include(s => s.Owner).Include(s => s.Users).Where(s => s.Id == Id).FirstOrDefault();
                Game game = db.Games.Include(g => g.Players.Select(p => p.User)).Where(g => g.SessionId == session.Id && g.InProgress == true).FirstOrDefault();

                // Check the status of the game and update who won
                if (game != null)
                {
                    bool mafiaDead = game.Players.Count(p => p.Mafia && p.Alive) == 0;
                    bool mafiaTied = game.Players.Count(p => p.Mafia && p.Alive) >= game.Players.Count(p => !p.Mafia && p.Alive);
                    if (mafiaDead || mafiaTied)
                    {
                        game.MafiaWon = mafiaTied;
                        db.SaveChanges();
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, new SessionModel
                {
                    Id = session.Id,
                    Name = session.Name,
                    OwnerId = session.OwnerId,
                    Owner = new UserModel
                    {
                        Username = session.Owner.Username,
                        FirstName = session.Owner.FirstName,
                        LastName = session.Owner.LastName
                    },
                    TimeLimit = session.TimeLimit.HasValue ? session.TimeLimit.Value : 0,
                    Users = session.Users.Select(u => new UserModel
                    {
                        Id = u.Id,
                        Username = u.Username,
                        FirstName = u.FirstName,
                        LastName = u.LastName
                    }).ToList(),
                    Game = game == null ? new GameModel() : new GameModel()
                    {
                        Id = game.Id,
                        SessionId = game.SessionId,
                        EndTime = game.EndTime,
                        InProgress = game.InProgress,
                        MafiaWon = game.MafiaWon,
                        Players = game.Players.Select(p => new PlayerModel()
                        {
                            UserId = p.UserId,
                            User = new UserModel()
                            {
                                Username = p.User.Username,
                                FirstName = p.User.FirstName,
                                LastName = p.User.LastName
                            },
                            GameId = p.GameId,
                            Alive = p.Alive,
                            Mafia = p.Mafia
                        }).ToList()
                    }
                });
            }
        }

        [HttpGet]
        [ActionName("SessionByOwner")]
        public HttpResponseMessage SessionByOwner(int Id)
        {
            using (EZMafiaEntities db = new EZMafiaEntities())
            {
                Session session = db.Sessions.Include(s => s.Owner).Where(s => s.OwnerId == Id && s.InProgress).FirstOrDefault();
                if (session == null) return Request.CreateResponse(HttpStatusCode.OK); // Frontend handles empty results
                return Request.CreateResponse(HttpStatusCode.OK, session.Id);
            }
        }

        [HttpPost]
        [ActionName("Session")]
        public HttpResponseMessage Session(SessionModel session)
        {
            using (EZMafiaEntities db = new EZMafiaEntities())
            {
                Session dbSession;
                if (session.Id == -1)
                {
                    dbSession = db.Sessions.Add(new Session
                    {
                        Name = session.Name,
                        OwnerId = session.OwnerId,
                        TimeLimit = session.TimeLimit,
                        InProgress = true
                    });
                    db.SaveChanges();

                    // Add owner as user in session
                    db.Users.Find(session.OwnerId).SessionId = dbSession.Id;
                    db.SaveChanges();
                } else
                {
                    dbSession = db.Sessions.Find(session.Id);
                    dbSession.TimeLimit = session.TimeLimit;
                    db.SaveChanges();
                }

                return Request.CreateResponse(HttpStatusCode.OK, dbSession.Id);
            }
        }

        [HttpGet]
        [ActionName("SessionDelete")]
        public HttpResponseMessage SessionDelete(int Id)
        {
            using (EZMafiaEntities db = new EZMafiaEntities())
            {
                Session session = db.Sessions.Find(Id);
                if (session != null)
                {
                    db.Users.Where(u => u.SessionId == session.Id).ToList().ForEach(u => u.SessionId = null);
                    db.Games.RemoveRange(db.Games.Where(g => g.SessionId == session.Id && g.InProgress == true));
                    session.InProgress = false;
                    db.SaveChanges();
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        [HttpGet]
        [ActionName("StartGame")]
        public HttpResponseMessage StartGame(int sessionId)
        {
            using (EZMafiaEntities db = new EZMafiaEntities())
            {
                Session session = db.Sessions.Include(s => s.Users).Where(s => s.Id == sessionId).FirstOrDefault();
                if (session != null)
                {
                    // Create new game
                    Game game = db.Games.Add(new Game()
                    {
                        SessionId = session.Id,
                        EndTime = session.TimeLimit != null ? DateTime.Now.AddMinutes(session.TimeLimit.Value) : (DateTime?)null,
                        InProgress = true
                    });

                    // Determine mafia
                    int players = session.Users.Count();
                    List<int> mafiaIndices = GetMafiaIndeces(players);

                    // Add players
                    int idx = 0;
                    session.Users.ToList().ForEach(u =>
                    {
                        db.Players.Add(new Player()
                        {
                            UserId = u.Id,
                            GameId = game.Id,
                            Alive = true,
                            Mafia = mafiaIndices.Contains(idx)
                        });
                        idx++;
                    });
                    db.SaveChanges();
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        [HttpGet]
        [ActionName("EndGame")]
        public HttpResponseMessage EndGame(int gameId)
        {
            using (EZMafiaEntities db = new EZMafiaEntities())
            {
                Game game = db.Games.Find(gameId);
                if (game != null)
                {
                    game.InProgress = false;
                    db.SaveChanges();
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        [HttpGet]
        [ActionName("KillPlayer")]
        public HttpResponseMessage KillPlayer(string username, int gameId)
        {
            using (EZMafiaEntities db = new EZMafiaEntities())
            {
                Player dbPlayer = db.Players.Include(p => p.User).Where(p => p.User.Username == username && p.GameId == gameId).FirstOrDefault();
                if (dbPlayer != null)
                {
                    dbPlayer.Alive = false;
                    db.SaveChanges();
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
        }

        private List<int> GetMafiaIndeces(int players)
        {
            int mafia = 2;
            if (players > 8) mafia = players / 3;

            List<int> result = new List<int>();
            for (int i = 0; i < mafia; i++)
            {
                int newIdx;
                do
                {
                    newIdx = new Random().Next(players);
                } while (result.Contains(newIdx));
                result.Add(newIdx);
            }
            return result;
        }
    }
}