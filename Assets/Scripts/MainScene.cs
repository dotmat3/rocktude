using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Google;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine.UI;
using UnityEngine.Networking;

public class MainScene : MonoBehaviour {

    public const string WEB_CLIENT_ID = "855129623962-klds3ljikb6645rmb28q6ude1o758p4v.apps.googleusercontent.com";

    public GameObject loginMenu;
    public GameObject mainMenu;

    private FirebaseAuth auth;
    private FirebaseUser user;

    private GoogleSignInConfiguration configuration;

    
    private void Awake() {
        configuration = new GoogleSignInConfiguration {
            WebClientId = WEB_CLIENT_ID,
            RequestIdToken = true
        };
    }

    private void Start() {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            DependencyStatus dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available) {
                auth = FirebaseAuth.DefaultInstance;
                OnAuthStateChanged(this, null);
            } else
                Debug.LogError(string.Format("Could not resolve all Firebase dependencies: {0}", dependencyStatus));
        });
    }

    private void OnAuthStateChanged(object sender, System.EventArgs evetnArgs) {
        if (auth.CurrentUser != user) {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null) {
                Debug.Log("Signed out");
                ShowLoginMenu();
            }
            user = auth.CurrentUser;
            if (signedIn) {
                Debug.Log("Signed in " + user.UserId);
                ShowMainMenu();
            }
        }
    }

    public void LogIn() {
        if (Application.platform != RuntimePlatform.Android) {
            ShowMainMenu();
            return;
        }

        if (GoogleSignIn.Configuration == null) {
            GoogleSignIn.Configuration = configuration;
            GoogleSignIn.Configuration.UseGameSignIn = false;
            GoogleSignIn.Configuration.RequestIdToken = true;
        }

        Task<GoogleSignInUser> signIn = GoogleSignIn.DefaultInstance.SignIn();

        TaskCompletionSource<FirebaseUser> signInCompleted = new TaskCompletionSource<FirebaseUser>();

        signIn.ContinueWith(task => {
            if (task.IsCanceled) {
                signInCompleted.SetCanceled();
                Debug.Log("User canceled login");
            } else if (task.IsFaulted) {
                signInCompleted.SetException(task.Exception);
                Debug.LogError("An error occurred: " + task.Exception);
            } else {
                Credential credential = GoogleAuthProvider.GetCredential(task.Result.IdToken, null);

                auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(authTask => {
                    if (authTask.IsCanceled) {
                        signInCompleted.SetCanceled();
                        Debug.Log("Log in cancelled");
                    } else if (authTask.IsFaulted) {
                        signInCompleted.SetException(authTask.Exception);
                        Debug.LogError("Error during log in: " + authTask.Exception.Message);
                    } else {
                        signInCompleted.SetResult(authTask.Result);
                        OnAuthStateChanged(this, null);
                    }
                });

            }
        });
    }

    public void Logout() {
        auth.SignOut();
        GoogleSignIn.DefaultInstance.SignOut();

        OnAuthStateChanged(this, null);
    }

    public void Play() {
        SceneManager.LoadScene(1);
    }

    public void MultiPlayer() {
        SceneManager.LoadScene(2);
    }

    public void Quit() {
        Application.Quit();
    }

    private void Update() {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            if (Application.platform == RuntimePlatform.Android) {
                AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
                activity.Call<bool>("moveTaskToBack", true);
            }
            else Quit();
        }
    }

    private void ShowMainMenu() {
        mainMenu.SetActive(true);
        loginMenu.SetActive(false);

        if (auth.CurrentUser != null) {
            RawImage img = mainMenu.GetComponentInChildren<RawImage>();
            System.Uri url = auth.CurrentUser.PhotoUrl;

            StartCoroutine(SetImage(img, url.ToString()));        
        }
    }

    private void ShowLoginMenu() {
        mainMenu.SetActive(false);
        loginMenu.SetActive(true);
    }

    private IEnumerator SetImage(RawImage img, string url) {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError("Request error: " + www.error);
        } else {
            Texture2D myTexture = ((DownloadHandlerTexture) www.downloadHandler).texture;
            img.texture = myTexture;
        }
    }
}
