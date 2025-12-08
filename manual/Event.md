[Back](/manual/Framework.md)
# Event as asbtract class
**Event is an abstract class that defines the interest points in the ABM world and their mechanism of interaction.**
<br>

***The following code shows the format of defining an Event with an abstract class:***

```C#

//The abstract definition of the "Event": 
 public abstract class Event
    public abstract class Event
    {
        public EventType Type;
        public string ID;
        public Point3d locate=Point3d.Origin;//Main location of the event;

        public List<Point3d> sub_locates=new List<Point3d>();
        public List<string> sub_locates_userID=new List<string>();

        /// <summary>
        /// The agents are using and activating the event, they are not waiting or approaching the event;
        /// sub_locates_userID: records the ID of the users in the queue;
        /// beTarget: The agents set their target towards this event, they will be waiting or approaching the event;
        /// waitlist: The agents are waiting for this event;
        /// </summary>

        public List<Agent> users=new List<Agent>();
        public List<Agent> beTarget = new List<Agent>();
        public List<Agent> waitlist=new List<Agent>();

        public int timeCons=10;

        public double R= -1.0;
        public Object RT;

        public int Poss = 0;//Possure of the agent;

        public bool IsGlobal=false; //If the Event is defined as Global parameter, it will be listened by every agent in each iteration;
        public bool IsRadar = true; //If the Event is defined as radar, it refers to a scope to affect the agents;

        Extensible Ext;

        public bool initialized=false;
        public virtual void Default_Construct(string id, string Etype, Func<List<Event>, Agent, ABM, List<Event>> SelFunc, Point3d poi, int TimeConseume, List<Point3d> sub_pts, Object Range, double Ra, int poss)
        {
            Type = new EventType(Etype, SelFunc);
            ID = id;
            locate = poi;

            users = new List<Agent>();
            beTarget = new List<Agent>();
            waitlist = new List<Agent>();

            R = Ra;
            RT = Range;
            sub_locates = sub_pts.Count==0? new List<Point3d>() {poi} : sub_pts;
            sub_locates_userID = sub_locates.Select(p => "null").ToList();
            timeCons = TimeConseume;
            Poss = poss;

            Ext = new Extensible();
        }
        /// <summary>
        /// flag[0]=>NotNull  |
        /// flag[1]=>TypeMatch  |
        /// flag[2]=>HasMemory  |
        /// flag[3]=>HasMemoryAndNotFinished
        /// </summary>
        /// <param name="P">Agent</param>
        /// <param name="ABM">Agent-based Modelling</param>
        public virtual bool[] Default_ActivateConditions(Agent P, ABM ABM)
        {
            //NotNull
            //TypeMatch
            //HasMemory
            //IsTheLastMemoFinished

            bool[] flags=new bool[4] { false,false,false, true };
            if (P.temp.Target != null && P.attr.Locate != null) { flags[0] = true; }
            if (IsType(P.temp.Target.Type.Name) && P.temp.Target.ID == ID) { flags[1] = true; }

            Memory[] IfDid = FindRelatedMemos(P);
            if (IfDid.Length > 0)
            {
                flags[2] = true;
                if (IfDid.Last().IfFinished()) { flags[3]=false; }//If the task is finished;
            }

            return flags;

        }
        public abstract void Affect(Agent P, ABM ABM);
        public abstract bool Activate(Agent P, ABM ABM);
        public abstract void Approach(Agent P, ABM ABM);
        public abstract bool CanProceed(Agent P, ABM ABM);
        public abstract bool CanBeVisit(Agent P, ABM ABM);
        public virtual void Initialize(Agent P, ABM ABM)
        {
            initialized = true;
        }
        public virtual void Reset()
        {
            users.Clear(); beTarget.Clear(); waitlist.Clear();
            sub_locates_userID = sub_locates.Select(p => "null").ToList();
        }
        public virtual void TryApproach(Agent P, ABM EN)
        {
            Event E = this;
            if (E.locate == null && E.users.Count==0) { return; }

            Point3d Locate_TryAppr = E.sub_locates.Count > 0 ? E.sub_locates.Last() : E.users.Last().attr.Locate;

            P.temp.Path = PathNet.PathSearch(EN.Space.GraphMap, P.attr.Locate, Locate_TryAppr, Config.SearchRange, Config.SoomthRate);
            if (P.temp.Path.Count > 1) { P.temp.Path.RemoveAt(P.temp.Path.Count - 1); }
            P.temp.step = 0;
        }
        public virtual void Register_Target(Agent P)
        {
            if (!beTarget.Contains(P)) { beTarget.Add(P); }
            P.temp.Target = this;
        }
        public virtual void Register_Waiter(Agent P)
        {
            if (!waitlist.Contains(P)) { waitlist.Add(P); }
        }
        public virtual void Register_Memory(Agent P)//Make sure the remove ToDoList after register
        {
            int t = timeCons;
            string next = P.bhav.ToDoList.Count > 0 ? P.bhav.ToDoList[0] : "";
            if (int.TryParse(GetEncodeInfor(Decode(next), "time"), out int infor)) { t = infor; }
            P.log = $"Register:{ID} to {P.attr.ID} with plan [{next}]";
            P.temp.Brain.Add(new Memory(this, t));
        }
        public virtual void Register_User(Agent P)
        {
            if (!users.Contains(P)) {users.Add(P);}
        }
        public virtual int IndexInQueue(Agent P)
        {
            int k = -1;
            for (int i = 0; i < sub_locates_userID.Count; i++)
            {
                if (sub_locates_userID[i] == P.attr.ID)
                {
                    k = i;
                }
            }
            return k;
        }
        public virtual void Organize_Queue()
        {
            int k = 0;
            for (int i = 0; i < sub_locates.Count; i++)
            {
                k++;
                if (sub_locates_userID[i] == "null")
                {
                    sub_locates_userID.RemoveAt(i);
                    sub_locates_userID.Add("null");
                    i--;
                }
                if (k == sub_locates.Count) { break; }
            }
        }

        public virtual int InertQueue_InOrder(Agent P)
        {
            if (IndexInQueue(P) > -1)
            { return -2; }

            Organize_Queue();
            List<int> AvailIndex = AvailIndex_InQueue();
            if (AvailIndex.Count > 0)
            {
                sub_locates_userID[AvailIndex[0]] = P.attr.ID;
                return AvailIndex[0];
            }

            return -1;
        }

        public virtual int InertQueue_Random(Agent P,int seed)
        {
            if (IndexInQueue(P) > -1)
            { return -2; }

            Random random = new Random(seed);
            List<int> AvailIndex = AvailIndex_InQueue();
            
            if (AvailIndex.Count > 0)
            {
                int index = random.Next(0, AvailIndex.Count);
                int k = AvailIndex[index];
                sub_locates_userID[k] = P.attr.ID;
                return k;
            }

            return -1;
        }

        public virtual bool PreviouslyCompeleted(Agent P)
        {
            Memory[] RelatedMemors = FindRelatedMemos(P);
            if (RelatedMemors.Length > 0)
            {
                bool AllCompeleted = true;
                foreach (Memory M in RelatedMemors)
                {
                    if (!M.IfFinished()) { AllCompeleted = false; break; }
                }
                return AllCompeleted;
            }
            else { return false; }
        }

        /// <summary>
        /// If the event has vacant poistion for agents to activate their activity;
        /// Attention that some Events do not need this judgement;
        /// </summary>
        /// <returns></returns>
        public virtual bool IsVacant()
        {
            bool vacant = AvailIndex_InQueue().Count > 0;
            return vacant;
        }
        public virtual List<int> AvailIndex_InQueue()
        {
            List<int> viables = new List<int>();
            for (int i = 0; i < sub_locates_userID.Count; i++)
            {
                if (sub_locates_userID[i] == "null") { viables.Add(i); }
            }
            return viables;
        }
        public virtual Memory[] FindRelatedMemos(Agent P)
        {
            return P.temp.Brain.Where(p => p.Event.ID == ID).ToArray();
        }
        public bool IsType(string type)
        {
            string[] decode = Decode(type);
            if (decode.Length == 0) return false;

            if (Type != null)
            {
                if (Type.Name != null)
                {
                    if (Type.Name == decode[0]) { return true; }//if the main class is matched
                }
            }

            if (ID != null)
            {
                if (ID == decode[0])
                {
                    return true;// if the specific ID is matched
                }
            }
            return false;
        }
        public virtual string[] Decode(string input)
        {
            if (string.IsNullOrEmpty(input))
                return Array.Empty<string>();

            var result = new List<string>();

            // 1. 先取第一个 '[' 之前的部分，这是前缀，如 "ID"
            int firstBracket = input.IndexOf('[');
            if (firstBracket == -1)
            {
                // 没有中括号，就只有一个元素
                result.Add(input);
                return result.ToArray();
            }

            // 前缀不为空就加进去
            string prefix = input.Substring(0, firstBracket);
            if (!string.IsNullOrEmpty(prefix))
                result.Add(prefix);

            // 2. 从前缀后开始，依次找 [xxx] 的内容
            int i = firstBracket;
            while (i < input.Length)
            {
                int start = input.IndexOf('[', i);
                if (start == -1) break;
                int end = input.IndexOf(']', start + 1);
                if (end == -1) break; // 不完整就退出

                string content = input.Substring(start + 1, end - start - 1);
                result.Add(content);

                i = end + 1;
            }

            return result.ToArray();
        }
        public virtual string GetEncodeInfor(string[] decoded, string title)
        {
            if (decoded == null || decoded.Length <= 1)
                return "";

            // 从第二项开始查找
            for (int i = 1; i < decoded.Length; i++)
            {
                var item = decoded[i];
                if (item.StartsWith(title+":", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = item.Split(':');
                    if (parts.Length == 2)
                        return parts[1];
                }
            }

            return "";
        }
        public virtual void Expire(Agent P)
        {
            Expire_WaiterList(P);
            Expire_TargetList(P);
            Expire_UserList(P);
            Expire_QueueList(P);
        }
        public virtual void Expire_WaiterList(Agent P)
        {
            if (waitlist.Contains(P)) { waitlist.Remove(P); }
        }
        public virtual void Expire_TargetList(Agent P)
        {
            if (beTarget.Contains(P)) { beTarget.Remove(P); }
        }
        public virtual void Expire_UserList(Agent P)
        {
            if (users.Contains(P)) { users.Remove(P); }
        }
        public virtual void Expire_QueueList(Agent P)
        {
            int k = IndexInQueue(P);
            if (k != -1)
            {
                sub_locates_userID.RemoveAt(k);
                sub_locates_userID.Add("null");
            }
        }
        public override string ToString()
        {
            return $"Event: Type:{Type.Name} |ID:{ID} |IsGlobal:{IsGlobal} |IsRadar:{IsRadar} |SubPts.Count:{sub_locates.Count}";
        }

    }
```
<br>

**An instance of the Event class:**
## IEvent_S_vS_aS_St <br>
__The event can be used by [S: single agent], visited in [vS: single time], activated by [aS: single agent], Standing [St]__
<br>

```C#
       public class IEvent_S_vS_aS_St : Event
    {
        double activate_dis = 0.5;
        public IEvent_S_vS_aS_St(string ID, Point3d locate, int TimeConseume, double activate_dis, string Etype, Func<List<Event>, Agent, ABM, List<Event>> SelFunc, double ra, int poss)
        {
            Default_Construct(ID, Etype, SelFunc, locate, TimeConseume,new List<Point3d>(), SpaceSearch.ToRTRange(locate, ra),ra,poss);
            this.activate_dis = activate_dis;
        }
        public override void Affect(Agent P, ABM ABM)
        {
                 if (P.temp.State != Agent_state.Busy)
                {
                    P.temp.State = Agent_state.Busy;
                    Register_User(P);
                    InertQueue_InOrder(P);
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

            return locate.DistanceTo(P.attr.Locate) < activate_dis;
        }

        public override void Approach(Agent P, ABM EN)
        {
            P.temp.Path = PathNet.PathSearch(EN.Space.GraphMap, P.attr.Locate, this.locate, Config.SearchRange, Config.SoomthRate);
            P.temp.step = 0;
            P.temp.State = Agent_state.Walk;
        }

        public override bool CanProceed(Agent P, ABM EN)
        {
            if (P.attr.Locate.DistanceTo(locate) <= activate_dis) { return true; }
            else { return false; }
        }

        public override bool CanBeVisit(Agent P, ABM ABM)
        {
            //This event can be visited only once and by one person;
            if (PreviouslyCompeleted(P)) { return false; }
            return true;
        }
    }

```
[See more instances of IEvent](/manual/_IEvent.md)
