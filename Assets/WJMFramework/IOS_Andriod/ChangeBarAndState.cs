using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeBarAndState : MonoBehaviour
{

	// Use this for initialization
	void Start ()
    {
/*
        // Toggles the dimmed out state (where status/navigation content is darker)
        ApplicationChrome.dimmed = !ApplicationChrome.dimmed;

        // Set the status/navigation background color (set to 0xff000000 to disable)
        ApplicationChrome.statusBarColor = ApplicationChrome.navigationBarColor = 0xffff3300;

        // Makes the status bar and navigation bar visible (default)
        ApplicationChrome.statusBarState = ApplicationChrome.navigationBarState = ApplicationChrome.States.Visible;

        // Makes the status bar and navigation bar visible over the content (different content resize method) 
        ApplicationChrome.statusBarState = ApplicationChrome.navigationBarState = ApplicationChrome.States.VisibleOverContent;

        // Makes the status bar and navigation bar visible over the content, but a bit transparent
        ApplicationChrome.statusBarState = ApplicationChrome.navigationBarState = ApplicationChrome.States.TranslucentOverContent;

        // Makes the status bar and navigation bar invisible (animated)
        ApplicationChrome.statusBarState = ApplicationChrome.navigationBarState = ApplicationChrome.States.Hidden;
*/
       
        ApplicationChrome.statusBarState = ApplicationChrome.navigationBarState = ApplicationChrome.States.Visible;
    }

	

}
