### The interface of the function is:
```C#
    public interface Metabolism
    {
        bool IfEmerge(Person P, ABM ABM);
        bool IfExpire(Person P, ABM ABM);
        object log (Person P, ABM ABM);
    }
```
__You can define an instance in which the agent emerge at certain iteration of the Simulation:__
```C#
 public class MetaP:Metabolism
  {
    //We define an iteration value to give birth to an agent;
    public int emergeIteration;
    
    public MetaP(int Iteration)
    {
      this.emergeIteration = Iteration;
    }
    
    public bool IfEmerge(Person P, ABM ABM)
    {
      if(ABM.PS.iteration == emergeIteration){return true;}
      else{return false;}
    }

    public bool IfExpire(Person P, ABM ABM)
    {
      if(P.State == Person_state.Gone){return true;}
      else{return false;}
    }

    public object log (Person P, ABM ABM)
    {
      return null;
    }

    public override string ToString()
    {
      return "Emerge at " + emergeIteration.ToString();
    }
  }
```
