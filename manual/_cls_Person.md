[Back](/manual/Framework.md)
<br>
__The attributes and features of an agent (Person):__
```C#
    public class Agent
    {
        private List<string> _log = new List<string>();
        private int max = 20;
        public string log
        {
            get
            {
                return string.Join("\n", _log);
            }
            set
            {
                string[] splitResult = value.Split(new[] { Environment.NewLine, "\n", "\r\n" }, StringSplitOptions.None);
                if (splitResult.Length > 0) 
                {
                    int remove = _log.Count + splitResult.Length - max;
                    if (remove>0)
                    {
                        for (int i = 0; i < remove; i++) { if(_log.Count>0)_log.RemoveAt(0); }
                    }
                    _log.AddRange(splitResult);
                }
            }
        }

        public Agent_Attr attr;
        public Agent_Bhav bhav;
        public Agent_View view;
        public Agent_Meta meta;

        public Agent_Temp temp;

        public Agent DeepCopy()
        {
            Agent copy=new Agent(this);
            copy.temp = new Agent_Temp(temp);
            return copy;
        }

        public Agent(Agent a) 
        {
            this.log = a.log;
            attr=new Agent_Attr(a.attr);
            bhav=new Agent_Bhav(a.bhav);
            view=new Agent_View(a.view);
            meta=a.meta;
            temp=new Agent_Temp();
        }

        public Agent(Agent_Attr attr, Agent_View view, Agent_Bhav bhav, Agent_Meta Meta)
        {
            log = "";
            this.attr = new Agent_Attr(attr);
            this.bhav = new Agent_Bhav(bhav);
            this.view = new Agent_View(view);
            this.meta = Meta;
            this.temp = new Agent_Temp();
        }

        public void DocMemory(ref HashSet<Agent_Doc> docs, int tab)
        {
            if (docs.TryGetValue(new Agent_Doc(this, tab), out Agent_Doc doc))
            {
                doc.record(this);
            }
            else
            {
                docs.Add(new Agent_Doc(this,tab));
            }
        }

        #region GetValues
        public Vector3d GetFvec()
        {
            if (temp.Path.Count > 0)
            {
                Vector3d vec = new Vector3d(temp.Path[temp.step] - attr.Locate);
                attr.Fvec = vec;
                return vec;
            }
            else if (temp.Target != null)
            {
                Vector3d vec = new Vector3d(temp.Target.locate - attr.Locate);
                if (vec.Length > 0) { attr.Fvec = vec; return vec; } else { return attr.Fvec; }
            }
            else
            {
                return attr.Fvec;
            }

        }
        public Vector3d GetSpdVec()
        {
            if (temp.State == Agent_state.Walk && temp.Path.Count > 0 && temp.step >= 0)
            {
                Vector3d vec = temp.Path[temp.step] - attr.Locate; vec.Unitize();
                vec *= attr.Speed;
                return vec;
            }
            else
            {
                return Vector3d.Zero;
            }
        }
        public int GetPoss()
        {
            if (temp.Target != null && temp.State == Agent_state.Busy && temp.Target.Poss < attr.Postures.Length)
            { return temp.Target.Poss; }
            else { return 0; }
        }
        #endregion

        public override string ToString()
        {
            string report =
               $"\n________________" +
               attr.ToString() +
               bhav.ToString() +
               temp.ToString() +
               $"\n________________";
            return report;
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
