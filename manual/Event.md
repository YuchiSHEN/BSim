[Back](/manual/Framework.md)
# Event as asbtract class
**Event is an abstract class that defines the interest points in the ABM world and their mechanism of interaction.**
<br>

***The following code shows the format of defining an Event with an abstract class:***

```C#

//The abstract definition of the "Event": 
 public abstract class Event
 {
     public EventType Type;
     public string ID;
     public Point3d locate;
     public List<Person> users=new List<Person>();
     public List<Person> beTarget = new List<Person>();
     public double R= -1.0; //The radius value of the affect circle of the event.

     public abstract void Affect(Person P, ABM ABM);
     public abstract bool Activate(Person P, ABM ABM);
     public abstract void Approach(Person P, ABM ABM);
     public abstract bool CanProceed(Person P, ABM ABM);
     public abstract bool CanBeVisit(Person P, ABM ABM);
     public abstract void Expire(Person P);

     public bool IsType(string type)
     {
         if(Type!=null)
        {  
          if (Type.Name != null)
          {
              if (Type.Name == type) { return true; }
          }
         }

         if (ID != null)
         {
             if (ID==type)
             {
                 return true;
             }
         }
          return false;  
     }
 }
```
<br>

**An instance of the Event class:**
## IEvent_S_vS_aS_St <br>
Shows how the agent decide the work of the next step;
<br>

```C#
    public class IEvent_S_vS_aS_St : Event
    {
        double activate_dis = 0.5; //The event can be activated within the distance of this value;
        int timeCons = 50; //The length of the time(delta t) for visiting this event;

        public IEvent_S_vS_aS_St(string ID, Point3d locate,int TimeConseume, double activate_dis, string Etype)
        {
            this.Type =new EventType(Etype);
            this.ID = ID;
            this.locate = locate;
            this.users = new List<Person>();
            this.beTarget = new List<Person>();
            this.activate_dis = activate_dis;
            this.R = activate_dis * 10;
            timeCons = TimeConseume;
        }

        public override void Affect(Person P, ABM ABM)
        {
                 if (P.State != Person_state.Busy)
                {
                    P.State = Person_state.Busy;
                    
                    if (!this.users.Contains(P) && IfDidBefore(P).Count==0)
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
            bool DidBefore = false;
            //if watched
            List<Memory>RelatedMemors= IfDidBefore(P);
            if (RelatedMemors.Count>0)
            {
                if (RelatedMemors[0].Step == RelatedMemors[0].MaxStep)
                { DidBefore = true; } 
            }

            double Distance = this.locate.DistanceTo(P.Locate);

            //Not visited, Distance close, Fullfill the plan;
            if (!DidBefore && Distance < activate_dis && IsType(P.NextToDo) && P.Target.ID==this.ID)
            { return true; }
            else
            { return false;}
        }

        public override void Approach(Person P, ABM EN)
        {
            P.Path = PathNet.ShortestPath_Quick(EN.Space.Quick_WalkSpace, P.Locate, this.locate, Config.SearchRange, Config.SoomthRate);
            P.step = 0;
            P.State = Person_state.Walk;
        }

        public override bool CanProceed(Person P, ABM EN)
        {
            if (P.Locate.DistanceTo(this.locate) <= activate_dis) { return true; }
            else { return false; }
        }

        private List<Memory> IfDidBefore(Person P)
        {
            List<Memory> RelatedMemors = P.Brain.Where(p => p.Event.ID == this.ID).ToList();
            return RelatedMemors;
        }

        public override bool CanBeVisit(Person P, ABM ABM)
        {
            //This event can be visited only once and by one person;
            if (IfDidBefore(P).Count > 0 || users.Count > 0 || this.beTarget.Count> 0)
            { return false; }
            else { return true; }
        }

        public override void Expire(Person P)
        {
            if (users.Contains(P))
            {
                users.Remove(P);
            }
            if (beTarget.Contains(P))
            {
                beTarget.Remove(P);
            }
        }
    }

```
[See more instances of IEvent](/manual/_IEvent.md)
