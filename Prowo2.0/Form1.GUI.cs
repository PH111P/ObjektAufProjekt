using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DataStructures;
using System.Windows.Forms;
using System.Drawing;

namespace Prowo
{
    /// <summary>
    /// Diese Datei enthält die Methoden, die mit dem User interagieren
    /// </summary>
    public partial class Form1 : Form
    {

        bool beenden = false;
        bool flag = false;
        public List<TabPage> TabPages = new List<TabPage>();
        public List<Button> ColorButtons = new List<Button>();

        #region Data I/O
        public bool openProject(const_type<string> Path)
        {
            Projekte.Clear();
            try
            {
                using (StreamReader sr = new StreamReader(Path))
                    while (!sr.EndOfStream)
                    {
                        string s = sr.ReadLine();
                        if (s.StartsWith("*")) continue;
                        if (s.Length == 0) continue;

                        foreach (string s2 in s.Split('/')[3].Split(','))
                            new Klasse(s2);
                    }
                using (StreamReader sr = new StreamReader(Path))
                {
                    while (!sr.EndOfStream)
                    {
                        string s = sr.ReadLine();
                        if (s.StartsWith("*")) continue;
                        if (s.Length == 0) continue;

                        BitString HilfsString = new BitString(0, Klasse.ID_rev.Count);

                        var Line = s.Split('/');

                        foreach (string s2 in s.Split('/')[3].Split(','))
                            HilfsString[Klasse.ID[new Klasse(s2)]] = true;

                        bool edit = true, imp = false;
                        int L = Line.Length;
                        if (L > 4)
                            edit = Convert.ToBoolean(Line[4]);
                        string Descr = "";
                        if (L > 5)
                            Descr = (Line[5]);
                        if (L > 6)
                            imp = Convert.ToBoolean(Line[6]);

                        Projekt P =
                            new Projekt(Line[0], Convert.ToInt32(Line[1]),
                                Convert.ToInt32(Line[2]), HilfsString, edit, Descr, imp);
                        listView1.Items.Add(P.AsItem(Projekte.Count));
                        Projekte.Add(P);
                        listView3.Groups.Add(new ListViewGroup(Line[0]));
                    }
                }
            }
            catch (Exception o)
            {
                MessageBox.Show(o.Message + " - Could not load file.", "ERROR");
                textBox1.Text = "";
                button11.Enabled = false;
                listView1.Items.Clear();
                listView3.Groups.Clear();
                return false;
            }
            return true;
        }
        public bool openSchüler(const_type<string> Path)
        {
            schüler.Clear();
            foreach (var item in Projekte)
            {
                item.reset();
            }
            try
            {
                using (StreamReader sr = new StreamReader(Path))
                    while (!sr.EndOfStream)
                    {
                        string s = sr.ReadLine();
                        if (s.StartsWith("*")) continue;
                        if (s.Length == 0) continue;
                        List<int> Wünsche = new List<int>();

                        for (int i = 2; i < s.Split('/', ',').Length - 1; ++i)
                        {
                            var ac = s.Split('/', ',')[i];
                            try
                            {
                                Wünsche.Add(Convert.ToInt32(ac));
                            }
                            catch (FormatException)
                            {
                                for (int j = 0; j < Projekte.Count; ++j)
                                    if (Projekte[j].Projektname == ac)
                                    {
                                        Wünsche.Add(j);
                                        break;
                                    }
                            }
                        }

                        string[] klasse = (s.Split('/', ',')[s.Split('/', ',').Length - 1]).Split('-');
                        aKlasse acKlasse = new Klasse(klasse[0].Trim(' '));
                        for (int i = 1; i < klasse.Length; ++i)
                            acKlasse = new KlasseDecorator(acKlasse, klasse[i].Trim(' '));

                        if (s.StartsWith("@"))
                        {
                            Objekt s2 = new Objekt(s.Split('/', ',')[0].Remove(0, 1), s.Split('/', ',')[1],
                                acKlasse, Wünsche, true);
                            Projekte[Wünsche[0]].Add(s2);
                            schüler.Add(s2);
                        }
                        else
                            schüler.Add(new Objekt(s.Split('/', ',')[0], s.Split('/', ',')[1],
                                acKlasse, Wünsche));
                    }
                int cnt = 0;
                foreach (var s in schüler)
                    listView2.Items.Add(s.AsItem(GetColor(s),button7.BackColor, cnt++));
            }
            catch (Exception o)
            {
                MessageBox.Show(o.Message + " - Could not load file.", "ERROR");
                textBox2.Text = "";
                listView2.Items.Clear();
                listView4.Items.Clear();
                return false;
            }
            fill_LV4(Projekte);
            return true;
        }
        public bool writeTEXFile(const_type<string> Path, bool Anwesenheit = true)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(Path))
                {

                    sw.WriteLine(@"\documentclass[a4paper,12pt]{article}");
                    sw.WriteLine(@"\usepackage[ngerman]{babel}");
                    sw.WriteLine(@"\usepackage[utf8]{inputenc}");
                    sw.WriteLine(@"\usepackage{fancyhdr}");
                    sw.WriteLine(@"\usepackage{longtable}");
                    sw.WriteLine(@"\setlength{\textwidth}{17.5cm}");
                    sw.WriteLine(@"\setlength{\textheight}{26cm}");
                    sw.WriteLine(@"\setlength{\topmargin}{-1.7cm}");
                    sw.WriteLine(@"\setlength{\evensidemargin}{-7mm}");
                    sw.WriteLine(@"\setlength{\oddsidemargin}{-7mm}");
                    sw.WriteLine(@"\parskip 6pt plus 1pt minus 1pt");
                    sw.WriteLine(@"\parindent0pt");
                    sw.WriteLine(@"\pagestyle{fancy}");

                    sw.WriteLine(@"\lhead{ObjektAufProjekt}");
                    sw.WriteLine(@"\chead{}");
                    sw.WriteLine(@"\rhead{Matchings}");
                    sw.WriteLine(@"\lfoot{}");
                    sw.WriteLine(@"\cfoot{}");
                    sw.WriteLine(@"\rfoot{}");

                    sw.WriteLine(@"\begin{document}");

                    List<Tuple<Objekt, string>> schler = new List<Tuple<Objekt, string>>();
                    foreach (Projekt P in Solution)
                    {
                        if (P.TeilnehmerCount == 0)
                            continue;
                        sw.WriteLine(@"\subsection*{" + P.Projektname + "}");
                        sw.WriteLine(P.Description);
                        if (P.AnzLeiter > 0)
                        {
                            List<Objekt> cpy = new List<Objekt>(P.GetLeiterList());
                            cpy.Sort();

                            sw.WriteLine(@"\subsubsection*{Projekt's leader}");
                            // sw.WriteLine(@"\begin{center}");
                            sw.WriteLine(@"\begin{tabular}{l|c}");
                            sw.WriteLine(@"\textbf{Name} & \textbf{Klasse} \\ \hline \hline");
                            foreach (Objekt S in cpy)
                                sw.WriteLine(S.Name + ", " + S.Vorname + " & " + S.GetKlasse() + @"\\");
                            sw.WriteLine(@"\end{tabular}");
                            // sw.WriteLine(@"\end{center}");
                        }
                        {
                            List<Objekt> cpy = new List<Objekt>(P.GetList());
                            cpy.Sort();

                            sw.WriteLine(@"\subsubsection*{Participants}");
                            //   sw.WriteLine(@"\begin{center}");
                            sw.WriteLine(@"\begin{tabular}{l|c}");
                            sw.WriteLine(@"\textbf{Name} & \textbf{Klasse} \\ \hline \hline");
                            foreach (Objekt S in cpy)
                            {
                                sw.WriteLine(S.Name + ", " + S.Vorname + " & " + S.GetKlasse() + @"\\");
                                schler.Add(new Tuple<Objekt, string>(new Objekt(S), P.Projektname));
                            }
                            sw.WriteLine(@"\end{tabular}");
                            // sw.WriteLine(@"\end{center}");
                        }
                        sw.WriteLine(@"\newpage");

                        if (Anwesenheit)
                        {
                            List<Objekt> cpy = new List<Objekt>(P.GetList());
                            cpy.Sort();

                            sw.WriteLine(@"\subsubsection*{Anwesenheit}");
                            //   sw.WriteLine(@"\begin{center}");
                            sw.WriteLine(@"\begin{tabular}{l||c|c|c|c|c|}");
                            sw.WriteLine(@"\textbf{Name} & Tag 1 & Tag 2 & Tag 3 & Tag 4 & Tag 5 \\ \hline \hline");
                            foreach (Objekt S in cpy)
                                sw.WriteLine(S.Name + ", " + S.Vorname + @"& \mbox{} & \mbox{} & \mbox{} & \mbox{} & \mbox{}\\ \hline");
                            sw.WriteLine(@"\end{tabular}");
                            // sw.WriteLine(@"\end{center}");
                            sw.WriteLine(@"\newpage");
                        }
                    }

                    schler.Sort(new TupleCmp<Objekt, string>());
                    foreach (var s in Klasse.ID_rev)
                    {
                        int cnt = 0;
                        for (int i = 0; i < schler.Count; i++)
                            if (Klasse.ID_rev[schler[i].Item1.GetKlasse()] == s)
                                ++cnt;
                        if (cnt == 0)
                            continue;
                        sw.WriteLine(@"\subsection*{Klasse " + s + "}");
                        sw.WriteLine(@"\begin{longtable}{l|l}");
                        sw.WriteLine(@"\textbf{Name} (cont.) & \textbf{Projekt} (cont.) \\ \hline \hline");
                        sw.WriteLine(@"\endhead");

                        sw.WriteLine(@"\textbf{Name} & \textbf{Projekt} \\ \hline \hline");
                        sw.WriteLine(@"\endfirsthead");
                        sw.WriteLine(@"\multicolumn{2}{r}{To be continued on the following page.}");
                        sw.WriteLine(@"\endfoot");
                        sw.WriteLine(@"\multicolumn{2}{l}{\mbox{}}");
                        sw.WriteLine(@"\endlastfoot");
                        for (int i = 0; i < schler.Count; i++)
                            if (Klasse.ID_rev[schler[i].Item1.GetKlasse()] == s)
                                sw.WriteLine(schler[i].Item1.Name + ", " + schler[i].Item1.Vorname + " & " + schler[i].Item2 + @"\\");
                        sw.WriteLine(@"\end{longtable}");
                        sw.WriteLine(@"\newpage");
                    }
                    sw.WriteLine(@"\end{document}");
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Obsolete("Tex-Output should be preferred for readability")]
        public bool writeFile(const_type<string> Path)
        {
            try
            {
                using (StreamWriter sr = new StreamWriter(Path))
                {
                    int cnt = 0;
                    listView3.Items.Clear();
                    for (int i = 0; i < Solution.Count; i++)
                    {
                        sr.WriteLine(Solution[i].Projektname + ": ");
                        foreach (var item in Solution[i].GetLeiterList())
                        {
                            var Item = item.AsItem(GetColor(item),button7.BackColor, cnt, i);
                            Item.Group = listView3.Groups[i];

                            Item.BackColor = button7.BackColor;

                            listView3.Items.Add(Item);
                        }
                        int cnt2 = 0;
                        foreach (var item in Solution[i].GetList())
                        {
                            var Item = item.AsItem(GetColor(item), button7.BackColor, cnt, i, cnt2++);
                            Item.Group = listView3.Groups[i];
                            if (!Solution[i].AllowedKlassen[item.GetKlasse()])
                                Item.ForeColor = Color.Red;
                            for (int W = 0; W < item.Wünsche.Count; ++W)
                                if (item.Wünsche[W] == i)
                                {
                                    Item.ForeColor = ColorButtons[W].BackColor;
                                    break;
                                }
                            if (Solution[i].TeilnehmerCount < Solution[i].MinAnz)
                                Item.BackColor = button2.BackColor;
                            if (Solution[i].TeilnehmerCount > Solution[i].MaxAnz)
                                Item.BackColor = button3.BackColor;

                            listView3.Items.Add(Item);
                            sr.WriteLine(item.Name + ", " + item.Vorname);
                        }
                        sr.WriteLine();
                    }
                }
                return true;
            }
            catch (Exception eg)
            {
                MessageBox.Show(eg.Message + " - Solution not saved.", "ERROR");
                while (listView3.Items.Count > 0)
                    listView3.Items.RemoveAt(0);
                return false;
            }
        }

        public bool writeTEXProjList(const_type<string> Path)
        {
            try
            {
                using (var sw = new StreamWriter(Path))
                {
                    sw.WriteLine(@"\documentclass[a4paper,12pt]{article}");
                    sw.WriteLine(@"\usepackage[ngerman]{babel}");
                    sw.WriteLine(@"\usepackage[utf8]{inputenc}");
                    sw.WriteLine(@"\usepackage{fancyhdr}");
                    sw.WriteLine(@"\usepackage{longtable}");
                    sw.WriteLine(@"\setlength{\textwidth}{17.5cm}");
                    sw.WriteLine(@"\setlength{\textheight}{26cm}");
                    sw.WriteLine(@"\setlength{\topmargin}{-1.7cm}");
                    sw.WriteLine(@"\setlength{\evensidemargin}{-7mm}");
                    sw.WriteLine(@"\setlength{\oddsidemargin}{-7mm}");
                    sw.WriteLine(@"\parskip 6pt plus 1pt minus 1pt");
                    sw.WriteLine(@"\parindent0pt");
                    sw.WriteLine(@"\pagestyle{fancy}");

                    sw.WriteLine(@"\lhead{ObjektAufProjekt}");
                    sw.WriteLine(@"\chead{}");
                    sw.WriteLine(@"\rhead{Projekte}");
                    sw.WriteLine(@"\lfoot{}");
                    sw.WriteLine(@"\cfoot{}");
                    sw.WriteLine(@"\rfoot{}");

                    sw.WriteLine(@"\begin{document}");
                    sw.WriteLine(@"\subsection*{Projekte}");

                    int colcnt = Klasse.ID_rev.Count + 3;

                    sw.WriteLine(@"\begin{longtable}{p{5cm}|p{3cm}|*{" + Klasse.ID_rev.Count + @"}{|p{0.25cm}|}|p{3cm}}");

                    string klnames = "";

                    for (int i = 0; i < Klasse.ID_rev.Count; i++)
                    {
                        if (i != 0)
                            klnames += " & ";
                        klnames += Klasse.ID_rev[i].ToString();
                    }

                    sw.WriteLine(@"\textbf{Name} (cont.) & \textbf{Projekt's leader} (cont.) & " + klnames + @" & \textbf{Description}  \\ \hline \hline");
                    sw.WriteLine(@"\endhead");
                    sw.WriteLine(@"\textbf{Name} & \textbf{Projekt's leader} & " + klnames + @" & \textbf{Description} \\ \hline \hline");
                    sw.WriteLine(@"\endfirsthead");
                    sw.WriteLine(@"\multicolumn{" + colcnt + @"}{r}{To be continued on the following page.}");
                    sw.WriteLine(@"\endfoot");
                    sw.WriteLine(@"\multicolumn{" + colcnt + @"}{l}{\mbox{}}");
                    sw.WriteLine(@"\endlastfoot");

                    var pro = new List<Projekt>();
                    pro.AddRange(Projekte);
                    pro.Sort();
                    foreach (Projekt P in pro)
                    {
                        if (P.MaxAnz == 0)
                            continue;

                        for (int i = 0; i < P.AllowedKlassen.Length; i++)
                            if (P.AllowedKlassen[i])
                                goto CONT;
                        continue;
                    CONT:
                        string s = P.Projektname + " & ";
                        foreach (Objekt S in P.GetLeiterList())
                        {
                            s += S.Name + ", " + S.Vorname + " (" + S.GetKlasse() + ")";
                            s += "; ";
                        }
                        s.Remove(s.Length - 2);
                        s += " & ";
                        for (int i = 0; i < P.AllowedKlassen.Length; i++)
                            if (P.AllowedKlassen[i])
                                s += @"\textsf{X} & ";
                            else
                                s += @"\mbox{} & ";
                        s += P.Description.Replace("$", "\\") + @"\\ \hline";
                        sw.WriteLine(s);
                    }
                    sw.WriteLine(@"\end{longtable}");

                    sw.WriteLine(@"\end{document}");
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool writeTEXWishList(const_type<string> Path, const_type<bool> drawWishes, const_type<bool> markSol)
        {
            if (markSol && Solution == null)
                return false;
            if (!markSol)
                return writeTEXWishList(Path, drawWishes);
            try
            {
                using (var sw = new StreamWriter(Path))
                {
                    sw.WriteLine(@"\documentclass[a4paper,10pt,landscape]{article}");
                    sw.WriteLine(@"\usepackage[ngerman]{babel}");
                    sw.WriteLine(@"\usepackage[utf8]{inputenc}");
                    sw.WriteLine(@"\usepackage{fancyhdr}");
                    sw.WriteLine(@"\usepackage{longtable}");
                    sw.WriteLine(@"\usepackage{rotating}");
                    sw.WriteLine(@"\usepackage{geometry}");
                    sw.WriteLine(@"\usepackage[table]{xcolor}");

                    sw.WriteLine(@"\setlength{\textwidth}{26cm}");
                    sw.WriteLine(@"\setlength{\textheight}{19cm}");
                    sw.WriteLine(@"\setlength{\topmargin}{-2cm}");
                    sw.WriteLine(@"\setlength{\evensidemargin}{-7mm}");
                    sw.WriteLine(@"\setlength{\oddsidemargin}{-7mm}");
                    sw.WriteLine(@"\parskip 6pt plus 1pt minus 1pt");
                    sw.WriteLine(@"\parindent0pt");
                    sw.WriteLine(@"\pagestyle{fancy}");

                    sw.WriteLine(@"\lhead{ObjektAufProjekt}");
                    sw.WriteLine(@"\chead{}");
                    sw.WriteLine(@"\rhead{Wishlist}");
                    sw.WriteLine(@"\lfoot{}");
                    sw.WriteLine(@"\cfoot{}");
                    sw.WriteLine(@"\rfoot{}");

                    sw.WriteLine(@"\begin{document}");

                    List<Tuple<Objekt, string>> schler = new List<Tuple<Objekt, string>>();
                    foreach (Projekt P in Solution)
                    {
                        foreach (Objekt S in P.GetLeiterList())
                            schler.Add(new Tuple<Objekt, string>(new Objekt(S), P.Projektname + "@"));
                        foreach (Objekt S in P.GetList())
                            schler.Add(new Tuple<Objekt, string>(new Objekt(S), P.Projektname));
                    }
                    schler.Sort(new TupleCmp<Objekt, string>());

                    List<Tuple<Projekt, int>> projekte = new List<Tuple<Projekt, int>>();
                    for (int i = 0; i < Projekte.Count; ++i)
                        projekte.Add(new Tuple<Projekt, int>(new Projekt(Projekte[i]), i));
                    projekte.Sort(new TupleCmp<Projekt, int>());

                    foreach (var s in Klasse.ID_rev)
                    {
                        int cnt = 0;
                        foreach (var P in projekte)
                            if (P.Item1.AllowedKlassen[Klasse.ID[s]])
                                cnt++;
                        if (cnt == 0)
                            continue;
                        sw.WriteLine(@"\begin{longtable}{l|*{" + cnt + @"}{|p{0.2cm}}|}");

                        Dictionary<string, int> Names = new Dictionary<string, int>();
                        int[] inds = new int[projekte.Count];
                        for (int i = 0; i < inds.Length; ++i)
                            inds[i] = -1;

                        sw.WriteLine(@"\chead{" + s + @"}");
                        string S = @"\textbf{Name} ";
                        cnt = 0;
                        foreach (var P in projekte)
                            if (P.Item1.AllowedKlassen[Klasse.ID[s]])
                            {
                                S += @" & \begin{sideways}" + P.Item1.Projektname + @" \end{sideways} ";
                                inds[P.Item2] = cnt++;
                                Names[P.Item1.Projektname] = inds[P.Item2];
                            }
                        S += @"\\ \hline \hline";
                        sw.WriteLine(S);
                        sw.WriteLine(@"\endhead");
                        sw.WriteLine(S);
                        sw.WriteLine(@"\endfirsthead");
                        sw.WriteLine(@"\multicolumn{" + cnt + @"}{r}{To be continued on the following page.}");
                        sw.WriteLine(@"\endfoot");
                        sw.WriteLine(@"\multicolumn{" + cnt + @"}{l}{\mbox{}}");
                        sw.WriteLine(@"\endlastfoot");
                        foreach (var Sch in schler)
                        {
                            if (Klasse.ID_rev[Sch.Item1.GetKlasse()] != s)
                                continue;
                            int[] indices = new int[cnt];
                            if (Sch.Item2.EndsWith("@"))
                            {
                                if (inds[Sch.Item1.Wünsche[0]] != -1)
                                    indices[inds[Sch.Item1.Wünsche[0]]] = -1;
                            }
                            else
                                if (inds[Sch.Item1.Wünsche[0]] != -1)
                                    indices[inds[Sch.Item1.Wünsche[0]]] = 1;
                            for (int i = 1; i < Sch.Item1.Wünsche.Count; ++i)
                                if (inds[Sch.Item1.Wünsche[i]] != -1)
                                    if (indices[inds[Sch.Item1.Wünsche[i]]] == 0)
                                        indices[inds[Sch.Item1.Wünsche[i]]] = (i + 1);
                            S = Sch.Item1.Name + ", " + Sch.Item1.Vorname;
                            for (int i = 0; i < indices.Length; i++)
                            {
                                if (indices[i] == -1)
                                {
                                    S += @" & \cellcolor{blue!25}\textsf{PL}";
                                    continue;
                                }
                                S += " & ";
                                if (!Sch.Item2.EndsWith("@") && i == Names[Sch.Item2])
                                    S += @"\cellcolor{blue!25}\mbox{}";
                                if (drawWishes && indices[i] != 0)
                                    S += indices[i];
                            }
                            S += @"\\ \hline";
                            sw.WriteLine(S);
                        }
                        sw.WriteLine(@"\end{longtable}");
                        sw.WriteLine(@"\newpage");
                    }
                    sw.WriteLine(@"\end{document}");
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private bool writeTEXWishList(const_type<string> Path, const_type<bool> drawWishes)
        {
            try
            {
                using (var sw = new StreamWriter(Path))
                {
                    sw.WriteLine(@"\documentclass[a4paper,10pt,landscape]{article}");
                    sw.WriteLine(@"\usepackage[ngerman]{babel}");
                    sw.WriteLine(@"\usepackage[utf8]{inputenc}");
                    sw.WriteLine(@"\usepackage{fancyhdr}");
                    sw.WriteLine(@"\usepackage{longtable}");
                    sw.WriteLine(@"\usepackage{rotating}");
                    sw.WriteLine(@"\usepackage{geometry}");
                    sw.WriteLine(@"\usepackage[table]{xcolor}");
                    sw.WriteLine(@"\usepackage{array}");

                    sw.WriteLine(@"\setlength{\textwidth}{26cm}");
                    sw.WriteLine(@"\setlength{\textheight}{19cm}");
                    sw.WriteLine(@"\setlength{\topmargin}{-2cm}");
                    sw.WriteLine(@"\setlength{\evensidemargin}{-7mm}");
                    sw.WriteLine(@"\setlength{\oddsidemargin}{-7mm}");
                    sw.WriteLine(@"\parskip 6pt plus 1pt minus 1pt");
                    sw.WriteLine(@"\parindent0pt");
                    sw.WriteLine(@"\pagestyle{fancy}");

                    sw.WriteLine(@"\lhead{Projektwoche}");
                    sw.WriteLine(@"\chead{}");
                    sw.WriteLine(@"\rhead{Wunschtabelle}");
                    sw.WriteLine(@"\lfoot{}");
                    sw.WriteLine(@"\cfoot{}");
                    sw.WriteLine(@"\rfoot{}");

                    sw.WriteLine(@"\begin{document}");

                    List<Tuple<Objekt, bool>> schler = new List<Tuple<Objekt, bool>>();
                    foreach (Objekt S in schüler)
                        if (!S.isLeiter)
                            schler.Add(new Tuple<Objekt, bool>(new Objekt(S), false));
                    foreach (Projekt P in Projekte)
                        foreach (Objekt S in P.GetLeiterList())
                            schler.Add(new Tuple<Objekt, bool>(new Objekt(S), true));
                    schler.Sort(new TupleCmp<Objekt, bool>());

                    List<Tuple<Projekt, int>> projekte = new List<Tuple<Projekt, int>>();
                    for (int i = 0; i < Projekte.Count; ++i)
                        projekte.Add(new Tuple<Projekt, int>(new Projekt(Projekte[i]), i));
                    projekte.Sort(new TupleCmp<Projekt, int>());

                    HashSet<string> allKlassen = new HashSet<string>();
                    List<aKlasse> a = new List<aKlasse>();

                    foreach (var s in schler)
                        if (!allKlassen.Contains(s.Item1.Klasse.Data.GetName()))
                        {
                            allKlassen.Add(s.Item1.Klasse.Data.GetName());
                            a.Add(s.Item1.Klasse);
                        }

                    foreach (var s in a)
                    {
                        int cnt = 0;
                        foreach (var P in projekte)
                            if (P.Item1.AllowedKlassen[Klasse.ID[(Klasse)s.Base()]])
                                cnt++;
                        if (cnt == 0)
                            continue;
                        sw.WriteLine(@"\begin{longtable}{l|*{" + cnt + @"}{|b{0.2cm}}|}");

                        Dictionary<string, int> Names = new Dictionary<string, int>();
                        int[] inds = new int[projekte.Count];
                        for (int i = 0; i < inds.Length; ++i)
                            inds[i] = -1;

                        sw.WriteLine(@"\chead{" + s.GetName() + @"}");
                        string S = @"\textbf{Name} ";
                        cnt = 0;

                        bool[] b = new bool[projekte.Count];
                        foreach (var P in projekte)
                            if (P.Item1.AllowedKlassen[Klasse.ID[(Klasse)s.Base()]])
                            {
                                S += @" & \begin{sideways}" + P.Item1.Projektname + @" \end{sideways} ";
                                inds[P.Item2] = cnt++;
                                Names[P.Item1.Projektname] = inds[P.Item2];
                                b[cnt - 1] = P.Item1.editable;
                            }
                        S += @"\\ \hline \hline";
                        sw.WriteLine(S);
                        sw.WriteLine(@"\endhead");
                        sw.WriteLine(S);
                        sw.WriteLine(@"\endfirsthead");
                        sw.WriteLine(@"\multicolumn{" + cnt + @"}{r}{Fortsetung auf der nächsten Seite}");
                        sw.WriteLine(@"\endfoot");
                        sw.WriteLine(@"\multicolumn{" + cnt + @"}{l}{\mbox{}}");
                        sw.WriteLine(@"\endlastfoot");
                        foreach (var Sch in schler)
                        {
                            if (Sch.Item1.Klasse.Data.GetName() != s.GetName())
                            {
                                System.Diagnostics.Debug.WriteLine(Sch.Item1.Klasse.Data.GetName() + " != " + s.GetName());
                                continue;
                            }

                            bool allM2 = false;

                            int[] indices = new int[cnt];
                            if (Sch.Item2)
                            {
                                if (Sch.Item1.Wünsche.Count > 0 && inds[Sch.Item1.Wünsche[0]] != -1)
                                    indices[inds[Sch.Item1.Wünsche[0]]] = -1;
                            }
                            else if (Sch.Item1.Wünsche.Count > 0 && inds[Sch.Item1.Wünsche[0]] != -1 && !Projekte[Sch.Item1.Wünsche[0]].editable)
                            {
                                indices[inds[Sch.Item1.Wünsche[0]]] = -2;
                                allM2 = true;
                            }
                            else
                                if (Sch.Item1.Wünsche.Count > 0 && inds[Sch.Item1.Wünsche[0]] != -1)
                                    indices[inds[Sch.Item1.Wünsche[0]]] = 1;
                            for (int i = 1; i < Sch.Item1.Wünsche.Count; ++i)
                                if (inds[Sch.Item1.Wünsche[i]] > -1)
                                    if (indices[inds[Sch.Item1.Wünsche[i]]] == 0)
                                        indices[inds[Sch.Item1.Wünsche[i]]] = (i + 1);
                                                        S = Sch.Item1.Name + ", " + Sch.Item1.Vorname;
                            for (int i = 0; i < indices.Length; i++)
                                if (indices[i] == -1)
                                    S += @" & {\small{\textbf{\textsf{PL}}}}";
                                else if (indices[i] == -2)
                                    S += @" & {\textsf{X}}";
                                else if (drawWishes && indices[i] != 0)
                                    S += " & " + indices[i];
                                else if (allM2 || !b[i])
                                    S += @" & \cellcolor{black!75}\mbox{}";
                                else
                                    S += @" & ";
                            S += @"\\ \hline";
                            sw.WriteLine(S);
                        }
                        sw.WriteLine(@"\end{longtable}");
                        sw.WriteLine(@"\newpage");
                    }
                    sw.WriteLine(@"\end{document}");
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #region EventHandler
        private void SetProData(object sender, EventArgs e)
        {
            while (listView1.Items.Count > 0)
                listView1.Items.RemoveAt(0);
            while (listView3.Groups.Count > 0)
                listView3.Groups.RemoveAt(0);
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "File containing Projekte's data|*.prd;*.txt";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    textBox1.Text = dlg.FileName;
                    button11.Enabled = true;
                }
                catch (Exception ed)
                {
                    MessageBox.Show(ed.Message + " - Could not load file.", "ERROR");
                    return;
                }
            }
            else return;

            button11.Enabled = openProject(dlg.FileName);

            for (int i = 0; i < TabPages.Count; i++)
            {
                ((ComboBox)(TabPages[i].Controls[0])).Items.Clear();
                ((ComboBox)(TabPages[i].Controls[0])).Items.AddRange(Projekte.ToArray());
            }
            toolStripComboBox1.Items.Clear();
            toolStripComboBox1.Items.AddRange(Projekte.ToArray());
        }
        private void SetSchData(object sender, EventArgs e)
        {
            if (!button11.Enabled)
            {
                MessageBox.Show("Specify a file with Projekte first!", "ERROR");
                return;
            }
            listView2.Items.Clear();
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "File containing Objekte and their wishes|*.scd;*.txt";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    textBox2.Text = dlg.FileName;
                    button12.Enabled = true;
                }
                catch (Exception ed)
                {
                    MessageBox.Show(ed.Message + " - Could not load file.", "ERROR");
                    return;
                }
            }
            else return;
            button12.Enabled = openSchüler(dlg.FileName);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Raw text (*.txt)|*.txt|TeX file (*.tex)|*.tex|TeX file (without Anwesenheitstabellen) (*.tex)|*.tex";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                if (sfd.FilterIndex == 1)
                    writeFile(sfd.FileName);
                else if (sfd.FilterIndex == 2)
                    writeTEXFile(sfd.FileName);
                else if (sfd.FilterIndex == 3)
                    writeTEXFile(sfd.FileName, false);
                else
                {
                    MessageBox.Show("An error occured. Could not save file.", "ERROR");
                    return;
                }
        }

        private void projList_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "TeX file (*.tex)|*.tex";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                writeTEXProjList(sfd.FileName);
        }

        private void wishList_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "TeX file (*.tex)|*.tex|TeX file (including wishes) (*.tex)|*.tex";
            if (Solution != null)
                sfd.Filter += "|TeX file (including wishes and solution) (*.tex)|*.tex";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                if (sfd.FilterIndex == 1)
                    writeTEXWishList(sfd.FileName, false);
                else if (sfd.FilterIndex == 2)
                    writeTEXWishList(sfd.FileName, true);
                else if (sfd.FilterIndex == 3)
                    writeTEXWishList(sfd.FileName, true, true);
                else
                {
                    MessageBox.Show("An error occured. Could not save file.", "ERROR");
                    return;
                }
        }
        #endregion
        #endregion

        private void berechnungStarten(object sender, EventArgs e)
        {
            if (!button12.Enabled || !button11.Enabled)
            {
                MessageBox.Show("Specify both input files first!", "ERROR");
                return;
            }
            aktBestSolValue = -1;
            setSave(false);

            toolStripButton2.Visible = true;
            toolStripButton3.Visible = true;
            toolStripButton1.Visible = false;
            beenden = false;
            int a, b, c;
            do calculate();
            while (bewerte(out a, out b, out c) < schüler.Count * SchHWunsch[0].Value && !beenden);
            toolStripButton2.Visible = false;
            toolStripButton3.Visible = false;
            toolStripButton1.Visible = true;
        }

        #region Interaktion mit Projekten/Schülern
        private void AddProjekt(object sender, EventArgs e)
        {
            if (!button11.Enabled)
            {
                MessageBox.Show("Specify a file with Projekte first!", "ERROR");
                return;
            }
            if (textBox4.Text.Length == 0)
            {
                MessageBox.Show("Nameless Projekte are somewhat strange!", "ERROR");
                return;
            }
            StreamWriter sw = null;
            try
            { sw = new StreamWriter(textBox1.Text, true); }
            catch (Exception w)
            {
                MessageBox.Show(w.Message + " - Could not add Projekt.", "ERROR");
                sw.Close();
                return;
            }
            bool b = false;
            string KlStufe = "";
            BitString KlStufen = new BitString(new bool[] { });
            for (int i = 0; i < Projekte[SelectedProj].AllowedKlassen.Length; i++)
            {
                KlStufen.AddBit(checkedListBox1.CheckedIndices.Contains(i));
                if (checkedListBox1.CheckedIndices.Contains(i))
                {
                    if (b)
                        KlStufe += ",";
                    b = true;
                    KlStufe += (string)Klasse.ID_rev[i].GetName();
                }
            }
            if (numericUpDown4.Value > numericUpDown5.Value)
            {
                MessageBox.Show("Wrong input!", "ERROR");
                sw.Close();
                return;
            }
            string Descr = "";
            foreach (var Line in textBox5.Lines)
                Descr += Line + " ";
            sw.WriteLine("\n" + textBox4.Text + "/" + (int)numericUpDown4.Value
                + "/" + (int)numericUpDown5.Value + "/" + KlStufe + "/" + Offen.Checked + "/" + Descr);
            sw.Close();
            listView3.Groups.Add(new ListViewGroup(textBox4.Text));
            Projekt P = new Projekt(textBox4.Text, (int)numericUpDown4.Value, (int)numericUpDown5.Value, KlStufen, Offen.Checked, Descr, checkBox4.Checked);
            foreach (var item in TabPages)
                ((ComboBox)item.Controls[0]).Items.Add(P);
            listView1.Items.Add(P.AsItem(Projekte.Count));
            toolStripComboBox1.Items.Add(P);
            Projekte.Add(P);

            fill_LV4(Projekte);
            this.Refresh();
        }
        private void AddSchüler(object sender, EventArgs e)
        {
            if (textBox7.Text.Length == 0 || textBox8.Text.Length == 0)
            {
                MessageBox.Show("Fill in all the fields!", "ERROR");
                return;
            }
            if (!button11.Enabled)
            {
                MessageBox.Show("Specify a file with Projekte first!", "ERROR");
                return;
            }
            List<int> inds = new List<int>();
            try
            {
                for (int i = 0; i < TabPages.Count; ++i)
                    if (((ComboBox)TabPages[i].Controls[0]).SelectedIndex != -1)
                        inds.Add(((ComboBox)TabPages[i].Controls[0]).SelectedIndex);
                //else if (i == 0)
                //{
                //    MessageBox.Show("Fill in all the fields!\n(Specify wishes!)", "ERROR");
                //    return;
                //}
                //else
                //    inds.Add(inds.Last());
            }
            catch (Exception eg)
            {
                MessageBox.Show(eg.Message + " - Could not add Objekt.", "ERROR");
                return;
            }

            StreamWriter sw = null;
            try
            { sw = new StreamWriter(textBox2.Text, true); }
            catch (Exception es)
            {
                MessageBox.Show(es.Message + " - Could not add Objekt.", "ERROR");
                sw.Close();
            }

            aKlasse acKlasse = new Klasse(textBox6.Text.Split('-')[0].Trim(' '));
            var txt = textBox6.Text.Split('-');
            for (int i = 1; i < txt.Length; ++i)
                acKlasse = new KlasseDecorator(acKlasse, txt[i].Trim(' '));

            Objekt S = new Objekt(textBox7.Text, textBox8.Text, acKlasse, inds, checkBox1.Checked);
            string out_ = "";

            if (checkBox1.Checked)
                Projekte[inds[0]].Add(S);
            schüler.Add(S);

            listView2.Items.Add(S.AsItem(GetColor(S), button7.BackColor, schüler.Count - 1));

            if (checkBox1.Checked)
                out_ += "@";

            out_ += textBox7.Text + "/" + textBox8.Text;
            foreach (var ind in inds)
                out_ += "/" + Projekte[ind].Projektname;
            out_ += "/" + acKlasse.GetName();
            sw.WriteLine(out_);
            sw.Close();

            fill_LV4(Projekte);
        }

        #region Change Schüler
        int SelectedSchüler = -1;
        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                SelectedSchüler = Convert.ToInt32(listView2.Items[listView2.SelectedIndices[0]].SubItems[3].Text);
                textBox7.Text = schüler[SelectedSchüler].Name;
                textBox8.Text = schüler[SelectedSchüler].Vorname;
                textBox6.Text = schüler[SelectedSchüler].Klasse.Data.GetName();
                checkBox1.Checked = schüler[SelectedSchüler].isLeiter;
                button13.Enabled = button8.Enabled = true;
                for (int i = 0; i < TabPages.Count; i++)
                {
                    try
                    {
                        ((ComboBox)(TabPages[i].Controls[0])).SelectedIndex =
                            schüler[SelectedSchüler].Wünsche[i];

                        ((Label)(TabPages[i].Controls[1])).Text =
                            Projekte[schüler[SelectedSchüler].Wünsche[i]].Description;
                    }
                    catch (Exception)
                    {
                        ((ComboBox)(TabPages[i].Controls[0])).SelectedIndex = -1;
                    }
                }
            }
            catch (Exception)
            {
                button13.Enabled = button8.Enabled = false;
            }
        }
        private void ChangeSchüler(object sender, EventArgs e)
        {
            List<int> inds = new List<int>();
            try
            {
                for (int i = 0; i < TabPages.Count; ++i)
                    if (((ComboBox)TabPages[i].Controls[0]).SelectedIndex != -1)
                        inds.Add(((ComboBox)TabPages[i].Controls[0]).SelectedIndex);
                //else
                //{
                //    MessageBox.Show("Fill in all the fields!", "ERROR");
                //    return;
                //}
                fill_LV4(Projekte);
            }
            catch (Exception eg)
            {
                MessageBox.Show(eg.Message + " - Could not modify Objekt.", "ERROR");
                return;
            }

            aKlasse acKlasse = new Klasse(textBox6.Text.Split('-')[0].Trim(' '));
            var txt = textBox6.Text.Split('-');
            for (int i = 1; i < txt.Length; ++i)
                acKlasse = new KlasseDecorator(acKlasse, txt[i].Trim(' '));

            schüler[SelectedSchüler] = new Objekt(textBox7.Text, textBox8.Text, acKlasse, inds);
            listView2.Items[listView2.SelectedIndices[0]] = schüler[SelectedSchüler].AsItem(GetColor(schüler[SelectedSchüler]), button7.BackColor, SelectedSchüler);

            int selLine = SelectedSchüler;
            var Lines = File.ReadAllLines(textBox2.Text);
            for (int i = 0; i <= selLine; i++)
                if (Lines[i].Length == 0 || Lines[i].StartsWith("*"))
                    selLine++;
            string out_ = "";
            if (checkBox1.Checked)
                out_ += "@";
            out_ += textBox7.Text + "/" + textBox8.Text;
            foreach (var ind in inds)
                out_ += "/" + Projekte[ind].Projektname;
            out_ += "/" + acKlasse.GetName();
            Lines[selLine] = out_;
            File.WriteAllLines(textBox2.Text, Lines);
            if (checkBox1.Checked)
            {
                listView2.Items.Clear();
                openSchüler(textBox2.Text);
            }
        }
        private void remSchueler(object sender, EventArgs e)
        {
            int selLine = SelectedSchüler;
            var Lines = File.ReadAllLines(textBox2.Text);
            for (int i = 0; i <= selLine; i++)
                if (Lines[i].Length == 0 || Lines[i].StartsWith("*") || Lines[i].StartsWith("@"))
                    selLine++;

            Lines[selLine] = null;
            File.WriteAllLines(textBox2.Text, Lines);
            listView2.Items.Clear();
            openSchüler(textBox2.Text);
            button13.Enabled = button8.Enabled = false;

            fill_LV4(Projekte);
        }
        #endregion

        #region ChangeProjekt
        int SelectedProj = -1;
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                SelectedProj = Convert.ToInt32(listView1.Items[listView1.SelectedIndices[0]].SubItems[4].Text);
                textBox4.Text = Projekte[SelectedProj].Projektname;
                numericUpDown4.Value = Projekte[SelectedProj].MinAnz;
                numericUpDown5.Value = Projekte[SelectedProj].MaxAnz;

                checkedListBox1.Items.Clear();
                for (int i = 0; i < Klasse.ID_rev.Count; ++i)
                {
                    checkedListBox1.Items.Add((string)Klasse.ID_rev[i].GetName());
                }
                for (int i = 0; i < Projekte[SelectedProj].AllowedKlassen.Length; i++)
                    checkedListBox1.SetItemChecked(i, Projekte[SelectedProj].AllowedKlassen[i]);

                Offen.Checked = Projekte[SelectedProj].editable;
                checkBox4.Checked = Projekte[SelectedProj].erhaltenswert;
                textBox5.Lines = Projekte[SelectedProj].Description.Split('$');
                button4.Enabled = true;
            }
            catch (Exception)
            {
                button4.Enabled = false;
                textBox4.Text = "";
                numericUpDown4.Value = numericUpDown4.Minimum;
                numericUpDown5.Value = numericUpDown5.Minimum;
                for (int i = 0; i < Projekte[SelectedProj].AllowedKlassen.Length; i++)
                    checkedListBox1.SetItemChecked(i, false);
                Offen.Checked = true;
                checkBox4.Checked = false;
                textBox5.Lines = new string[] { "A description of this Projekt goes here." };
            }
        }

        private void ChangeProjekt(object sender, EventArgs e)
        {
            if (textBox4.Text.Length == 0)
            {
                MessageBox.Show("Nameless Projekte are somewhat strange!", "ERROR");
                return;
            }
            int selLine = SelectedProj;
            var Lines = File.ReadAllLines(textBox1.Text);

            bool b = false;
            string KlStufe = "";
            BitString KlStufen = new BitString(new bool[] { });
            for (int i = 0; i < Projekte[SelectedProj].AllowedKlassen.Length; i++)
            {
                KlStufen.AddBit(checkedListBox1.CheckedIndices.Contains(i));
                if (checkedListBox1.CheckedIndices.Contains(i))
                {
                    if (b)
                        KlStufe += ",";
                    b = true;
                    KlStufe += (string)Klasse.ID_rev[i].GetName();
                }
            }
            if (numericUpDown4.Value > numericUpDown5.Value)
            {
                MessageBox.Show("Wrong input!", "ERROR");
                return;
            }

            for (int i = 0; i <= selLine; i++)
                if (Lines[i].Length == 0 || Lines[i].StartsWith("*"))
                    selLine++;

            string Descr = "";
            foreach (var Line in textBox5.Lines)
                Descr += Line + "$";

            Lines[selLine] = textBox4.Text + "/" + (int)numericUpDown4.Value
                + "/" + (int)numericUpDown5.Value + "/" + KlStufe + "/" + Offen.Checked + "/" + Descr + "/" + checkBox4.Checked;

            File.WriteAllLines(textBox1.Text, Lines);

            Projekte[SelectedProj] =
                new Projekt(textBox4.Text, (int)numericUpDown4.Value, (int)numericUpDown5.Value,
                    KlStufen, Offen.Checked, Descr, checkBox4.Checked);
            listView1.Items[listView1.SelectedIndices[0]] = Projekte[SelectedProj].AsItem(SelectedProj);

            toolStripComboBox1.Items.Clear();
            toolStripComboBox1.Items.AddRange(Projekte.ToArray());

            fill_LV4(Projekte);
        }
        #endregion
        #endregion

        #region Lösungs-Modifikation
        int SelectedSolSch = -1, SelectedSolPro = -1;
        private void listView3_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                SelectedSolPro = Convert.ToInt32(listView3.Items[listView3.SelectedIndices[0]].SubItems[4].Text);
                SelectedSolSch = Convert.ToInt32(listView3.Items[listView3.SelectedIndices[0]].SubItems[5].Text);

                contextMenuStrip1.Enabled = true;
                toolStripComboBox1.SelectedIndex = SelectedSolPro;
                toolStripMenuItem1.Enabled = true;
            }
            catch (Exception)
            {
                contextMenuStrip1.Enabled = false;
                toolStripMenuItem1.Enabled = false;
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            Projekte = new List<Projekt>();
            for (int i = 0; i < Solution.Count; i++)
                Projekte.Add(new Projekt(Solution[i]));

            toolStripButton4.Visible = false;
            button7_Click(sender, e);
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            toolStripButton4.Visible = true;

            Solution[toolStripComboBox1.SelectedIndex].Add(new Objekt(Solution[SelectedSolPro][SelectedSolSch]));
            Solution[SelectedSolPro].Remove(Solution[SelectedSolPro][SelectedSolSch]);
            int cnt = 0;
            listView3.Items.Clear();
            int a, b, c;
            var B = bewerte_sol(out a, out b, out c)
                * (a > 0 ? (numericUpDown2.Value / 100) : 1)
                * (b > 0 ? (numericUpDown3.Value / 100) : 1) * (c > 0 ? 0 : 1);

            numericUpDown1.Value = ((B * 100) / (SchHWunsch[0].Value * schüler.Count));

            label1.Text = "Die aktuelle Lösung ist grob approximiert zu " + (int)((B * 100) / (schüler.Count * SchHWunsch[0].Value))
                + "% optimal. (Bewertung: " + B + ")";

            for (int i = 0; i < Solution.Count; i++)
            {
                int cnt2 = 0;
                foreach (var item in Solution[i].GetList().Union(Solution[i].GetLeiterList()))
                {
                    var Item = item.AsItem(GetColor(item), button7.BackColor, cnt, i, cnt2++);
                    Item.Group = listView3.Groups[i];
                    if (!Solution[i].AllowedKlassen[item.GetKlasse()])
                        Item.ForeColor = Color.Red;
                    for (int W = 0; W < item.Wünsche.Count; ++W)
                        if (item.Wünsche[W] == i)
                        {
                            Item.ForeColor = ColorButtons[W].BackColor;
                            break;
                        }
                    if (Solution[i].TeilnehmerCount < Solution[i].MinAnz && !item.isLeiter)
                        Item.BackColor = button2.BackColor;
                    if (Solution[i].TeilnehmerCount > Solution[i].MaxAnz && !item.isLeiter)
                        Item.BackColor = button3.BackColor;

                    listView3.Items.Add(Item);
                }
            }
        }
        #endregion

        #region Rest
        void setSettings(const_type<bool> val)
        {
            foreach (Control C in tabPage8.Controls)
                C.Enabled = val;
            textBox1.Enabled = textBox2.Enabled = val;
            panel2.Enabled = groupBox6.Enabled = val;
        }
        void setSave(const_type<bool> val, bool expL = false)
        {
            foreach (Control C in groupBox5.Controls)
                C.Enabled = val;
            if (!expL)
                button9.Enabled = false;
        }
        private void button7_Click(object sender, EventArgs e)
        {
            flag = !flag;
            if (!flag)
            {
                toolStripButton2.ToolTipText = "Berechnung pausieren";
                toolStripButton2.Text = "BERECHNUNG PAUSIEREN";
                setSettings(false);
                setSave(false);
            }
            if (flag)
            {
                toolStripButton2.ToolTipText = "Berechnung fortsetzen";
                toolStripButton2.Text = "BERECHNUNG FORTSETZEN";
                setSettings(true);
                if (Solution != null)
                    setSave(true);
            }
            while (true)
            {
                if (!flag) break;
                Application.DoEvents();
            }
        }
        private void button8_Click(object sender, EventArgs e)
        {
            this.beenden = true;
            this.flag = false;
            toolStripButton2.ToolTipText = "Pause calculation.";
            toolStripButton2.Text = "PAUSE CALCULATION";
            setSettings(true);

            setSave(true, Solution != null);
        }

        private void listView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            bool? num = false;
            if (((ListView)sender).Columns[e.Column].Text == "KLASSE")
                num = null;
            else if (((ListView)sender).Columns[e.Column].Text.Contains("#")
                || ((ListView)sender).Columns[e.Column].Text.Contains("AS")
                || ((ListView)sender).Columns[e.Column].Text.Contains("ALWAYS"))
                num = true;

            bool PatternFilter = false;
            if (((ListView)sender).Columns[e.Column].Text == "NAME")
                PatternFilter = true;

            ((ListView)sender).ListViewItemSorter = new ListViewComparer(e.Column, num,
                ((ListView)sender).Sorting == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending, PatternFilter);
            ((ListView)sender).Sorting = ((ListView)sender).Sorting == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
        }

        private void schließenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (DialogResult.OK == colorDialog1.ShowDialog())
                ((Button)sender).BackColor = colorDialog1.Color;
        }

        private void Form1_Closed(object sender, EventArgs e)
        {
            this.flag = !flag;
            this.beenden = true;
        }

        public int getE_Wert(int Proj_Ind, List<Projekt> Projekte)
        {
            double val = 0;
            for (int j = 0; j < (int)numericUpDown7.Value; j++)
                val += (CountWisch(j, Proj_Ind) / (j + 1));
            return (int)(val - Projekte[Proj_Ind].MinAnz);
        }
        private void fill_LV4(List<Projekt> Projekte)
        {
            listView4.Items.Clear();
            for (int i = 0; i < Projekte.Count; ++i)
            {
                List<string> dat = new List<string>();
                dat.Add(Projekte[i].Projektname);
                double val = 0;
                for (int j = 0; j < (int)numericUpDown7.Value; j++)
                {
                    const_type<int> a = CountWisch(j, i);
                    val += (a / (j + 1));
                    dat.Add("" + a);
                }
                dat.Add("" + (val - Projekte[i].MinAnz));
                var LVI = new ListViewItem(dat.ToArray());
                if (val < Projekte[i].MinAnz)
                    LVI.BackColor = button2.BackColor;
                if (val > Projekte[i].MaxAnz)
                    LVI.BackColor = button3.BackColor;
                listView4.Items.Add(LVI);
            }
        }
        #endregion

        public void initGUI(int anzW)
        {
            int MAX = (int)Math.Pow(10, (int)numericUpDown7.Value - 1);
            TabPages.Clear();
            ColorButtons.Clear();
            groupBox4.Controls.Clear();
            SchHWunsch.Clear();
            List<Control> cpy = new List<Control>();
            foreach (Control C in groupBox3.Controls)
                if (C.Tag == null || (int)C.Tag != 42)
                    cpy.Add(C);
            groupBox3.Controls.Clear();
            groupBox3.Controls.AddRange(cpy.ToArray());
            tabControl2.TabPages.Clear();
            listView4.Columns.Clear();
            listView4.Columns.Add("PROJEKT's NAME");
            listView4.Columns[0].Width = 120;

            for (int i = 0; i < (int)numericUpDown7.Value; i++)
            {
                TabPage T = new TabPage("wish #" + (i + 1));
                tabControl2.TabPages.Add(T);
                TabPages.Add(T);

                ComboBox CB = new ComboBox();
                CB.Dock = DockStyle.Top;
                CB.Items.AddRange(Projekte.ToArray());
                CB.DropDownStyle = ComboBoxStyle.DropDownList;
                CB.BackColor = System.Drawing.Color.GhostWhite;
                CB.SelectedIndexChanged += CB_SelectedIndexChanged;
                CB.Tag = i;
                TabPages[i].Controls.Add(CB);
                Label L = new Label(), M = new Label(), N = new Label();
                L.Dock = DockStyle.Bottom;
                TabPages[i].Controls.Add(L);

                M.Text = "Objekte's color that got matched to their wish #" + (i + 1) + ".";
                M.Location = new Point(label5.Location.X, label5.Location.Y + (i + 1) * 30);
                M.AutoSize = true;
                M.Tag = 42;
                Button B = new Button();
                groupBox3.Controls.Add(B);
                groupBox3.Controls.Add(M);
                B.FlatStyle = FlatStyle.Flat;
                B.Text = "Adjust";
                B.Size = button1.Size;
                B.Location = new Point(button1.Location.X, button1.Location.Y + (i + 1) * 30);
                B.BackColor = Color.FromArgb(255, 0, (4003 - (i * 40)) % 256, 0);
                B.Click += button1_Click_1;
                B.Tag = 42;
                ColorButtons.Add(B);

                N.Text = "Score of a fulfilled wish #" + (i + 1) + ".";
                N.Location = new Point(label2.Location.X, label2.Location.Y + (30 * i));
                N.AutoSize = true;
                groupBox4.Controls.Add(N);
                NumericUpDown NUD = new NumericUpDown();
                NUD.Maximum = int.MaxValue;
                NUD.Size = numericUpDown1.Size;
                NUD.Location = new Point(numericUpDown1.Location.X, numericUpDown1.Location.Y + (i * 30));
                NUD.TextAlign = HorizontalAlignment.Center;
                NUD.Value = MAX;
                NUD.BackColor = System.Drawing.Color.GhostWhite;
                MAX /= 10;

                SchHWunsch.Add(NUD);
                groupBox4.Controls.Add(NUD);

                listView4.Columns.Add("AS WISH #" + (i + 1), 60, HorizontalAlignment.Center);

            }
            listView4.Columns.Add("ALWAYS SURVIVING", 60, HorizontalAlignment.Center);
        }

        void CB_SelectedIndexChanged(object sender, EventArgs e)
        {
            ((Label)TabPages[(int)((ComboBox)sender).Tag].Controls[1]).Text
                = Projekte[((ComboBox)(TabPages[(int)((ComboBox)sender).Tag]).Controls[0]).SelectedIndex].Description;
        }

        private void numericUpDown7_ValueChanged(object sender, EventArgs e)
        {
            initGUI((int)numericUpDown7.Value);
        }

        public Form1()
        {
            InitializeComponent();
            this.Show();

            button1.BackColor = colorDialog1.Color;
            button2.BackColor = Color.PapayaWhip;
            button3.BackColor = Color.Plum;

            this.MinimumSize = this.Size;

            initGUI((int)numericUpDown7.Value);

        }
    }


    public static class SCH_ERW
    {
        public static ListViewItem AsItem(this Objekt S, const_type<Color> DefColor, const_type<Color> PRColor, const_type<int> index, int index2 = (-1), int index3 = -1)
        {
            List<string> data = new List<string>();
            data.Add(S.Name);
            data.Add(S.Vorname);
            data.Add(S.Klasse.Data.GetName());
            data.Add("" + index);
            data.Add("" + index2);
            data.Add("" + index3);

            ListViewItem _ret = new ListViewItem(data.ToArray());
            if (S.isLeiter)
                _ret.BackColor = PRColor;
            else
                _ret.BackColor = DefColor;
            
            return _ret;
        }

        public static ListViewItem AsItem(this Projekt P, const_type<int> index)
        {
            List<string> data = new List<string>();
            data.Add(P.Projektname);
            data.Add("" + P.MinAnz);
            data.Add("" + P.MaxAnz);

            string s = "";
            for (int i = 0; i < Klasse.ID_rev.Count; i++)
                if (P.AllowedKlassen[i])
                    s += Klasse.ID_rev[i] + " ";
            data.Add(s.TrimEnd(' '));
            data.Add("" + index);
            return new ListViewItem(data.ToArray());
        }
    }
}
