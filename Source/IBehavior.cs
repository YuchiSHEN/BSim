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
  private void RunScript(ref object BE)
  {
    

    List <Behavior> Behaviors = new List<Behavior>(){
        new IBehav_NextJob(IBehavior_Func.RandomSelectTheLeastOccupied),//IBehavior_Func.RandomSelectTheLeastOccupied new IBehav_NextJob(SelectTheCloset)
        new IBehav_DoJob(),
        new IBehav_AvoidObs(4),
        new IBehav_Wait(0.9, 2)
        };

    BE = Behaviors;
  }

  // <Custom additional code> 
    public static List<Event> SelectTheCloset(Person P, ABM ABM)
  {
    List<Event> ValidPoIs = new List<Event>();
    //Pack the untraveled interest points
    foreach (Event poi in ABM.POIs)
    {
      if (!poi.IsType(P.NextToDo)) {continue;}
      if (poi.CanBeVisit(P, ABM))
      {
        ValidPoIs.Add(poi);
      }
    }

    if (ValidPoIs.Count == 0) { return new List<Event>(); }

    //Select the least beTarget Poi
    ValidPoIs.Sort(
      (a, b) => (a.locate.DistanceTo(P.Locate) * Math.Pow((a.beTarget.Count + 1), 2)).CompareTo(b.locate.DistanceTo(P.Locate) * Math.Pow((b.beTarget.Count + 1), 2))
      );

    //Select the next Poi
    Event POI = ValidPoIs[0];
    return new List<Event>() { POI };
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
      object BE = null;


    //4. Invoke RunScript
    RunScript(ref BE);
      
    try
    {
      //5. Assign output parameters to component...
            if (BE != null)
      {
        if (GH_Format.TreatAsCollection(BE))
        {
          IEnumerable __enum_BE = (IEnumerable)(BE);
          DA.SetDataList(0, __enum_BE);
        }
        else
        {
          if (BE is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(0, (Grasshopper.Kernel.Data.IGH_DataTree)(BE));
          }
          else
          {
            //assign direct
            DA.SetData(0, BE);
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