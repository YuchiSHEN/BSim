# Behaviour

```C#

//Behavior is an abstract class that implements the behavior of agents 
    public abstract class Behavior
    {
        public Behavior() { }

        //By calling the Act method, general agents activities are implemented
        public abstract List<IGoal> Act(Person P, ABM EN);
    }
```

<br>
## IBehav_NextJob <br>
An instance of behavior; Shows how the agent decide the work of the next step;
<br>

```C#
//IBhav is an instance of behavior will be called in very iteration;
//This code shows the decision logic of agents;
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
            //empty physical goals;
            List<IGoal> Goals = new List<IGoal>(0);

            //If the agent is already doing something; then return;
            if (P.State != Person_state.Free) { return Goals; }

            //Check if there is unfinished works in the memory;
            List<Person.Memory> NotFinished = P.Brain.Where(p => p.IfFinished() == false).ToList();

            //If all works in person's brain is finished;
            if (NotFinished.Count == 0)
            {
                //And ToDoList is not empty;
                if (P.ToDoList.Count > 0)
                {
                    P.NextToDo = P.ToDoList[0];

                    //Pick the Event
                    List<Event> POIs = SelFunc(P, EN);

                    //When there is no POI matchs the person's plan
                    if (POIs.Count == 0)
                    { return Goals; }

                    Event POI = POIs[0];
                    P.ToDoList.RemoveAt(0);

                    P.Target = POI;
                    if (!POI.beTarget.Contains(P))
                    { POI.beTarget.Add(P); }
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
                Event ContinueWork = NotFinished[NotFinished.Count - 1].Event;
                P.Target = ContinueWork;
            }

            //Change the walking path according to the Event;
            P.Target.Approach(P, EN);
            return Goals;
        }
    }

```