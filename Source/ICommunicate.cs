using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using BSim;

using System.Windows.Forms;

using System.Linq;

using KangarooSolver;

/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance : GH_ScriptInstance
{
#region Utility functions
  /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
  /// <param name="text">String to print.</param>
  private void Print(string text) { __out.Add(text); }
  /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
  /// <param name="format">String format.</param>
  /// <param name="args">Formatting parameters.</param>
  private void Print(string format, params object[] args) { __out.Add(string.Format(format, args)); }
  /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj)); }
  /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj, string method_name) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj, method_name)); }
#endregion

#region Members
  /// <summary>Gets the current Rhino document.</summary>
  private RhinoDoc RhinoDocument;
  /// <summary>Gets the Grasshopper document that owns this script.</summary>
  private GH_Document GrasshopperDocument;
  /// <summary>Gets the Grasshopper script component that owns this script.</summary>
  private IGH_Component Component; 
  /// <summary>
  /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
  /// Any subsequent call within the same solution will increment the Iteration count.
  /// </summary>
  private int Iteration;
#endregion

  /// <summary>
  /// This procedure contains the user code. Input parameters are provided as regular arguments, 
  /// Output parameters as ref arguments. You don't have to assign output parameters, 
  /// they will have a default value.
  /// </summary>
  private void RunScript(ref object C)
  {
        List<Communicate> PP_Commun = new List<Communicate>()
      {
        new ICom_Avoid(1.05, 2),
        new ICom_Talk(0.5, 30, 5, 1)
        };

    C = PP_Commun;
  }

  // <Custom additional code> 
    public class ICom_Talk_: Communicate
  {
    double Distance;
    int talkTime;

    public ICom_Talk_(double Distance, int talkTime)
    {
      this.Distance = Distance;
      this.talkTime = talkTime;
    }

    public override List<IGoal> Interact(ABM EN, Person P0, Person P1)
    {
      int max = 5;

      List<IGoal> goals = new List<IGoal>(0);

      bool feasible = false;

      if (P0.State == Person_state.Walk && P1.State == Person_state.Walk)
      {
        feasible = true;
      }

      if (!feasible) { return goals; }

      double Dis = P0.SizeR + P1.SizeR + Distance;
      if (P0.Locate.DistanceTo(P1.Locate) < Dis && feasible)
      {
        //Did not talked before;
        List<BSim.Person.Memory> P0_talked = P0.Brain.Where(p => p.Event.IsType("Talk")).ToList();
        List<BSim.Person.Memory> P1_talked = P1.Brain.Where(p => p.Event.IsType("Talk")).ToList();

        if (P0_talked.Count < max && P1_talked.Count < max)
        {
          int seed = EN.Iteration;
          seed += EN.PPs.IndexOf(P0);

          Random rd = new Random(seed);
          int num = rd.Next(0, 100);

          if (num == 5)
          {
            if (P0.Target != null)
            {
              P0.ToDoList.Insert(0, P0.Target.Type.Name);
              P0.Target.Expire(P0);
            }

            P0.Target = new IEvent_Talk("Talk", P1, Dis);
            P0.State = Person_state.Talk;
            P0.Brain.Add(new Person.Memory(P0.Target, talkTime));
            P0.Path = new List<Point3d>();

            if (P1.Target != null)
            {
              P1.ToDoList.Insert(0, P1.Target.Type.Name);
              P1.Target.Expire(P1);
            }

            P1.Target = new IEvent_Talk("Talk", P0, Dis);
            P1.State = Person_state.Talk;
            P1.Brain.Add(new Person.Memory(P1.Target, talkTime));
            P1.Path = new List<Point3d>();
          }
        }
      }


      IEvent_Talk P0_t = P0.Target as IEvent_Talk;
      IEvent_Talk P1_t = P1.Target as IEvent_Talk;

      if (P0_t != null && P1_t != null)
      {
        if (P0_t.PP.Name == P1.Name && P1_t.PP.Name == P0.Name)
        {
          if (P0.Brain[P0.Brain.Count - 1].Step > P1.Brain[P1.Brain.Count - 1].Step)
          {
            P0.Brain[P0.Brain.Count - 1].Step = P1.Brain[P1.Brain.Count - 1].Step;
          }
          else
          {
            P1.Brain[P1.Brain.Count - 1].Step = P0.Brain[P0.Brain.Count - 1].Step;
          }
        }
      }

      return goals;
    }
  }
  // </Custom additional code> 

  private List<string> __err = new List<string>(); //Do not modify this list directly.
  private List<string> __out = new List<string>(); //Do not modify this list directly.
  private RhinoDoc doc = RhinoDoc.ActiveDoc;       //Legacy field.
  private IGH_ActiveObject owner;                  //Legacy field.
  private int runCount;                            //Legacy field.
  
  public override void InvokeRunScript(IGH_Component owner, object rhinoDocument, int iteration, List<object> inputs, IGH_DataAccess DA)
  {
    //Prepare for a new run...
    //1. Reset lists
    this.__out.Clear();
    this.__err.Clear();

    this.Component = owner;
    this.Iteration = iteration;
    this.GrasshopperDocument = owner.OnPingDocument();
    this.RhinoDocument = rhinoDocument as Rhino.RhinoDoc;

    this.owner = this.Component;
    this.runCount = this.Iteration;
    this. doc = this.RhinoDocument;

    //2. Assign input parameters
    

    //3. Declare output parameters
      object C = null;


    //4. Invoke RunScript
    RunScript(ref C);
      
    try
    {
      //5. Assign output parameters to component...
            if (C != null)
      {
        if (GH_Format.TreatAsCollection(C))
        {
          IEnumerable __enum_C = (IEnumerable)(C);
          DA.SetDataList(0, __enum_C);
        }
        else
        {
          if (C is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(0, (Grasshopper.Kernel.Data.IGH_DataTree)(C));
          }
          else
          {
            //assign direct
            DA.SetData(0, C);
          }
        }
      }
      else
      {
        DA.SetData(0, null);
      }

    }
    catch (Exception ex)
    {
      this.__err.Add(string.Format("Script exception: {0}", ex.Message));
    }
    finally
    {
      //Add errors and messages... 
      if (owner.Params.Output.Count > 0)
      {
        if (owner.Params.Output[0] is Grasshopper.Kernel.Parameters.Param_String)
        {
          List<string> __errors_plus_messages = new List<string>();
          if (this.__err != null) { __errors_plus_messages.AddRange(this.__err); }
          if (this.__out != null) { __errors_plus_messages.AddRange(this.__out); }
          if (__errors_plus_messages.Count > 0) 
            DA.SetDataList(0, __errors_plus_messages);
        }
      }
    }
  }
}