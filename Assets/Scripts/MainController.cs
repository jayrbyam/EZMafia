using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using UnityEngine.SceneManagement;
using UnityEngine.UI.ProceduralImage;

public class MainController : MonoBehaviour {

	public GameObject signIn;
	public GameObject firstLoadComponents;
	public GameObject signInComponents;
	public GameObject signUpComponents;
	public GameObject backButton;
	public GameObject loadingScreen;
	public GameObject starter;
	public GameObject searchBar;
	public GameObject searchresults;
	public GameObject noResults;
	public GameObject originalSearchResult;
	public GameObject createSession;
	public Confirmation confirmation;

	// Session details page
	public GameObject sessionDetails;
	public Text sessionName;
	public GameObject endSessionButton;
	public Text sessionOwnerName;
	public GameObject firstDivider;
	public GameObject actionsPanel;
	public InputField timeLimitInput;
	public GameObject playersLabel;
	public GameObject originalPlayer;
	public GameObject timeRemaining;
	public GameObject startGameButton;
	public GameObject roleScreen;
	public List<Texture2D> personas;

    private string serverPath = "http://206.174.123.164:8080/api/Game/";
	private List<string> greetings = new List<string> () { "What's good", "Sup", "Hey", "Hi", "Hello", "Howdy", "Welcome", "Yo", "Greetings", "Salutations" };
	private string results;
	private int backAction = 0;
	private SessionModel session = new SessionModel();
	private UserModel user = new UserModel();
	private bool? playerUpdate = null; // null: no update. true: player update. false: owner update
	private float updateTime = 3f;
	private Color green;
	private Color gray = new Color(0.42f, 0.42f, 0.42f, 1f);
	private bool endGameTriggered = false;

	// Use this for initialization
	void Start () {
		green = startGameButton.GetComponent<ProceduralImage>().color;

		// Randomly assign image for persona
		roleScreen.transform.GetChild(1).GetComponent<RawImage>().texture = personas[Random.Range(0, personas.Count - 1)];

		// Prompt for the username by default. If the username is already known, don't need to.
		if (PlayerPrefs.HasKey ("Username")) {
			GET ("User?name=" + PlayerPrefs.GetString ("Username"), delegate {
				if (results == "") {
					signIn.GetComponent<MoveTo> ().MoveImmediate (Vector3.zero);
				}
				else {
					JSONNode userData = JSON.Parse(results);
					user.Id = int.Parse(userData["Id"].Value);
					user.Username = userData["Username"].Value;
					user.FirstName = userData["FirstName"].Value;
					user.LastName = userData["LastName"].Value;
					user.SessionId = userData["SessionId"].Value == null ? (int?)null : int.Parse(userData["SessionId"].Value);
					PlayerPrefs.SetString("Username", user.Username);
					GameObject.Find ("Greeting").GetComponent<Text> ().text = greetings[Random.Range(0, greetings.Count)] + ", " + user.Username + "!";
					signIn.SetActive (false);
					GET ("SessionByOwner?Id=" + user.Id, delegate {
						if (results == "") {
							if (user.SessionId != null) {
								backButton.SetActive(true);
								SessionSelected(user.SessionId.ToString(), 3);
							}
							else Invoke ("ShowStarter", 0f);
						}
						else SessionSelected(results);
					});
				}
			});
		} else {
			signIn.GetComponent<MoveTo> ().MoveImmediate (Vector3.zero);
		}
	}
	
	void Update () {
		// Rotate the loading indicator if the loading screen is showing
		if (loadingScreen.activeSelf) loadingScreen.transform.GetChild(0).localEulerAngles = new Vector3(0f, 0f, loadingScreen.transform.GetChild(0).localEulerAngles.z - 2f);
	
		if (playerUpdate != null) {
			if (updateTime >= 3) {
				updateTime = 0f;
				SessionSelected(session.Id.ToString(), 2, true, true);
				if (playerUpdate == true) {
					// Is the session still going?
				} else {
					startGameButton.GetComponent<Button>().enabled = session.Users.Count >= 5;
					startGameButton.GetComponent<ProceduralImage>().color = 
						startGameButton.GetComponent<Button>().enabled ? green : gray;
					startGameButton.GetComponentInChildren<Text>().color = 
						startGameButton.GetComponent<Button>().enabled ? Color.white : new Color(Color.white.r, Color.white.g, Color.white.b, 0.5f);
				}
			}
			updateTime += Time.deltaTime;
		}
	}

	public void ShowSignInComponents() {
		firstLoadComponents.SetActive (false);
		signInComponents.SetActive (true);
		backButton.SetActive (true);
	}

	public void ShowSignUpComponents() {
		firstLoadComponents.SetActive (false);
		signUpComponents.SetActive (true);
		backButton.SetActive (true);
	}

	public void Back() {
		switch (backAction) {
		case 0:
			firstLoadComponents.SetActive (true);
			signInComponents.SetActive (false);
			signUpComponents.SetActive (false);
			backButton.SetActive (false);
			break;
		case 1:
			ShowStarter ();
			searchBar.GetComponent<MoveTo> ().GoHome(2f);
			searchresults.SetActive (false);
			break;
		case 2:
			confirmation.message.text = "Wanna leave the session?";
			confirmation.onConfirm = delegate {
				POST ("User?name=" + user.Username, new Dictionary<string, string>() { 
					{"SessionId", "-1"},
					{"Username", user.Username}
				},
				delegate {
					sessionDetails.GetComponent<MoveTo> ().GoHome (2f);
					backAction = 1;
					SearchSessions(null);
					playerUpdate = null;
				});
			};
			confirmation.Show();
			break;
		case 3:
			confirmation.message.text = "Wanna leave the session?";
			confirmation.onConfirm = delegate {
				POST ("User?name=" + user.Username, new Dictionary<string, string>() { 
					{"SessionId", "-1"},
					{"Username", user.Username}
				},
				delegate {
					SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
				});
			};
			confirmation.Show();
			break;
		}

	}

	public void SignIn(InputField input) {
		if (input.text == "")
			return;

		GET ("User?name=" + input.text, delegate {
			if (results == "") {
				input.text = "";
				signInComponents.transform.GetChild(1).GetComponent<Text>().color = new Vector4(
					signInComponents.transform.GetChild(1).GetComponent<Text>().color.r, 
					signInComponents.transform.GetChild(1).GetComponent<Text>().color.g, 
					signInComponents.transform.GetChild(1).GetComponent<Text>().color.b, 
					1f
				);
			}
			else {
				JSONNode userData = JSON.Parse(results);
				user.Id = int.Parse(userData["Id"].Value);
				user.Username = userData["Username"].Value;
				user.FirstName = userData["FirstName"].Value;
				user.LastName = userData["LastName"].Value;
				user.SessionId = userData["SessionId"].Value == null ? (int?)null : int.Parse(userData["SessionId"].Value);
				PlayerPrefs.SetString("Username", user.Username);
				SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
			}
		});
	}

	public void SignUp() {
		var username = signUpComponents.transform.GetChild (0).GetComponent<InputField>();
		var firstName = signUpComponents.transform.GetChild (2).GetComponent<InputField>();
		var lastName = signUpComponents.transform.GetChild (3).GetComponent<InputField>();

		if (username.text == "" || firstName.text == "" || lastName.text == "")
			return;

		POST ("User", new Dictionary<string, string>() { {"Username", username.text}, {"FirstName", firstName.text}, {"LastName", lastName.text} }, delegate {
			if (results == "") {
				username.text = "";
				Text message = signUpComponents.transform.GetChild (1).GetComponent<Text>();
				message.color = new Vector4(message.color.r, message.color.g, message.color.b, 1f);
			} else {
				user.Id = int.Parse(results);
				user.Username = username.text;
				user.FirstName = firstName.text;
				user.LastName = lastName.text;
				user.SessionId = (int?)null;
				PlayerPrefs.SetString("Username", user.Username);
				GameObject.Find ("Greeting").GetComponent<Text> ().text = greetings[Random.Range(0, greetings.Count)] + ", " + username.text + "!";
				signIn.GetComponent<MoveTo> ().GoHome (2f);
				ShowStarter ();
			}
		});
	}

	public void ShowSessionSearch() {
		starter.GetComponent<MoveTo> ().GoHome (2f);
		searchBar.GetComponent<MoveTo> ().MoveToPos (new Vector3 (searchBar.transform.localPosition.x, searchBar.transform.localPosition.y - 100f), 2f);
		searchresults.SetActive (true);
		backButton.SetActive (true);
		backAction = 1;
	}

	public void SearchSessions(InputField input) {
		// Clear search results
		noResults.SetActive(true);
		foreach (Transform child in originalSearchResult.transform.parent) {
			if (child != originalSearchResult.transform)
				Destroy (child.gameObject);
		}

		if (input != null) {
			WWW search = GET ("Sessions?search=" + input.text, delegate {
				if (results != "[]") {
					noResults.SetActive(false);
					JSONNode resultData = JSON.Parse(results);
					int i = 0;
					while (resultData[i] != null) {
						GameObject listItem = Instantiate(originalSearchResult);
						listItem.transform.GetChild(0).GetComponent<Text>().text = resultData[i]["Name"].Value;
						listItem.transform.GetChild(1).GetComponent<Text>().text = resultData[i]["Owner"]["Username"].Value;
						listItem.transform.GetChild(2).GetComponent<Text>().text = resultData[i]["Id"].Value;
						listItem.transform.SetParent(originalSearchResult.transform.parent);
						Button listItemButton = listItem.GetComponent<Button>();
						listItem.GetComponent<Button>().onClick.AddListener(() => JoinSession(listItem.transform.GetChild(1).GetComponent<Text>().text, listItem.transform.GetChild(2).GetComponent<Text>().text));
						listItem.SetActive(true);
						++i;
					}
				}
			});
		}
	}

	public void ShowCreateSession() {
		createSession.SetActive (true);
		createSession.GetComponent<Fader> ().Fade (2f, 0.9f);
		createSession.transform.GetChild(0).GetComponent<MoveTo> ().MoveToPos (Vector3.zero, 2f);
	}

	public void HideCreateSession() {
		StartCoroutine (HideCreateSessionCR());
	}

	public void CreateSession(InputField input) {
		if (input.text == "")
			return;

		POST ("Session", new Dictionary<string, string>() { {"Id", "-1"}, {"Name", input.text}, {"OwnerId", PlayerPrefs.GetInt("UserId").ToString()}, {"TimeLimit", "0"} }, delegate {
			SessionSelected(results);
			HideCreateSession();
		});
	}

	private void ShowStarter() {
		starter.GetComponent<MoveTo> ().MoveToPos (new Vector3 (starter.transform.localPosition.x, starter.transform.localPosition.y + 2000f), 2f);
		backButton.SetActive (false);
	}

	private WWW GET(string url, System.Action onComplete, bool noLoadingScreen = false) {
		WWW www = new WWW (serverPath + url);
		StartCoroutine (WaitForRequest (www, onComplete, noLoadingScreen));
		return www;
	}

	private WWW POST(string url, Dictionary<string,string> post, System.Action onComplete, bool noLoadingScreen = false) {
		WWWForm form = new WWWForm();

		foreach(KeyValuePair<string,string> post_arg in post) {
			form.AddField(post_arg.Key, post_arg.Value);
		}

		WWW www = new WWW(serverPath + url, form);

		StartCoroutine(WaitForRequest(www, onComplete, noLoadingScreen));
		return www;
	}

	private IEnumerator WaitForRequest(WWW www, System.Action onComplete, bool noLoadingScreen) {
		if (!noLoadingScreen) loadingScreen.SetActive (true);

		yield return www;

		// Check for errors
		if (www.error == null) {
			results = www.text;
			onComplete();
			loadingScreen.SetActive (false);
		} else {
			Debug.Log (www.error);
		}
	}

	private IEnumerator HideCreateSessionCR() {
		createSession.GetComponent<Fader> ().Fade (2f, 0f);
		createSession.transform.GetChild(0).GetComponent<MoveTo> ().GoHome (2f);

		while (createSession.transform.GetChild (0).GetComponent<MoveTo> ().bIsAnimating) yield return null;

		createSession.SetActive (false);
	}

	private void ShowSessionDetails(bool inPlace) {
		playerUpdate = session.OwnerId != user.Id;
		bool gameInProgress = session.Game.Id != 0;
		// Get player matching user
		PlayerModel currentPlayer = new PlayerModel();
		foreach (PlayerModel player in session.Game.Players) {
			if (player.UserId == user.Id) {
				currentPlayer = player;
				break;
			}
		}

		if (gameInProgress && session.Game.MafiaWon != null && !endGameTriggered) {
			Invoke("EndGame", 15f);
			endGameTriggered = true;
		}

		if (!inPlace) sessionDetails.GetComponent<MoveTo> ().MoveToPos (new Vector3 (sessionDetails.transform.localPosition.x - 2000, sessionDetails.transform.localPosition.y), 2f);
		sessionName.text = session.Name;
		if (session.OwnerId == user.Id) {
			timeLimitInput.transform.parent.gameObject.SetActive (!gameInProgress);
			originalPlayer.transform.parent.gameObject.SetActive (true);
			//timeLimitInput.text = session.TimeLimit.ToString ();
			sessionOwnerName.gameObject.SetActive(false);
			actionsPanel.SetActive(gameInProgress);
			actionsPanel.transform.GetChild(2).gameObject.SetActive(false);
			firstDivider.SetActive(gameInProgress);
			endSessionButton.SetActive(true);
			startGameButton.SetActive(!gameInProgress);
		} else {
			sessionOwnerName.gameObject.SetActive(true);
			actionsPanel.SetActive(true);
			actionsPanel.transform.GetChild(2).gameObject.SetActive(!gameInProgress);
			firstDivider.SetActive(true);
			endSessionButton.SetActive(false);
			startGameButton.SetActive(false);
			sessionOwnerName.text = "Host: " + session.OwnerFirstName + " " + session.OwnerLastName;
			timeLimitInput.transform.parent.gameObject.SetActive (false);
		}

		actionsPanel.transform.GetChild(0).gameObject.SetActive(gameInProgress && currentPlayer.Alive);
		actionsPanel.transform.GetChild(1).gameObject.SetActive(gameInProgress && currentPlayer.Alive);
		actionsPanel.transform.GetChild(3).gameObject.SetActive(gameInProgress && !currentPlayer.Alive && session.Game.MafiaWon == null);
		actionsPanel.transform.GetChild(4).gameObject.SetActive(session.Game.MafiaWon == true);
		actionsPanel.transform.GetChild(5).gameObject.SetActive(session.Game.MafiaWon == false);

		// Populate Users list
		foreach (Transform child in originalPlayer.transform.parent) {
			if (child != originalPlayer.transform)
				Destroy(child.gameObject);
		}
		if (gameInProgress) {
			foreach (PlayerModel player in session.Game.Players) {
				GameObject listItem = Instantiate(originalPlayer);
				listItem.transform.GetChild(0).GetComponent<Text>().text = 
					player.User.Username + " (" + player.User.FirstName + " " + player.User.LastName + ")";
				listItem.transform.SetParent(originalPlayer.transform.parent);
				listItem.transform.GetChild(1).gameObject.SetActive(player.Alive && session.Game.MafiaWon == null);
				listItem.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition = 
					new Vector2(session.OwnerId == user.Id && session.Game.MafiaWon == null ? -80 : -5, listItem.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition.y);
				listItem.transform.GetChild(2).gameObject.SetActive(!player.Alive && session.Game.MafiaWon == null);
				listItem.transform.GetChild(2).GetComponent<RectTransform>().anchoredPosition = 
					new Vector2(session.OwnerId == user.Id && session.Game.MafiaWon == null ? -80 : -5, listItem.transform.GetChild(2).GetComponent<RectTransform>().anchoredPosition.y);
				listItem.transform.GetChild(3).gameObject.SetActive(session.OwnerId == user.Id && session.Game.MafiaWon == null);
				listItem.transform.GetChild(4).gameObject.SetActive(session.Game.MafiaWon != null && player.Mafia);
				listItem.SetActive(true);
			}
		} else {
			foreach (UserModel userModel in session.Users) {
				GameObject listItem = Instantiate(originalPlayer);
				listItem.transform.GetChild(0).GetComponent<Text>().text = 
					userModel.Username + " (" + userModel.FirstName + " " + userModel.LastName + ")";
				listItem.transform.SetParent(originalPlayer.transform.parent);
				listItem.transform.GetChild(1).gameObject.SetActive(false);
				listItem.transform.GetChild(2).gameObject.SetActive(false);
				listItem.transform.GetChild(3).gameObject.SetActive(session.OwnerId == user.Id);
				listItem.transform.GetChild(4).gameObject.SetActive(false);
				listItem.SetActive(true);
			}
		}

		timeRemaining.GetComponent<Text> ().text = "";//session.TimeLimit != 0 ? session.TimeLimit + ":00" : "";
	}

	private void SessionSelected(string hiddenId, int back = 2, bool inPlace = false, bool noLoadingScreen = false) {
		GET ("Session?Id=" + hiddenId, delegate {
			JSONNode sessionData = JSON.Parse(results);
			session.Id = int.Parse(sessionData["Id"].Value);
			session.Name = sessionData["Name"].Value;
			session.OwnerId = int.Parse(sessionData["OwnerId"].Value);
			session.OwnerFirstName = sessionData["Owner"]["FirstName"].Value;
			session.OwnerLastName = sessionData["Owner"]["LastName"].Value;
			session.TimeLimit = int.Parse(sessionData["TimeLimit"].Value);

			int userIdx = 0;
			session.Users = new List<UserModel>();
			while (sessionData["Users"][userIdx] != null) {
				session.Users.Add(new UserModel {
					Id = int.Parse(sessionData["Users"][userIdx]["Id"]),
					Username = sessionData["Users"][userIdx]["Username"],
					FirstName = sessionData["Users"][userIdx]["FirstName"],
					LastName = sessionData["Users"][userIdx]["LastName"]
				});
				userIdx++;
			}

			if (int.Parse(sessionData["Game"]["Id"].Value) != 0) {
				session.Game.Id = int.Parse(sessionData["Game"]["Id"].Value);
				session.Game.SessionId = session.Id;
				session.Game.InProgress = sessionData["Game"]["InProgress"].AsBool;
				session.Game.MafiaWon = sessionData["Game"]["MafiaWon"].Value == null ? (bool?)null : sessionData["Game"]["MafiaWon"].AsBool;
				
				userIdx = 0;
				session.Game.Players = new List<PlayerModel>();
				while (sessionData["Game"]["Players"][userIdx] != null) {
					session.Game.Players.Add(new PlayerModel {
						Id = int.Parse(sessionData["Game"]["Players"][userIdx]["Id"]),
						UserId = int.Parse(sessionData["Game"]["Players"][userIdx]["UserId"]),
						User = new UserModel() {
							Username = sessionData["Game"]["Players"][userIdx]["User"]["Username"].Value,
							FirstName = sessionData["Game"]["Players"][userIdx]["User"]["FirstName"].Value,
							LastName = sessionData["Game"]["Players"][userIdx]["User"]["LastName"].Value
						},
						GameId = int.Parse(sessionData["Game"]["Players"][userIdx]["GameId"]),
						Alive = sessionData["Game"]["Players"][userIdx]["Alive"].AsBool,
						Mafia = sessionData["Game"]["Players"][userIdx]["Mafia"].AsBool
					});
					userIdx++;
				}
			}

			ShowSessionDetails(inPlace);
			backAction = back;
		}, noLoadingScreen);
	}

	private void JoinSession(string owner, string hiddenId) {
		confirmation.message.text = "Join " + owner + "'s session?";
		confirmation.onConfirm = delegate {
			POST ("User?name=" + user.Username, new Dictionary<string, string>() { 
				{"SessionId", hiddenId},
				{"Username", user.Username}
			},
			delegate {
				SessionSelected(hiddenId);
			});
		};
		confirmation.Show();
	}

	public void EndSession() {
		confirmation.message.text = "Are you sure you want to end the session?";
		confirmation.onConfirm = delegate {
			GET ("SessionDelete" + "?Id=" + session.Id, delegate {
				SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
			});
		};
		confirmation.Show();
	}

	public void SaveTimeLimit(InputField input) {
		if (input.text == "") {
			input.text = "0";
			return;
		}

		POST ("Session", new Dictionary<string, string>() { 
			{"Id", session.Id.ToString()},
			{"TimeLimit", input.text} 
		}, delegate {});
	}

	public void RemoveSessionPlayer(Text label) {
		string username = label.text.Split(' ')[0];
		bool gameInProgress = session.Game.Id != 0;
		confirmation.message.text = "Are you sure you want to remove " + username + " from this session?";
		if (gameInProgress) confirmation.message.text = "Are you sure " + username + " is dead?";
		confirmation.onConfirm = delegate {
			if (gameInProgress) {
				GET ("KillPlayer?username=" + username + "&gameId=" + session.Game.Id.ToString(),
				delegate {
					SessionSelected(session.Id.ToString(), 2, true);
				});
			} else {
				POST ("User?name=" + username, new Dictionary<string, string>() { 
					{"SessionId", "-1"},
					{"Username", username}
				},
				delegate {
					SessionSelected(session.Id.ToString(), 2, true);
				});
			}
		};
		confirmation.Show();
	}

	public void StartGame() {
		GET ("StartGame?sessionId=" + session.Id.ToString(), delegate {
			SessionSelected(session.Id.ToString(), 2, true);
		});
	}

	public void ShowRole() {
		confirmation.message.text = "Are you sure no one is peeking?";
		confirmation.onConfirm = delegate {
			// Get player matching user
			PlayerModel currentPlayer = new PlayerModel();
			foreach (PlayerModel player in session.Game.Players) {
				if (player.UserId == user.Id) {
					currentPlayer = player;
					break;
				}
			}

			roleScreen.SetActive(true);
			roleScreen.transform.GetChild(0).gameObject.SetActive(currentPlayer.Mafia);
			roleScreen.transform.GetChild(2).gameObject.SetActive(currentPlayer.Mafia);
			roleScreen.transform.GetChild(3).gameObject.SetActive(currentPlayer.Mafia);
			roleScreen.transform.GetChild(4).gameObject.SetActive(currentPlayer.Mafia);
			roleScreen.transform.GetChild(5).gameObject.SetActive(!currentPlayer.Mafia);
		};
		confirmation.Show();
	}

	public void HideRole() {
		roleScreen.SetActive(false);
	}

	public void IDied() {
		confirmation.message.text = "Are you sure you are dead?";
		confirmation.onConfirm = delegate {
			GET ("KillPlayer?username=" + user.Username + "&gameId=" + session.Game.Id.ToString(),
			delegate {
				SessionSelected(session.Id.ToString(), 2, true);
			});
		};
		confirmation.Show();
	}

	private void EndGame() {
		GET ("EndGame?gameId=" + session.Game.Id.ToString(),
		delegate {
			session.Game = new GameModel();
			endGameTriggered = false;
			SessionSelected(session.Id.ToString(), 2, true);
		});
	}
}
