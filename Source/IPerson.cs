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
  private void RunScript(string ID, Point3d Locate, double RSize, double Strengh, double VLength, double VAngle, System.Object IWalk, System.Object Meta, List<System.Object> BE, List<System.Object> TDL, ref object IPerson)
  {
        List<Behavior> Behaves = BE.Select(p => p as Behavior).ToList();
    List<string> ToDoList = TDL.Select(p => (string) p).ToList();

    Person P = new Person(ID, Locate, RSize, Strengh, VLength, VAngle, (Behavior) IWalk, Behaves, ToDoList);
    P.Meta = (Metabolism) Meta;
    IPerson = P;
  }

  // <Custom additional code> 
  
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
        string ID = default(string);
    if (inputs[0] != null)
    {
      ID = (string)(inputs[0]);
    }

    Point3d Locate = default(Point3d);
    if (inputs[1] != null)
    {
      Locate = (Point3d)(inputs[1]);
    }

    double RSize = default(double);
    if (inputs[2] != null)
    {
      RSize = (double)(inputs[2]);
    }

    double Strengh = default(double);
    if (inputs[3] != null)
    {
      Strengh = (double)(inputs[3]);
    }

    double VLength = default(double);
    if (inputs[4] != null)
    {
      VLength = (double)(inputs[4]);
    }

    double VAngle = default(double);
    if (inputs[5] != null)
    {
      VAngle = (double)(inputs[5]);
    }

    System.Object IWalk = default(System.Object);
    if (inputs[6] != null)
    {
      IWalk = (System.Object)(inputs[6]);
    }

    System.Object Meta = default(System.Object);
    if (inputs[7] != null)
    {
      Meta = (System.Object)(inputs[7]);
    }

    List<System.Object> BE = null;
    if (inputs[8] != null)
    {
      BE = GH_DirtyCaster.CastToList<System.Object>(inputs[8]);
    }
    List<System.Object> TDL = null;
    if (inputs[9] != null)
    {
      TDL = GH_DirtyCaster.CastToList<System.Object>(inputs[9]);
    }


    //3. Declare output parameters
      object IPerson = null;


    //4. Invoke RunScript
    RunScript(ID, Locate, RSize, Strengh, VLength, VAngle, IWalk, Meta, BE, TDL, ref IPerson);
      
    try
    {
      //5. Assign output parameters to component...
            if (IPerson != null)
      {
        if (GH_Format.TreatAsCollection(IPerson))
        {
          IEnumerable __enum_IPerson = (IEnumerable)(IPerson);
          DA.SetDataList(0, __enum_IPerson);
        }
        else
        {
          if (IPerson is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(0, (Grasshopper.Kernel.Data.IGH_DataTree)(IPerson));
          }
          else
          {
            //assign direct
            DA.SetData(0, IPerson);
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