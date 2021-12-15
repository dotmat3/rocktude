using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;

public class UserController {

    private static UserController instance;

    public static UserController DefaultInstance {
        get {
            if (instance == null)
                instance = new UserController();
            return instance;
        }
    }

    private FirebaseAuth auth;

    public UserController() {
        auth = FirebaseAuth.DefaultInstance;
    }

    public FirebaseUser GetUser() {
        return auth.CurrentUser;
    }

    public string GetUsername() {
        if (GetUser() != null)
            return GetUser().DisplayName;
        return "Desktop user";
    }

    public string GetUserId() {
        if (GetUser() != null)
            return GetUser().UserId;
        return "123456789";
    }
}
