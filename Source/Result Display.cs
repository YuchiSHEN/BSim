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
using System.Drawing;

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
  private void RunScript(System.Object Model, ref object A, ref object M)
  {
        ABM ABM = (ABM) Model;
    Person_Body = ABM.PPs.Select(p => p.Body).ToList();
    Person_Text = ABM.PPs.Select(p => p.State.ToString()).ToList();
    Person_Bar = ABM.PPs.Select(p => ProgressBar(p)).ToList();
    Person_Locate = ABM.PPs.Select(p => p.Locate + new Vector3d(0, -p.SizeR * 0.25, 0) + new Vector3d(-p.SizeR * 0.5, 0, 0)).ToList();


    foreach(Person P in ABM.PPs)
    {
      Print(P.Name);
      Print(P.State.ToString());
      Print(P.NextToDo);
      Print("_____________");
    }
    M = ABM.PPs;
  }

  // <Custom additional code> 
    List<Point3d>Person_Locate = new List<Point3d>();
  List<Curve>Person_Body = new List<Curve>();
  List<string>Person_Text = new List<string>();
  List<List<Point3d>>Person_Bar = new List<List<Point3d>>();

  public List<Point3d> ProgressBar(Person P)
  {
    if(P.Brain.Count > 0)
    {
      double percent = (double) P.Brain[P.Brain.Count - 1].Step / (double) P.Brain[P.Brain.Count - 1].MaxStep;
      Point3d basePt = new Point3d(P.Locate); basePt.Y += P.SizeR * 1.3;
      Point3d P0 = basePt - new Vector3d(P.SizeR, 0, 0) * percent + new Vector3d(0, 0.02, 0);
      Point3d P1 = basePt - new Vector3d(P.SizeR, 0, 0) * percent - new Vector3d(0, 0.02, 0);
      Point3d P2 = basePt + new Vector3d(P.SizeR, 0, 0) * percent - new Vector3d(0, 0.02, 0);
      Point3d P3 = basePt + new Vector3d(P.SizeR, 0, 0) * percent + new Vector3d(0, 0.02, 0);

      if(percent == 1){return new List <Point3d>();}
      List <Point3d> polygon = new List<Point3d>(){P0,P1,P2,P3,P0};
      return polygon;
    }
    else
    {
      return new List <Point3d>(){};
    }
  }

  public override void DrawViewportWires(IGH_PreviewArgs args)
  {
    Plane pl0_Plane;
    args.Viewport.GetFrustumFarPlane(out pl0_Plane);
    Rhino.Display.Text3d drawText = null;  // Initialize to null

    for (int i = 0; i < Person_Bar.Count; i++)
    {
      pl0_Plane.Origin = Person_Locate[i];

      //args.Display.DrawCurve(new PolylineCurve(Person_Bar[i]), Color.Black, 2);
      args.Display.DrawPolygon(Person_Bar[i], Color.Gray, true);
      args.Display.DrawCurve(Person_Body[i], Color.Black, 2);

      drawText = new Rhino.Display.Text3d(Person_Text[i], pl0_Plane, 0.1);
      args.Display.Draw3dText(drawText, Color.Black);
    }

    if(drawText != null)
    {drawText.Dispose();}

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
        System.Object Model = default(System.Object);
    if (inputs[0] != null)
    {
      Model = (System.Object)(inputs[0]);
    }



    //3. Declare output parameters
      object A = null;
  object M = null;


    //4. Invoke RunScript
    RunScript(Model, ref A, ref M);
      
    try
    {
      //5. Assign output parameters to component...
            if (A != null)
      {
        if (GH_Format.TreatAsCollection(A))
        {
          IEnumerable __enum_A = (IEnumerable)(A);
          DA.SetDataList(1, __enum_A);
        }
        else
        {
          if (A is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(1, (Grasshopper.Kernel.Data.IGH_DataTree)(A));
          }
          else
          {
            //assign direct
            DA.SetData(1, A);
          }
        }
      }
      else
      {
        DA.SetData(1, null);
      }
      if (M != null)
      {
        if (GH_Format.TreatAsCollection(M))
        {
          IEnumerable __enum_M = (IEnumerable)(M);
          DA.SetDataList(2, __enum_M);
        }
        else
        {
          if (M is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(2, (Grasshopper.Kernel.Data.IGH_DataTree)(M));
          }
          else
          {
            //assign direct
            DA.SetData(2, M);
          }
        }
      }
      else
      {
        DA.SetData(2, null);
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