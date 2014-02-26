using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Project prj = new Project();

            Activity A = prj.AddActivity("A", 3, 5);
            Activity B = prj.AddActivity("B", 5, 7);
            Activity C = prj.AddActivity("C", 2, 5);
            Activity D = prj.AddActivity("D", 7, 8);
            Activity E = prj.AddActivity("E", 9, 10);
            Activity F = prj.AddActivity("F", 10, 10);
            Activity G = prj.AddActivity("G", 5, 4);

            prj.AddRelation(A, B);
            prj.AddRelation(B, C);
            prj.AddRelation(A, D);
            prj.AddRelation(D, C);
            prj.AddRelation(E, D);
            prj.AddRelation(E, G);
            prj.AddRelation(G, F);
            prj.AddRelation(D, F);

            /* for another test
            Activity A = prj.AddActivity("A", 5, 5);
            Activity B = prj.AddActivity("B", 5, 7);
            Activity C = prj.AddActivity("C", 3, 5);
            Activity D = prj.AddActivity("D", 3, 8);

            prj.AddRelation(A, B);
            prj.AddRelation(C, B);
            prj.AddRelation(A, D);
            */
            /* for AnyCPM
            Activity A = prj.AddActivity("A", 3, 5);
            Activity B = prj.AddActivity("B", 6, 7);
            Activity C = prj.AddActivity("C", 6, 5);
            Activity D = prj.AddActivity("D", 3, 8);

            prj.AddRelation(A, B);
            prj.AddRelation(C, D);
            prj.AddRelation(A, D);
            */
            List<Activity> lst = prj.TopoSort();

            Console.WriteLine("Toposort:");
            foreach (Activity a in lst)
                Console.Write(a.Name);
            Console.WriteLine();
            //prj.CritPath(lst);
            Console.WriteLine("AnyCPM:\n" + prj.AnyCPM(prj.CritPath(lst)));
            prj.SLK(lst);
            prj.FSLK(lst);
            prj.countMTS();

            foreach (Activity a in lst)
            {
                Console.WriteLine("//Name: {0},\n[EST: {1}, EET: {2}, LST: {3}, LET: {4}, SLK: {5}, FSLK: {6}, MTS: {7}, Duration: {8}]", a.Name, a.EST, a.EET, a.LST, a.LET, a.SLK, a.FSLK, a.MTS, a.Duration);
            }
            Console.WriteLine();
            Console.WriteLine("Total number of links: {0}", prj.Relations.Count());
            Console.WriteLine("NC: " + prj.NC());
            Console.WriteLine("Cost: " + prj.Cost());
            //Console.WriteLine("the full number of A's successors is: "+prj.MTS(A));
            /////////////////////////////////////////////////////
            /*
            prj.DelRelation(A, D);
            //prj.DelActivity(C);
            List<Activity> dd = prj.TopoSort();

            foreach (Activity a in dd)
            {
                Console.WriteLine("Name: {0}, EST: {1}, EET: {2}, LST: {3}, LET: {4}, Duration: {5}", a.Name, a.EST, a.EET, a.LST, a.LET, a.Duration);
            }
            Console.WriteLine();
            Console.WriteLine("Total number of links: {0}", prj.Relations.Count());
            Console.WriteLine("NC: " + prj.NC());
            Console.WriteLine("Cost: " + prj.Cost());
            */
            Console.ReadKey();
        }
    }

    class Project
    {
        public List<Activity> Activities = new List<Activity>();
        public List<Relation> Relations = new List<Relation>();
        private List<Activity> sortedActivities = new List<Activity>();


        public List<Activity> CopyActivities(List<Activity> acts)
        {
            List<Activity> actsMimic = new List<Activity>();
            foreach (Activity a in acts)
            {
                actsMimic.Add(a);
            }
            return actsMimic;
        }


        public Activity AddActivity(string name, double duration = 0, double cost = 0)
        {
            Activity act = new Activity();
            act.Name = name;
            act.Duration = duration;
            act.Cost = cost;

            Activities.Add(act);
            return act;
        }
        /*
        public void DelActivity(Activity act)
        {
        }
        */
        public Relation AddRelation(Activity p, Activity s)
        {
            Relation rel = new Relation();
            rel.Predecessor = p;
            rel.Successor = s;

            Relations.Add(rel);
            p.Successors.Add(rel);
            s.Predecessors.Add(rel);

            return rel;
        }

        public void DelRelation(Activity p, Activity s)
        {
            // need to refactor?
            Relation rel = new Relation();

            foreach (Relation r in Relations)
            {
                if ((r.Predecessor == p) && (r.Successor == s))
                {
                    rel = r;
                    break;
                }
            }

            Relations.Remove(rel);
            p.Successors.Remove(rel);
            s.Predecessors.Remove(rel);
        }

        private void setActivity(Activity x)
        {
            sortedActivities.Add(x);
            foreach (Relation rel in x.Successors)
            {
                Activity s = rel.Successor;
                s.rel_count++;

                if (s.rel_count == s.Predecessors.Count)
                {
                    setActivity(s);
                }
            }
        }

        public void checkFirstLastUniqueness()
        {
            List<Activity> uFirst = new List<Activity>();
            List<Activity> uLast = new List<Activity>();

            foreach (Activity a in Activities)
            {
                if (a.Predecessors.Count == 0)
                    uFirst.Add(a);
                if (a.Successors.Count == 0)
                    uLast.Add(a);
            }

            Console.WriteLine("uFirst: {0}, uLast: {1}", uFirst.Count(), uLast.Count());
            makeUnique(uFirst, uLast);
        }

        private void makeUnique(List<Activity> uFirst, List<Activity> uLast)
        {
            if (uFirst.Count() > 1)
            {
                Activity FIRST = AddActivity("[FIRST]");
                foreach (Activity U in uFirst)
                {
                    AddRelation(FIRST, U);
                }
            }
            if (uLast.Count() > 1)
            {
                Activity LAST = AddActivity("[LAST]");
                foreach (Activity U in uLast)
                {
                    AddRelation(U, LAST);
                }
            }
        }

        public List<Activity> TopoSort()
        {
            checkFirstLastUniqueness();
            foreach (Activity a in Activities)
            {
                a.rel_count = 0;
            }

            sortedActivities.Clear();

            foreach (Activity a in Activities)
            {
                if (a.Predecessors.Count == 0)
                    setActivity(a);
            }
            return sortedActivities;
        }

        public double NC()
        {
            return (double)Relations.Count() / Activities.Count();
        }

        public double Cost()
        {
            double cost = 0;
            foreach (Activity a in Activities)
            {
                cost += a.Cost;
            }
            return cost;
        }

        private void CalculateTimeForward(List<Activity> toposorted)
        {
            // assume that list 'toposorted' is sorted
            toposorted[0].EET = toposorted[0].EST + toposorted[0].Duration;

            for (int i = 1; i < Activities.Count(); i++)
            {
                foreach (Relation rel in toposorted[i].Predecessors)
                {
                    Activity pred = rel.Predecessor;
                    if (toposorted[i].EST < pred.EET)
                        toposorted[i].EST = pred.EET;
                }
                toposorted[i].EET = toposorted[i].EST + toposorted[i].Duration;
            }
        }

        private void CalculateTimeBackward(List<Activity> toposorted)
        {
            toposorted[toposorted.Count() - 1].LET = toposorted[toposorted.Count() - 1].EET;
            toposorted[toposorted.Count() - 1].LST = toposorted[toposorted.Count() - 1].LET - toposorted[toposorted.Count() - 1].Duration;

            for (int i = toposorted.Count() - 2; i >= 0; i--)
            {
                foreach (Relation rel in toposorted[i].Successors)
                {
                    Activity suc = rel.Successor;
                    if (toposorted[i].LET == 0)
                        toposorted[i].LET = suc.LST;
                    if (toposorted[i].LET == 0)
                        break;
                    if (toposorted[i].LET > suc.LST)
                        toposorted[i].LET = suc.LST;
                }
                toposorted[i].LST = toposorted[i].LET - toposorted[i].Duration;
            }
        }

        public List<Activity> CritPath(List<Activity> toposorted)
        {
            CalculateTimeForward(toposorted);
            CalculateTimeBackward(toposorted);

            List<Activity> crit = new List<Activity>();
            foreach (Activity act in toposorted)
                if ((act.EET == act.LET) && (act.EST == act.LST))
                    crit.Add(act);
            Console.Write("CritPath is:");
            foreach (Activity c in crit)
                Console.Write("{0}", c.Name);
            Console.WriteLine("");
            return crit;
        }

        public string AnyCPM(List<Activity> cpm, int i = 0)
        {
            // argument - critPath list
            Console.WriteLine("I is : " + i);
            string res = "";
            if (cpm[i].Successors.Count() == 0)
            {
                return cpm[i].Name + "\n";
            }
            foreach (Relation rel in cpm[i].Successors)
            {
                Activity suc = rel.Successor;
                if ((cpm.IndexOf(suc) > 0) && (cpm[i].EET == suc.EST))
                {
                    res += cpm[i].Name + AnyCPM(cpm, cpm.IndexOf(suc));
                }
            }
            return res;
        }

        public void SLK(List<Activity> toposorted)
        {
            // полный резерв
            foreach (Activity a in toposorted)
            {
                a.SLK = a.LST - a.EST;
            }
        }

        public void FSLK(List<Activity> toposorted)
        {
            // свободный резерв
            // FSLKi = min(ESTj-EETi)
            foreach (Activity a in toposorted)
            {
                double s = double.MaxValue;
                foreach (Relation suc in a.Successors)
                {
                    double diff = suc.Successor.EST - a.EET;
                    if (s > diff)
                        s = diff;
                }
                if (s == double.MaxValue)
                    s = 0;
                a.FSLK = s;
            }
        }

        public string scs = "";

        public int MTS(Activity a)
        {
            // the full number of all successors
            int c = 0;
            if (a.Successors.Count() == 0)
            {
                //Console.WriteLine("//Name of a is: "+a.Name);
                return c;
            }
            //Console.WriteLine("Name of a is: "+a.Name);
            foreach (Relation rel in a.Successors)
            {
                Activity suc = rel.Successor;

                if (scs.IndexOf(suc.Name) < 0)
                {
                    //Console.WriteLine("Name of suc is: "+suc.Name);
                    //Console.WriteLine("1jj is: "+scs);
                    scs += suc.Name;
                    //Console.WriteLine("2jj is: "+scs);
                    c += 1 + MTS(suc);
                }
            }
            //Console.WriteLine("c is :"+c);
            return c;
        }

        public void countMTS()
        {
            foreach (Activity a in Activities)
            {
                scs = "";
                a.MTS = MTS(a);
                scs = "";
            }
        }

    }

    class Activity
    {
        public string Name;
        public double Duration;
        public double EST;
        public double EET;
        public double LST;
        public double LET;
        public double SLK;
        public double FSLK;
        public double MTS;

        public double Cost;
        public int rel_count;

        public List<Relation> Successors = new List<Relation>();
        public List<Relation> Predecessors = new List<Relation>();

        public double optimistic, pessimistic, mostlikely, mean, var, sigma;

        public void setPERT(double opt, double moda, double pes)
        {
            optimistic = opt;
            pessimistic = pes;
            mostlikely = moda;

            mean = (opt + moda * 4 + pes) / 6;
            sigma = (pes + opt) / 6;
            var = sigma * sigma;
            Duration = mean;
        }

    }

    class Relation
    {
        public Activity Predecessor;
        public Activity Successor;
    }
}
