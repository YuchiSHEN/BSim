[Back](/manual/Behaviour.md)
## IBehav_NextJob: 

<div align="left">
<img src="/pic/IBehav_NextToDo.svg" width="550">
</div>

__The State Controller for agents to decide the next work in the simulation__
```C#
   public class IBehav_StateController : Behavior
   {
       public IBehav_StateController()
       {
       }

       public List<IPhysic> Actions=new List<IPhysic>();
       public override List<IPhysic> Act(Agent P, ABM abm)
       {
           if (P.temp.State == Agent_state.Free)
           {
               return Action_Free(P, abm);
           }

           if (P.temp.State == Agent_state.Wait)
           {
               return Action_Wait(P, abm);
           }

           if (P.temp.State == Agent_state.Walk)
           {
               return Action_Walk(P, abm);
           }

           if (P.temp.State == Agent_state.Busy)
           {
               return Action_Busy(P, abm);
           }

           if (P.temp.State == Agent_state.Talk)
           {
               return Action_Talk(P, abm);
           }

           if (P.temp.State == Agent_state.Gone)
           {
               return Action_Gone(P, abm);
           }

           return Actions;
       }
       private List<IPhysic> Action_Free(Agent P, ABM abm)
       {
           List<IPhysic> Action = new List<IPhysic>();
           Memory[] LastUnFinishedMemo = P.temp.TheLastUnFinishedMemo();

           //Case A: there is un-finished work;
           if (LastUnFinishedMemo.Length > 0)
           {
               Event ContinueEvent = LastUnFinishedMemo[0].Event;
               ContinueEvent.Register_Target(P);
               if (Config.debug) P.log += $"\n[Action_Free]: UnFinished work detected:{ContinueEvent.ID}";

               ContinueEvent.Approach(P,abm);
               return Action;//[End Action]
           }

           //Case B: all work finished, ToDoList is empty;
           if (P.bhav.ToDoList.Count == 0)
           {
               P.bhav.ToDoList.Add("Leave");
               List<Event> Leave_Events = MatchedType_Events("Leave", abm);
               if (Leave_Events.Count > 0)
               {
                   List<Event> Leave = Leave_Events[0].Type.SectionRule(Leave_Events, P, abm);
                   if (Leave.Count > 0)
                   {
                       Leave[0].Register_Target(P);
                       if (Config.debug) P.log += $"\n[Action_Free]: [{Leave[0].ID}] is selected for leave";
                   }
                   if (Config.debug) P.log += $"\n[Action_Free]: None of the {Leave.Count} leave type candidates is selected for leave";
               }
               if (Config.debug) P.log += $"\n[Action_Free]: Can not find any valid Leave type for agents to leave";

               P.temp.Target.Approach(P,abm);

               return Action;//[End Action]
           }

           //Case C: ToDoList is not empty;
           //Filter the list:
           string next = P.bhav.ToDoList[0];

           //Filter the events with three principles;
           List<Event> Events_MatchedType = MatchedType_Events(next, abm);
           List<Event> Events_Visitable = Visitable_Events(Events_MatchedType, P, abm);
           List<Event> Events_IsVacant = Events_Visitable.Where(e => e.IsVacant()).ToList();

           //Case C-1: if none of the Events can match the plan, skipp it;
           if (Events_Visitable.Count == 0)
           {
               P.log = $"[Action_Free]: can not match next plan: [{next}], and it is skipped";
               P.bhav.ToDoList.RemoveAt(0);//skipp the plan;
               return Actions;
           }

           //Case C-2: Select event;
           if (Events_IsVacant.Count > 0)
           {
               List<Event> Events_Selected = Events_IsVacant[0].Type.SectionRule(Events_IsVacant, P, abm);
               if (Events_Selected.Count == 0) { throw new Exception($"[Action Free]: None Event Selected from Vacant list, round 115 line"); }

               Event SelEvent = Events_Selected[0];
               if (Config.debug)
               {
                   P.log += "\n[Action_Free]:" +
                         $"\n----The event [{SelEvent.ID}] matches the [{P.bhav.ToDoList[0]}] in ToDoList" +
                         $"\n----The agent approaches the event [{SelEvent.ID}]" +
                         $"\n----[{P.attr.ID}]Register Memory: Event:[{SelEvent.ID}|Type:{SelEvent.Type.Name}]";
               }

               SelEvent.Register_Target(P);
               SelEvent.Register_Memory(P); P.log = $"Case C-2 Pick Event: register at [Action_Free]";

               P.log = $"[Action_Free]: Match Event {SelEvent.ID} : ToDoList[{next}], and it is removed";
               P.bhav.ToDoList.RemoveAt(0);

               P.temp.Target.Approach(P, abm);
               if (Config.debug) P.log += $"\n[Action_Free]:The agent[{P.attr.ID}] approach the event[{P.temp.Target.ID}]";

               return Action;//[End Action]
           }

           //Case C-3:wait;
           if (Events_IsVacant.Count == 0)
           {
               List<Event> Events_WaitList = Rules_Selection.RaSel_WaitList(Events_Visitable, P, abm);
               if (Events_WaitList.Count == 0) { throw new Exception("[Action_Free]: [WaitList Choosen False]"); }

               Event Event_WaitTarget = Events_WaitList[0];

               Event_WaitTarget.Register_Waiter(P);
               Event_WaitTarget.Register_Target(P);
               Event_WaitTarget.TryApproach(P, abm);
               P.temp.State = Agent_state.Wait;

               if (Config.debug)
               {
                   P.log += $"\n[Action_Free]: The agent set [Free ——> Wait] for {Event_WaitTarget.ID}:userCount:{Event_WaitTarget.users.Count}";
                   P.log += $"\n[Action_Free]: The agent [{Event_WaitTarget.ID}] TryApproach [{Event_WaitTarget.ID}]";
               }
               return Action;
           }

           return Action;
       }
       private List<IPhysic> Action_Wait(Agent P, ABM abm)
       {
               if (P.temp.Target != null)
               {
                   bool available = P.temp.Target.IsVacant();
               if (available && P.temp.Target.waitlist.Contains(P))
                   {  
                       P.log = "register at [Action Wait]";
                       P.temp.Target.Register_Memory(P);

                       if (P.bhav.ToDoList.Count == 0) { throw new Exception("[Action_Wait]:The waited target is available now, but the ToDoList is empty"); }
                       if (!P.temp.Target.IsType(P.bhav.ToDoList[0])) 
                         { throw new Exception($"[Action_Wait]:The waited target [{P.temp.Target.ID}] can get now, but the ToDoList [{P.bhav.ToDoList[0]} of {P.attr.ID}] is not matching"); }

                       if (Config.debug) { 
                               P.log += $"\n[Action_Wait]: Waitting End:" +
                                     $"\n----The event [{P.temp.Target.ID}] matches the [{P.bhav.ToDoList[0]}] in ToDoList" +
                                     $"\n----ToDolist removed:[{P.bhav.ToDoList[0]}] and waitlist removed:[{P.temp.Target.ID}]" +
                                     $"\n----The agent[{P.attr.ID}] approach the event[{P.temp.Target.ID}]"+
                                     $"\n----[{P.attr.ID}]Register Memory: Event:[{P.temp.Target.ID}|Type:{P.temp.Target.Type.Name}]";
                                          }

                       P.bhav.ToDoList.RemoveAt(0); //dangerous
                       P.temp.Target.waitlist.Remove(P);
                       P.temp.Target.Approach(P, abm);//[->Walk]
                       return Actions;
                   }
                   else
                   {
                       return Actions;
                   }
               }
               else{ throw new Exception($"\n[Action_Wait]:The agent[{P.attr.ID}] has no target, while it is waiting for something"); }
       }
       private List<IPhysic> Action_Walk(Agent P, ABM abm)
       {
           if (P.temp.Path.Count == 0 && P.temp.step == 0)
           { 
               P.temp.State = Agent_state.Free;
               if (Config.debug) P.log += $"\n[Action_Walk]: [{P.attr.ID}] walk action end and [Walk ——> Free]";
           }
           return new List<IPhysic>();
       }
       private List<IPhysic> Action_Busy(Agent P, ABM abm)
       {
           //Search if there is unfinished event in the memory;
           Memory[] Memory_UnderGoing=P.temp.TheLastUnFinishedMemo();
           if (Memory_UnderGoing.Length == 0)
           {
               if (Config.debug) P.log += $"\n[Action_Busy]: [{P.attr.ID}] has no unfinishedMemo and setted with [Busy——>Free]";
               P.temp.State = Agent_state.Free;
           }
           else
           {
               Memory UnFinMemo = Memory_UnderGoing[0];
               Event UnDoneEvent = UnFinMemo.Event;
               UnDoneEvent.Register_Target(P);

               if (P.temp.Target.CanProceed(P, abm))
               {
                   if (UnFinMemo.Step < UnFinMemo.MaxStep) 
                   {
                       UnFinMemo.Step++;
                   }
                   if (UnFinMemo.IfFinished()) 
                   {
                       if (Config.debug) P.log += $"\n[Action_Busy]: [{P.attr.ID}] finished & expired the event[{P.temp.Target.ID}], and [Busy——>Free]";
                       P.temp.Target.Expire(P);
                       P.temp.State = Agent_state.Free;
                   }
               }
               else
               {
                   if (P.temp.Path.Count == 0)
                   {
                       P.temp.Target.Approach(P, abm);//[——>Walk]
                   }
               }
           }

           return Actions;
       }
       private List<IPhysic> Action_Talk(Agent P, ABM abm)
       {
           //Search if there is unfinished event in the memory;
           Memory[] Memory_UnderGoing = P.temp.TheLastUnFinishedMemo();

           if (Memory_UnderGoing.Length == 0)
           {
               P.temp.State = Agent_state.Free;
           }
           else
           {
               Memory UnFinMemo = Memory_UnderGoing[0];
               UnFinMemo.Event.Register_Target (P);

               if (P.temp.Target.CanProceed(P, abm))
               {
                   if (UnFinMemo.Step < UnFinMemo.MaxStep) { UnFinMemo.Step++; }
                   if (UnFinMemo.IfFinished() == true) { P.temp.Target.Expire(P); }
               }
               else
               {
                   if (P.temp.Path.Count == 0)
                   {
                       P.temp.Target.Approach(P, abm);
                   }
               }
           }

           return Actions;
       }
       private List<IPhysic> Action_Gone(Agent P, ABM abm)
       {
           return new List<IPhysic>();
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

        public override List<IPhysic> Act(Agent P, ABM EN)
        {          
            return new List<IPhysic>() 
            { new WalkFollowPath(P, ATol, Speed, Strength) };
        }
    }
```
