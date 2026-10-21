Terms used in InstaloaderSharp GUI and this manual. Instagram terms and InstaloaderSharp's own terms are explained in more detail in the [InstaloaderSharp glossary](https://github.com/InstaloaderSharp/InstaloaderSharp/wiki/Glossary).

| Term | Meaning |
|---|---|
| **Account chip** | The box at the bottom of the sidebar that shows how the program accesses Instagram; click it to open the Account page |
| **Activity page** | The page that shows the progress and output of the current download |
| **Anonymous** | Downloading without an Instagram account. Only public profiles and posts are available |
| **App interface** | The interface of Instagram's mobile apps. Used when signed in for the largest versions of pictures and videos (**Use iPhone API**), stories and search |
| **Argument file** | A text file with one InstaloaderSharp command-line argument per line, run with `InstaloaderSharp.exe +args.txt`. Saved and imported under *Settings → Command line* |
| **Badge** | A small coloured label: the type of a target in the target list, **Requires login** on options, or **Verified**, **Private** and the story badge on the Look up page |
| **Browser cookies** | The sign-in method that imports your login from a web browser (`--load-cookies`) |
| **Caption** | The text of a post. Saved in the post's `.txt` file, whose contents can be changed with a caption template |
| **Checkpoint** | A verification that Instagram demands before it accepts a sign-in or further requests, done in a browser or the app |
| **Download folder** | The folder downloads are saved into. Set under **Save to** on the Download page |
| **Equivalent command** | The InstaloaderSharp command that does the same as the GUI's current targets and options, shown under *Settings → Command line* and at the top of a download's output |
| **Exit code** | The number with which `InstaloaderSharp.exe` ends: 0 success, 1 finished with errors, 2 invalid options, 3 login failed, 4 aborted, 5 stopped by the user, 99 unexpected error. The GUI shows the matching result in words |
| **Fast update** | Stopping at the first post that was downloaded before (`--fast-update`) |
| **Feed** | The posts of the accounts you follow, as on Instagram's home page. Target `:feed` |
| **Filter** | A condition that a post or story item must meet to be downloaded, written as a Python expression; see [Filtering posts and stories](Filtering-posts-and-stories) |
| **Followees** | The accounts a profile follows. A followees target (`@profilename`) downloads all of them |
| **Highlight** | A collection of former stories that a profile keeps on its page. Downloaded with **Highlights** |
| **Instagram login** | The sign-in method that signs in with your username and password (`--login`) and saves a session |
| **InstaloaderSharp** | The C# version of Instaloader that does the downloading. The GUI uses `InstaloaderSharp.exe` as a library |
| **Instaloader** | The original Python program that InstaloaderSharp is based on |
| **Latest stamps** | The times of the newest downloaded items per profile, stored by **Remember latest downloads** (`--latest-stamps`) |
| **Metadata file** | A `.json` or `.json.xz` file with a post's data, such as likes and location. Can be used as a target to download the post again |
| **Pattern** | A template for folder or file names, such as `{target}` or `{date_utc}_UTC`, with placeholders in curly brackets |
| **Placeholder** | A `{name}` in a pattern or caption template, replaced by a property of the post |
| **Preview** | The card on the Look up page that shows a profile or hashtag |
| **Reel** | A short video post shown in a profile's reels tab. Downloaded with **Reels** |
| **Resume file** | An `iterator_….json.xz` file in a target's folder that stores where a stopped download was, so it can continue there |
| **Session** | The saved login of an account, stored in a session file in `%LOCALAPPDATA%\Instaloader`. It contains the login cookie, not the password |
| **Shortcode** | The identifier of a post in its link, the part after `/p/` or `/reel/`. Written with a leading `-` as a target |
| **Sidecar** | A post with several pictures or videos (Instagram calls it a *carousel*). Each picture gets a numbered file; **Images of multi-image posts** selects some of them |
| **Status bar** | The bar at the bottom of the window with the current state and the **Start download** or **Stop** button |
| **Story** | Pictures and videos that a profile shares for 24 hours. Downloaded with **Stories**, or for all accounts you follow with the **Followed stories** target |
| **Story item** | One picture or video of a story |
| **Tagged posts** | Posts of other accounts in which a profile is tagged. Downloaded with **Tagged posts** |
| **Target** | Something to download: a profile, hashtag, location, single post, your feed, your saved posts, followed stories, followees or a metadata file |
| **Two-factor authentication** | An extra code from an authenticator app or text message that some accounts need when signing in |
| **UTC** | Coordinated Universal Time. File names use the UTC time of a post by default |
