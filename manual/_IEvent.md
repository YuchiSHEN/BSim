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

        public IEvent_M_vS_aS_St(string ID, Point3d locate, int TimeConseume, double act_dis, List<Point3d> act_pts, string Etype, Func<List<Event>, Agent, ABM, List<Event>> SelFunc, Object Range, double ra,int poss)
        {
            Default_Construct(ID, Etype, SelFunc, locate, TimeConseume, act_pts, SpaceSearch.ToRTRange(Range, ra), ra, poss);
            activate_dis = act_dis;
        }

        public override void Affect(Agent P, ABM ABM)
        {
            if (P.temp.State != Agent_state.Busy)
            {
                P.temp.State = Agent_state.Busy;
                Register_User(P);
                P.temp.ClearPath();
            }
        }

        public override bool Activate(Agent P, ABM ABM)
        {
            bool[] condition = Default_ActivateConditions(P, ABM);
            if (!condition[0]) { return false; }
            if (!condition[1]) { return false; }
            if (!condition[2]) { return false; }
            if (!condition[3]) { return false; }

            int k = IndexInQueue(P);
            if (k > -1) { return sub_locates[k].DistanceTo(P.attr.Locate) < activate_dis; }

            return false;
        }

        public override void Approach(Agent P, ABM EN)
        {
            int k = IndexInQueue(P);
            if (k > -1)
            {
                P.temp.Path = PathNet.PathSearch(EN.Space.GraphMap, P.attr.Locate, sub_locates[k], Config.SearchRange, Config.SoomthRate);
                P.temp.step = 0;
                P.temp.State = Agent_state.Walk;
                return;
            }

            int index= InertQueue_Random(P,EN.Iteration);
            if (index > -1)
            {
                P.temp.Path = PathNet.PathSearch(EN.Space.GraphMap, P.attr.Locate, sub_locates[index], Config.SearchRange, Config.SoomthRate);
                P.temp.step = 0;
                P.temp.State = Agent_state.Walk;
            }
        }

        public override bool CanProceed(Agent P, ABM EN)
        {
            int k = IndexInQueue(P);
            if (k == -1 || P.attr.Locate.DistanceTo(sub_locates[k]) >= activate_dis)
            { return false; }
            return true;
        }

        public override bool CanBeVisit(Agent P, ABM ABM)
        {
            if (PreviouslyCompeleted(P)) { return false; }
            return true;
        }
    }
```

## IEvent_M_vS_aM_St: 
__The event can be used by [M: multiple agents], visited in [vS: single time], activated by [aM: multiple agents], Standing [St]__
```C#
    public class IEvent_M_vS_aM_St : Event
    {
        double activate_dis = 0.5;

        public IEvent_M_vS_aM_St(string ID, Point3d locate, int TimeConseume, double act_dis, List<Point3d> act_pts, string Etype, Func<List<Event>, Agent, ABM, List<Event>> SelFunc, Object Range, double ra, int poss)
        {
            Default_Construct(ID, Etype, SelFunc, locate, TimeConseume, act_pts, SpaceSearch.ToRTRange(Range, ra), ra, poss);
            activate_dis = act_dis;
        }

        public override void Affect(Agent P, ABM ABM)
        {
            if (P.temp.State != Agent_state.Busy)
            {
                P.temp.State = Agent_state.Busy;

                Register_User(P);

                P.temp.ClearPath();
            }
        }

        public override bool Activate(Agent P, ABM ABM)
        {
            bool[] condition = Default_ActivateConditions(P, ABM);
            if (!condition[0]) { return false; }
            if (!condition[1]) { return false; }
            if (!condition[2]) { return false; }
            if (!condition[3]) { return false; }

            int k = IndexInQueue(P);
            if (k > -1) { return sub_locates[k].DistanceTo(P.attr.Locate) < activate_dis; }

            return false;
        }

        public override void Approach(Agent P, ABM EN)
        {
            int k = IndexInQueue(P);
            if (k > -1)
            {
                P.temp.Path = PathNet.PathSearch(EN.Space.GraphMap, P.attr.Locate, sub_locates[k], Config.SearchRange, Config.SoomthRate);
                P.temp.step = 0;
                P.temp.State = Agent_state.Walk;
                return;
            }

            int index = InertQueue_Random(P, EN.Iteration);
            if (index > -1)
            {
                P.temp.Path = PathNet.PathSearch(EN.Space.GraphMap, P.attr.Locate, sub_locates[index], Config.SearchRange, Config.SoomthRate);
                P.temp.step = 0;
                P.temp.State = Agent_state.Walk;
            }
        }

        public override bool CanProceed(Agent P, ABM EN)
        {
            if (AvailIndex_InQueue().Count > 0) { return false; }
            if (users.Count < sub_locates.Count) { return false; }
            foreach (Agent p in users)
            {
                int k = IndexInQueue(p);
                if (k == -1)
                { return false; }
                if (p.attr.Locate.DistanceTo(sub_locates[k]) >= activate_dis)
                { return false; }
            }

            return true;
        }

        public override bool CanBeVisit(Agent P, ABM ABM)
        {
            if(PreviouslyCompeleted(P))return false;
            return true;
        }

        public override void Expire(Agent P)
        {
            foreach (Agent user in users) 
            {
                base.Expire(user);
            }
            Reset();
        }
    }
```

## IEvent_M_vS_aS_St_Q: 
__The event can be used by [M: multiple agents], visited in [vS: single time], activated by [aS: single agent], Standing [St], and form a *queue*__
```C#
       public class IEvent_M_vS_aS_St_Q : Event
    {
        string log = "";
        double activate_dis = 0.5;

        //A event that provides a queue for users;
        public IEvent_M_vS_aS_St_Q(string ID, Point3d locate, int TimeConseume, double act_dis, List<Point3d> queue_pts, string Etype, Func<List<Event>, Agent, ABM, List<Event>> SelFunc, Object Range, double ra, int poss)
        {
            Default_Construct(ID, Etype, SelFunc, locate, TimeConseume, queue_pts, SpaceSearch.ToRTRange(Range, ra), ra, poss);
            activate_dis = act_dis;
        }

        public override void Affect(Agent P, ABM ABM)
        {
            if (P.temp.State != Agent_state.Busy)
            {
                P.temp.State = Agent_state.Busy;

                Register_User(P);

                P.temp.ClearPath();
            }
        }
        public override bool Activate(Agent P, ABM ABM)
        {
            bool[] condition= Default_ActivateConditions(P, ABM);
            if (!condition[0]) { return false; }
            if (!condition[1]) { return false; }
            if (!condition[2]) { return false; }
            if (!condition[3]) { return false; }
  
            int k = IndexInQueue(P);
            bool reach = false;
            if (k > -1)
            {
                reach = sub_locates[k].DistanceTo(P.attr.Locate) < activate_dis;//Reach activation scope
            }
            else
            {
                int index = InertQueue_InOrder(P);
                if (index > -1){reach = sub_locates[index].DistanceTo(P.attr.Locate) < activate_dis;}
            }

            //The task is not finished, Location fulfil the queue point, Fullfill the plan;
            if (reach)
            {
                return true;
            }
            else if (P.temp.State != Agent_state.Busy)
            {
                if (k != -1)
                {
                    P.temp.Path = new List<Point3d>() { sub_locates[k] };
                    P.temp.step = 0;
                    P.temp.State = Agent_state.Walk;
                }
                else
                {
                    P.log += "\n [Can not find occupation in queue:]" + this.ID;
                }

                return false;
            }

            return false;
        }
        public override void Approach(Agent P, ABM EN)
        {
            bool InScope = users.Contains(P);

            int k = IndexInQueue(P);
            if (k > -1)
            {
                if (InScope)
                {
                    P.temp.Path = new List<Point3d>() { sub_locates[k] };
                }
                else
                {
                    P.temp.Path = PathNet.PathSearch(EN.Space.GraphMap, P.attr.Locate, sub_locates[k], Config.SearchRange, Config.SoomthRate);
                }
                P.temp.step = 0;
                P.temp.State = Agent_state.Walk;
            }

            if (k == -1)
            {
                int index= InertQueue_InOrder(P);
                if (index > -1)
                {
                    sub_locates_userID[index] = P.attr.ID;

                    if (InScope)
                    {
                        P.temp.Path = new List<Point3d>() { sub_locates[index] };
                    }
                    else
                    {
                        P.temp.Path = PathNet.PathSearch(EN.Space.GraphMap, P.attr.Locate, sub_locates[index], Config.SearchRange, Config.SoomthRate);
                    }
                    P.temp.step = 0;
                    P.temp.State = Agent_state.Walk;
                }
            }
        }
        public override bool CanProceed(Agent P, ABM EN)
        {
            int k = IndexInQueue(P);
            if (k == -1 || P.attr.Locate.DistanceTo(sub_locates[k]) >= activate_dis)
            {
                return false;
            }
            else if (k == 0)//can only work while the agent is in the first of the queue;
            {
                return true;
            }
            else if (sub_locates_userID[k - 1] == "null")
            {
                sub_locates_userID.RemoveAt(k - 1); sub_locates_userID.Add("null");
            }
            else if (sub_locates_userID[k - 1] != "null" && !users.Select(p => p.attr.ID).Contains(sub_locates_userID[k - 1]))
            {
                string name = sub_locates_userID[k - 1];
                sub_locates_userID[k - 1] = P.attr.ID;
                sub_locates_userID[k] = name;
            }

            return false;

        }

        public override bool IsVacant()
        {
            return AvailIndex_InQueue().Count > 0;
        }

        public override bool CanBeVisit(Agent P, ABM ABM)
        {
            if (PreviouslyCompeleted(P)) return false;
            return true;
        }

        public override string ToString()
        {
            return log + "\n" + string.Join("|", sub_locates_userID.ToArray());
        }
    }
```
