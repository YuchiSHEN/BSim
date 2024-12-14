[Back](/manual/Event.md)
<br>
<br>
Goto:
<br>
[IEvent_M_vS_aS_St](/manual/_IEvent.md#ievent_m_vs_as_st)
<br>
[IEvent_M_vS_aM_St](/manual/_IEvent.md#ievent_m_vs_am_st)
<br>
[IEvent_M_vS_aS_St_Q](/manual/_IEvent.md#ievent_m_vs_as_st_q)
<br>
<br>

## IEvent_M_vS_aS_St: 
__The event can be used by [M: multiple agents], visited in [vS: single time], activated by [aS: single agent], Standing [St]__
```C#
       public class IEvent_M_vS_aS_St : Event
    {
        double activate_dis = 0.5;
        int timeCons = 50;
        List<Point3d> activate_pts;
        List<string> activate_pts_ocu;
        List<bool> activate_pts_exp;

        public IEvent_M_vS_aS_St(string ID, Point3d locate, int TimeConseume, double act_dis, List<Point3d> act_pts, string Etype)
        {
            this.Type = new EventType(Etype);
            this.ID = ID;
            this.locate = locate;
            this.users = new List<Person>();
            this.beTarget = new List<Person>();
            activate_dis = act_dis;
            this.R = act_dis * 10;
            activate_pts = act_pts;
            activate_pts_ocu = act_pts.Select(p => "null").ToList();
            activate_pts_exp = act_pts.Select(p => false).ToList();
            timeCons = TimeConseume;
        }

        public override void Affect(Person P, ABM ABM)
        {
            if (P.State != Person_state.Busy)
            {
                P.State = Person_state.Busy;

                if (!this.users.Contains(P) && IfDidBefore(P).Count == 0)
                {
                    this.users.Add(P);
                    P.Brain.Add(new Memory(this, timeCons));
                }

                P.Path = new List<Point3d>();
                P.step = 0;
            }
        }

        public override bool Activate(Person P, ABM ABM)
        {
            bool Did = false;

            //if Did
            List<Memory> RelatedMemors = IfDidBefore(P);

            if (RelatedMemors.Count > 0)
            {
                if (RelatedMemors[0].Step == RelatedMemors[0].MaxStep)
                { Did = true; }
            }

            int k = NameInOccupancy(P);

            bool reach = false;

            if (k != -1)
            {
                reach = activate_pts[k].DistanceTo(P.Locate) < activate_dis;
            }

            //Not visited, Distance close, Fullfill the plan;
            if (!Did && reach && IsType(P.NextToDo) && P.Target.ID == this.ID)
            {
                return true;
            }
            else
            { return false; }
        }

        public override void Approach(Person P, ABM EN)
        {
            int k = NameInOccupancy(P);
            if (k != -1)
            {
                P.Path = PathNet.ShortestPath_Quick(EN.Space.Quick_WalkSpace, P.Locate, activate_pts[k], Config.SearchRange, Config.SoomthRate);
                P.step = 0;
                P.State = Person_state.Walk;
            }

            if (k == -1)
            {
                List<int> via_act_pts = Viable_act_pts();
                if (via_act_pts.Count > 0)
                {
                    activate_pts_ocu[via_act_pts[0]] = P.Name;
                    P.Path = PathNet.ShortestPath_Quick(EN.Space.Quick_WalkSpace, P.Locate, activate_pts[via_act_pts[0]], Config.SearchRange, Config.SoomthRate);
                    P.step = 0;
                    P.State = Person_state.Walk;
                }
            }
        }

        public override bool CanProceed(Person P, ABM EN)
        {
            int k = NameInOccupancy(P);
            if (k == -1 || P.Locate.DistanceTo(activate_pts[k]) >= activate_dis)
            { return false; }
            return true;
        }

        private List<Memory> IfDidBefore(Person P)
        {
            List<Memory> RelatedMemors = P.Brain.Where(p => p.Event.ID == this.ID).ToList();
            return RelatedMemors;
        }

        private List<int> Viable_act_pts()
        {
            List<int> viables = new List<int>();
            for (int i = 0; i < activate_pts.Count; i++)
            {
                if (activate_pts_ocu[i] == "null") { viables.Add(i); }
            }
            return viables;
        }

        private int NameInOccupancy(Person P)
        {
            int k = -1;
            for (int i = 0; i < activate_pts_ocu.Count; i++)
            {
                if (activate_pts_ocu[i] == P.Name)
                {
                    k = i;
                }
            }
            return k;
        }

        public override bool CanBeVisit(Person P, ABM ABM)
        {
            //
            bool Did = false;

            //if Did
            List<Memory> RelatedMemors = IfDidBefore(P);

            if (RelatedMemors.Count > 0)
            {
                if (RelatedMemors[0].Step == RelatedMemors[0].MaxStep)
                { Did = true; }
            }

            if (Did) { return false; }
            if (this.beTarget.Contains(P)) { return true; }
            if (this.beTarget.Count >= activate_pts.Count) { return false; }
            return true;
        }

        public override void Expire(Person P)
        {
            int k = NameInOccupancy(P);
            if (k == -1) { return; }

            if (users.Contains(P)) { users.Remove(P); }
            if (beTarget.Contains(P)) { beTarget.Remove(P); }

            activate_pts_ocu[k] = "null";
            activate_pts_exp[k] = false;
        }
    }
```

## IEvent_M_vS_aM_St: 
__The event can be used by [M: multiple agents], visited in [vS: single time], activated by [aM: multiple agents], Standing [St]__
```C#
    public class IEvent_M_vS_aM_St : Event
    {
        double activate_dis = 0.5;
        int timeCons = 50;
        List<Point3d> activate_pts;
        List<string> activate_pts_ocu;
        List<bool> activate_pts_exp;


        public IEvent_M_vS_aM_St(string ID, Point3d locate, int TimeConseume, double act_dis, List<Point3d> act_pts, string Etype)
        {
            this.Type =new EventType(Etype);
            this.ID = ID;
            this.locate = locate;
            this.users = new List<Person>();
            this.beTarget = new List<Person>();
            activate_dis = act_dis;
            this.R = act_dis * 10;
            activate_pts = act_pts;
            activate_pts_ocu = act_pts.Select(p => "null").ToList();
            activate_pts_exp = act_pts.Select(p => false).ToList();
            timeCons = TimeConseume;
        }

        public override void Affect(Person P, ABM ABM)
        {
            if (P.State != Person_state.Busy)
            {
                P.State = Person_state.Busy;

                if (!this.users.Contains(P) && IfDidBefore(P).Count == 0)
                {
                    this.users.Add(P);
                    P.Brain.Add(new Memory(this, timeCons));
                }

                P.Path = new List<Point3d>();
                P.step = 0;
            }
        }

        public override bool Activate(Person P, ABM ABM)
        {
            bool Did = false;

            //if Did
            List<Memory> RelatedMemors = IfDidBefore(P);

            if (RelatedMemors.Count > 0)
            {
                if (RelatedMemors[0].Step == RelatedMemors[0].MaxStep)
                { Did = true; }
            }

            int k = NameInOccupancy(P);

            bool reach = false;

            if (k != -1)
            {
                reach = activate_pts[k].DistanceTo(P.Locate) < activate_dis;
            }

            //Not visited, Distance close, Fullfill the plan;
            if (!Did && reach && IsType(P.NextToDo) && P.Target.ID == this.ID)
            {
                //activate_pts_ocu[sel] = P.Name;
                return true;
            }
            else
            { return false; }
        }

        public override void Approach(Person P, ABM EN)
        {
            int k = NameInOccupancy(P);
            if (k != -1)
            {
                P.Path = PathNet.ShortestPath_Quick(EN.Space.Quick_WalkSpace, P.Locate, activate_pts[k], Config.SearchRange, Config.SoomthRate);
                P.step = 0;
                P.State = Person_state.Walk;
            }

            if (k == -1)
            {
                List<int> via_act_pts = Viable_act_pts();
                if (via_act_pts.Count > 0)
                {
                    activate_pts_ocu[via_act_pts[0]] = P.Name;
                    P.Path = PathNet.ShortestPath_Quick(EN.Space.Quick_WalkSpace, P.Locate, activate_pts[via_act_pts[0]], Config.SearchRange, Config.SoomthRate);
                    P.step = 0;
                    P.State = Person_state.Walk;
                }
            }
        }

        public override bool CanProceed(Person P, ABM EN)
        {
            List<string> Nulls = activate_pts_ocu.Where(p => p == "null").ToList();
            if (Nulls.Count > 0) { return false; }

            if (this.users.Count < activate_pts.Count) { return false; }
            foreach (Person p in this.users)
            {
                int k = NameInOccupancy(p);
                if (k == -1)
                { return false; }
                if (p.Locate.DistanceTo(activate_pts[k]) >= activate_dis)
                { return false; }
            }

            return true;
        }

        private List<Memory> IfDidBefore(Person P)
        {
            List<Memory> RelatedMemors = P.Brain.Where(p => p.Event.ID == this.ID).ToList();
            return RelatedMemors;
        }

        private List<int> Viable_act_pts()
        {
            List<int> viables = new List<int>();
            for (int i = 0; i < activate_pts.Count; i++)
            {
                if (activate_pts_ocu[i] == "null") { viables.Add(i); }
            }
            return viables;
        }

        private int NameInOccupancy(Person P)
        {
            int k = -1;
            for (int i = 0; i < activate_pts_ocu.Count; i++)
            {
                if (activate_pts_ocu[i] == P.Name)
                {
                    k = i;
                }
            }
            return k;
        }

        public override bool CanBeVisit(Person P, ABM ABM)
        {
            //This event can be visited only once and by one person;
            //
            bool Did = false;

            //if Did
            List<Memory> RelatedMemors = IfDidBefore(P);

            if (RelatedMemors.Count > 0)
            {
                if (RelatedMemors[0].Step == RelatedMemors[0].MaxStep)
                { Did = true; }
            }

            if (Did) { return false; }

            if (this.beTarget.Contains(P)) { return true; }
            if (this.beTarget.Count >= activate_pts.Count) { return false; }
            return true;
        }

        public override void Expire(Person P)
        {
            int k = NameInOccupancy(P);
            if (k == -1) { return; }
            List<bool> NotFinished = activate_pts_exp.Where(p => p == false).ToList();
            if (NotFinished.Count == 1 && activate_pts_exp[k] == false)
            {
                users = new List<Person>();
                beTarget = new List<Person>();
                activate_pts_ocu = activate_pts.Select(p => "null").ToList();
                activate_pts_exp = activate_pts.Select(p => false).ToList();
            }
            else
            {
                activate_pts_exp[k] = true;
            }
        }
    }
```

## IEvent_M_vS_aS_St_Q: 
__The event can be used by [M: multiple agents], visited in [vS: single time], activated by [aS: single agent], Standing [St], and form a *queue*__
```C#
    public class IEvent_M_vS_aS_St_Q : Event
    {
        double activate_dis = 0.5;
        int timeCons = 50;
        List<Point3d> queue_pts;
        List<string> queue_ocu_ids;

        //A event that provides a queue for users;
        public IEvent_M_vS_aS_St_Q(string ID, Point3d locate, int TimeConseume, double act_dis, List<Point3d> queue_pts, string Etype)
        {
            this.Type = new EventType(Etype);
            this.ID = ID;
            this.locate = locate;
            this.users = new List<Person>();
            this.beTarget = new List<Person>();
            activate_dis = act_dis;
            this.R = 100;
            this.queue_pts = queue_pts;
            queue_ocu_ids = queue_pts.Select(p => "null").ToList();
            timeCons = TimeConseume;
        }

        public override void Affect(Person P, ABM ABM)
        {
            if (P.State != Person_state.Busy)
            {
                P.State = Person_state.Busy;

                if (!this.users.Contains(P) && IfDidBefore(P).Count == 0)
                {
                    this.users.Add(P);
                    P.Brain.Add(new Memory(this, timeCons));
                }

                P.Path = new List<Point3d>();
                P.step = 0;

            }
        }

        public override bool Activate(Person P, ABM ABM)
        {

            bool Did = false;
            List<Memory> IfDid = IfDidBefore(P);
            if (IfDid.Count > 0)
            {
                Did = IfDid.Last().Step == IfDid.Last().MaxStep;
            };

            int k = NameInOccupancy(P);

            bool reach = false;

            if (k != -1)
            {
                reach = queue_pts[k].DistanceTo(P.Locate) < activate_dis;
            }

            //Not visited, Distance close, Fullfill the plan;
            if (!Did && reach && IsType(P.NextToDo) && P.Target.ID == this.ID)
            {
                return true;
            }
            else if (!Did && !reach && IsType(P.NextToDo) && P.Target.ID == this.ID && P.State != Person_state.Busy)
            {
                Approach(P, ABM);
                return false;
            }
            else
            {
                return false;
            }
        }

        public override void Approach(Person P, ABM EN)
        {
            int k = NameInOccupancy(P);
            if (k != -1)
            {
                P.Path = PathNet.ShortestPath_Quick(EN.Space.Quick_WalkSpace, P.Locate, queue_pts[k], Config.SearchRange, Config.SoomthRate);
                P.step = 0;
                P.State = Person_state.Walk;
            }

            if (k == -1)
            {
                List<int> via_act_pts = Viable_act_pts();
                if (via_act_pts.Count > 0)
                {
                    queue_ocu_ids[via_act_pts[0]] = P.Name;
                    P.Path = PathNet.ShortestPath_Quick(EN.Space.Quick_WalkSpace, P.Locate, queue_pts[via_act_pts[0]], Config.SearchRange, Config.SoomthRate);
                    P.step = 0;
                    P.State = Person_state.Walk;
                }
            }
        }

        public override bool CanProceed(Person P, ABM EN)
        {
            int k = NameInOccupancy(P);
            if (k == -1 || P.Locate.DistanceTo(queue_pts[k]) >= activate_dis)
            {
                return false;
            }
            else if (k == 0)//can only work while the agent is in the first of the queue;
            {
                return true;
            }
            else
            {
                if (queue_ocu_ids[k - 1] == "null")
                { queue_ocu_ids.RemoveAt(k - 1); queue_ocu_ids.Add("null"); }
            }
            return false;
        }

        private List<Memory> IfDidBefore(Person P)
        {
            List<Memory> RelatedMemors = P.Brain.Where(p => p.Event.ID == this.ID).ToList();
            return RelatedMemors;
        }

        private List<int> Viable_act_pts()
        {
            List<int> viables = new List<int>();
            for (int i = 0; i < queue_pts.Count; i++)
            {
                if (queue_ocu_ids[i] == "null") { viables.Add(i); }
            }
            return viables;
        }

        private int NameInOccupancy(Person P)
        {
            int k = -1;
            for (int i = 0; i < queue_ocu_ids.Count; i++)
            {
                if (queue_ocu_ids[i] == P.Name)
                {
                    k = i;
                }
            }
            return k;
        }

        public override bool CanBeVisit(Person P, ABM ABM)
        {
            //
            bool Did = false;

            //if Did
            List<Memory> RelatedMemors = IfDidBefore(P);

            if (RelatedMemors.Count > 0)
            {
                if (RelatedMemors[0].Step == RelatedMemors[0].MaxStep)
                { Did = true; }
            }

            if (Did) { return false; }
            if (this.beTarget.Contains(P)) { return true; }
            if (this.beTarget.Count >= queue_pts.Count) { return false; }
            return true;
        }

        public override void Expire(Person P)
        {
            int k = NameInOccupancy(P);
            if (k == -1) { return; }

            if (k == 0)
            {
                queue_ocu_ids.RemoveAt(0);
                queue_ocu_ids.Add("null");
            }
            if (users.Contains(P)) { users.Remove(P); }
            if (beTarget.Contains(P)) { beTarget.Remove(P); }
        }
    }
```
