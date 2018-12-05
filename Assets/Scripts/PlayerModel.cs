using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerModel {

	public int Id { get; set; }
	public int UserId { get; set; }
	public UserModel User { get; set; }
	public int GameId { get; set; }
	public bool Alive { get; set; }
	public bool Mafia { get; set; }

	public PlayerModel() {
		User = new UserModel();
	}

}
