using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

using DataStructures;

namespace Prowo
{
    public partial class Form1 : Form
    {
        public static List<Schüler> schüler = new List<Schüler>();
        public static List<Projekt> Projekte = new List<Projekt>(0);

        public static List<Projekt> Solution;
        public static List<Projekt> Start;

        public List<NumericUpDown> SchHWunsch = new List<NumericUpDown>();

        public int CountWisch(const_type<int> Wish, const_type<int> ProjektIndex)
        {
            int cnt = 0;
            foreach (Schüler S in schüler)
                if (S.Wünsche.Count <= Wish)
                    continue;
                else if (Wish == 0 && S.isLeiter)
                    continue;
                else if (Wish > 0 && S.Wünsche[Wish - 1] == S.Wünsche[Wish])
                    continue;
                else if (S.Wünsche[Wish] == ProjektIndex)
                    ++cnt;
            return cnt;
        }

        public List<Schüler> calc()
        {
            List<Schüler> Rest = new List<Schüler>();//Students who cannot get one of their wishs

            foreach (var s in schüler)
            {
                bool aBool = true;
                for (int i = 0; i < TabPages.Count; ++i)
                {
                    try { s.Wünsche[i] = s.Wünsche[i]; }
                    catch (Exception) { continue; }

                    if (Start[s.Wünsche[i]].TeilnehmerCount < Start[s.Wünsche[i]].MaxAnz || s.isLeiter)
                    {
                        Start[s.Wünsche[i]].Add(new Schüler(s));
                        aBool = false;
                        break;
                    }
                }
                if (aBool)
                    Rest.Add(new Schüler(s));
            }
            return Rest;
        }
        public List<Schüler> calcBlind()
        {
            for (int i = 0; i < Start.Count; i++)
                Start[i].reset();
            List<Schüler> Rest = new List<Schüler>();
            foreach (var s in schüler)
            {
                bool aBool = true;

                if (!s.WishesSet)
                    continue;

                if (s.isLeiter || Start[s.Wünsche[0]].editable)
                {
                    Start[s.Wünsche[0]].Add(new Schüler(s));
                    aBool = false;
                    break;
                }
                if (aBool)
                    Rest.Add(new Schüler(s));
            }
            return Rest;
        }
        public void init(const_type<bool> blind,const_type<bool> remUseLess)
        {
            List<Schüler> Rest;
            Start = Projekte;
            if (blind)
                Rest = calcBlind();
            else
                Rest = calc();

            if (remUseLess)
                Rest.AddRange(remUselessProj(Start));

            int cnt = 0;
            while (cnt < Rest.Count)
            {
                bool aB = true;
                for (int i = 0; i < Projekte.Count; i++)
                    if (Start[i].editable && Start[i].KlassenStufen[(int)Rest[0].Klassenstufe - 5] && Start[i].TeilnehmerCount < Projekte[i].MinAnz)
                    {
                        Start[i].Add(new Schüler(Rest[0]));
                        Rest.RemoveAt(0);
                        aB = false;
                        break;
                    }
                if (aB)
                    ++cnt;
            }
            cnt = 0;
            while (cnt < Rest.Count)
            {
                bool aB = true;
                for (int i = 0; i < Start.Count; i++)
                    if (Start[i].editable && Start[i].KlassenStufen[(int)Rest[0].Klassenstufe - 5] && Start[i].TeilnehmerCount < Projekte[i].MaxAnz)
                    {
                        Start[i].Add(new Schüler(Rest[0]));
                        Rest.RemoveAt(0);
                        aB = false;
                        break;
                    }
                if (aB)
                    ++cnt;
            }

        }

        public List<Schüler> remUselessProj(List<Projekt> Projekte)
        {
            List<Schüler> ret = new List<Schüler>();
            for (int i = 0; i < Projekte.Count; ++i)
                if (!Projekte[i].erhaltenswert && getE_Wert(i,Projekte) < numericUpDown6.Value)
                    ret.AddRange(Projekte[i].kill());
            return ret;
        }

        static decimal aktBestSolValue = -1;
        static SortedSet<int> Temperaturfolge = new SortedSet<int>();
        static List<int> Schritte = new List<int>();
        static int SchrittCounter = 0;

        decimal bewerte(out int AnzMin, out int AnzMax, out int AnzCMiss)
        {
            decimal score = 0;
            AnzMin = 0; AnzMax = 0; AnzCMiss = 0;
            for (int i = 0; i < Projekte.Count; ++i)
            {
                foreach (var S in Projekte[i].GetList())
                {
                    int cnt = 0;
                    foreach (var W in S.GetList())
                        if (W == i)
                        {
                            score += (decimal)SchHWunsch[cnt].Value;
                            break;
                        }
                        else ++cnt;
                    if (!(Projekte[i].KlassenStufen[(int)S.Klassenstufe - 5]))
                        ++AnzCMiss;
                }
                if (Projekte[i].TeilnehmerCount < Projekte[i].MinAnz)
                    ++AnzMin;
                else if (Projekte[i].TeilnehmerCount > Projekte[i].MaxAnz)
                    ++AnzMax;
            }
            return score;
        }
        decimal bewerte_sol(out int AnzMin, out int AnzMax, out int AnzCMiss)
        {
            decimal score = 0;
            AnzMin = 0; AnzMax = 0; AnzCMiss = 0;
            for (int i = 0; i < Solution.Count; ++i)
            {
                foreach (var S in Solution[i].GetList())
                {
                    int cnt = 0;
                    foreach (var W in S.GetList())
                        if (W == i)
                        {
                            score += (decimal)SchHWunsch[cnt].Value;
                            break;
                        }
                        else
                            ++cnt;
                    if (!(Solution[i].KlassenStufen[(int)S.Klassenstufe - 5]))
                        ++AnzCMiss;
                }
                if (Solution[i].TeilnehmerCount < Solution[i].MinAnz)
                    ++AnzMin;
                else if (Solution[i].TeilnehmerCount > Solution[i].MaxAnz)
                    ++AnzMax;
            }
            return score;
        }

        public void calculate()
        {
            setSettings(false);
            int ANZMIN, ANZMAX, ANZCMISS;
            if (Start == null)
                init(checkBox2.Checked, checkBox3.Checked);
            for (; !beenden; )
            {
                Random rnd = new Random();
                for (int i = 0; i < 5000; i++)
                {
                    int b = rnd.Next(1, schüler.Count * (int)SchHWunsch[0].Value);
                    Temperaturfolge.Add(b);
                    Schritte.Add(b);
                }
                SchrittCounter = 0;

                Projekte = Start;

                Application.DoEvents();

                while (bewerte(out ANZMIN, out ANZMAX, out ANZCMISS)
                        * (ANZMIN > 0 ? (numericUpDown2.Value / 100) : 1) * (ANZMAX > 0 ? (numericUpDown3.Value / 100) : 1)
                        * (ANZCMISS > 0 ? 0 : 1) < schüler.Count * (int)SchHWunsch[0].Value && !beenden)
                {
                    Application.DoEvents();

                    decimal BewertungOld = (bewerte(out ANZMIN, out ANZMAX, out ANZCMISS)
                        * (ANZMIN > 0 ? (numericUpDown2.Value / 100) : 1) * (ANZMAX > 0 ? (numericUpDown3.Value / 100) : 1)
                        * (ANZCMISS > 0 ? 0 : 1));

                    if (Temperaturfolge.Count == 0 || (BewertungOld == 0 && (rnd.Next(1, schüler.Count * (int)SchHWunsch[0].Value) > Temperaturfolge.Last())))
                        break;
                    if (SchrittCounter > Temperaturfolge.Last())
                    {
                        SchrittCounter = 0;
                        Temperaturfolge.Remove(Temperaturfolge.Last());
                    }

                    #region Austausch zweier Schüler
                    int ZufProj1 = rnd.Next(0, Projekte.Count);
                    while (!Projekte[ZufProj1].editable || Projekte[ZufProj1].TeilnehmerCount == 0)
                        ZufProj1 = rnd.Next(0, Projekte.Count);
                    int ZufProj2 = rnd.Next(0, Projekte.Count);
                    while (ZufProj1 == ZufProj2 || !Projekte[ZufProj2].editable || Projekte[ZufProj2].TeilnehmerCount == 0)
                        ZufProj2 = rnd.Next(0, Projekte.Count);

                    bool Prj1CanBeEmpty = Projekte[ZufProj1].TeilnehmerCount < Projekte[ZufProj1].MaxAnz;

                    int ZufSchüler1 = rnd.Next(0, Projekte[ZufProj1].Length + (Prj1CanBeEmpty ? 1 : 0));
                    bool isMax = ZufSchüler1 == Projekte[ZufProj1].Length;
                    while (!isMax && Projekte[ZufProj1][ZufSchüler1].isLeiter)
                    {
                        ZufSchüler1 = rnd.Next(0, Projekte[ZufProj1].Length + (Prj1CanBeEmpty ? 1 : 0));
                        isMax = ZufSchüler1 == Projekte[ZufProj1].Length;
                    }

                    int ZufSchüler2 = rnd.Next(0, Projekte[ZufProj2].Length);
                    while (Projekte[ZufProj2][ZufSchüler2].isLeiter)
                        ZufSchüler2 = rnd.Next(0, Projekte[ZufProj2].Length);

                    Schüler Z1 = null, Z2 = new Schüler(Projekte[ZufProj2][ZufSchüler2]);
                    if (!isMax)
                        Z1 = new Schüler(Projekte[ZufProj1][ZufSchüler1]);

                    if (!isMax)
                        Projekte[ZufProj2].Add(Z1);
                    Projekte[ZufProj1].Add(Z2);
                    Projekte[ZufProj2].Remove(Projekte[ZufProj2][ZufSchüler2]);
                    if (!isMax)
                        Projekte[ZufProj1].Remove(Projekte[ZufProj1][ZufSchüler1]);

                    decimal BewertungNew = (bewerte(out ANZMIN, out ANZMAX, out ANZCMISS)
                        * (ANZMIN > 0 ? (numericUpDown2.Value / 100) : 1) * (ANZMAX > 0 ? (numericUpDown3.Value / 100) : 1)
                        * (ANZCMISS > 0 ? 0 : 1));

                    if (BewertungNew <= BewertungOld && (rnd.Next(1, schüler.Count * (int)SchHWunsch[0].Value) < Temperaturfolge.Last()))
                    {
                        Projekte[ZufProj2].Add(new Schüler(Z2));
                        Projekte[ZufProj1].Remove(Z2);
                        if (!isMax)
                        {
                            Projekte[ZufProj1].Add(new Schüler(Z1));
                            Projekte[ZufProj2].Remove(Z1);
                        }
                        SchrittCounter++;
                    }

                    #endregion

                    decimal B2 = BewertungOld > BewertungNew ? BewertungOld : BewertungNew;
                    int Quality = 66;
                    try
                    { Quality = (int)numericUpDown1.Value; }//Convert.ToInt32(textBox12.Text); }
                    catch (Exception) { }
                    if ((B2 * 100) / (schüler.Count * (int)SchHWunsch[0].Value) >= Quality && B2 > aktBestSolValue)
                    {
                        if (B2 > aktBestSolValue)
                        {
                            SchrittCounter = 0;
                            if (Temperaturfolge.Count > 0)
                                Temperaturfolge.Remove(Temperaturfolge.Last());
                        }
                        aktBestSolValue = B2;
                        numericUpDown1.Value = ((aktBestSolValue * 100) / (SchHWunsch[0].Value * schüler.Count));

                        label1.Text = "Die aktuelle Lösung hat eine Güte von " + (int)((B2 * 100) / (schüler.Count * SchHWunsch[0].Value))
                            + "%. (Bewertung: " + B2 + ")";
                        label1.Visible = true;

                        Solution = new List<Projekt>();
                        int cnt = 0;
                        listView3.Items.Clear();
                        for (int i = 0; i < Projekte.Count; i++)
                        {
                            Solution.Add(new Projekt(Projekte[i]));
                            int cnt2 = 0;
                            foreach (var item in Projekte[i].GetList().Union(Projekte[i].GetLeiterList()))
                            {
                                var Item = item.AsItem(button7.BackColor, cnt, i, cnt2++);
                                Item.Group = listView3.Groups[i];
                                if (!Projekte[i].KlassenStufen[(int)item.Klassenstufe - 5])
                                    Item.ForeColor = Color.Red;
                                for (int W = 0; W < item.Wünsche.Count; ++W)
                                    if (item.Wünsche[W] == i)
                                    {
                                        Item.ForeColor = ColorButtons[W].BackColor;
                                        break;
                                    }
                                if (Projekte[i].TeilnehmerCount < Projekte[i].MinAnz && !item.isLeiter)
                                    Item.BackColor = button2.BackColor;
                                if (Projekte[i].TeilnehmerCount > Projekte[i].MaxAnz && !item.isLeiter)
                                    Item.BackColor = button3.BackColor;

                                listView3.Items.Add(Item);
                            }
                        }
                        this.Refresh();
                    }
                }
            }
            setSettings(true);
            }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }
    }
}