using System;
using System.Collections;
using System.Windows.Forms;
using Prowo;

public class ListViewComparer : IComparer
{
    private int col;
    private SortOrder order;
    private bool IsNumeric;
    private bool PatternFilter;

    public ListViewComparer(int col, bool? IsNumeric, SortOrder order, bool PatternFilter = false)
    {
        this.col = col;
        this.order = order;
        this.IsNumeric = IsNumeric == null ? false : (bool)IsNumeric;
        this.PatternFilter = PatternFilter;
    }

    public int Compare(object x, object y)
    {
        ListViewItem item1 = (ListViewItem)x, item2 = (ListViewItem)y;
        try
        {
            if (IsNumeric)
            {
                if (Convert.ToInt32(item1.SubItems[col].Text) < Convert.ToInt32(item2.SubItems[col].Text))
                    return SortOrder.Ascending == order ? -1 : 1;
                else if (Convert.ToInt32(item1.SubItems[col].Text) == Convert.ToInt32(item2.SubItems[col].Text))
                    return 0;
                else return SortOrder.Ascending == order ? 1 : -1;
            }
        }
        catch (Exception) { }

        int st1 = 0, st2 = 0;
        if (PatternFilter)
        {
            while (item1.SubItems[col].Text[st1] == ' ' || Char.IsLower(item1.SubItems[col].Text[st1]))
                ++st1;
            while (item2.SubItems[col].Text[st2] == ' ' || Char.IsLower(item2.SubItems[col].Text[st2]))
                ++st2;
        }
        if (this.order == SortOrder.Ascending)
            return item1.SubItems[col].Text.Substring(st1).CompareTo(item2.SubItems[col].Text.Substring(st2));
        else
            return item2.SubItems[col].Text.Substring(st2).CompareTo(item1.SubItems[col].Text.Substring(st1));
    }
}