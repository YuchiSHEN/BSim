# BSim - Behaviour Simulator

The Behaviour Simulator is an agent-based model framework designed as a plugin for Rhino software. This implementation offers a lightweight and easily customizable system, enabling users to define agent behaviors and explore system dynamics through simulations. The development is implemented as a plug-in for the CAD environment [McNeel Rhino/Grasshopper](https://www.rhino3d.com/) for both Windows and MacOS.
<br>
<br>

This library is developed and maintained by:
- __Yuchi Shen__ [Southeast University of Nanjing, School of Architecture](http://arch.seu.edu.cn/jz_en/main.htm)
- __Mengting Zhang__ [City University of Macau, Faculty of Innovation and Design](https://fiad.cityu.edu.mo/)
<br>

If you use the BSim library, please reference the official GitHub repository: <br>
@Misc{vgs2021, <br>
author = {Shen, Yuchi and Zhang, Mengting}, <br>
title = {{BSim: Behaviour Simulator}}, <br>
year = {2024}, <br>
note = {Release 1.00 Beta}, <br>
url = { https://github.com/YuchiSHEN/BSim.git }, <br>
}
<br>
<br>

This library makes use of the following libraries: 
- [Kangaroo2](https://www.rhino3d.com/) by Daniel Piker. [For the physical system in ABM]
<br>
<br>

To install  BSim, please copy the folder "BSim.dll" to any address in the computer, and link all the components to this dll. If you work on Windows, please make sure that the files are unlocked.
<br>
<br>

Publications related to the BSim project include:
- __Shen Yuchi, Xinyi Hu, Xiaotong Wang, Mengting Zhang, Lirui Deng ,Wei Wang__: Integrated framework for space-and energy-efficient retrofitting in multifunctional buildings: A synergy of agent-based modeling and performance-based modeling, Building Simulation, Build. Simul. 17, 1579–1600 (2024). https://doi.org/10.1007/s12273-024-1148-z 

## Quick Start:
An ABM system can be setted with:

```C#
// import the namespace of BSim.dll:
using BSim;

//Construt a ABM class with four inputs [ISpace] [Event] [Person] [Communicate];
ABM AgentBasedModel = new ABM(ISpaces, Event, Person, Communicate);

//You can run the simulation with:

if (Reset)
    {
     //refresh the system
      ABM_simu = (ABM) Model; //nominate the ABM as ABM_simu
      agentCount = ABM_simu.PPs_Wait.Count;
    }
    else if(Run)
    {
      if(ABM_simu != null && ABM_simu.PS.iteration < MaxIter && ABM_simu.PPs_Sink.Count < agentCount)
      {
        try{
          Print(ABM_simu.Simulate(Reset, out timeRep));
        }
        catch (Exception e)
        {
          Print(e.ToString());
        }
      }
    }

//
```
