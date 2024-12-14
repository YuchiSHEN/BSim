[Back](/manual/Framework.md)

# Behavior

```C#

//Behavior is an abstract class that implements the behavior of agents 
    public abstract class Behavior
    {
        public Behavior() { }

        //By calling the Act method, general agents activities are implemented
        public abstract List<IGoal> Act(Person P, ABM EN);
    }
```
[The instances of Behavior in BSim](/manual/_IBehav.md)
