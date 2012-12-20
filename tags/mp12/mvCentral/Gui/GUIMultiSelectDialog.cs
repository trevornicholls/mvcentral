#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
// Cornerstone
using Cornerstone.Database;
using Cornerstone.Database.Tables;
using Cornerstone.GUI.Dialogs;
using Cornerstone.Tools;
// Internal
using mvCentral.Database;
using mvCentral;
using mvCentral.LocalMediaManagement;
using mvCentral.Playlist;
using mvCentral.Utils;
using mvCentral.Localizations;
// Mediaportal
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using Action = MediaPortal.GUI.Library.Action;
using Layout = MediaPortal.GUI.Library.GUIFacadeControl.Layout;

using NLog;

/*
 * void InitializeSearch		-> Initialize e.g. SQL data connections
 * bool DoSearch						-> True if successful, false if not (
 * bool LocateFirst					-> True if >0 results, false if not
 * SearchResult NextResult	-> null if there are no results anymore
 * void FinalizeSearch			-> Clean up all allocated blocks and finalize data connections
 */

namespace MediaPortal.Dialogs
{
  public enum ModalResult
  {
    None = 1,
    OK = 2,
    Cancel = 4
  }

  public class GUIDialogMultiSelect : GUIDialogMenu
  {
    #region Skin attributes
    [SkinControlAttribute(3)]
    public GUIListControl SelectionList = null;

    [SkinControlAttribute(10)]
    protected GUIButtonControl btnOK = null;

    [SkinControlAttribute(11)]
    protected GUIButtonControl btnCancel = null;
    #endregion

    #region Attributes
    public ModalResult DialogModalResult = ModalResult.None;
    protected bool m_bRunning = true;
    public List<GUIListItem> ListItems = new List<GUIListItem>();
    #endregion

    #region Overrides

    public override string GetModuleName()
    {
      return mvCentralCore.Settings.HomeScreenName;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\DialogMvMultiSelect.xml");
    }

    public override int GetID
    {
      get
      {
        return 112014;
      }
    }

    protected override void OnClicked(int controlId, GUIControl control, Action.ActionType actionType)
    {
      if (control == btnOK)
      {
        DialogModalResult = ModalResult.OK;
        Close();
      }

      if (control == btnCancel)
      {
        DialogModalResult = ModalResult.Cancel;
        Close();
      }

      if (control == SelectionList)
      {
        if (SelectionList.SelectedListItem != null)
        {
          SelectionList.SelectedListItem.Selected = !SelectionList.SelectedListItem.Selected;
          return;
        }
      }

      base.OnClicked(controlId, control, actionType);
    }

    public new void DoModal(int dwParentId)
    {
      m_bRunning = true;
      DialogModalResult = ModalResult.None;
      base.DoModal(dwParentId);
    }

    public new void Reset()
    {
      ListItems.Clear();
      base.Reset();
    }

    public new void Add(string strLabel)
    {
      int iItemIndex = ListItems.Count + 1;
      GUIListItem pItem = new GUIListItem();
      if (base.ShowQuickNumbers)
        pItem.Label = iItemIndex.ToString() + " " + strLabel;
      else
        pItem.Label = strLabel;

      pItem.ItemId = iItemIndex;
      ListItems.Add(pItem);

      base.Add(strLabel);
    }

    public new void Add(GUIListItem pItem)
    {
      ListItems.Add(pItem);
      base.Add(pItem);
    }

    #endregion

    #region Virtual methods
    public virtual void Close()
    {
      if (m_bRunning == false) return;
      m_bRunning = false;
      GUIWindowManager.IsSwitchingToNewWindow = true;
      lock (this)
      {
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_WINDOW_DEINIT, GetID, 0, 0, 0, 0, null);
        base.OnMessage(msg);

        GUIWindowManager.UnRoute();
        m_bRunning = false;
      }
      GUIWindowManager.IsSwitchingToNewWindow = false;
    }
    #endregion
  }
}



