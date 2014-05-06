using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using DataStructures;

using System.Numerics;

namespace Prowo
{
  public partial class Form1 : Form
  {
    public static List<Objekt> schüler = new List<Objekt> ( );
    public static List<Projekt> Projekte = new List<Projekt> (0);
    public static List<Projekt> Solution;
    public static List<Projekt> Start;
    public List<NumericUpDown> SchHWunsch = new List<NumericUpDown> ( );

    public int CountWisch ( const_type<int> Wish, const_type<int> ProjektIndex )
    {
      int cnt = 0;
      foreach ( Objekt S in schüler )
        if ( S.Wünsche.Count <= Wish )
          continue;
        else if ( Wish == 0 && S.isLeiter )
          continue;
        else if ( Wish > 0 && S.Wünsche[ Wish - 1 ] == S.Wünsche[ Wish ] )
          continue;
        else if ( S.Wünsche[ Wish ] == ProjektIndex )
          ++cnt;
      return cnt;
    }

    public List<Objekt> calc ( )
    {
      List<Objekt> Rest = new List<Objekt> ( );//Students who cannot get one of their wishs

      foreach ( var s in schüler )
      {
        bool aBool = true;
        for ( int i = 0; i < TabPages.Count; ++i )
        {
          if ( i >= s.Wünsche.Count )
            continue;

          if ( Start[ s.Wünsche[ i ] ].TeilnehmerCount < Start[ s.Wünsche[ i ] ].MaxAnz || s.isLeiter )
          {
            Start[ s.Wünsche[ i ] ].Add (new Objekt (s));
            aBool = false;
            break;
          }
        }
        if ( aBool )
          Rest.Add (new Objekt (s));
      }
      return Rest;
    }

    public List<Objekt> calcBlind ( )
    {
      for ( int i = 0; i < Start.Count; i++ )
        Start[ i ].reset ( );
      List<Objekt> Rest = new List<Objekt> ( );
      foreach ( var s in schüler )
      {
        bool aBool = true;

        if ( !s.WishesSet )
          continue;

        if ( s.isLeiter || Start[ s.Wünsche[ 0 ] ].editable )
        {
          Start[ s.Wünsche[ 0 ] ].Add (new Objekt (s));
          aBool = false;
          break;
        }
        if ( aBool )
          Rest.Add (new Objekt (s));
      }
      return Rest;
    }

    public void init ( const_type<bool> blind, const_type<bool> remUseLess )
    {
      List<Objekt> Rest;
      Start = Projekte;
      schüler.Sort (( Objekt a, Objekt b ) => new Random ( ).Next (-1, 2));
      if ( blind )
        Rest = calcBlind ( );
      else
        Rest = calc ( );
      if ( remUseLess )
        Rest.AddRange (remUselessProj (Start));

      int cnt = 0;
      while ( cnt < Rest.Count )
      {
        bool aB = true;
        for ( int i = 0; i < Projekte.Count; i++ )
          if ( Start[ i ].editable && Start[ i ].AllowedKlassen[ Rest[ 0 ].GetKlasse ( ) ] && Start[ i ].TeilnehmerCount < Projekte[ i ].MinAnz )
          {
            Start[ i ].Add (new Objekt (Rest[ 0 ]));
            Rest.RemoveAt (0);
            aB = false;
            break;
          }
        if ( aB )
          ++cnt;
      }
      cnt = 0;
      while ( cnt < Rest.Count )
      {
        bool aB = true;
        for ( int i = 0; i < Start.Count; i++ )
          if ( Start[ i ].editable && Start[ i ].AllowedKlassen[ Rest[ 0 ].GetKlasse ( ) ] && Start[ i ].TeilnehmerCount < Projekte[ i ].MaxAnz )
          {
            Start[ i ].Add (new Objekt (Rest[ 0 ]));
            Rest.RemoveAt (0);
            aB = false;
            break;
          }
        if ( aB )
          ++cnt;
      }

    }

    public List<Objekt> remUselessProj ( List<Projekt> Projekte )
    {
      List<Objekt> ret = new List<Objekt> ( );
      for ( int i = 0; i < Projekte.Count; ++i )
        if ( !Projekte[ i ].erhaltenswert && getE_Wert (i, Projekte) < numericUpDown6.Value )
          ret.AddRange (Projekte[ i ].kill ( ));
      return ret;
    }

    static BigInteger aktBestSolValue = -1;
    static SortedSet<int> Temperaturfolge = new SortedSet<int> ( );
    static List<int> Schritte = new List<int> ( );
    static int SchrittCounter = 0;

    BigInteger bewerte ( out int AnzMin, out int AnzMax, out int AnzCMiss )
    {
      BigInteger score = 0;
      AnzMin = 0;
      AnzMax = 0;
      AnzCMiss = 0;
      for ( int i = 0; i < Projekte.Count; ++i )
      {
        foreach ( var S in Projekte[ i ].GetList ( ) )
        {
          int cnt = 0;
          foreach ( var W in S.GetList ( ) )
            if ( W == i )
            {
              score += (BigInteger)SchHWunsch[ cnt ].Value;
              break;
            }
            else
              ++cnt;
          if ( !( Projekte[ i ].AllowedKlassen[ S.GetKlasse ( ) ] ) )
            ++AnzCMiss;
        }
        if ( Projekte[ i ].TeilnehmerCount < Projekte[ i ].MinAnz )
          ++AnzMin;
        else if ( Projekte[ i ].TeilnehmerCount > Projekte[ i ].MaxAnz )
          ++AnzMax;
      }
      return score;
    }

    BigInteger bewerte_sol ( out int AnzMin, out int AnzMax, out int AnzCMiss )
    {
      BigInteger score = 0;
      AnzMin = 0;
      AnzMax = 0;
      AnzCMiss = 0;
      for ( int i = 0; i < Solution.Count; ++i )
      {
        foreach ( var S in Solution[ i ].GetList ( ) )
        {
          int cnt = 0;
          foreach ( var W in S.GetList ( ) )
            if ( W == i )
            {
              score += (BigInteger)SchHWunsch[ cnt ].Value;
              break;
            }
            else
              ++cnt;
          if ( !( Solution[ i ].AllowedKlassen[ S.GetKlasse ( ) ] ) )
            ++AnzCMiss;
        }
        if ( Solution[ i ].TeilnehmerCount < Solution[ i ].MinAnz )
          ++AnzMin;
        else if ( Solution[ i ].TeilnehmerCount > Solution[ i ].MaxAnz )
          ++AnzMax;
      }
      return score;
    }

    public void calculate ( )
    {
      setSettings (false);
      int ANZMIN, ANZMAX, ANZCMISS;
      if ( Start == null )
        init (checkBox2.Checked, checkBox3.Checked);
      for ( ; !beenden; )
      {
        Random rnd = new Random ( );

        Schritte.Clear ( );
        for ( int i = 0; i < 5000; i++ )
        {
          int b = rnd.Next (1, schüler.Count * (int)SchHWunsch[ 0 ].Value);
          Temperaturfolge.Add (b);
          Schritte.Add (b);
        }
        SchrittCounter = 0;

        Projekte = Start;

        Application.DoEvents ( );

        while ( bewerte (out ANZMIN, out ANZMAX, out ANZCMISS)
                          * (BigInteger)( ANZMIN > 0 ? ( numericUpDown2.Value / 100 ) : 1 ) * (BigInteger)( ANZMAX > 0 ? ( numericUpDown3.Value / 100 ) : 1 )
                          * (BigInteger)( ANZCMISS > 0 ? 0 : 1 ) <= schüler.Count * (BigInteger)SchHWunsch[ 0 ].Value && !beenden )
        {
          Application.DoEvents ( );
          BigInteger BewertungOld = ( bewerte (out ANZMIN, out ANZMAX, out ANZCMISS)
                                              * (BigInteger)( ANZMIN > 0 ? ( numericUpDown2.Value / 100 ) : 1 ) * (BigInteger)( ANZMAX > 0 ? ( numericUpDown3.Value / 100 ) : 1 )
                                              * (BigInteger)( ANZCMISS > 0 ? 0 : 1 ) );
          ;
          BigInteger B2 = BewertungOld;
          int Quality = 66;

          if ( bewerte (out ANZMIN, out ANZMAX, out ANZCMISS)
                           * (BigInteger)( ANZMIN > 0 ? ( numericUpDown2.Value / 100 ) : 1 ) * (BigInteger)( ANZMAX > 0 ? ( numericUpDown3.Value / 100 ) : 1 )
                           * (BigInteger)( ANZCMISS > 0 ? 0 : 1 ) < schüler.Count * (BigInteger)SchHWunsch[ 0 ].Value && !beenden )
          {
            if ( Temperaturfolge.Count == 0 || ( BewertungOld == 0 && ( rnd.Next (1, schüler.Count * (int)SchHWunsch[ 0 ].Value) > Temperaturfolge.Last ( ) ) ) )
              break;
            if ( SchrittCounter > Temperaturfolge.Last ( ) )
            {
              SchrittCounter = 0;
              Temperaturfolge.Remove (Temperaturfolge.Last ( ));
            }

            #region Austausch zweier Schüler
            int ZufProj1 = rnd.Next (0, Projekte.Count);
            while ( ( !Projekte[ ZufProj1 ].editable || Projekte[ ZufProj1 ].TeilnehmerCount == 0 ) && !beenden )
            {
              ZufProj1 = rnd.Next (0, Projekte.Count);
              Application.DoEvents ( );
            }
            int ZufProj2 = rnd.Next (0, Projekte.Count);
            while ( ( ZufProj1 == ZufProj2 || !Projekte[ ZufProj2 ].editable || Projekte[ ZufProj2 ].TeilnehmerCount == 0 ) && !beenden )
            {
              ZufProj2 = rnd.Next (0, Projekte.Count);
              Application.DoEvents ( );
            }

            if ( beenden )
              continue;

            bool Prj1CanBeEmpty = Projekte[ ZufProj1 ].TeilnehmerCount < Projekte[ ZufProj1 ].MaxAnz;

            int ZufSchüler1 = rnd.Next (0, Projekte[ ZufProj1 ].Length + ( Prj1CanBeEmpty ? 1 : 0 ));
            bool isMax = ZufSchüler1 == Projekte[ ZufProj1 ].Length;
            while ( !isMax && Projekte[ ZufProj1 ][ ZufSchüler1 ].isLeiter )
            {
              ZufSchüler1 = rnd.Next (0, Projekte[ ZufProj1 ].Length + ( Prj1CanBeEmpty ? 1 : 0 ));
              isMax = ZufSchüler1 == Projekte[ ZufProj1 ].Length;
            }

            int ZufSchüler2 = rnd.Next (0, Projekte[ ZufProj2 ].Length);
            while ( Projekte[ ZufProj2 ][ ZufSchüler2 ].isLeiter )
              ZufSchüler2 = rnd.Next (0, Projekte[ ZufProj2 ].Length);

            Objekt Z1 = null, Z2 = new Objekt (Projekte[ ZufProj2 ][ ZufSchüler2 ]);
            if ( !isMax )
            {
              Z1 = new Objekt (Projekte[ ZufProj1 ][ ZufSchüler1 ]);
              Projekte[ ZufProj2 ].Add (Z1);
            }
            Projekte[ ZufProj1 ].Add (Z2);
            Projekte[ ZufProj2 ].Remove (Projekte[ ZufProj2 ][ ZufSchüler2 ]);
            if ( !isMax )
              Projekte[ ZufProj1 ].Remove (Projekte[ ZufProj1 ][ ZufSchüler1 ]);

            BigInteger BewertungNew = ( bewerte (out ANZMIN, out ANZMAX, out ANZCMISS)
                                                  * (BigInteger)( ANZMIN > 0 ? ( numericUpDown2.Value / 100 ) : 1 ) * (BigInteger)( ANZMAX > 0 ? ( numericUpDown3.Value / 100 ) : 1 )
                                                  * (BigInteger)( ANZCMISS > 0 ? 0 : 1 ) );

            if ( BewertungNew <= BewertungOld && ( rnd.Next (1, schüler.Count * (int)SchHWunsch[ 0 ].Value) < Temperaturfolge.Last ( ) ) )
            {
              Projekte[ ZufProj2 ].Add (new Objekt (Z2));
              Projekte[ ZufProj1 ].Remove (Z2);
              if ( !isMax )
              {
                Projekte[ ZufProj1 ].Add (new Objekt (Z1));
                Projekte[ ZufProj2 ].Remove (Z1);
              }
              SchrittCounter++;
            }

            #endregion

            B2 = BewertungOld > BewertungNew ? BewertungOld : BewertungNew;
            Quality = 66;
            try
            {
              Quality = (int)numericUpDown1.Value;
            }
            catch ( Exception )
            {
            }
          }
          if ( ( B2 * 100 ) / ( schüler.Count * (int)SchHWunsch[ 0 ].Value ) >= Quality && B2 > aktBestSolValue )
          {
            if ( B2 > aktBestSolValue )
            {
              SchrittCounter = 0;
              if ( Temperaturfolge.Count > 0 )
                Temperaturfolge.Remove (Temperaturfolge.Last ( ));
            }
            aktBestSolValue = B2;
            numericUpDown1.Value = (decimal)( ( aktBestSolValue * 100 ) / ( (BigInteger)SchHWunsch[ 0 ].Value * (BigInteger)schüler.Count ) );

            label1.Text = "Current solution's score is " + (int)( ( B2 * 100 ) / ( (BigInteger)schüler.Count * (BigInteger)SchHWunsch[ 0 ].Value ) )
            + "%. (Score: " + B2 + ")";
            label1.Visible = true;

            Solution = new List<Projekt> ( );
            int cnt = 0;
            listView3.Items.Clear ( );
            for ( int i = 0; i < Projekte.Count; i++ )
            {
              Solution.Add (new Projekt (Projekte[ i ]));
              int cnt2 = 0;
              foreach ( var item in Projekte[ i ].GetList ( ).Union (Projekte[ i ].GetLeiterList ( )) )
              {
                var Item = item.AsItem (GetColor (item), button7.BackColor, cnt, i, cnt2++);
                Item.Group = listView3.Groups[ i ];
                if ( !Projekte[ i ].AllowedKlassen[ item.GetKlasse ( ) ] )
                  Item.ForeColor = Color.Red;
                for ( int W = 0; W < item.Wünsche.Count; ++W )
                  if ( item.Wünsche[ W ] == i )
                  {
                    Item.ForeColor = ColorButtons[ W ].BackColor;
                    break;
                  }
                if ( Projekte[ i ].TeilnehmerCount < Projekte[ i ].MinAnz && !item.isLeiter )
                  Item.BackColor = button2.BackColor;
                if ( Projekte[ i ].TeilnehmerCount > Projekte[ i ].MaxAnz && !item.isLeiter )
                  Item.BackColor = button3.BackColor;

                listView3.Items.Add (Item);
              }
            }
            this.Refresh ( );
          }
        }
      }
      setSettings (true);
    }

    public Color GetColor ( Objekt item )
    {
      Color DefColor = listView1.BackColor;
      if ( item.Wünsche.Count > 0 )
        if ( !Projekte[ item.Wünsche[ 0 ] ].editable )
          DefColor = button14.BackColor;
      return DefColor;
    }

    private void textBox3_TextChanged ( object sender, EventArgs e )
    {

    }
  }
}