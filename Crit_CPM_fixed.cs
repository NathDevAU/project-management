
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication3
{

    class Program
    {
        static void Main(string[] args)
        {
            Project prj = new Project();
            // test 1
            /*
            Activity A = prj.AddActivity("A", 3, 5, 300, 550);
            Activity B = prj.AddActivity("B", 5, 7, 500, 1000);
            Activity C = prj.AddActivity("C", 2, 5, 200, 680);
            Activity D = prj.AddActivity("D", 8, 12, 1600, 2950);
            Activity E = prj.AddActivity("E", 9, 10, 900, 1300);
            Activity F = prj.AddActivity("F", 10, 12, 1000, 1500);
            Activity G = prj.AddActivity("G", 5, 7, 500, 800);
            prj.AddRelation(A, B);
            prj.AddRelation(B, C);
            prj.AddRelation(A, D);
            prj.AddRelation(D, C);
            prj.AddRelation(E, D);
            prj.AddRelation(E, G);
            prj.AddRelation(G, F);
            prj.AddRelation(D, F);
            */
            // test 2

            Activity A = prj.AddActivity("A", 5, 6, 300, 350);
            Activity B = prj.AddActivity("B", 2, 5, 200, 980);
            Activity C = prj.AddActivity("C", 7, 9, 200, 500);
            Activity D = prj.AddActivity("D", 7, 11, 1600, 2600);
            Activity E = prj.AddActivity("E", 5, 6, 900, 1020);
            Activity F = prj.AddActivity("F", 8, 9, 1000, 1110);
            Activity G = prj.AddActivity("G", 9, 10, 500, 600);
            Activity H = prj.AddActivity("H", 9, 10, 400, 670);
            prj.AddRelation(A, B);
            prj.AddRelation(B, C);
            prj.AddRelation(C, D);
            prj.AddRelation(D, E);
            prj.AddRelation(B, F);
            prj.AddRelation(F, D);
            prj.AddRelation(A, G);
            prj.AddRelation(G, H);
            prj.AddRelation(H, E);

            /* for another test
            Activity A = prj.AddActivity("A", 5, 5);
            Activity B = prj.AddActivity("B", 5, 7);
            Activity C = prj.AddActivity("C", 3, 5);
            Activity D = prj.AddActivity("D", 3, 8);

            prj.AddRelation(A, B);
            prj.AddRelation(C, B);
            prj.AddRelation(A, D);
            end another test */
            /* for AnyCPM
            Activity A = prj.AddActivity("A", 3, 5);
            Activity B = prj.AddActivity("B", 6, 7);
            Activity C = prj.AddActivity("C", 6, 5);
            Activity D = prj.AddActivity("D", 3, 8);

            prj.AddRelation(A, B);
            prj.AddRelation(C, D);
            prj.AddRelation(A, D);
            end AnyCPM  */

            prj.Crit_CPM();

            Console.ReadKey();

        }
    }

    class Project
    {
        public List<Activity> Activities = new List<Activity>();
        public List<Relation> Relations = new List<Relation>();
        public List<Activity> sortedActivities = new List<Activity>();
        public List<Activity> crit = new List<Activity>();
        public List<Activity> cuActivities = new List<Activity>();
        public double projectCost = 0;

        public double prjDuration;
        // -----------------------------------------------------------------------------------
        public void Crit_CPM()
        {
            // start with minimal and maximal values of costs and duration
            TopoSort();
            countDurationsWithCosts(false);
            CalculateTimeForward(sortedActivities);
            double minDurationProject = sortedActivities[sortedActivities.Count - 1].EET;
            double maxCostProject = calculateCosts();
            Console.WriteLine("Minimal duration of the project is: {0}\nMaximal costs of the project are: {1}", minDurationProject, maxCostProject);
            countDurationsWithCosts(true);

            CalculateTimeForward(sortedActivities);
            double maxDurationProject = sortedActivities[sortedActivities.Count - 1].EET;
            double minCostProject = calculateCosts();
            Console.WriteLine("Maximal duration of the project is: {0}\nMinimal costs of the project are: {1}", maxDurationProject, minCostProject);



            double reqDuration = new Double();
            while (true)
            {
                Console.WriteLine("\nPlease enter the appropriate duration of the project");
                reqDuration = double.Parse(Console.ReadLine());

                if (reqDuration > maxDurationProject)
                {
                    Console.WriteLine("You should not choose duration more than {0}", maxDurationProject);
                }
                else if (reqDuration < minDurationProject)
                {
                    Console.WriteLine("You cannot choose duration less than {0}", minDurationProject);
                } else if (reqDuration == maxDurationProject) {
                    CalculateTimeForward(sortedActivities);
                    CalculateTimeBackward(sortedActivities);
                    toConsole();
                    return;
                }
                else
                {
                    break;
                }
            }

            Console.WriteLine("You've chosen duration {0}", reqDuration);
            Console.WriteLine("Processing...");

            while (reqDuration < sortedActivities[sortedActivities.Count - 1].EET)
            {
                setTimesToZero();
                CalculateTimeForward(sortedActivities);
                CalculateTimeBackward(sortedActivities);
                //Console.Write("EET last is {0}", sortedActivities[sortedActivities.Count - 1].EET);
                foreach (Activity act in sortedActivities)
                {
                    if ((act.Name == "[FIRST]") || (act.Name == "[LAST]"))
                    {
                        act.CU = double.MaxValue;
                        continue;
                    }

                    act.CU = (act.maxCost - act.Cost)/(act.Duration - act.minDuration);
                    //Console.WriteLine("{0} cu is {1}", act.Name, act.CU);

                }

                Activity ActivityWithMinCU = findActivityWithMinCU();
                if ((ActivityWithMinCU.Duration > ActivityWithMinCU.minDuration) || (ActivityWithMinCU.Cost < ActivityWithMinCU.maxCost))
                {
                    ActivityWithMinCU.Duration--;
                    ActivityWithMinCU.Cost += ActivityWithMinCU.CU; // here? previous CU vs new Duration
                    //Console.WriteLine("MinCU {0} {1}", ActivityWithMinCU.Name, ActivityWithMinCU.CU);
                }
                else
                {
                    cuActivities.Remove(ActivityWithMinCU);
                }

                //Console.ReadKey();
            }

            Console.WriteLine("Ready");
            toConsole();
        }

        public void setTimesToZero()
        {
            foreach (Activity act in sortedActivities)
            {
                act.EST = 0;
                act.EET = 0;
                act.LST = 0;
                act.LET = 0;
            }
        }

        public Activity findActivityWithMinCU()
        {
            Activity ActivityWithMinCU = cuActivities[0];
            foreach (Activity act in cuActivities)
            {
                if (ActivityWithMinCU.CU > act.CU)
                {
                    ActivityWithMinCU = act;
                }
            }
            return ActivityWithMinCU;
        }

        public void countDurationsWithCosts(bool maxDuration = true)
        {
            foreach (Activity act in sortedActivities)
            {
                act.Duration = maxDuration ? act.maxDuration : act.minDuration;
                act.Cost = maxDuration ? act.minCost : act.maxCost;
            }
        }

        public void toConsole()
        {
            calculateCosts();
            Console.WriteLine();
            Console.WriteLine("======================");
            Console.WriteLine("Project data:");
            foreach (Activity act in sortedActivities)
            {
                Console.WriteLine("Name: {0}", act.Name);
                Console.WriteLine("Duration: {0}\tEST: {1}\tEET: {2}\tLST: {3}\tLET: {4}\t Cost: {5}", act.Duration, act.EST, act.EET, act.LST, act.LET, act.Cost);
            }
            Console.WriteLine("======================");
            Console.WriteLine("Total duration of the project is: {0}", sortedActivities[sortedActivities.Count - 1].EET);
            Console.WriteLine("Total cost of the project is: {0}", projectCost);
        }

        public double calculateCosts()
        {
            projectCost = 0;
            foreach (Activity act in sortedActivities)
            {
                projectCost += act.Cost;
            }
            return projectCost;
        }
        // -----------------------------------------------------------------------------------
        public List<Activity> CopyActivities(List<Activity> acts)
        {
            List<Activity> actsMimic = new List<Activity>();
            foreach (Activity a in acts)
            {
                actsMimic.Add(a);
            }
            return actsMimic;
        }


        public Activity AddActivity(string name, double minDuration = 0, double maxDuration = 0, double minCost = 0, double maxCost = 0)
        {
            Activity act = new Activity();
            act.Name = name;

            act.maxDuration = maxDuration;
            act.minDuration = minDuration;
            act.minCost = minCost;
            act.maxCost = maxCost;

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
            cuActivities.Add(x);
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
            cuActivities.Clear();

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
            crit.Clear();
            CalculateTimeForward(toposorted);
            CalculateTimeBackward(toposorted);

            foreach (Activity act in toposorted)
                if ((act.EET == act.LET) && (act.EST == act.LST))
                {
                    Console.WriteLine("{0} is critpoint", act.Name);
                    crit.Add(act);
                }
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
        public double ACI;
        public double maxDuration;
        public double minDuration;
        public double minCost;
        public double maxCost;
        public double CU;
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

