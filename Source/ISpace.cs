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
  private void RunScript(List<Curve> Obstacles, double Obs_Div, List<Line> Path_Net, bool LoadFile, ref object SP)
  {
        if(LoadFile)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.Filter = "CSV files (*.csv)|*.csv";
      openFileDialog.Title = "Open Path Data";
      openFileDialog.RestoreDirectory = true;

      if (openFileDialog.ShowDialog() == DialogResult.OK)
      {
        List<Point3d> Nodes;
        PathNet.ShortPathGraph Graph = PathNet.PathNetWork(Path_Net, tol, out Nodes);
        try
        {
          PathNet.QuickPath QP = new PathNet.QuickPath(openFileDialog.FileName, Nodes);
          Space = new ISpace(Obstacles, Obs_Div, QP, Nodes);
          Component.Message = "Loaded:\n" + openFileDialog.SafeFileName;
        }
        catch(Exception e)
        {
          Component.Message = "Error: UnMatch";
          Print(e.ToString());
        }

      }

    }

    SP = Space;
  }

  // <Custom additional code> 
    ISpace Space;
  double tol = 0.001;
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
        List<Curve> Obstacles = null;
    if (inputs[0] != null)
    {
      Obstacles = GH_DirtyCaster.CastToList<Curve>(inputs[0]);
    }
    double Obs_Div = default(double);
    if (inputs[1] != null)
    {
      Obs_Div = (double)(inputs[1]);
    }

    List<Line> Path_Net = null;
    if (inputs[2] != null)
    {
      Path_Net = GH_DirtyCaster.CastToList<Line>(inputs[2]);
    }
    bool LoadFile = default(bool);
    if (inputs[3] != null)
    {
      LoadFile = (bool)(inputs[3]);
    }



    //3. Declare output parameters
      object SP = null;


    //4. Invoke RunScript
    RunScript(Obstacles, Obs_Div, Path_Net, LoadFile, ref SP);
      
    try
    {
      //5. Assign output parameters to component...
            if (SP != null)
      {
        if (GH_Format.TreatAsCollection(SP))
        {
          IEnumerable __enum_SP = (IEnumerable)(SP);
          DA.SetDataList(1, __enum_SP);
        }
        else
        {
          if (SP is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(1, (Grasshopper.Kernel.Data.IGH_DataTree)(SP));
          }
          else
          {
            //assign direct
            DA.SetData(1, SP);
          }
        }
      }
      else
      {
        DA.SetData(1, null);
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