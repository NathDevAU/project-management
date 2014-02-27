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
            // mock graph
            Activity A = prj.AddActivity("A", 3, 5, 300, 500);
            Activity B = prj.AddActivity("B", 5, 7, 500, 700);
            Activity C = prj.AddActivity("C", 2, 5, 200, 500);
            Activity D = prj.AddActivity("D", 7, 8, 700, 800);
            Activity E = prj.AddActivity("E", 9, 10, 900, 1000);
            Activity F = prj.AddActivity("F", 10, 12, 1000, 1200);
            Activity G = prj.AddActivity("G", 5, 7, 500, 700);
            prj.AddRelation(A, B);
            prj.AddRelation(B, C);
            prj.AddRelation(A, D);
            prj.AddRelation(D, C);
            prj.AddRelation(E, D);
            prj.AddRelation(E, G);
            prj.AddRelation(G, F);
            prj.AddRelation(D, F);


            Console.ReadKey();
        }

        static public void CPU_Count(Project project)
        {

            // first - count with min duration and max costs
            // second - with max duration and min costs

            bool minDuration = true;
            string minmaxDuration = "";
            string minmaxCosts = "";
            double limDuration = 0;
            string yno = "";
            List<List<Activity>> toposorted = new List<List<Activity>>();

            for (int i = 0; i<2; i++)
            {
                toposorted.Add(project.TopoSort(setDurationAndCosts(project.Activities, minDuration)));
                project.CalculateTimeForward(toposorted[i]);
                double costs = project.sumCosts(toposorted[i]);
                limDuration = minDuration ? toposorted[i][toposorted.Count - 1].EET : 0;

                minmaxDuration = minDuration ? "minimally" : "maximally";
                minmaxCosts = minDuration ? "maximal" : "minimal";

                Console.WriteLine("The {0} possible duration of the project with {1} costs is: {2} days and {3} dollars", minmaxDuration, minmaxCosts, toposorted[i][toposorted.Count - 1].EET, costs);

                minDuration = false;
            }

            
            while (true)
            {
                double duration = toposorted[1][toposorted.Count - 1].EET;
                Console.WriteLine("Are {0} days acceptable for the project? Enter Y/YES or N/No...", duration);
                yno = Console.ReadLine();

                if ((yno == "Y") || (yno == "Yes"))
                {
                    Console.WriteLine("=====Total result:=====");
                    Console.WriteLine("Total duration of the project: {0}", duration);
                    toConsole(toposorted[1]);
                    break;
                }
                else if ((yno == "N") || (yno == "No"))
                {
                    double requiredDuration = 0;
                    while (true)
                    {
                        Console.WriteLine("Please enter a required duration: ");
                        double reqDuration = double.Parse(Console.ReadLine());
                        if (reqDuration < limDuration)
                        {
                            Console.WriteLine("The duration cannot be less than: {0}", limDuration);
                        }
                        else if (reqDuration == limDuration)
                        {
                            requiredDuration = reqDuration;
                            Console.WriteLine("=====Total result:=====");
                            Console.WriteLine("Total duration of the project: {0}", reqDuration);
                            toConsole(toposorted[0]);
                            break;
                        }
                        else
                        {
                            List<Activity> optimizedActivities = optimizeProjectDuration(toposorted[1]);
                            break;
                        }
                    }

                    
                }
            }
            

        }

        static private List<Activity> setDurationAndCosts(List<Activity> activities, bool minDuration = true)
        {
            foreach (Activity act in activities)
            {
                act.Duration = minDuration? act.DurationMin : act.DurationMax;
                act.Cost = minDuration? act.CostMax : act.CostMin;
            }
            return activities;
        }

        static public List<Activity> optimizeProjectDuration(List<Activity> toposorted)
        {

            return null;
        }

        static public void toConsole(List<Activity> activities)
        {
            foreach(Activity act in activities)
            {
                Console.WriteLine("Name: {0}; Duration: {1}; Cost: {2}; EST: {3}; EET: {4};", act.Name, act.Duration, act.Cost, act.EST, act.EET);
            }
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


        public Activity AddActivity(string name, double dMin = 0, double dMax = 0, double cMin = 0, double cMax = 0)
        {
            Activity act = new Activity();
            act.Name = name;
            act.DurationMin = dMin;
            act.DurationMax = dMax;
            act.CostMin = cMin;
            act.CostMax = cMax;


            // what with duration and cost?
            Activities.Add(act);
            return act;
        }

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

        public List<Activity> TopoSort(List<Activity> activities)
        {
            checkFirstLastUniqueness();
            foreach (Activity a in activities)
            {
                a.rel_count = 0;
            }

            sortedActivities.Clear();

            foreach (Activity a in activities)
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

        public double sumCosts(List<Activity> activities)
        {
            double cost = 0;
            foreach (Activity a in activities)
            {
                cost += a.Cost;
            }
            return cost;
        }

        public void CalculateTimeForward(List<Activity> toposorted)
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

        public void CalculateTimeBackward(List<Activity> toposorted)
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

        // duration, cost - min, max;   
        public double DurationMin;
        public double DurationMax;
        public double CostMin;
        public double CostMax;


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
