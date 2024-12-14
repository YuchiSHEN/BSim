[Back](/manual/Framework.md)
__The attributes and features of an agent (Person):__
```C#
    public class Person
    {
        #region attributes
        //Basic Behaviors
        public List<Behavior>Behaviors;
        public Behavior IMove;

        //Plan
        public List<string>ToDoList;
        public string NextToDo;
        public Person_state State;
        public Event Target;

        //Memory
        public List<Memory> Brain;

        //Body Volume
        public double SizeR;

        //Body as Curve
        public Curve Body;

        //Attribute
        public string Name;
        public double Strength;
        public double Cur_Strength;

        //Location
        public Point3d Locate;
        public List<Point3d> Path;
        public int step;//The stage in the path list;

        private Vector3d Fvec;
        public Vector3d FaceVec;

        private double ViewLength;
        private double ViewAngle;
        public Curve ViewRange;

        #endregion

        #region OpenFunction
        public Metabolism Meta;
        #endregion

        public Person(string name, Point3d P, double R, double S,double VLength,double VAngle, Behavior IWalk, List<Behavior> behaviors, List<string> ToDoList){ }
        

        public List<IGoal> Act(ABM EN)
        {
            //the goals into physical system
            List<IGoal> Goals = new List<IGoal>();

            foreach (Behavior Be in Behaviors)
            {
                Goals.AddRange(Be.Act(this,EN));
            }
            return Goals;
        }

        public List<IGoal> DoBehav(ABM ABM, Behavior IBehav)
        {
            return IBehav.Act(this, ABM);
        }

        public class Memory
        {
            public Event Event;
            //Two paras show the state of usage of certain Event;
            public int Step;
            public int MaxStep;

            public Memory(Event Event,int MaxStep)
            {
                this.Event = Event;
                this.MaxStep = MaxStep;
                this.Step = 0;
            }
            public bool IfFinished()
            {
                if (Step == MaxStep) { return true; }
                else { return false; }
            }
            public override string ToString()
            {
                return Event.ID.ToString() + string.Format(" : {0}/{1}", this.Step.ToString(), this.MaxStep.ToString());
            }
        }

        public void DocMemory(ref HashSet<Person_Doc> docs)
        {
            if (docs.TryGetValue(new Person_Doc(this), out Person_Doc doc))
            {
                doc.record(this);
            }
            else
            {
                docs.Add(new Person_Doc(this));
            }
        }
    }
```

__Currently a person have such states:__
```C#
    public enum Person_state
    {
        Busy,
        Walk,
        Talk,
        Free,
        Wait,
        Gone
    }
```
