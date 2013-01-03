using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using mvCentral.DataProviders;
//using MediaPortal.Music.Database;

namespace mvCentral.Utils
{
  static class AMGHTMLParser
  {
    // artist regular expressions
    private const string ArtistDetailsRegExp = @"<dl class=""details"">.*</dl>";
    private static readonly Regex ArtistDetailsRegEx = new Regex(ArtistDetailsRegExp, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private const string GenreRegExp = @"<dt>Genres</dt>\s*<dd class=""genres"">\s*<ul>(?<genres>.*?)</ul>";
    private static readonly Regex GenreRegEx = new Regex(GenreRegExp, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private const string StyleRegExp = @"<dt>Styles</dt>\s*<dd class=""styles"">\s*<ul>(?<styles>.*?)</ul>";
    private static readonly Regex StyleRegEx = new Regex(StyleRegExp, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private const string ActiveRegExp = @"<dt>Active</dt>\s*<dd class=""active"">(?<active>.*?)</dd>";
    private static readonly Regex ActiveRegEx = new Regex(ActiveRegExp, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    
    private const string BornRegExp = @"<dd class=""birth"">\s*<span>(?<born>.*?)</span>";
    private static readonly Regex BornRegEx = new Regex(BornRegExp, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    private const string DeathRegExp = @"<dd class=""death"">\s*<span>(?<death>.*?)</span>";
    private static readonly Regex DeathRegEx = new Regex(DeathRegExp, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    private const string TonesRegExp = @"<h4>artist moods</h4>\s*<ul>(?<tones>.*?)</ul>";
    private static readonly Regex TonesRegEx = new Regex(TonesRegExp, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private const string BIORegExp = @"<div id=""bio"">\s*<div class=""heading"">.*?</div>(?<BIO>.*?)<div class=""advertisement leaderboard"">";
    private static readonly Regex BIORegEx = new Regex(BIORegExp, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private const string ImgRegExp = @"<div class=""artist-image"">\s*<div class=""image-container has-gallery"">\s*<img src=""(?<imgURL>.*?)""";
    private static readonly Regex ImgRegEx = new Regex(ImgRegExp, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
    private const string AlbumRowRegExp = @"<tr>.*?</tr>";
    private static readonly Regex AlbumRowRegEx = new Regex(AlbumRowRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private const string ArtistAlbumYearRegExp = @"<td class=""year.*?>(?<year>.*?)</td>";
    private static readonly Regex ArtistAlbumYearRegEx = new Regex(ArtistAlbumYearRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private const string ArtistAlbumNameRegExp = @"<td class=""title primary_link"".*?<a href="".*?"" class=""title.*?"" data-tooltip="".*?"">(?<albumName>.*?)</a>";
    private static readonly Regex ArtistAlbumNameRegEx = new Regex(ArtistAlbumNameRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private const string ArtistAlbumLabelRegExp = @"<td class=""label"".*?<span class=""full-title"">(?<label>.*?)</span>";
    private static readonly Regex ArtistAlbumLabelRegEx = new Regex(ArtistAlbumLabelRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    // album regular expressions
    private const string AlbumImgURLRegExp = @"<div class=""image-container"" data-large="".*?http(?<imageURL>.*?)&quot;,&quot;author";
    private static readonly Regex AlbumImgURLRegEx = new Regex(AlbumImgURLRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private const string AlbumRatingRegExp = @"<span class=""hidden"" itemprop=""rating"">(?<rating>.*?)</span>";
    private static readonly Regex AlbumRatingRegEx = new Regex(AlbumRatingRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private const string AlbumYearRegExp = @"<dd class=""release-date"">.*(?<year>\d{4}?)</dd>";
    private static readonly Regex AlbumYearRegEx = new Regex(AlbumYearRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private const string AlbumGenreRegExp = @"<dd class=""genres"">\s*<ul>(?<genres>.*?)</ul>";
    private static readonly Regex AlbumGenreRegEx = new Regex(AlbumGenreRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    private const string AlbumStylesRegExp = @"<dd class=""styles"">\s*<ul>(?<styles>.*?)</ul>.*</dl>";
    private static readonly Regex AlbumStylesRegEx = new Regex(AlbumStylesRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    
    private const string AlbumMoodsRegExp = @"<h4>album moods</h4>\s*<ul>(?<moods>.*?)</ul>";
    private static readonly Regex AlbumMoodsRegEx = new Regex(AlbumMoodsRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    
    private const string AlbumReviewRegExp = @"<div class=""editorial-text collapsible-content"" itemprop=""description"">(?<review>.*?)</div>";
    private static readonly Regex AlbumReviewRegEx = new Regex(AlbumReviewRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
    
    private const string AlbumTracksRegExp = @"<div id=""tracks"">.*<tbody>(?<tracks>.*?)</tbody>";
    private static readonly Regex AlbumTracksRegEx = new Regex(AlbumTracksRegExp, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

    private const string TrackRegExp = @"<tr>.*?<td class=""tracknum"">(?<trackNo>.*?)</td>.*?<div class=""title"">\s*<a.*?>\s*(?<title>.*?)?\s*</a>.*?<td class=""time"">\s*(?<time>.*?)\s*</td>.*?</tr>";
    private static readonly Regex TrackRegEx = new Regex(TrackRegExp, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private const string TrackURLRegExp = @"<div class=""title"">\s*<a href=""(?<url>.*?)?\s*"".*?";
    private static readonly Regex TrackURLRegEx = new Regex(TrackURLRegExp, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static Regex composerRegEx = new Regex("Composed by:\\s*(?:<a\\s*href=\"[^\"]+\">(?<composer>[^<]+)</a>(?:<span>\\s*/\\s*</span>)?)*\\s*</div>", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    // general regular expressions
    private const string HTMLListRegExp = @"<li>.*?</li>";
    private static readonly Regex HTMLListRegEx = new Regex(HTMLListRegExp, RegexOptions.Singleline | RegexOptions.IgnoreCase);
    private const string HTMLRegExp = @"<.*?>";
    private static readonly Regex HTMLRegEx = new Regex(HTMLRegExp, RegexOptions.Singleline | RegexOptions.Compiled);

    public static AlbumInfo ParseAlbumHTML(string albumHTML, string strAlbum, string strAlbumArtist)
    {


      // Image URL
      var imgURL = string.Empty;
      var imgMatch = AlbumImgURLRegEx.Match(albumHTML);
      if (imgMatch.Success)
      {
        imgURL = imgMatch.Groups["imageURL"].Value;
        imgURL = imgURL.Replace(@"\", @"");
        if (!string.IsNullOrEmpty(imgURL))
        {
          imgURL = "http" + imgURL;
        }
      }

      // Rating
      var dRating = 0.0;
      var ratingMatch = AlbumRatingRegEx.Match(albumHTML);
      if (ratingMatch.Success)
      {
        double.TryParse(ratingMatch.Groups["rating"].Value.Trim(), out dRating);
      }

      // year
      var iYear = 0;
      var yearMatch = AlbumYearRegEx.Match(albumHTML);
      if (yearMatch.Success)
      {
        int.TryParse(yearMatch.Groups["year"].Value.Trim(), out iYear);
      }

      // review
      var reviewMatch = AlbumReviewRegEx.Match(albumHTML);
      var strReview = string.Empty;
      if (reviewMatch.Success)
      {
        strReview = HTMLRegEx.Replace(reviewMatch.Groups["review"].Value.Trim(), "");
      }

      // build up track listing into one string
      var strTracks = string.Empty;
      var trackMatch = AlbumTracksRegEx.Match(albumHTML);
      if (trackMatch.Success)
      {
        var trackURLs = TrackURLRegEx.Matches(trackMatch.Groups["tracks"].Value.Trim());
        var tracks = TrackRegEx.Matches(trackMatch.Groups["tracks"].Value.Trim());

        foreach (Match track in tracks)
        {
          var strDuration = track.Groups["time"].Value;
          var iDuration = 0;
          var iPos = strDuration.IndexOf(":", StringComparison.Ordinal);
          if (iPos >= 0)
          {
            var strMin = strDuration.Substring(0, iPos);
            var strSec = strDuration.Substring(iPos + 1);
            int iMin = 0, iSec = 0;
            Int32.TryParse(strMin, out iMin);
            Int32.TryParse(strSec, out iSec);
            iDuration = (iMin * 60) + iSec;
          }

          strTracks += track.Groups["trackNo"].Value + "@" + track.Groups["title"].Value + "@" +
                       iDuration.ToString(CultureInfo.InvariantCulture) + "@" + trackURLs[int.Parse(track.Groups["trackNo"].Value) - 1].Groups["url"].Value + "|";
        }
      }

      // build up genres into one string
      var strGenres = string.Empty;
      var genreMatch = AlbumGenreRegEx.Match(albumHTML);
      if (genreMatch.Success)
      {
        var genres = HTMLListRegEx.Matches(genreMatch.Groups["genres"].Value.Trim());
        foreach (var genre in genres)
        {
          var cleanGenre = HTMLRegEx.Replace(genre.ToString(), "");
          strGenres += cleanGenre + ", ";
        }
        strGenres = strGenres.TrimEnd(new[] { ' ', ',' });
      }

      // build up styles into one string
      var strStyles = string.Empty;
      var styleMatch = AlbumStylesRegEx.Match(albumHTML);
      if (styleMatch.Success)
      {
        var styles = HTMLListRegEx.Matches(styleMatch.Groups["styles"].Value.Trim());
        foreach (var style in styles)
        {
          var cleanStyle = HTMLRegEx.Replace(style.ToString(), "");
          strStyles += cleanStyle + ", ";
        }
        strStyles = strStyles.TrimEnd(new[] { ' ', ',' });
      }

      // build up moods into one string
      var strMoods = string.Empty;
      var moodMatch = AlbumMoodsRegEx.Match(albumHTML);
      if (moodMatch.Success)
      {
        var moods = HTMLListRegEx.Matches(moodMatch.Groups["moods"].Value.Trim());
        foreach (var mood in moods)
        {
          var cleanMood = HTMLRegEx.Replace(mood.ToString(), "");
          strMoods += cleanMood + ", ";
        }
        strMoods = strMoods.TrimEnd(new[] { ' ', ',' });
      }

      var album = new AlbumInfo
      {
        Album = strAlbum,
        Artist = strAlbumArtist,
        Genre = string.Empty,
        Tones = strMoods,
        Styles = strStyles,
        Review = strReview,
        Image = imgURL,
        Rating = (int)(dRating * 2),
        Tracks = strTracks,
        AlbumArtist = strAlbumArtist,
        Year = iYear
      };

      return album;

    }

    public static ArtistInfo ParseArtistHTML(string strArtistHTML, string strArtist)
    {
      var match = ArtistDetailsRegEx.Match(strArtistHTML);
      if (!match.Success)
      {
        return null;
      }

      var artistDetails = match.Value;

      // build up genres into one string
      var strGenres = string.Empty;
      var genreMatch = GenreRegEx.Match(artistDetails);
      if (genreMatch.Success)
      {
        var genres = HTMLListRegEx.Matches(genreMatch.Groups["genres"].Value.Trim());
        foreach (var genre in genres)
        {
          var cleanGenre = HTMLRegEx.Replace(genre.ToString(), "");
          strGenres += cleanGenre + ", ";
        }
        strGenres = strGenres.TrimEnd(new[] { ' ', ',' });
      }

      // build up styles into one string
      var strStyles = string.Empty;
      var styleMatch = StyleRegEx.Match(artistDetails);
      if (styleMatch.Success)
      {
        var styles = HTMLListRegEx.Matches(styleMatch.Groups["styles"].Value.Trim());
        foreach (var style in styles)
        {
          var cleanStyle = HTMLRegEx.Replace(style.ToString(), "");
          strStyles += cleanStyle + ", ";
        }
        strStyles = strStyles.TrimEnd(new[] { ' ', ',' });
      }

      // years active
      var strActive = string.Empty;
      var activeMatch = ActiveRegEx.Match(artistDetails);
      if (activeMatch.Success)
      {
        strActive = activeMatch.Groups["active"].Value.Trim();
      }

      // born / formed
      var strBorn = string.Empty;
      var strFormed = string.Empty;
      // Find the match - Bordn or Formed are both in as Birth
      var bornMatch = BornRegEx.Match(artistDetails);
      // Check it Born or Formed
      var asBorn = Regex.IsMatch(artistDetails, @"<dt>\s*born\s*</dt>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
      if (bornMatch.Success)
      {
        if (asBorn)
          strBorn = bornMatch.Groups["born"].Value.Trim();
        else
          strFormed = bornMatch.Groups["born"].Value.Trim();
      }

      // Death / Disbanded
      var strDeath = string.Empty;
      var strDisbanded = string.Empty;
      // Find the match - Bordn or Formed are both in as Birth
      var deathMatch = DeathRegEx.Match(artistDetails);
      // Check if Died or Disbanded
      var asDisbanded = Regex.IsMatch(artistDetails, @"<dt>\s*disbanded\s*</dt>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
      if (deathMatch.Success)
      {
        if (asDisbanded)
          strDisbanded = deathMatch.Groups["death"].Value.Trim();
        else
          strDeath = deathMatch.Groups["death"].Value.Trim();
      }


      // build up tones into one string
      var strTones = string.Empty;
      var tonesMatch = TonesRegEx.Match(strArtistHTML);
      if (tonesMatch.Success)
      {
        var tones = HTMLListRegEx.Matches(tonesMatch.Groups["tones"].Value.Trim());
        foreach (var tone in tones)
        {
          var cleanTone = HTMLRegEx.Replace(tone.ToString(), "");
          strTones += cleanTone + ", ";
        }
        strTones = strTones.TrimEnd(new[] { ' ', ',' });
      }

      // Biography
      var AMGBIO = string.Empty;
      var AMGBioMatch = BIORegEx.Match(strArtistHTML);
      if (AMGBioMatch.Success)
      {
        AMGBIO = AMGBioMatch.Groups["BIO"].Value.Trim();
        AMGBIO = HTMLRegEx.Replace(AMGBIO, "");
        AMGBIO = AMGBIO.TrimStart(' ','\n');
      }

      // artist image URL
      var strImg = string.Empty;
      var imgMatch = ImgRegEx.Match(strArtistHTML);
      if (imgMatch.Success)
      {
        strImg = imgMatch.Groups["imgURL"].Value;
      }

      // list albums
      var albumRows = AlbumRowRegEx.Matches(strArtistHTML);
      var albumList = string.Empty;
      foreach (Match albumRow in albumRows)
      {
        var albumNameMatch = ArtistAlbumNameRegEx.Match(albumRow.Value);
        if (!albumNameMatch.Success)
        {
          continue;
        }
        var albumName = albumNameMatch.Groups["albumName"].Value.Trim();
        var albumYear = ArtistAlbumYearRegEx.Match(albumRow.Value).Groups["year"].Value.Trim();
        var albumLabel = ArtistAlbumLabelRegEx.Match(albumRow.Value).Groups["label"].Value.Trim();
        albumList += string.Format("{0} - {1} ({2})", albumYear, albumName, albumLabel) + Environment.NewLine;
      }

      var artistInfo = new ArtistInfo
      {
        AMGBio = AMGBIO,
        Albums = albumList,
        Artist = strArtist,
        Born = strBorn,
        Formed = strFormed,
        Death = strDeath,
        Disbanded = strDisbanded,
        Compilations = string.Empty,
        Genres = strGenres,
        Image = strImg,
        Instruments = string.Empty,
        Misc = string.Empty,
        Singles = string.Empty,
        Styles = strStyles,
        Tones = strTones,
        YearsActive = strActive
      };

      return artistInfo;

    }
  }

}
