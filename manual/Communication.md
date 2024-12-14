[Back](/manual/Framework.md)

# Communication
**Communication is an abstract class that defines how the agents interact with eachother in the simulation.**

```C#
    public abstract class Communicate
    {
        public Communicate() { }
        public abstract List<IGoal> Interact(ABM EN, Person P0, Person P1);
    }
```

[More instances of Communication](/manual/_ICom.md)
