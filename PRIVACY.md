# Lorevia privacy information

Lorevia stores travel plans, saved places, custom destination notes, heritage records and downloaded articles on your Windows account’s local storage. It has no Lorevia account, advertising SDK or automatic analytics upload. Startup errors are written locally. Files are not encrypted by Lorevia; protect your Windows account and backups.

## Online browsing

Opening maps, Wikipedia, source links, support or release pages connects to those websites through the embedded Microsoft Edge WebView2 browser. Those sites receive normal web request information such as your IP address and may store cookies or collect information under their own policies. Wikipedia searches include your search terms. Check for updates contacts the public GitHub releases API only when requested.

Use Settings > Clear browsing data to clear the embedded profile’s cookies, cache, history and sign-ins. Downloaded articles and travel records are separate and remain until you delete them. WebView2 Runtime is a Microsoft component and may update independently. No precise device location is requested by Lorevia; “Local” means your selected home country.

## Optional folder sync and sharing

Connecting a cloud folder writes readable snapshots of travel plans, custom destination notes, favorites and travel preferences to the selected folder. Your installed OneDrive, Dropbox or other folder-sync provider transfers them under its own policies. Lorevia does not receive the provider’s credentials. Anyone with folder access can read snapshots. Archive records and offline articles are not included in travel sync.

Disconnecting sync stops future writes. Existing snapshots remain in the selected folder until you remove them using your file manager. Cloud providers may retain their own versions. Shared trip files include the selected trip, its notes, budget and referenced destinations. Review them before sending to someone else.

## Recovery and deletion

Lorevia retains up to ten previous versions of each managed data file for recovery. Deleting an item from the app does not immediately remove it from recovery copies. Uninstalling leaves personal data and cloud files intact so you can reinstall without losing plans. To remove all local data, close Lorevia and remove %LOCALAPPDATA%\DigitalHeritage using Windows File Explorer. This removes travel plans, archive records, downloads, browser data, logs and recovery copies. Independently remove exported files or cloud copies if you no longer want them.

## Support

The About and support screen opens the project’s GitHub issue form with the app version and the description you enter. Nothing is submitted until you submit the form yourself. GitHub issues may be public: do not include private trip details, credentials or personal data.

Project support: https://github.com/ZeroTrace0245/Digital-heritage-preservation-app/issues

Updated: 2026-09-11
