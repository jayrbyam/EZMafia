using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SessionModel {

	public int Id { get; set; }
	public string Name { get; set; }
	public int OwnerId { get;set; }
	public string OwnerFirstName { get; set; }
	public string OwnerLastName { get; set; }
	public int TimeLimit { get; set; }
	public List<UserModel> Users { get; set; }
	public GameModel Game { get; set; }

	public SessionModel() {
		TimeLimit = 0;
		Users = new List<UserModel>();
		Game = new GameModel();
	}

}
