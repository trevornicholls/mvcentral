![http://mvCentral.googlecode.com/svn/wiki/Images/mvCentralLogo.jpg](http://mvCentral.googlecode.com/svn/wiki/Images/mvCentralLogo.jpg)



# Introduction #

This document related to mvCentral release 1.0.1 (1st March 2012)

The purpose of this document is to help you understand how mvCentral interacts with the MediaPortal's skin engine. In this document you're going to find a general overview of how the plug-in is structured, the components that you, as a skinner should be interested in, and a list of available properties that you can use.

It's recommended that you use the DefaultWide (16:9) or Default (4:3) skin as a base which is installed with the release. It is written by the same people that write the plug-in, so if you are having trouble getting something to work right, refer to this skin.

If you want to see whats NEW in a particular version, search for the version throughout this page e.g. To see what has changed in v1.0.5, perform a search on term '1.0.5'.


## Translations ##

You can make use any of the plugins string translations to create button menus, headings, labels etc. Syntax for translations are just simple skin properties in the form #mvCentral.Translation.$(string).Label e.g.
| **Property** | **English** |
|:-------------|:------------|
| #mvCentral.Translation.Artists.Label | Artists |
| #mvCentral.Translation.FavouriteVideos.Label | Favourite Videos |
| #mvCentral.Translation.PlayAllRandom.Label | Play All (Random) |
| #mvCentral.Translation.Tracks.Label | Tracks |

There is a wide selection of translated strings to choose from, [here](http://mvCentral.googlecode.com/svn/trunk/Language/mvCentral/en-US.xml) is a complete list. Use the Field value in the skin property to get the translation you require.


---

## Skin Files required for mvCentral ##


There are 5 skin files required for mvCentral, this Wiki will detailed the Controls, required images and skin properities avaiable for each skin. Please look at the included Default and DefaultWide, these can be used as a referance when building your own skin files.
If you find what you think is a bug or if you have any suggestions for improvement please mail me or add to the issue tracker

| **Skin Filename** | **Skin ID** | **Description** |
|:------------------|:------------|:----------------|
| mvCentral.xml | 112011 | Main skin file, loaded when plugin is selected |
| mvCentral.Playlist.xml | 112012 | Playlist skin file, access from mvCentral.xml and SmartDJ.xml |
| mvCentral.StatsAndInfo.xml | 112013 | Most played Artist and Video. Also displays top ten videos and progress bars for background processes |
| mvCentral.SmartDJ.xml |112015 | SmartDJ screen, allows creating of playlists by either matching or filtering on fields |
| DialogMvMultiSelect.xml | 112014 | Custom select dialog allowing for muliple selections |


---

## Common Properities ##

**Primary Properities used within all screens**

| **Skin Property**     | **Description** |
|:----------------------|:----------------|
| #itemtype           | Set as the localized translation description of #itemcount |
| #itemcount          | Set to total number of Artists |
| #selecteditem       | Label of current selected facade item |
| #selectedthumb      | Texture of current selected facade item |
| #mvCentral.Hierachy | This skin property is altered depending on where the user has navigated to, it displays the View names initially and will expand with each level the user accesses |



---

## mvCentral.xml ##

This is the main skin file for mvCentral and is displayed when the user first enters the plugin.

This screen will allow display of the video files in 4 views, Artists, Albums (If enabled), Tracks (Videos) and Genres. In addition this screen will also display the progress of background metadata refresh, this is normally a once off process unless configured in Advanced setting in the plugin configuration.

### Controls ###

The following is a list of the controls used in this skin file, all but ID 12 and 50 are nornally used in a menu.

| **Control ID** | **Type** | **Text** | **Description** |
|:---------------|:---------|:---------|:----------------|
| 2 | Button | Layout: type | MediaPortal Standard Layout Button - Calls dialog with layout choices, List, Icon (small/large), Filmstrip & Coverflow |
| 3 | Button | Sort | MediaPortal Standard Sort Button - Calls dialog with sort options |
| 5 | Button | Switch View | Mediaportal Standard View Button - Calls dislog with View options, Artist, Album, Tracks and Genres |
| 6 | Button | Play All Random | Create and play all vidoes in random order |
| 7 | Button | Smart Playlists | Calls a Menu Dialog with Smart Playlist option |
| 8 | Button | Playlist | Access the Playlist screen (mvCentral.Playlist.xml) |
| 9 | Button | Stats and Info | Access the Status and Information screen (mvCentral.StatsAndInfo.xml) |
| 10 | Button | Configured Genres | Call the Multi Select Dialog (DialogMvMultiSelect.xml) to allow Genres to be created from Last.Fm tags |
| 11 | Button | Search Artists | Brings up the virtual keyboard to allow entry of search string |
| 12 | Progressbar | - | Metadata background update progress bar Note: When scan is active #mvCentral.Metadata.Scan.Active will be set to 'true' |
| 50 | Facade | - | Facade control - List, Icon Small, Icon Large, Filmstrip and Coverflow supported |

### Exposed Skin Properities ###

As of Version 1.0.1 the skin currently supports 4 views and a 5th is in development.

  * Artist - Initial display is Artists (Defaut View)
  * Albums - Initial display is Albums
  * Tracks - Inital display is tracks
  * Genres - Inital display is Genres
  * DVD - Initial display will be DVDs only (this is work in progress as of the 1st March 2012)

For each view there is a corrsponding skin property, this will be set to **true** for the currely selected view, see the table below.

| **Skin Property** | **Description**                              |
|:------------------|:---------------------------------------------|
| #mvCentral.ArtistView | Set to 'true' if Artists is selected view |
| #mvCentral.AlbumView  | Set to 'true' if Albums is selected view  |
| #mvCentral.TrackView  | Set to 'true' if Tracks is selected view  |
| #mvCentral.GenreView  | Set to 'true' if Genres is selected view  |
| #mvCentral.DVDView    | Currently not used - reserved           |


For each view there is a set of skin properties exposed when an items is selected, the following sections detail the view and properities that are exposed.

### Artist View ###

**Visibility test**
> `<visible>string.equals(#mvCentral.ArtistView,true)</visible>`
**Required Image File**
> defaultArtistBig.png

**Selected item skin properities**

| **Skin Property** | **Description** |
|:------------------|:----------------|
| #mvCentral.ArtistBio	|Set to the full text of the Artist biography or the localaized "No Biography Available for Artist **artist**" text |
| #mvCentral.ArtistName	| Set to the Artist Name |
| #mvCentral.ArtistImg	| Set to the path of the full resolution artist image |
| #mvCentral.VideosByArtist	| Set to the total number videos by this Artist |
| #mvCentral.ArtistTracksRuntime	| Set to the total runtime of all videos by this artist, format HH:MM:SS |
| #mvCentral.BornOrFormed	| Set to the date the Artist was born or group formed, if no data is available this is set to "No Born/Formed Details" |
| #mvCentral.Genre	| Set to the main genre for the Artist |
| #mvCentral.ArtistTags	| List of Last.FM tags seperated by a space |



### Album View ###
**Visibility test**
> `<visible>string.equals(#mvCentral.AlbumView,true)</visible>`
**Required Image File**
> defaultAlbum.png

**Selected item skin properities**

| **Skin Property** | **Description** |
|:------------------|:----------------|
| #mvCentral.Album | Set to Album Name (Note: this is not present in version 1.0.1) |
| #mvCentral.Album.Rating | Set to rating value for the Album between 0 and 9 |
| #mvCentral.ArtistName	| Set to the Artist Name |
| #mvCentral.Watched.Count | Set to the number of time the video has been watched (0 if not watched) |
| #mvCentral.VideoImg	| Set to the Album cover image |
| #mvCentral.TrackInfo | Set to the full text of the Album description or localized message if not description exists |
| #mvCentral.AlbumInfo | Set to the full text of the Album description or localized message if not description exists |
| #mvCentral.AlbumTracksRuntime | Set to the total runtime for all videos in the album |
| #mvCentral.TracksForAlbum | Set to the number of videos within this album |


### Track (Video) View ###
**Visibility test**
> `<visible>string.equals(#mvCentral.TrackView,true)</visible>`
**Required Image File**
> defaultVideoBig.png

  * elected item skin properities

| **Skin Property** | **Description** |
|:------------------|:----------------|
| #iswatched | Set to 'yes' if this video has been watched and 'no' if not. |
| #mvCentral.Watched.Count | Set to the number of time the video has been watched (0 if not watched) |
| #mvCentral.VideoImg	| Set to the Video thumbnail image |
| #mvCentral.TrackInfo | Set to the full text of the track description |
| #mvCentral.Track.Rating | Set to rating value for the track between 0 and 9 |
| #mvCentral.Composers | Set to the composer(s), if more than 1 composers are seperated by a comma |
| #mvCentral.ArtistName	| Set to the Artist Name |
| #mvCentral.Genre	| Set to Artist Genre |
| #mvCentral.Duration | Set to the playtime of the track in the format MM:SS or HH:MM:SS if longer than 1hr |


Video and Audio Media Details as extracted and reported by MediaInfo

| **Skin Property** | **Description** |
|:------------------|:----------------|
| #mvCentral.LocalMedia.videoresolution | Video resolution 720p, 1080i, 1080p etc |
| #mvCentral.LocalMedia.videoaspectratio | Set to\*fullscreen**or**widescreen|
| #mvCentral.LocalMedia.videocodec | Set to the video codec |
| #mvCentral.LocalMedia.videowidth | Set to the video width |
| #mvCentral.LocalMedia.videoheight | Set to the video height |
| #mvCentral.LocalMedia.videoframerate | Set to the video framerate |
| #mvCentral.LocalMedia.audiocodec | Set to the audio codec |
| #mvCentral.LocalMedia.audiochannels | Set to the numbe of audio channels |
| #mvCentral.LocalMedia.audio | Combined setting of #mvCentral.LocalMedia.audiocodec #mvCentral.LocalMedia.audiochannels |

These additional properities are set/changed if this track is part of an Album

| **Skin Property** | **Description** |
|:------------------|:----------------|
| #mvCentral.Album.Rating | Set to rating value for the Album between 0 and 9 |
| #mvCentral.Hierachy | Will be set to **Artist Name|Album Name** |
| #mvCentral.Album | Set to Album Name (Note: this is not present in version 1.0.1) |


### Genre View ###

This view will only be available if Genres have been configured or Artist has AllMusic Genre.

**Visibility test**
> `<visible>string.equals(#mvCentral.GenreView,true)</visible>`
**Required Image File**
> DefaultGenre.png

**Selected item skin properities**

| **Skin Property**     | **Description** |
|:----------------------|:----------------|
| #mvCentral.ArtistTracksRuntime | Total runtime of all videos for Artists that match the Genre |
| #mvCentral.VideosByArtist | Total number of videos for Artists that match the Genre |

### Background Metadata process - Progress Bar ###

Control ID 12 is a process bat that display the progess of the background Metadata process. This process is normally only run one when the user updates to a new version that has a new provider though this can be overiddeen in advanced settings.

There are two skin properties that are linked to this progress bar.

| **Skin Property** | **Description** |
|:------------------|:----------------|
| #mvCentral.Metadata.Scan.Active | This is set to the value **true** when the background process is active |
| #mvCentral.Metadata.Update.Progress| The percentage complete, a value between 1 and 100 (Format #.##% Complete) Note: Complete is a localized string |

Code Example

```
    <control>
      <type>label</type>
      <description>Background Metadata Refresh Text Percentage</description>
      <id>0</id>
      <posX>320</posX>
      <posY>600</posY>
      <width>800</width>
      <font>font9</font>
      <align>left</align>
      <label>Background Metadata Refresh: #mvCentral.Metadata.Update.Progress</label>
      <visible>control.isvisible(12)</visible>
      <animation effect="fade" time="70" reversible="false">visible</animation>
    </control>
    <control>
      <description>Background Metadata Refresh Progress Bar</description>
      <type>progress</type>
      <id>12</id>
      <posX>320</posX>
      <posY>630</posY>
      <width>440</width>
      <height>20</height>
      <label>-</label>
      <texturebg>osd_progress_background.png</texturebg>
      <onlymidtexture>yes</onlymidtexture>
      <midwidth>440</midwidth>
      <midheight>18</midheight>
      <midoffsetX>1</midoffsetX>
      <midoffsetY>1</midoffsetY>
      <midtexture>osd_progress_mid.png</midtexture>
      <animation effect="fade" time="70" reversible="false">visible</animation>
      <visible>string.equals(#mvCentral.Metadata.Scan.Active,true)</visible>
    </control>
```



---

## mvCentral.Playlist.xml ##

The playlist screen allows the display of the current Playlist using various layouts, current supported layouts are Small Icon, Large Icon, Filmstrip, Playlist and Coverflow. This screen also allows for loading and saveing of playlistsas well as standard Playlist functions.

### Controls ###

| **Control ID** | **Type** | **Text** | **Description** |
|:---------------|:---------|:---------|:----------------|
| 2 | Button | Layout: type | MediaPortal Standard Layout Button - Calls dialog with layout choices, Icon (small/large), Filmstrip, Playlist & Coverflow |
| 9 | Button | Load | Load a dialog to allow loading pf previous saved Playlists |
| 20 | Button | Shuffle | Shuffle currently loaded/playing Playlist |
| 21 | Button | Save | Displays a virtual keboard to allow entry of playlist name to save |
| 22 | Button | Clear | Clear the current Playlist |
| 23 | Button | Play | Start plying the loaded Playlist |
| 24 | Button | Next | Skip to next track in Playlist |
| 25 | Button | Previous | Play previous track in Playlist |
| 30 | ToggleButton | Repeat | If enabled will repeat the Playlist |
| 40 | ToggleButton | Auto Play | When enabled will start playing the Playlist when loaded |
| 50 | Facade | - | Facade control - Icon Small, Icon Large, Filmstrip, Playlist and Coverflow supported |

**Global Properites**

| **Skin Property** | **Description** |
|:------------------|:----------------|
| #mvCentral.Hierachy	| Set to the current navigation level |
| #mvCentral.Playlist.Count	| Set to the total number of videos in the Playlist |
| #mvCentral.Playlist.Runtime | Set to the total runtime for all videos in the playlist |

**Selected item skin properities**

| **Skin Property** | **Description** |
|:------------------|:----------------|
| #iswatched | Set to 'yes' if this video has been watched and 'no' if not. |
| #mvCentral.Watched.Count | Set to the number of time the video has been watched (0 if not watched) |
| #mvCentral.ArtistName | Set to the Artist Name |
| #mvCentral.ArtistImg | Set to the path of the full resolution Artist image |
| #mvCentral.ArtistTags	| List of Last.FM tags seperated by a space |
| #mvCentral.Genre	| Set to the main genre for the Artist |
| #mvCentral.BornOrFormed	| Set to the date the Artist was born or group formed, if no data is available this is set to "No Born/Formed Details" |
| #mvCentral.VideoImg	| Set to the Video thumbnail image |
| #mvCentral.Track.Rating | Set to rating value for the track between 0 and 9 |
| #mvCentral.Composers | Set to the composer(s), if more than 1 composers are seperated by a comma |
| #mvCentral.Description | Set to the full text of the track description or Artist Bio if no track description |
| #mvCentral.Duration | Set to the playtime of the track in the format MM:SS or HH:MM:SS if longer than 1hr |

**Video and Audio Media Details as extracted and reported by MediaInfo**

| **Skin Property** | **Description** |
|:------------------|:----------------|
| #mvCentral.LocalMedia.videoresolution | Video resolution 720p, 1080i, 1080p etc |
| #mvCentral.LocalMedia.videoaspectratio | Set to\*fullscreen**or**widescreen|
| #mvCentral.LocalMedia.videocodec | Set to the video codec |
| #mvCentral.LocalMedia.audiocodec | Set to the audio codec |
| #mvCentral.LocalMedia.audiochannels | Set to the numbe of audio channels |
| #mvCentral.LocalMedia.audio | Combined setting of #mvCentral.LocalMedia.audiocodec #mvCentral.LocalMedia.audiochannels |



---

## mvCentral.StatsAndInfo.xml ##

The Status and Information screen provides an overview of the current video collection, it displays

**Total Artists and Videos**Most Played Artist
**Most Played Video**Top Ten Played Videos
**Progess bars for Metadata and Artwork Background Processes**

### Controls ###

| **Control ID** | **Type** | **Text** | **Description** |
|:---------------|:---------|:---------|:----------------|
| 12 | Progress | - | Metadata background process progress bar |
| 13 | Progress | - | Artwork background process progress bar |
| 14 | Button | - | Hidden button, need to be present and used as a default control when entering the plugin |
| 15 | Label | Vx.x.x | Text set by the plugin to the version |
| 16 | Label | - | Set by the plugin as 'Database has X Videos across X Artists' |
| 18 | Image | - | Set by the plugin to the Video thumbnail image |
| 20 | Image | - | Set by the plugin to the Artist image thumbnail |
| 30 | Label | - | Top Ten display (1) - Set by the plugin to the 'Artist - Track' |
| 31 | Label | - | Top Ten display (2) - Set by the plugin to the 'Artist - Track' |
| 32 | Label | - | Top Ten display (3) - Set by the plugin to the 'Artist - Track' |
| 33 | Label | - | Top Ten display (4) - Set by the plugin to the 'Artist - Track' |
| 34 | Label | - | Top Ten display (5) - Set by the plugin to the 'Artist - Track' |
| 35 | Label | - | Top Ten display (6) - Set by the plugin to the 'Artist - Track' |
| 36 | Label | - | Top Ten display (7) - Set by the plugin to the 'Artist - Track' |
| 37 | Label | - | Top Ten display (8) - Set by the plugin to the 'Artist - Track' |
| 38 | Label | - | Top Ten display (9) - Set by the plugin to the 'Artist - Track' |
| 39 | Label | - | Top Ten display (10) - Set by the plugin to the 'Artist - Track' |

**Exposed Skin Properites**

| **Skin Property** | **Description** |
|:------------------|:----------------|
| #mvCentral.Hierachy	| Set to the current navigation level |
| #mvCentral.MostPlayed	| Most played video track name |
| #mvCentral.FavArtist	| Most played Artist name |

### Background Metadata & Artwork processes - Progress Bars ###

Control ID 12 and 13 are progress bars they display the progess of the background Metadata and Artwork processess.

**Metadata Refresh Progress Bar**

Control ID: 12

There is one skin propertie linked to this progress bar.

| **Skin Property** | **Description** |
|:------------------|:----------------|
| #mvCentral.Metadata.Update.Progress| The percentage complete, a value between 1 and 100 (Format #.##% Complete) Note: Complete is a localized string |

**Artwork Refresh Progress Bar**

Control ID: 13

There is one skin propertie linked to this progress bar.

| **Skin Property** | **Description** |
|:------------------|:----------------|
| #mvCentral.Artwork.Update.Progress| The percentage complete, a value between 1 and 100 (Format #.##% Complete) Note: Complete is a localized string |



---

## mvCentral.SmartDJ.xml ##

SmartDJ is basiclly a Playlist builder, it has two modes.

**Match**
This mode displays buttons for the 6 main infomation metadata fields

| Genre | AllMusic Genre |
|:------|:---------------|
| Last.FM Tag | Tags from Last.FM |
| Style | Styles as sourced from AllMusic |
| Tones | Tones as sourced from AllMusic |
| Composer | Track composer(s) |

By clicking a button a select dialog is displayed allowing the user to chose an item, all vidoes are then searched and these are displayed in a Facade list. Each subsquent field selected will add to this list.

**Filter**
This mode allows the user to chose the order of fields, selecting a button will first display a list of fields (same as those for Match mode) and after selection will then display the data for that field allowing the user to chose as per Match mode and the result is displayed as a Facade list. Where filter mode differs is that each subsquent field chosen will use selected data set as input and will thus narrow the selected data.


### **Controls** ###

| **Control ID** | **Type** | **Text** | **Description** |
|:---------------|:---------|:---------|:----------------|
| 20 | Button | Match or Filter | This button text will swap bewteen Match and Filter each time selected |
| 21 | Button | Play Playlist | This will convert the selected items on the Facade into a Play list and start playing |
| 22 | Button | Save Playlist | Displays a virtual keboard to allow entry of playlist name to save |
| 23 | Checkbutton | Shuffle | If selected this will shuffle the Playlist before playing |
| 24 | Button | Playlist | Jump to Playlist screen |
| 30 | Button | - | Search field buuton, will either be 'Genre' or 'Select Filter Field...' |
| 31 | Button | - | Search field buuton, will either be 'Last.Fm Tag' or 'Select Filter Field...' |
| 32 | Button | - | Search field buuton, will either be 'Style' or 'Select Filter Field...' |
| 33 | Button | - | Search field buuton, will either be 'Tone' or 'Select Filter Field...' |
| 34 | Button | - | Search field buuton, will either be 'Composer' or 'Select Filter Field...' |
| 35 | Button | - | Search field buuton, will either be 'Keyword' or 'Select Filter Field...' |
| 37 | Label | - | Displays the total artists and videos selected (localized) |
| 50 | Facade | - | Facade control - List supported |


### **Exposed Skin Properites** ###

| **Skin Property** | **Description** |
|:------------------|:----------------|
| #mvCentral.SmartDF.Mode	| Set to either 'Mode: Match' or 'Mode: Filter' |

### **Selected item skin properties** ###

| **Skin Property** | **Description** |
|:------------------|:----------------|
| #mvCentral.#mvCentral.ArtistName	| Set to the Artist Name **(v1.0.2)**|
| #mvCentral.#mvCentral.TrackInfo	| Set to the full text of the track description **(v1.0.2)**|
| #mvCentral.#mvCentral.Track.Rating	| Set to rating value for the track between 0 and 9 **(v1.0.2)**|
| #mvCentral.#mvCentral.Composers	| Set to the composer(s), if more than 1 composers are seperated by a comma **(v1.0.2)**|
| #mvCentral.#mvCentral.Duration	| Set to the playtime of the track in the format MM:SS or HH:MM:SS if longer than 1hr **(v1.0.2)**|
| #mvCentral.#mvCentral.ArtistName	| Artist name for selected item on Facade list **(v1.0.2)**|
| #iswatched | Set to **yes** if the track has been previouly played or **no** if not. **(v1.0.2)** |
| ##mvCentral.Watched.Count | Set to the number of times the video has been watched **(v1.0.2)** |


---

## DialogMvMultiSelect.xml ##

Custom Multi Select Dialog, there is not much that need to be changed with this file as uses std dialog images as per DialogSelect.xml.



---

## Play Properities ##

This secion list the properties that are set when a video starts to play, currently playing video and next video in playlist properities are provided.


**mvCentral specific current play properties**

| **Skin Property** | **Description** |
|:------------------|:----------------|
| #mvCentral.isPlaying | Set to **true** if mvCentral is playing a video |
| #Play.Current.mvArtist | Set to the artist name of the currently playing track |
| #Play.Current.mvAlbum | Set to the Album name the track is from or blank in no assoicated album |
| #Play.Current.mvVideo | Set to the video title |
| #Play.Current.Video.Thumb | Set to the video thumbnail |

**Standard current play properties**

| **Skin Property** | **Description** |
|:------------------|:----------------|
| #Play.Current.Title | Set to the Video title |
| #Play.Current.Thumb | Set to the path full resolution Artist image|
| #Play.Current.Genre | Set to Artist genre (**v1.0.2)** |
| #Play.Current.Runtime | Set to Runtime of the playing track (**v1.0.2)** |
| #Play.Current.Rating | Set to the current track rating (between 0 and 9) (**v1.0.2)** |
| #Play.Current.Plot | Set to the full track description  |
| #Play.Current.IsWatched | Set to **yes** if the track has been previouly played or **no** if not. (**v1.0.2)** |


**Video and Audio Media Details as extracted and reported by MediaInfo**

| **Skin Property** | **Description** |
|:------------------|:----------------|
| #Play.Current.AspectRatio | Set to\*fullscreen**or**widescreen**(**v1.0.2)|
| #Play.Current.VideoCodec.Texture | Set to the video codec (**v1.0.2)** |
| #Play.Current.VideoResolution | Video resolution 720p, 1080i, 1080p etc (**v1.0.2)** |
| #mvCentral.Current.videowidth | Set to the video width |
| #mvCentral.Current.videoheight | Set to the video height |
| #mvCentral.Current.videoframerate | Set to the video framerate |
| #Play.Current.AudioCodec.Texture | Set to the audio codec (**v1.0.2)** |
| #Play.Current.AudioChannels | et to the number of audio channels |

**mvCentral specific current play properties**

| **Skin Property** | **Description** |
|:------------------|:----------------|
| #mvCentral.isPlaying | Set to **true** if mvCentral is playing a video |
| #Play.Next.mvArtist | Set to the artist name of the currently playing track |
| #Play.Next.mvAlbum | Set to the Album name the track is from or blank in no assoicated album |
| #Play.Next.mvVideo | Set to the video title |
| #Play.Next.Video.Thumb | Set to the video thumbnail |

**Standard current play properties**

| **Skin Property** | **Description** |
|:------------------|:----------------|
| #Play.Next.Title | Set to the Video title |
| #Play.Next.Thumb | Set to the path full resolution Artist image|
| #Play.Next.Genre | Set to Artist genre (**v1.0.2)** |
| #Play.Next.Runtime | Set to Runtime of the playing track (**v1.0.2)** |
| #Play.Next.Rating | Set to the current track rating (between 0 and 9) (**v1.0.2)** |
| #Play.Next.Plot | Set to the full track description  |
| #Play.Next.IsWatched | Set to **yes** if the track has been previouly played or **no** if not. (**v1.0.2)** |


**Video and Audio Media Details as extracted and reported by MediaInfo**

| **Skin Property** | **Description** |
|:------------------|:----------------|
| #Play.Next.AspectRatio | Set to\*fullscreen**or**widescreen**(**v1.0.2)|
| #Play.Next.VideoCodec.Texture | Set to the video codec (**v1.0.2)** |
| #Play.Next.VideoResolution | Video resolution 720p, 1080i, 1080p etc (**v1.0.2)** |
| #mvCentral.Next.videowidth | Set to the video width |
| #mvCentral.Next.videoheight | Set to the video height |
| #mvCentral.Next.videoframerate | Set to the video framerate |
| #Play.Next.AudioCodec.Texture | Set to the audio codec (**v1.0.2)** |
| #Play.Next.AudioChannels | Set to the number of audio channels |


## Video info pop-up on Video Start **(v1.0.2)** ##

Property
|#mvCentral.Play.Started|
|:----------------------|

Possible Values
| true | For the first 5 secs (Default) when a video starts |
|:-----|:---------------------------------------------------|
| false | Set when timeout expires, this is 5 seconds by default but can be changed in configuration |

Using the above property you can provide an info pop-box each time a video starts, the image below is from StreamedMP.

![http://mvCentral.googlecode.com/svn/wiki/Images/thumbnails/videoInfo.jpg](http://mvCentral.googlecode.com/svn/wiki/Images/thumbnails/videoInfo.jpg)

Example code (only single control shown, full file can be found [HERE](http://mvCentral.googlecode.com/svn/trunk/Skin/StreamedMP/mvCentral.PlayStart.xml))
```
    <control>
      <description>Movie/File Status Icons group</description>
      <type>group</type>
      <dimColor>ffffffff</dimColor>
      <visible>!window.isosdvisible+string.equals(#mvCentral.Play.Started,true)</visible>
      <animation effect="fade" time="300" delay="300">VisibleChange</animation>
      <animation effect="slide" start="600,0" end="0,0" tween="quadratic" time="500" delay="100">VisibleChange</animation>
      <control>
        <description>background</description>
        <type>image</type>
        <id>0</id>
        <posX>700</posX>
        <posY>530</posY>
        <width>585</width>
        <height>192</height>
        <texture>mpinfo_filmbox.old.png</texture>
      </control>
      .
      .
      .
      .
    </control>
```

Some things to be aware of when using animations inside videoFullScreen.xml, do not use animation types WindowOpen or WindowClose as these will cause MediaPortal to crash. I have used VisibleChange type - I have not tested other types.

### Skins Supporting Video Info Pop-up ###

[StreamedMP](http://code.google.com/p/streamedmp/)

![http://mvCentral.googlecode.com/svn/wiki/Images/thumbnails/videoInfo.jpg](http://mvCentral.googlecode.com/svn/wiki/Images/thumbnails/videoInfo.jpg)

[aMPed](http://code.google.com/p/amped/)

![http://mvCentral.googlecode.com/svn/wiki/Images/thumbnails/videoInfo-aMPed.jpg](http://mvCentral.googlecode.com/svn/wiki/Images/thumbnails/videoInfo-aMPed.jpg)


---

## Latest Added Music Video Support ##

mvCentral will check for the last 3 added music vidoes and if found will set 3 skin properities for each music video found.

| **Skin Property** | **Description** |
|:------------------|:----------------|
| #mvCentral.latest.enabled | Set **true** if there are latest videos |


| **Skin Property** | **Description** |
|:------------------|:----------------|
| #mvCentral.Latest.ArtistX | X = 1, 2 or 3 - Artist Name |
| #mvCentral.Latest.ArtistImageX | X = 1,2 or 3 - Full path to Artist Image |
| #mvCentral.Latest.TrackX | X = 1,2 or 3 - Track Name |
