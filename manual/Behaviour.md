[Back](/manual/Framework.md)

# Behavior
**Behavior is an abstract class that defines how the agents make decisions according to their situations.**

```C#
    public abstract class Behavior
    {
        public Behavior() { }

        //By calling the Act method, general agents activities are implemented
        public abstract List<IGoal> Act(Person P, ABM EN);
    }
```

[More instances of Behavior](/manual/_IBehav.md)
