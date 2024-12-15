[Back](/manual/Behaviour.md)
## IBehav_NextJob: 

<div align="left">
<img src="/pic/IBehav_NextToDo.svg" width="550">
</div>

__The behavior for agents to decide the next work in the simulation__
```C#
    public class IBehav_NextJob : Behavior
    {
        private  delegate List<Event> SelectThePOI(Person P, ABM E);
        private SelectThePOI SelFunc;
        public IBehav_NextJob(Func<Person, ABM, List<Event>> Func_SelectPOI) 
        {
            SelFunc=new SelectThePOI(Func_SelectPOI);
        }

        public override List<IGoal> Act(Person P, ABM EN)
        {
            //Decide NextJob
            //Person: Target(Event)
            //Person: NextToDo(EventTypes)
            //Person: ToDoList(<EventTypes>)

            List<IGoal> Goals = new List<IGoal>(0);

            if (P.State != Person_state.Free) { return Goals; }

            List<Person.Memory> NotFinished = P.Brain.Where(p => p.IfFinished() == false).ToList();

            //If all works in person's brain is finished, try to get the next work;
            if (NotFinished.Count == 0)
            {
                //And ToDoList is not empty;
                if (P.ToDoList.Count > 0)
                {
                    P.NextToDo = P.ToDoList[0];

                    //Pick the Event
                    List<Event> SelectedEvent = SelFunc(P, EN);

                    //When there is no POI matchs the person's plan, return;
                    if (SelectedEvent.Count == 0)
                    { return Goals; }

                    Event evenT = SelectedEvent[0];
                    P.ToDoList.RemoveAt(0);

                    P.Target = evenT;
                    if (!evenT.beTarget.Contains(P))
                    { evenT.beTarget.Add(P); }
                }
                //And ToDoList is empty;
                else
                {
                    P.NextToDo = "Leave";
                    List<Event> Leave = EN.POIs.Where(p => p.IsType("Leave")).ToList(); 
                    if (Leave.Count > 0)
                    {
                        P.Target = Leave[0];
                    }
                }
            }
            //If there is work not finished;
            else
            {
                Event ContinueEvent = NotFinished.Last().Event;
                P.Target = ContinueEvent;
            }

            //Change the walking path according to the Event;
            P.Target.Approach(P, EN);
            return Goals;
        }
    }
```

## IBehav_DoJob
__The behavior for agents to process the current work__
```C#
    public class IBehav_DoJob : Behavior
    {
        public IBehav_DoJob() { }

        public override List<IGoal> Act(Person P, ABM EN)
        {
            List<IGoal> Goals = new List<IGoal>(0);

            bool Working = P.State == Person_state.Busy || P.State == Person_state.Talk;

            if (!Working) { return Goals; }

            //Search if there is unfinished event in the memory;
            List<Person.Memory> UnfinishedMemory = P.Brain.Where(p => !p.IfFinished()).ToList();

            if (UnfinishedMemory.Count == 0)
            {
                P.State = Person_state.Free;
            }
            else
            {
                Person.Memory UnDoneMemory= UnfinishedMemory.Last();
                Event UnDoneEvent = UnDoneMemory.Event;
                P.Target=UnDoneEvent;
                P.NextToDo = UnDoneEvent.Type.Name;

                if (UnDoneEvent.CanProceed(P, EN))
                {
                    if (UnDoneMemory.Step < UnDoneMemory.MaxStep) { UnDoneMemory.Step++; }
                    if (UnDoneMemory.IfFinished() == true) {UnDoneEvent.Expire(P);}
                }
                else
                {
                    UnDoneEvent.Approach(P, EN);
                }
            }

            return Goals;
        }
    }
```

## IBehav_MakeMove
__The behavior for agents to make movements__
```C#
    public class IBehav_MakeMove : Behavior
    {
        public double ATol;
        public double Speed;
        public double Strength;
        public IBehav_MakeMove(double ATol, double Speed, double Strength)
        {
            this.ATol = ATol;
            this.Speed = Speed;
            this.Strength = Strength;
        }

        public override List<IGoal> Act(Person P, ABM EN)
        {
            List<IGoal> Goals = new List<IGoal>(0);

            //Walk Goal
            Goals.Add(new WalkFollowPath(P, ATol, Speed, Strength));

            return Goals;
        }
    }
```
