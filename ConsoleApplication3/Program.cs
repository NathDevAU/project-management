using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MyApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Project prj = new Project();
            // mock graph
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
            Project project = ComposeProjectFromFile("mock");
            foreach (Relation rel in project.Relations)
            {
                Console.WriteLine("{0}->{1}", rel.Predecessor.Name, rel.Successor.Name);
            }
            CPU_Count(prj);
            Console.ReadKey();
        }
        static public Project ComposeProjectFromFile(string path)
        {
            string filePart = "";
            int relLineCount = 0;
            Project project = new Project();
            string[] values;
            try
            {
                using (StreamReader sr = new StreamReader("../../" + path+".csv"))
                {
                    String file = sr.ReadToEnd();
                    String[] lines = file.Split('\n');
                    Hashtable activities = new Hashtable();
                    foreach (string line in lines)
                    {
                        values = line.Split(',');
                        if (line.Contains("ACTIVITIES"))
                        {
                            filePart = "acts";
                            Console.WriteLine("read acts");
                        }
                        else if (line.Contains("RELATIONS"))
                        {
                            Console.WriteLine("read rels");
                            filePart = "rels";
                            relLineCount = 0;
                        }
                        if (line.Contains(",,,,") || line.Replace(' ', '\r').Length == 0)
                        {
                            continue;
                        }
                        else if (filePart == "acts" && relLineCount >= 2)
                        {
                            activities[values[0]] = project.AddActivity(values[0], Convert.ToDouble(values[1]), Convert.ToDouble(values[2]), Convert.ToDouble(values[3]));
                        }
                        else if (filePart == "rels" && relLineCount >= 1)
                        {
                            project.AddRelation((Activity)activities[values[0]], (Activity)activities[values[1]]);
                        }
                        relLineCount++;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
            return project;
        }
        static public void CPU_Count(Project project)
        {
            // result - determines the duration of the project and writes it in Console

            // first - count with min duration and max costs
            // second - with max duration and min costs

            bool minDuration = true;
            string minmaxDuration = "";
            string minmaxCosts = "";
            double limDuration = 0;
            string yno = "";
            List<List<Activity>> toposorted = new List<List<Activity>>();

            for (int i = 0; i < 2; i++)
            {
                project = setDurationAndCosts(project, minDuration);
                toposorted.Add(project.TopoSort(project.Activities));
                project.CalculateTimeForward(toposorted[i]);
                double costs = project.sumCosts(toposorted[i]);
                limDuration = minDuration ? toposorted[i][toposorted[i].Count - 1].EET : limDuration;

                minmaxDuration = minDuration ? "minimally" : "maximally";
                minmaxCosts = minDuration ? "maximal" : "minimal";

                Console.WriteLine("The {0} possible duration of the project with {1} costs is: {2} days and {3} dollars", minmaxDuration, minmaxCosts, toposorted[i][toposorted[i].Count - 1].EET, costs);

                minDuration = false;
            }

            // toposorted[0] is project with calculated minDuration and maxCosts
            // toposorted[1] is project with calculated maxDuration and minCosts

            double duration = toposorted[1][toposorted[1].Count - 1].EET;  // EET of the last vertex is the project's duration
            List<Activity> optimisedActivities = toposorted[1];

            while (true)
            {
                // asks whether the maxDuration with minCosts is acceptable or not for the current project
                // the program will repeat asking input, until the input is Yes or No

                Console.WriteLine("Are {0} days acceptable for the project? Enter Y/YES or N/No...", duration);
                yno = Console.ReadLine();

                if ((yno == "Y") || (yno == "Yes") || (yno == "y") || (yno == "yes"))
                {
                    // if yes then write the desired duration in Console
                    Console.WriteLine("=====Total result:=====");
                    Console.WriteLine("Total duration of the project: {0}", duration);
                    toConsole(optimisedActivities);
                    break;
                }
                else if ((yno == "N") || (yno == "No") || (yno == "n") || (yno == "no"))
                {
                    while (true)
                    {

                        //if the input is no - ask to enter the required duration
                        Console.WriteLine("Please enter a required duration: ");
                        double reqDuration = double.Parse(Console.ReadLine());

                        if (reqDuration < limDuration)
                        {
                            // if required duration is less than minDuration (already calculated), then repeat asking
                            Console.WriteLine("The duration cannot be less than: {0}", limDuration);
                        }
                        else
                        {
                            // if required duration is more than minDuration
                            project = optimizeProjectDuration(project, reqDuration); // try to change the total Duration
                            optimisedActivities = project.sortedActivities;
                            duration = optimisedActivities[optimisedActivities.Count - 1].EET;
                            // write the result in Console
                            break;
                        }
                    }
                }
            }


        }

        static private Project setDurationAndCosts(Project project, bool minDuration = true)
        {
            // returns a project with calculated duration and costs of each work

            // sets Duration to each activity
            // if mininal duration is required, than Duration equals minDuration and Costs equals maxCosts
            // if maximal Duration - vice versa
            foreach (Activity act in project.Activities)
            {
                act.Duration = minDuration ? act.DurationMin : act.DurationMax;
                act.Cost = minDuration ? act.CostMax : act.CostMin;
            }
            return project;
        }

        static public Project optimizeProjectDuration(Project project, double reqDuration)
        {
            // project - a Project
            // reqDuration - required duration set by user
            // returns a project with changed Duration of critical Activity (Point) with minimal CU and, as a consequence, with changed Duration of the whole project

            // due to EST & EET is alreay calculated in toposorted[1] it is required only to calculate LST and LET
            project.TopoSort(project.Activities);
            //project.CalculateTimeBackward(project.sortedActivities); // calculate LET & LST
            //project.determineCritPoints(project.sortedActivities); // all critical points - not critical path!
            //project.determineCU(); // calculate all CU for all critical Activity and determines an Activity with minimal CU (project.actMinCU)
            project = optimize(project);

            /*
            int d = project.sortedActivities.IndexOf(project.actMinCU); //  determines the index of an Activity with minCU -- needs to be deleted

            if (d > 0) // delete line
            {

                while (true)
                {
                    // stops when the solution is found or the duration of actMinCU (critical Activity with minimal CU) is less than its minimal Duration
                    if (project.actMinCU.Duration < project.actMinCU.DurationMin)
                    {
                        Console.WriteLine("The required duration {0} is unreachable. The minimally possible duration is {1}", reqDuration, project.sortedActivities[project.sortedActivities.Count - 1].EET + 1);
                        project.actMinCU.Duration = project.actMinCU.DurationMin; // set the actMinCU to the previous minimal Duration in order to start over
                        project = optimize(project); // see the comments on optimize()
                        continue;
                    }
                    else
                    {
                        if (project.sortedActivities[project.sortedActivities.Count - 1].EET <= reqDuration)
                        {
                            // if the duration of a project is less than or equals a required duration, than the solution is found
                            return project;
                        }
                        project.actMinCU.DurationMax -= 1; // subtract 1 unit of actMinCU's Duration 
                        project.actMinCU.Cost += project.actMinCU.CU; // actMinCu's Cost increases as its duration decreases. CU - is derivative - the speed of increasing
                        project = optimize(project);
                    }
                }



            }
            else
                Console.WriteLine("impossible");
            */
            while (true)
            {
                if (project.sortedActivities[project.sortedActivities.Count - 1].EET <= reqDuration)
                    break;
                if ((project.actMinCU.Duration > project.actMinCU.DurationMin) && (project.actMinCU.Cost < project.actMinCU.CostMax))
                {
                    if ((project.actMinCU.Duration != project.actMinCU.DurationMin) && (project.actMinCU.Cost != project.actMinCU.CostMax)){
                        project.actMinCU.Duration -= 1;
                        project.actMinCU.Cost += project.actMinCU.CU;
                    }
                    project = optimize(project);
                }
            }

            return project;
        }

        static public Project optimize(Project project)
        {
            //returns a project with calculated EST, EET, LST, LET of each activity

            project.setTimesToZero(project.sortedActivities);
            project.CalculateTimeForward(project.sortedActivities);
            project.CalculateTimeBackward(project.sortedActivities);

            project.determineCritPoints(project.sortedActivities);
            project.determineCU(); // calculate all CU for all critical Activity and determines an Activity with minimal CU (project.actMinCU)
            return project;
        }




        static public void toConsole(List<Activity> activities)
        {
            foreach (Activity act in activities)
            {
                Console.WriteLine("Name: {0}; Duration: {1}; Cost: {2}; EST: {3}; EET: {4}; LST: {5}; LET: {6}", act.Name, act.Duration, act.Cost, act.EST, act.EET, act.LST, act.LET);
            }
        }
    } // -----------------------------------------------------------------

    class Project
    {
        public List<Activity> Activities = new List<Activity>();
        public List<Relation> Relations = new List<Relation>();
        public List<Activity> sortedActivities = new List<Activity>();
        public List<Activity> critPoints = new List<Activity>();
        public Activity actMinCU = new Activity();


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

        public void setTimesToZero(List<Activity> toposorted)
        {
            // sets all EET, EST, LET, LST to zero
            foreach (Activity act in toposorted)
            {
                act.EET = act.EST = act.LET = act.LST = 0;
            }
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

        public List<Activity> determineCritPoints(List<Activity> toposorted)
        {
            //CalculateTimeForward(toposorted);
            //CalculateTimeBackward(toposorted);
            foreach (Activity act in toposorted)
                if ((act.EET == act.LET) && (act.EST == act.LST))
                {
                    critPoints.Add(act);
                }

            return critPoints;
        }

        public void determineCU()
        {
            // calculcates CU for all critical Activities of a project
            // determines an Activity with minimal CU
            double minCU = double.MaxValue;

            foreach (Activity crit in critPoints)
            {
                if ((crit.Predecessors.Count == 0) || (crit.Successors.Count == 0))
                {
                    crit.CU = double.MaxValue;
                    continue;
                }
                else if (crit.DurationMax <= crit.DurationMin)
                {
                    Console.WriteLine("CU  0 when " + crit.Name);
                    crit.CU = 0;
                }
                else
                {
                    crit.CU = (crit.CostMax - crit.Cost) / (crit.Duration - crit.DurationMin);
                }

                if (minCU > crit.CU)
                {
                    minCU = crit.CU;
                    actMinCU = crit;
                }
            }
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
        public double CU;

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
