ObjektAufProjekt
================
Often they are situations in which certain courses (or "Projekte") have to be "filled" with participants (or "Objekte"). 
But mostly these "Objekte" shall be offered to choice between multiple courses and supply a wishlist of "Projekte" they want to attend. As an organisator of such an event you may see a problem arising: how do I get these "Objekte" in those "Projekte" fulfilling their wishs optimally, or at least almost optimally. And which "Projekt" mustn't I offer as no one's interested?

That's the point where this tool will help you!

From now on, you'll be able to keep the process of filling the "Projekte" short and concentrate on other, more important things, e.g., organizing the "Projekte", ....

How it works
------------

First, you supply a list of all the available "Projekte" and of all the "Objekte" and their wishs. 

As you may have noticed, trying all possible distributions from "Objekte" to "Projekte" is rather slow, having a [complexity](http://en.wikipedia.org/wiki/Algorithmic_efficiency) of something like O(k^n), k = #Projekte, n = #Objekte.
So, to not keep you waiting forever, "ObjektAufProjekt" uses a [heuristic](http://en.wikipedia.org/wiki/Heuristic_algorithm) called [simulated annealing](http://en.wikipedia.org/wiki/Simulated_annealing).

The program first creates a "rough solution" by randomly fulfilling the "Objekte"'s wishs, the _initial state_. This initial state is given a certain so called _score_.
Then it randomly swaps two "Objekte" and checks whether the score increased. If yes, the new solution is kept, and the other one is discarded. If no, the new solution is only kept with a certain probability, which is decreasing in time. So at the beginning, there's a higher chance in keeping a _locally_ worse solution whereas after some time only locally better solutions are kept.

At anytime you can stop the process and view the most recent solution and may use it.

Features
--------
* Specify a certain minimum score (as a percentage from the maximum score) for solutions to have at least.
* Pause the process at any time, modify the solutione, and resume with the modified solution.
* "Objekte" may have from 2 to 10.
* Specify how to score a realised matching of a "Objekt" with his _n_th wish.
* Categorise "Objekte" in classes ("Klassen")
* Specify a minimum and a maximum amount of participants for "Projekte"
  * and how to score an overfilled or underfilled "Projekt".
* Specify specific "Klassen" for participants of a "Projekt" to be from
  * and how to score a "class mismatch".
* Generate TeX output,
  * including lists for "Objekte" to specify their wishs.
  * including lists of "Projekte" with thier participants.
* A colorful GUI.
