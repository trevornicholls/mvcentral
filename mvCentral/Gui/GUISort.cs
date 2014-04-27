using System;
using System.Windows.Forms;
using System.IO;
using Common.GUIPlugins;
using NLog;
using System.Collections.Generic;
using System.Collections;
using MediaPortal.Playlists;
using MediaPortal.GUI.Library;
using MediaPortal.Dialogs;
using MediaPortal.Player;
using WindowPlugins;


namespace mvCentral.GUI
{
    public partial class MvGuiMain : WindowPluginBase
    {
        long lastPress = 0;
        string sortString = "";
        int count = 0;
        bool reset = false;
        Timer timeOut = new Timer();

        private void DoSpell(MediaPortal.GUI.Library.Action.ActionType remoteNum)
        {
            switch (remoteNum)
            {
                case MediaPortal.GUI.Library.Action.ActionType.REMOTE_2:
                    GetSortChar("ABC");
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.REMOTE_3:
                    GetSortChar("DEF");
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.REMOTE_4:
                    GetSortChar("GHI");
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.REMOTE_5:
                    GetSortChar("JKL");
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.REMOTE_6:
                    GetSortChar("MNO");
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.REMOTE_7:
                    GetSortChar("PQRS");
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.REMOTE_8:
                    GetSortChar("TUV");
                    break;
                case MediaPortal.GUI.Library.Action.ActionType.REMOTE_9:
                    GetSortChar("WXYZ");
                    break;
            }
            doFacadeSort();
        }

        private void doFacadeSort()
        {
            int x = sortString.Length;
            for (int i = 0; i < facadeLayout.ListLayout.ListItems.Count; i++)
            {
                string tmp = facadeLayout.ListLayout.ListItems[i].Label.Substring(0, x).ToUpper();
                if (tmp == sortString)
                {
                    facadeLayout.SelectedListItemIndex = i;
                    break;
                }
            }
        }

        private void GetSortChar(string chars)
        {
            bool quickEnough = (DateTime.Now.Ticks - lastPress < (10000 * 1000));//1 second to keep working in same spot
            bool quickEnough2 = (DateTime.Now.Ticks - lastPress < (10000 * 3000));//3 seconds to start fresh
            lastPress = DateTime.Now.Ticks;
            int x = sortString.Length - 1;
            if (x < 0)
                sortString = chars[0].ToString();
            else
            {
                if (chars.Contains(sortString[x].ToString()))
                {
                    if (quickEnough)
                    {
                        char next = (char)((int)sortString[x] + 1);
                        if (next == (char)((int)chars[chars.Length - 1] + 1))
                            next = chars[0];
                        replaceChar(ref sortString, x, next);
                    }
                    else if (quickEnough2)
                    {
                        sortString += chars[0];
                    }
                }
                else
                {
                    sortString += chars[0];
                }
            }
            reset = true;
            GUIPropertyManager.SetProperty("#mvCentral.Sort", sortString);
            GUIPropertyManager.Changed = true;
            timeOut.Interval = 500;
            timeOut.Start();
        }

        void checkTime(object sender, EventArgs e)
        {
            if (reset)
            {
                count = 0;
                reset = false;
            }
            else
                count++;

            if (count >= 6)
            {
                sortString = "";
                GUIPropertyManager.SetProperty("#mvCentral.Sort", sortString.ToUpper());
                GUIPropertyManager.Changed = true;
                timeOut.Stop();
            }
        }

        void replaceChar(ref string text, int index, char charToUse)
        {
            char[] tmpBuffer = text.ToCharArray();
            tmpBuffer[index] = charToUse;
            text = new string(tmpBuffer);
        }
    }
}
