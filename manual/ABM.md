[Back](/manual/Framework.md)
# ABM is the class of the Agent-based Modelling in BSim, it assembles the entire data system.
**The ABM world's structure:**
<br>

```C#
    public class ABM
    {
        public string debug;//Debug Report;

        //Global Systems;
        public PhysicalSystem_K2 PS;
        public int Iteration;

        //The main four components;
        public ISpace Space;
        public List<Event> POIs;
        public List<Person> PPs;
        public List<Communicate> Communis;

        //The the person in metabolism;
        public List<Person> PPs_Wait;
        public List<Person> PPs_Sink;

        //Documentation;
        public HashSet<Person_Doc> Document;

        //Construction Function
        public ABM (ISpace Space, List<Event> POIs, List<Person> PPs, List<Communicate> Communis){...}

        //the function activate the metabolism;
        public void PPs_Emerge(){...}
        public void PPs_Expire(){...}
    }
```
<br>

**The decision and interaction routine:**
```C#
  //***This function defines the interactive mechanism inside the ABM simulation: (as a function in ABM class)***
        public List<IGoal> InteractSystem(out string TimeRep)
        {
            MiniWatch MWatch = new MiniWatch();

            List<IGoal> G = new List<IGoal>();

            //01_Every agent making decisions;_____________________________________________________
            foreach (Person P in PPs){ G.AddRange(P.Act(this)); }
            MWatch.Record("Decisions");

            //02_Every agent interact with Events;_____________________________________________________
            int PE_intertime = 0;
            int G_event = 0;
            RTree T_pp = new RTree();//Build the Rtree for the current Agents' locations;
            for (int i = 0; i < PPs.Count; i++) { T_pp.Insert(PPs[i].Locate, i); }

            for (int i = 0; i < POIs.Count; i++)
            {
                Event E = POIs[i];
                if (E.locate == null || E.R <= 0.0)//If the event does not have a location; (It is a global event that every agent should consider)
                {
                    G_event++;
                    foreach (Person P in PPs)
                    { if (E.Activate(P, this)) { E.Affect(P, this); }; PE_intertime++; }
                    continue; //Get to the next iteration;
                }

                Sphere SearchSp = new Sphere(E.locate, E.R);//If there being location attribute, Use Rtree to focus the events;
                List<int> inds = new List<int>();
                T_pp.Search(SearchSp, (sender, args) => { inds.Add(args.Id); });
                foreach (int n in inds)
                {
                    if (E.Activate(PPs[n], this)) { E.Affect(PPs[n], this); }
                    PE_intertime++;
                }
            }

            MWatch.Record(string.Format("Person|Pois: [{0}] Global {1}",PE_intertime.ToString(),G_event.ToString()));

            //03_Every person interact with other person;_____________________________________________________
            int PP_intertime = 0;
            double Rc = Config.SocialDistance;
            for (int i = 0; i < PPs.Count - 1; i++)
            {
                Sphere SearchSp = new Sphere(PPs[i].Locate, Rc);
                List<int> inds = new List<int>();
                T_pp.Search(SearchSp, (sender, args) => { inds.Add(args.Id); }); 
                foreach (int n in inds)
                {
                    if (n <= i) { continue; }
                    foreach (Communicate ICom in Communis){G.AddRange(ICom.Interact(this, PPs[i], PPs[n]));}
                    PP_intertime++;
                }
            }

            MWatch.Record(string.Format("Person|Person: [{0}, R={1} m] ", PP_intertime.ToString(), Rc.ToString()));

            //04_Making person move and output the person class in the Physical system;
            foreach (Person P in PPs) { G.AddRange(P.DoBehav(this, P.IMove)); }

            TimeRep = MWatch.GetTime();
            return G;
        }
```
<br>

**The main iteration in ABM Simulation:**
```C#
   //***ABM Simulation (as a function in ABM class)***
        public string Simulate(bool Reset,out string TimeRep)
        {
            string Log = Main.version+"_Simulate Report:\n";

            //Emerge and sink persons;
            PPs_Emerge();
            PPs_Expire();

            //Timer
            MiniWatch MWatch = new MiniWatch();

            //Run Interact
            List<IGoal> G = InteractSystem(out string Interact_report);
            Log += Interact_report;
            MWatch.Record("Run Interact");

            //Run Physical
            PS.pip(Reset, G, 1);
            Iteration = PS.iteration;
            MWatch.Record("Run Physical");

            //Document
            foreach (Person P in PPs){P.DocMemory(ref Document);}

            MWatch.Record("Run Document");

            TimeRep =MWatch.GetTime();

            return Log;
        }
```
