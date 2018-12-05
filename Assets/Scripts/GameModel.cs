using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameModel {

	public int Id { get; set; }
	public int SessionId { get;set; }
	public DateTime? EndDateTime { get; set; }
	public bool InProgress { get; set; }
	public bool? MafiaWon { get; set; }
	public List<PlayerModel> Players { get; set; }

	public GameModel() {
		Id = 0;
		Players = new List<PlayerModel>();
	}

}
