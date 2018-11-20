using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;

public class MainController : MonoBehaviour {

	public GameObject signIn;
	public GameObject firstLoadComponents;
	public GameObject signInComponents;
	public GameObject signUpComponents;
	public GameObject backButton;
	public GameObject loadingScreen;
	public GameObject starter;
	public GameObject searchBar;
	public GameObject searchResults;
	public GameObject noResults;
	public GameObject originalSearchResult;
	public GameObject createSession;
	public GameObject sessionDetails;
	public Text sessionName;
	public InputField timeLimitInput;
	public GameObject playersLabel;
	public GameObject originalPlayer;
	public GameObject timeRemaining;

    private string serverPath = "http://24.237.35.244:8080/api/Game/";
	private List<string> greetings = new List<string> () { "Hi", "Hello", "Howdy", "Welcome", "Yo", "Greetings", "Salutations" };
	private string results;
	private int backAction = 0;
	private SessionModel session = new SessionModel();

	// Use this for initialization
	void Start () {

		// Prompt for the username by default. If the username is already known, dont' need to.
		if (PlayerPrefs.HasKey ("Username")) {
			signIn.SetActive (false);
			GameObject.Find ("Greeting").GetComponent<Text> ().text = greetings[Random.Range(0, greetings.Count)] + ", " + PlayerPrefs.GetString ("Username") + "!";
			Invoke ("ShowStarter", 0.5f);
		} else {
			signIn.GetComponent<MoveTo> ().MoveImmediate (Vector3.zero);
		}
			
	}
	
	// Update is called once per frame
	void Update () {
		// Rotate the loading indicator if the loading screen is showing
		if (loadingScreen.activeSelf) loadingScreen.transform.GetChild(0).localEulerAngles = new Vector3(0f, 0f, loadingScreen.transform.GetChild(0).localEulerAngles.z - 2f);
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
			searchResults.SetActive (false);
			break;
		case 2:
			sessionDetails.GetComponent<MoveTo> ().GoHome (2f);
			backAction = 1;
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
				PlayerPrefs.SetInt("UserId", int.Parse(userData["Id"].Value));
				PlayerPrefs.SetString("Username", userData["Username"].Value);
				PlayerPrefs.SetString("UserFirst", userData["FirstName"].Value);
				PlayerPrefs.SetString("UserLast", userData["LastName"].Value);
				GameObject.Find ("Greeting").GetComponent<Text> ().text = greetings[Random.Range(0, greetings.Count)] + ", " + input.text + "!";
				signIn.GetComponent<MoveTo> ().GoHome (2f);
				ShowStarter ();
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
				PlayerPrefs.SetInt("UserId", int.Parse(results));
				PlayerPrefs.SetString("Username", username.text);
				PlayerPrefs.SetString("UserFirst", firstName.text);
				PlayerPrefs.SetString("UserLast", lastName.text);
				GameObject.Find ("Greeting").GetComponent<Text> ().text = greetings[Random.Range(0, greetings.Count)] + ", " + username.text + "!";
				signIn.GetComponent<MoveTo> ().GoHome (2f);
				ShowStarter ();
			}
		});
	}

	public void ShowSessionSearch() {
		starter.GetComponent<MoveTo> ().GoHome (2f);
		searchBar.GetComponent<MoveTo> ().MoveToPos (new Vector3 (searchBar.transform.localPosition.x, searchBar.transform.localPosition.y - 100f), 2f);
		searchResults.SetActive (true);
		backButton.SetActive (true);
		backAction = 1;
	}

	public void SearchSessions(InputField input) {
		// Clear search resuluts
		noResults.SetActive(true);
		foreach (Transform child in originalSearchResult.transform.parent) {
			if (child != originalSearchResult.transform)
				Destroy (child.gameObject);
		}

		WWW search = GET ("Sessions?search=" + input.text, delegate {
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
				listItem.GetComponent<Button>().onClick.AddListener(() => SessionSelected(listItem.transform.GetChild(2).GetComponent<Text>()));
				listItem.SetActive(true);
				++i;
			}
		});
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

		POST ("Session", new Dictionary<string, string>() { {"Name", input.text}, {"OwnerId", PlayerPrefs.GetInt("UserId").ToString()} }, delegate {
			session.Id = int.Parse(results);
			session.Name = input.text;
			session.OwnerId = PlayerPrefs.GetInt("UserId");
			session.OwnerFirstName = PlayerPrefs.GetString("UserFirst");
			session.OwnerLastName = PlayerPrefs.GetString("UserLast");

			HideCreateSession();
			ShowSessionDetails();
		});
	}

	private void ShowStarter() {
		starter.GetComponent<MoveTo> ().MoveToPos (new Vector3 (starter.transform.localPosition.x, starter.transform.localPosition.y + 2000f), 2f);
		backButton.SetActive (false);
	}

	private WWW GET(string url, System.Action onComplete) {
		WWW www = new WWW (serverPath + url);
		StartCoroutine (WaitForRequest (www, onComplete));
		return www;
	}

	private WWW POST(string url, Dictionary<string,string> post, System.Action onComplete) {
		WWWForm form = new WWWForm();

		foreach(KeyValuePair<string,string> post_arg in post) {
			form.AddField(post_arg.Key, post_arg.Value);
		}

		WWW www = new WWW(serverPath + url, form);

		StartCoroutine(WaitForRequest(www, onComplete));
		return www;
	}

	private IEnumerator WaitForRequest(WWW www, System.Action onComplete) {
		loadingScreen.SetActive (true);

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

	private void ShowSessionDetails() {
		sessionDetails.GetComponent<MoveTo> ().MoveToPos (new Vector3 (sessionDetails.transform.localPosition.x - 2000, sessionDetails.transform.localPosition.y), 2f);
		sessionName.text = session.Name;
		sessionName.transform.GetChild (0).GetComponent<Text> ().text = "Host: " + session.OwnerFirstName + " " + session.OwnerLastName;
		if (session.OwnerId == PlayerPrefs.GetInt ("OwnerId")) {
			timeLimitInput.transform.parent.gameObject.SetActive (true);
			playersLabel.SetActive (true);
			originalPlayer.transform.parent.gameObject.SetActive (true);
			timeLimitInput.text = session.TimeLimit.ToString ();
			// Populate Players list
		} else {
			timeLimitInput.transform.parent.gameObject.SetActive (false);
			playersLabel.SetActive (false);
			originalPlayer.transform.parent.gameObject.SetActive (false);
		}
		timeRemaining.GetComponent<Text> ().text = session.TimeLimit != 0 ? session.TimeLimit + ":00" : "";
	}

	private void SessionSelected(Text hiddenId) {
		GET ("Session?Id=" + hiddenId.text, delegate {
			JSONNode sessionData = JSON.Parse(results);
			session.Id = int.Parse(sessionData["Id"].Value);
			session.Name = sessionData["Name"].Value;
			session.OwnerId = int.Parse(sessionData["OwnerId"].Value);
			session.OwnerFirstName = sessionData["Owner"]["FirstName"].Value;
			session.OwnerLastName = sessionData["Owner"]["LastName"].Value;
			session.TimeLimit = int.Parse(sessionData["TimeLimit"].Value);
			session.GameEndTime = System.DateTime.Parse(sessionData["GameEndTime"].Value);
			ShowSessionDetails();
			backAction = 2;
		});
	}
}
