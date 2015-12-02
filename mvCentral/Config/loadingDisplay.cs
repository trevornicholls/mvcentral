#region GNU license
// MP-TVSeries - Plugin for Mediaportal
// http://www.team-mediaportal.com
// Copyright (C) 2006-2007
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
#endregion

using System.Windows.Forms;

namespace mvCentral
{
  public partial class loadingDisplay : Form
  {
    public loadingDisplay()
    {
      InitializeComponent();
      ShowWaiting();
    }

    public new void Close()
    {
      this.Dispose();
      base.Close();
    }

    void ShowWaiting()
    {
      this.lbTask.Text = "Startup";
      this.artists.Text = "0 " + Localizations.Localization.GetByName("Artists");
      this.videos.Text = "0 " + Localizations.Localization.GetByName("Videos");
      this.albums.Text = "0 " + Localizations.Localization.GetByName("Albums");
      this.version.Text = "v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
      this.Show();
      this.Refresh();
    }

    public void updateStats(string task, int artists, int albums, int videos)
    {
      this.lbTask.Text = task;
      this.artists.Text = artists.ToString() + " " + Localizations.Localization.GetByName("Artists");
      this.albums.Text = albums.ToString() + " " + Localizations.Localization.GetByName("Albums");
      this.videos.Text = videos.ToString() + " " + Localizations.Localization.GetByName("Vidoes");
      this.Refresh();
    }

  }
}