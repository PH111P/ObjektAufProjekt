ObjektAufProjekt
================
Often they are situations in which certain courses (or “Projekte”) have to be “filled” with participants (or “Objekte”). 
But mostly these “Objekte” shall be offered to choice between multiple courses and supply a wishlist of “Projekte” they want to attend. As an organisator of such an event you may see a problem arising: how do I get these “Objekte” in those “Projekte” fulfilling their wishs optimally, or at least almost optimally. And which “Projekt” mustn't I offer as no one's interested?

That's the point where this tool will help you!

From now on, you'll be able to keep the process of filling the “Projekte” short and concentrate on other, more important things, e.g., organizing the “Projekte”, ....

How it works
------------

First, you supply a list of all the available “Projekte” and of all the “Objekte” and their wishs. 

As you may have noticed, trying all possible distributions from “Objekte” to “Projekte” is rather slow, having a [complexity](http://en.wikipedia.org/wiki/Algorithmic_efficiency) of something like O(k^n), k = #Projekte, n = #Objekte.
So, to not keep you waiting forever, “ObjektAufProjekt” uses a [heuristic](http://en.wikipedia.org/wiki/Heuristic_algorithm) called [simulated annealing](http://en.wikipedia.org/wiki/Simulated_annealing).

The program first creates a “rough solution” by randomly fulfilling the “Objekte”'s wishs, the _initial state_. This initial state is given a certain so called _score_.
Then it randomly swaps two “Objekte” and checks whether the score increased. If yes, the new solution is kept, and the other one is discarded. If no, the new solution is only kept with a certain probability, which is decreasing in time. So at the beginning, there's a higher chance in keeping a _locally_ worse solution whereas after some time only locally better solutions are kept.

At anytime you can stop the process and view the most recent solution and may use it.

Building
--------

First, clone the repo or just download the `.zip` file. Then just open the `Prowo.sln` file located at `Prowo2.0/Prowo.sln` in MS Visual Studio or MonoDevelop (or the C# IDE of your choice) and build.

Usage
-----

First, create a file containing the Projekte. You may use the file extension `.prd` for those kind of files.
A such file may contain any number of Projekte, one per line, where a Projekt is specified by:
```
Name_of_Projekt/Min_no_of_Participants/Max_no_of_Participants/Allowed_Klassen/Participants_not_constant/Projekt_description
```

Second, create a file containing the Objekte. You may use the file extension `.scd` for those kind of files.
A such file may contain any number of Objekte, one per line, where a Objekt is specified by:
```
Last_name/First_name/[No. or Name]_of_wish_1/[...]/[No. or Name]_of_wish_MAXWISHES/Klasse
```

Let the program read those files and start calculating.

If you think the ObjektAufProjekt program has been calculating enough, you may stop the calculation and export the result  to a `.tex` document. See proper tutorials or instruction manuals of your choice if you don't know how to compile (La)TeX files into PDF documents.

Features
--------
* Specify a certain minimum score (as a percentage from the maximum score) for solutions to have at least.
* Pause the process at any time, modify the solutione, and resume with the modified solution.
* “Objekte” may have from 2 to 30 wishes.
* Specify how to score a realised matching of a “Objekt” with his _n_th wish.
* Categorise “Objekte” in classes (“Klassen”)
* Specify a minimum and a maximum amount of participants for “Projekte”
  * and how to score an overfilled or underfilled “Projekt”.
* Specify specific “Klassen” for participants of a “Projekt” to be from
  * and how to score a “class mismatch”.
* Generate TeX output,
  * including lists for “Objekte” to specify their wishs.
  * including lists of “Projekte” with thier participants.
* A colorful GUI.
* basic `.csv` import: No headline, values have to have the same order as they would have had in a `.scd` or `.prd` file.

Planned Features
----------------
* Directly export `.pdf` files
* Adjust the exported pages' header (and footer) directly
* Explicit scoring rules
